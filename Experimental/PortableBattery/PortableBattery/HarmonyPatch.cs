using Harmony;
using UnityEngine;
using Database;
using System.Collections.Generic;
using PeterHan.PLib;
namespace PortableBattery
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
                string str = "STRINGS.BUILDINGS.PREFABS." + PortableBatteryConfig.ID.ToUpper();
                Strings.Add(str + ".NAME", "Portable Battery");
                Strings.Add(str + ".DESC", "Its portable!");
                Strings.Add(str + ".EFFECT", "A portable battery that can be moved around!");
                ModUtil.AddBuildingToPlanScreen("Oxygen", PortableBatteryConfig.ID);
                Strings.Add("STRINGS.BUILDING.STATUSITEMS.PENDINGRELOCATE.NAME", "Pending Relocation");
                Strings.Add("STRINGS.BUILDING.STATUSITEMS.PENDINGRELOCATE.TOOLTIP", "Awaiting Duplicant");
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
                    PortableBatteryConfig.ID
                }.ToArray();
            }
        }

        [HarmonyPatch(typeof(PlayerController), "OnPrefabInit")]
        public static class PlayerController_OnPrefabInit
        {
            public static void Postfix(PlayerController __instance)
            {
                PToolMode.RegisterTool<PlaceTool>(__instance);
            }
        }

        //The BatteryUI class contains information to display the battery charge number and bar that appears in the Power overlay.
        //If the battery is a PortableBattery that is not placed, disable the BatteryUI component to prevent the battery charge from appearing in the overlay.
        //If the BatteryUI has just been placed but is inactive, re-enable the UI and reset to the new position/content.
        [HarmonyPatch(typeof(BatteryUI), "SetContent")]
        public static class BatteryUI_SetContent
        {
            public static void Postfix(Battery bat, BatteryUI __instance)
            {
                Placeable placeable;
                if (placeable = bat.GetComponent<Placeable>())
                {
                    if (!placeable.IsPlaced)
                    {
                        if (__instance.gameObject.activeSelf)
                            __instance.gameObject.SetActive(false);

                    }
                    else
                    {
                        if (!__instance.gameObject.activeSelf)
                        {
                            __instance.gameObject.SetActive(true);
                            __instance.GetComponent<RectTransform>().SetPosition(Vector3.up + Grid.CellToPos(bat.PowerCell, 0.5f, 0f, 0f) + (Vector3.up * 0.80f));
                            __instance.SetContent(bat);
                        }
                    }
                }
            }
        }

        //A BuildingCellVisualizer is only associated with a single building. If this building is a Placeable that is not placed, set the OverlayMode to None to prevent icons from being drawn for it.
        [HarmonyPatch(typeof(BuildingCellVisualizer))]
        [HarmonyPatch("DrawIcons")]
        public static class BCV_DrawUtlityIcons
        {
            public static void Prefix(BuildingCellVisualizer __instance, ref HashedString mode)
            {
                Placeable placeable = __instance.GetComponent<Placeable>();
                if (placeable != null && !placeable.IsPlaced)
                    mode = OverlayModes.None.ID;
            }
        }

        //Implement behavior similar to when the BuildTool is previewing the placement of a building.
        //This causes the the game to redraw ports every tick so that the ports of a preview will follow the mouse.
        //No extra behavior is needed to remove the previewVisualizer
        [HarmonyPatch(typeof(Game))]
        [HarmonyPatch("OnAddBuildingCellVisualizer")]
        public static class Game_OnAddBCV
        {
            public static void Prefix(Game __instance, BuildingCellVisualizer building_cell_visualizer)
            {
                if (PlayerController.Instance != null)
                {
                    PlaceTool placeTool = PlayerController.Instance.ActiveTool as PlaceTool;
                    if (placeTool != null && placeTool.visualizer == building_cell_visualizer.gameObject)
                    {
                        AccessTools.Field(typeof(Game), "previewVisualizer").SetValue(__instance, building_cell_visualizer);
                    }
                }
            }
        }

    }
}
