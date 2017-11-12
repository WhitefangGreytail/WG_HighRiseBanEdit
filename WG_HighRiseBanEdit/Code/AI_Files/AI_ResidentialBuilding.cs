using System;
using ColossalFramework.Math;
using ColossalFramework;
using UnityEngine;
using Boformer.Redirection;

namespace WG_HighRiseBanEdit
{
    [TargetType(typeof(ResidentialBuildingAI))]
    class AI_ResidentialBuilding : PrivateBuildingAI
    {
        [RedirectMethod(true)]
        public override void SimulationStep(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            base.SimulationStep(buildingID, ref buildingData, ref frameData);
            SimulationManager instance = Singleton<SimulationManager>.instance;
            DistrictManager instance2 = Singleton<DistrictManager>.instance;
            byte district = instance2.GetDistrict(buildingData.m_position);
            DistrictPolicies.CityPlanning cityPlanningPolicies = instance2.m_districts.m_buffer[(int)district].m_cityPlanningPolicies;
            if ((buildingData.m_flags & (Building.Flags.Completed | Building.Flags.Upgrading)) != Building.Flags.None)
            {
                instance2.m_districts.m_buffer[(int)district].AddResidentialData(buildingData.Width * buildingData.Length, (buildingData.m_flags & Building.Flags.Abandoned) != Building.Flags.None, (buildingData.m_flags & Building.Flags.Collapsed) != Building.Flags.None && frameData.m_fireDamage == 255, (buildingData.m_flags & Building.Flags.Collapsed) != Building.Flags.None && frameData.m_fireDamage != 255, this.m_info.m_class.m_subService);
            }
            if ((buildingData.m_levelUpProgress == 255 || (buildingData.m_flags & Building.Flags.Collapsed) == Building.Flags.None) && buildingData.m_fireIntensity == 0)
            {
/*
                if ((this.m_info.m_class.m_subService == ItemClass.SubService.ResidentialHigh || this.m_info.m_class.m_subService == ItemClass.SubService.ResidentialHighEco) && 
                    (cityPlanningPolicies & DistrictPolicies.CityPlanning.HighriseBan) != DistrictPolicies.CityPlanning.None && this.m_info.m_class.m_level == ItemClass.Level.Level5 &&
                    instance.m_randomizer.Int32(10u) == 0 && Singleton<ZoneManager>.instance.m_lastBuildIndex == instance.m_currentBuildIndex)
                {
                    District[] expr_184_cp_0 = instance2.m_districts.m_buffer;
                    byte expr_184_cp_1 = district;
                    expr_184_cp_0[(int)expr_184_cp_1].m_cityPlanningPoliciesEffect = (expr_184_cp_0[(int)expr_184_cp_1].m_cityPlanningPoliciesEffect | DistrictPolicies.CityPlanning.HighriseBan);
                    buildingData.m_flags |= Building.Flags.Demolishing;
                    instance.m_currentBuildIndex += 1u;
                }
*/
                if (instance.m_randomizer.Int32(10u) == 0)
                {
                    DistrictPolicies.Specialization specializationPolicies = instance2.m_districts.m_buffer[(int)district].m_specializationPolicies;
                    DistrictPolicies.Specialization specialization = this.SpecialPolicyNeeded();
                    if (specialization != DistrictPolicies.Specialization.None)
                    {
                        if ((specializationPolicies & specialization) == DistrictPolicies.Specialization.None)
                        {
                            if (Singleton<ZoneManager>.instance.m_lastBuildIndex == instance.m_currentBuildIndex)
                            {
                                buildingData.m_flags |= Building.Flags.Demolishing;
                                instance.m_currentBuildIndex += 1u;
                            }
                        }
                        else
                        {
                            District[] expr_240_cp_0 = instance2.m_districts.m_buffer;
                            byte expr_240_cp_1 = district;
                            expr_240_cp_0[(int)expr_240_cp_1].m_specializationPoliciesEffect = (expr_240_cp_0[(int)expr_240_cp_1].m_specializationPoliciesEffect | specialization);
                        }
                    }
                    else if ((specializationPolicies & DistrictPolicies.Specialization.Selfsufficient) != DistrictPolicies.Specialization.None && Singleton<ZoneManager>.instance.m_lastBuildIndex == instance.m_currentBuildIndex)
                    {
                        buildingData.m_flags |= Building.Flags.Demolishing;
                        instance.m_currentBuildIndex += 1u;
                    }
                }
            }
        }

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
            if (level3 > this.m_info.m_class.m_level)
            {
                num3 = 0;
                num5 = 0;
/*
                if (this.m_info.m_class.m_subService == ItemClass.SubService.ResidentialHigh && (cityPlanningPolicies & DistrictPolicies.CityPlanning.HighriseBan) != DistrictPolicies.CityPlanning.None && level3 == ItemClass.Level.Level5)
                {
                    District[] expr_41B_cp_0 = instance.m_districts.m_buffer;
                    byte expr_41B_cp_1 = district;
                    expr_41B_cp_0[(int)expr_41B_cp_1].m_cityPlanningPoliciesEffect = (expr_41B_cp_0[(int)expr_41B_cp_1].m_cityPlanningPoliciesEffect | DistrictPolicies.CityPlanning.HighriseBan);
                    level3 = ItemClass.Level.Level4;
                    num3 = 1;
                }
*/
                if (buildingData.m_problems == Notification.Problem.None && level3 > this.m_info.m_class.m_level && this.GetUpgradeInfo(buildingID, ref buildingData) != null && !Singleton<DisasterManager>.instance.IsEvacuating(buildingData.m_position))
                {
                    frameData.m_constructState = 0;
                    base.StartUpgrading(buildingID, ref buildingData);
                }
            }
            buildingData.m_levelUpProgress = (byte)(num3 | num5 << 4);
        }


        /* This is here because it's easier to have this, than call their function */
        private DistrictPolicies.Specialization SpecialPolicyNeeded()
        {
            ItemClass.SubService subService = this.m_info.m_class.m_subService;
            if (subService != ItemClass.SubService.ResidentialLowEco && subService != ItemClass.SubService.ResidentialHighEco)
            {
                return DistrictPolicies.Specialization.None;
            }
            return DistrictPolicies.Specialization.Selfsufficient;
        }
    }
}
