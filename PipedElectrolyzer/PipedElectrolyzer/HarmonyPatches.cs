using Harmony;
using System.Collections.Generic;
using Database;
using KSerialization;
using System;
using System.Linq;
namespace PipedElectrolyzer
{
    public class HarmonyPatches
    {
        [HarmonyPatch(typeof(GeneratedBuildings))]
        [HarmonyPatch("LoadGeneratedBuildings")]
        public static class PipedElectrolyzer_GeneratedBuildings_LoadGeneratedBuildings
        {
            public static void Prefix()
            {
                string prefix = "STRINGS.BUILDINGS.PREFABS." + PipedElectrolyzerConfig.ID.ToUpper();
                Strings.Add(prefix + ".NAME", "Piped Electrolyzer");
                Strings.Add(prefix + ".DESC", "Water goes in one end. life sustaining oxygen comes out the other");
                Strings.Add(prefix + ".EFFECT", "Converts Water into Oxygen and Hydrogen.\n\nBecomes idle when the area reaches maximum pressure capacity.\n\nSends oxygen directly to connected pipes at the cost of extra power.");
                ModUtil.AddBuildingToPlanScreen("Oxygen", PipedElectrolyzerConfig.ID);
            }
        }

        [HarmonyPatch(typeof(Db))]
        [HarmonyPatch("Initialize")]
        public static class PipedElectrolyzer_Db_Initialize
        {
            public static void Prefix()
            {
                List<string> list = new List<string>(Techs.TECH_GROUPING["ImprovedOxygen"]);
                list.Add(PipedElectrolyzerConfig.ID);
                Techs.TECH_GROUPING["ImprovedOxygen"] = list.ToArray();
            }
        }
    }
}
