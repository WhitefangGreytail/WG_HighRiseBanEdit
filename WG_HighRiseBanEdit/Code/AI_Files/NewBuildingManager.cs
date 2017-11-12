using System;
using ColossalFramework.Math;
using ColossalFramework;
using UnityEngine;
using Boformer.Redirection;
using System.Reflection;
using System.Collections.Generic;

namespace WG_HighRiseBanEdit
{
    [TargetType(typeof(BuildingManager))]
    public class NewBuildingManager : BuildingManager
    {
        FieldInfo m_buildingsRefreshed_Field;
        FieldInfo m_areaBuildings_Field;
        FieldInfo m_styleBuildings_Field;

        private bool m_buildingsRefreshed;
        private FastList<ushort>[] m_areaBuildings;
        private Dictionary<int, FastList<ushort>>[] m_styleBuildings;

        // BuildingManager
        [RedirectMethod(true)]
        public BuildingInfo GetRandomBuildingInfo(ref Randomizer r, ItemClass.Service service, ItemClass.SubService subService, ItemClass.Level level, int width, int length, BuildingInfo.ZoningMode zoningMode, int style)
        {
            getPrivates();

            if (!m_buildingsRefreshed)
            {
                CODebugBase<LogChannel>.Error(LogChannel.Core, "Random buildings not refreshed yet!");
                return null;
            }
            int num = GetAreaIndex(service, subService, level, width, length, zoningMode);
            FastList<ushort> fastList;
            if (style > 0)
            {
                style--;
                DistrictStyle districtStyle = Singleton<DistrictManager>.instance.m_Styles[style];
                if (style <= m_styleBuildings.Length && m_styleBuildings[style] != null && m_styleBuildings[style].Count > 0 && districtStyle.AffectsService(service, subService, level))
                {
                    if (m_styleBuildings[style].ContainsKey(num))
                    {
                        fastList = m_styleBuildings[style][num];
                    }
                    else
                    {
                        fastList = null;
                    }
                }
                else
                {
                    fastList = m_areaBuildings[num];
                }
            }
            else
            {
                fastList = m_areaBuildings[num];
            }
            if (fastList == null)
            {
                return null;
            }
            if (fastList.m_size == 0)
            {
                return null;
            }

            num = r.Int32((uint)fastList.m_size);
            BuildingInfo retVal = PrefabCollection<BuildingInfo>.GetPrefab((uint)fastList.m_buffer[num]);
            float heightLimit = getHeightLimit(service, subService, AI_PrivateBuilding.highRiseBan);
            AI_PrivateBuilding.highRiseBan = false;

            // Height limits are on all buildings which are reaosnably expected to have really tall buildings
            while (retVal.m_size.y > heightLimit)
            {
Debugging.writeDebugToFile(retVal.m_size.y + " > " + heightLimit);
                num = r.Int32((uint)fastList.m_size);
                // num++ % fastList.m_size - Maybe to just go through the list
                retVal = PrefabCollection<BuildingInfo>.GetPrefab((uint)fastList.m_buffer[num]);
            }

            return retVal;
        } // end


        [RedirectReverse(true)]
        private static int GetAreaIndex(ItemClass.Service service, ItemClass.SubService subService, ItemClass.Level level, int width, int length, BuildingInfo.ZoningMode zoningMode)
        {
            Debugging.writeDebugToFile("GetAreaIndex has not been reversed correctly!");
            return 0;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="subService"></param>
        /// <param name="highRiseBan"></param>
        /// <returns></returns>
        private float getHeightLimit(ItemClass.Service service, ItemClass.SubService subService, bool highRiseBan)
        {
            float retVal = 500000.0f;  // Anyone who exceeds this deserves a thumping.

            if (highRiseBan)
            {
                switch (service)
                {
                    case ItemClass.Service.Residential:
                        retVal = DataStore.getInstance().resHeightLimit;
                        break;
                    case ItemClass.Service.Industrial:
                        retVal = DataStore.getInstance().indHeightLimit;
                        break;
                    case ItemClass.Service.Commercial:
                        retVal = DataStore.getInstance().comHeightLimit;
                        break;
                    case ItemClass.Service.Office:
                        retVal = DataStore.getInstance().offHeightLimit;
                        break;
                }
            }

            return retVal;
        }


        private void getPrivates()
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

            Type bmType = typeof(BuildingManager);
            BuildingManager bmInstance = BuildingManager.instance;

            m_buildingsRefreshed_Field = bmType.GetField("m_buildingsRefreshed", bindFlags);
            m_areaBuildings_Field = bmType.GetField("m_areaBuildings", bindFlags);
            m_styleBuildings_Field = bmType.GetField("m_styleBuildings", bindFlags);

            m_buildingsRefreshed = (bool) m_buildingsRefreshed_Field.GetValue(instance);
            m_areaBuildings = (FastList<ushort>[]) m_areaBuildings_Field.GetValue(instance);
            m_styleBuildings = (Dictionary<int, FastList<ushort>>[]) m_styleBuildings_Field.GetValue(instance);
        }
    }
}
