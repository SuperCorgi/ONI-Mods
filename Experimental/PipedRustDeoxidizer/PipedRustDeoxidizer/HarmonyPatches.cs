using Database;
using Harmony;
using System.Collections.Generic;

namespace PipedRustDeoxidizer
{
    public class HarmonyPatches
    {
        //Allow the PipedRustDeoxidizer to appear in the build menu
        [HarmonyPatch(typeof(GeneratedBuildings))]
        [HarmonyPatch("LoadGeneratedBuildings")]
        public static class PipedElectrolyzer_GeneratedBuildings_LoadGeneratedBuildings
        {
            public static void Prefix()
            {
                string str = "STRINGS.BUILDINGS.PREFABS." + PipedRustDeoxidizerConfig.ID.ToUpper();
                Strings.Add(str + ".NAME", "Piped Rust Deoxidizer");
                Strings.Add(str + ".DESC", "Rust and salt goes in, oxygen goes out. Now piped!");
                //Strings.Add(str + "Converts " + UI.FormatAsLink("Rust", "RUST") + " into " + UI.FormatAsLink("Oxygen", "OXYGEN") + " and " + UI.FormatAsLink("Chlorine", "CHLORINE") + ".\n\nUpgraded with refined metals and plastic, this version prevents gasses from escaping the machine. Extra power is used to separate the gasses and send into dedicated pipes.");
                ModUtil.AddBuildingToPlanScreen("Oxygen", PipedRustDeoxidizerConfig.ID);
            }
        }
        //Defines what research this should appear in
        [HarmonyPatch(typeof(Db))]
        [HarmonyPatch("Initialize")]
        public static class PipedElectrolyzer_Db_Initialize
        {
            public static void Prefix()
            {
                Techs.TECH_GROUPING["ImprovedOxygen"] = new List<string>(Techs.TECH_GROUPING["ImprovedOxygen"])
                {
                    PipedRustDeoxidizerConfig.ID
                }.ToArray();
            }
        }
    } //End HarmonyPatches class
} //End Namespace