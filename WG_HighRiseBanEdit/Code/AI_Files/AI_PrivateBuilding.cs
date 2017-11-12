using ColossalFramework;
using ColossalFramework.Math;
using System;
using Boformer.Redirection;


namespace WG_HighRiseBanEdit
{
    [TargetType(typeof(PrivateBuildingAI))]
    public class AI_PrivateBuilding : PrivateBuildingAI
    {
        public static Boolean highRiseBan = false;

        // PrivateBuildingAI
        [RedirectMethod(true)]
        public override BuildingInfo GetUpgradeInfo(ushort buildingID, ref Building data)
        {
            if (this.m_info.m_class.m_level == ItemClass.Level.Level5)
            {
                return null;
            }
            Randomizer randomizer = new Randomizer((int)buildingID);
            for (int i = 0; i <= (int)this.m_info.m_class.m_level; i++)
            {
                randomizer.Int32(1000u);
            }
            ItemClass.Level level = this.m_info.m_class.m_level + 1;
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte district = instance.GetDistrict(data.m_position);
            DistrictPolicies.CityPlanning cityPlanningPolicies = instance.m_districts.m_buffer[(int)district].m_cityPlanningPolicies;
            highRiseBan = ((cityPlanningPolicies & DistrictPolicies.CityPlanning.HighriseBan) != DistrictPolicies.CityPlanning.None);
            ushort style = instance.m_districts.m_buffer[(int)district].m_Style;
            return Singleton<BuildingManager>.instance.GetRandomBuildingInfo(ref randomizer, this.m_info.m_class.m_service, this.m_info.m_class.m_subService, level, data.Width, data.Length, this.m_info.m_zoningMode, (int)style);
        }

    }
}
