using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
namespace ExampleMultiIOBridge
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
                string str = "STRINGS.BUILDINGS.PREFABS." + MyLiquidConduitBridgeConfig.ID.ToUpper();
                Strings.Add(str + ".NAME", "Custom Liquid Bridge");
                Strings.Add(str + ".DESC", "A lqiud bridge showcasing the MultiIO Conduit Updater feature!");
                //Strings.Add(str + "Converts " + UI.FormatAsLink("Rust", "RUST") + " into " + UI.FormatAsLink("Oxygen", "OXYGEN") + " and " + UI.FormatAsLink("Chlorine", "CHLORINE") + ".\n\nUpgraded with refined metals and plastic, this version prevents gasses from escaping the machine. Extra power is used to separate the gasses and send into dedicated pipes.");
                ModUtil.AddBuildingToPlanScreen("Oxygen", MyLiquidConduitBridgeConfig.ID);
            }
        }
    }
}
