using UnityEngine;
using System.Collections.Generic;
using PressurizedPipes.BuildingConfigs;
namespace PressurizedPipes.Components
{
    /*
     *  To simplify the logic to tint buildings. The PressurizedTuning class contains information that pertains specifically to Conduit and ConduitBridges.
     *  The tint colors are NOT saved within the component. This is to allow the possibility to adjust the tints later and have the new tints apply to old buildings
     *  Tint colors are saved in the static dictionary TintTable, tied to the buildings PrefabID.
     *  AddToTintTable adds to the dictionary so that ExtremePipes (and other potential future mods dependent on this) can utilize this component by adding their own entries for valves.
     *  A HarmonyPatch will search for this component when the OverlayModes switches and attemmpts to reset the tint back to white. If this component is present, it will use this components TintColour instead of white.
     */
     //DEPRECATED. BUILDINGS NOW HAVE CUSTOM KANIMS RATHER THAN TINT
    public class Tintable : KMonoBehaviour
    {
        [MyCmpGet]
        private KAnimControllerBase controller;
        public Color32 TintColour
        {
            get;
            private set;
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            InitTintColour();
            SetTint();
        }
        public void SetTint()
        {
            controller.TintColour = TintColour;
        }

        private void InitTintColour()
        {
            string id = GetComponent<Building>()?.Def.PrefabID;
            if (TintTable.ContainsKey(id))
                TintColour = TintTable[id];
            else
                TintColour = new Color32(255, 255, 255, 255);
        }

        private static Dictionary<string, Color32> TintTable = new Dictionary<string, Color32>()
        {
            {
                PressurizedGasValveConfig.ID, new Color32(228, 115, 89, 255)
            },
            {
                PressurizedLiquidValveConfig.ID, new Color32(235, 160, 120, 255)
            }
        };
        public static bool AddToTintTable(string id, Color32 color)
        {
            if(!TintTable.ContainsKey(id))
            {
                TintTable.Add(id, color);
                return true;
            }
            return false;
        }
    }
}
