using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using PressurizedPipes;
using UnityEngine;
namespace ExtremePipes
{
    public class HarmonyPatches
    {
        [HarmonyPatch(typeof(GeneratedBuildings))]
        [HarmonyPatch("LoadGeneratedBuildings")]
        public static class PipedElectrolyzer_GeneratedBuildings_LoadGeneratedBuildings
        {
            public static void Prefix()
            {
                string prefix = "STRINGS.BUILDINGS.PREFABS." + ExtremeGasConduitConfig.ID.ToUpper();
                Strings.Add(prefix + ".NAME", "Extreme Gas Pipe");
                Strings.Add(prefix + ".DESC", "Reinforced with large amounts of steel and plastics, this gas pipe is able to carry an extreme amount of gas.");
                Strings.Add(prefix + ".EFFECT", "Can carry a maximum of 10KG of gas. Similar to Pressurized Pipes, it can damage connected pipes if their maximum capacity is significantly lower than this pipes current contents.");
                ModUtil.AddBuildingToPlanScreen("HVAC", ExtremeGasConduitConfig.ID);
                PressurizedTuning.TryAddPressurizedInfo(ExtremeGasConduitConfig.ID, new PressurizedInfo()
                {
                    Capacity = 10f,
                    IncreaseMultiplier = 10f,
                    KAnimTint = new Color32(240, 40, 120, 255),
                    OverlayTint = new Color32(201, 0, 30, 0),
                    FlowTint = new Color32(140, 20, 15, 255),
                    FlowOverlayTint = new Color32(200, 120, 80, 0),
                    IsDefault = false
                });
                prefix = "STRINGS.BUILDINGS.PREFABS." + ExtremeLiquidConduitConfig.ID.ToUpper();
                Strings.Add(prefix + ".NAME", "Extreme Liquid Pipe");
                Strings.Add(prefix + ".DESC", "Reinforced with large amounts of steel and plastics, this liquid pipe is able to carry an extreme amount of liquid.");
                Strings.Add(prefix + ".EFFECT", "Can carry a maximum of 100KG of liquid. Similar to Pressurized Pipes, it can damage connected pipes if their maximum capacity is significantly lower than this pipes current contents.");
                ModUtil.AddBuildingToPlanScreen("Plumbing", ExtremeLiquidConduitConfig.ID);
                PressurizedTuning.TryAddPressurizedInfo(ExtremeLiquidConduitConfig.ID, new PressurizedInfo()
                {
                    Capacity = 100f,
                    IncreaseMultiplier = 10f,
                    KAnimTint = new Color32(240, 40, 120, 255),
                    OverlayTint = new Color32(201, 0, 30, 0),
                    FlowTint = new Color32(235, 40, 30, 255),
                    FlowOverlayTint = new Color32(200, 120, 80, 0),
                    IsDefault = false
                });
                prefix = "STRINGS.BUILDINGS.PREFABS." + ExtremeGasValveConfig.ID.ToUpper();
                Strings.Add(prefix + ".NAME", "Extreme Gas Valve");
                Strings.Add(prefix + ".DESC", "A gas valve that can fully support the flow of an Extreme Gas Pipe.");
                Strings.Add(prefix + ".EFFECT", "Can limit flow by up to 10KG. Will not be damaged by strong flows and will not damage connected outputs.");
                ModUtil.AddBuildingToPlanScreen("HVAC", ExtremeGasValveConfig.ID);

                prefix = "STRINGS.BUILDINGS.PREFABS." + ExtremeLiquidValveConfig.ID.ToUpper();
                Strings.Add(prefix + ".NAME", "Extreme Liquid Valve");
                Strings.Add(prefix + ".DESC", "A liquid valve that can fully support the flow of an Extreme Liquid Pipe.");
                Strings.Add(prefix + ".EFFECT", "Can limit flow by up to 100KG. Will not be damaged by strong flows and will not damage connected outputs.");
                ModUtil.AddBuildingToPlanScreen("Plumbing", ExtremeLiquidValveConfig.ID);
            }
        }
    }
}
