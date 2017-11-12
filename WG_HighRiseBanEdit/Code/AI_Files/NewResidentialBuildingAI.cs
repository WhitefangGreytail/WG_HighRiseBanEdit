using System;
using ColossalFramework.Math;
using ColossalFramework;
using UnityEngine;
using Boformer.Redirection;

namespace WG_HighRiseBanEdit
{
    [TargetType(typeof(ResidentialBuildingAI))]
    public class NewResidentialBuildingAI : ResidentialBuildingAI
    {
        private static CitizenManager instance = Singleton<CitizenManager>.instance;
        private static CitizenUnit[] citizenUnitArray = Singleton<CitizenManager>.instance.m_units.m_buffer;
        private static Citizen[] citizenArray = Singleton<CitizenManager>.instance.m_citizens.m_buffer;

        // ResidentialBuildingAI
        [RedirectMethod(true)]
        private void CheckBuildingLevel(ushort buildingID, ref Building buildingData, ref Building.Frame frameData, ref Citizen.BehaviourData behaviour)
        {
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte district = instance.GetDistrict(buildingData.m_position);
            DistrictPolicies.CityPlanning cityPlanningPolicies = instance.m_districts.m_buffer[(int)district].m_cityPlanningPolicies;
            int num = behaviour.m_educated1Count + behaviour.m_educated2Count * 2 + behaviour.m_educated3Count * 3;
            int num2 = behaviour.m_teenCount + behaviour.m_youngCount * 2 + behaviour.m_adultCount * 3 + behaviour.m_seniorCount * 3;
            int averageEducation;
            ItemClass.Level level;
            int num3;
            if (num2 != 0)
            {
                averageEducation = (num * 300 + (num2 >> 1)) / num2;
                num = (num * 72 + (num2 >> 1)) / num2;
                if (num < 15)
                {
                    level = ItemClass.Level.Level1;
                    num3 = 1 + num;
                }
                else if (num < 30)
                {
                    level = ItemClass.Level.Level2;
                    num3 = 1 + (num - 15);
                }
                else if (num < 45)
                {
                    level = ItemClass.Level.Level3;
                    num3 = 1 + (num - 30);
                }
                else if (num < 60)
                {
                    level = ItemClass.Level.Level4;
                    num3 = 1 + (num - 45);
                }
                else
                {
                    level = ItemClass.Level.Level5;
                    num3 = 1;
                }
                if (level < this.m_info.m_class.m_level)
                {
                    num3 = 1;
                }
                else if (level > this.m_info.m_class.m_level)
                {
                    num3 = 15;
                }
            }
            else
            {
                level = ItemClass.Level.Level1;
                averageEducation = 0;
                num3 = 0;
            }
            int num4;
            Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(ImmaterialResourceManager.Resource.LandValue, buildingData.m_position, out num4);
            ItemClass.Level level2;
            int num5;
            if (num3 != 0)
            {
                if (num4 < 6)
                {
                    level2 = ItemClass.Level.Level1;
                    num5 = 1 + (num4 * 15 + 3) / 6;
                }
                else if (num4 < 21)
                {
                    level2 = ItemClass.Level.Level2;
                    num5 = 1 + ((num4 - 6) * 15 + 7) / 15;
                }
                else if (num4 < 41)
                {
                    level2 = ItemClass.Level.Level3;
                    num5 = 1 + ((num4 - 21) * 15 + 10) / 20;
                }
                else if (num4 < 61)
                {
                    level2 = ItemClass.Level.Level4;
                    num5 = 1 + ((num4 - 41) * 15 + 10) / 20;
                }
                else
                {
                    level2 = ItemClass.Level.Level5;
                    num5 = 1;
                }
                if (level2 < this.m_info.m_class.m_level)
                {
                    num5 = 1;
                }
                else if (level2 > this.m_info.m_class.m_level)
                {
                    num5 = 15;
                }
            }
            else
            {
                level2 = ItemClass.Level.Level1;
                num5 = 0;
            }
            bool flag = false;
            if (this.m_info.m_class.m_level == ItemClass.Level.Level2)
            {
                if (num4 == 0)
                {
                    flag = true;
                }
            }
            else if (this.m_info.m_class.m_level == ItemClass.Level.Level3)
            {
                if (num4 < 11)
                {
                    flag = true;
                }
            }
            else if (this.m_info.m_class.m_level == ItemClass.Level.Level4)
            {
                if (num4 < 31)
                {
                    flag = true;
                }
            }
            else if (this.m_info.m_class.m_level == ItemClass.Level.Level5 && num4 < 51)
            {
                flag = true;
            }
            ItemClass.Level level3 = (ItemClass.Level)Mathf.Min((int)level, (int)level2);
            Singleton<BuildingManager>.instance.m_LevelUpWrapper.OnCalculateResidentialLevelUp(ref level3, ref num3, ref num5, ref flag, averageEducation, num4, buildingID, this.m_info.m_class.m_service, this.m_info.m_class.m_subService, this.m_info.m_class.m_level);
            if (flag)
            {
                buildingData.m_serviceProblemTimer = (byte)Mathf.Min(255, (int)(buildingData.m_serviceProblemTimer + 1));
                if (buildingData.m_serviceProblemTimer >= 8)
                {
                    buildingData.m_problems = Notification.AddProblems(buildingData.m_problems, Notification.Problem.LandValueLow | Notification.Problem.MajorProblem);
                }
                else if (buildingData.m_serviceProblemTimer >= 4)
                {
                    buildingData.m_problems = Notification.AddProblems(buildingData.m_problems, Notification.Problem.LandValueLow);
                }
                else
                {
                    buildingData.m_problems = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem.LandValueLow);
                }
            }
            else
            {
                buildingData.m_serviceProblemTimer = 0;
                buildingData.m_problems = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem.LandValueLow);
            }

/* Remove high rise ban
            if (level3 > this.m_info.m_class.m_level)
            {
                num3 = 0;
                num5 = 0;
                if (this.m_info.m_class.m_subService == ItemClass.SubService.ResidentialHigh && (cityPlanningPolicies & DistrictPolicies.CityPlanning.HighriseBan) != DistrictPolicies.CityPlanning.None && level3 == ItemClass.Level.Level5)
                {
                    District[] expr_41B_cp_0 = instance.m_districts.m_buffer;
                    byte expr_41B_cp_1 = district;
                    expr_41B_cp_0[(int)expr_41B_cp_1].m_cityPlanningPoliciesEffect = (expr_41B_cp_0[(int)expr_41B_cp_1].m_cityPlanningPoliciesEffect | DistrictPolicies.CityPlanning.HighriseBan);
                    level3 = ItemClass.Level.Level4;
                    num3 = 1;
                }
                if (buildingData.m_problems == Notification.Problem.None && level3 > this.m_info.m_class.m_level && this.GetUpgradeInfo(buildingID, ref buildingData) != null)
                {
                    frameData.m_constructState = 0;
                    base.StartUpgrading(buildingID, ref buildingData);
                }
            }
*/
            buildingData.m_levelUpProgress = (byte)(num3 | num5 << 4);
        }

    }
}
