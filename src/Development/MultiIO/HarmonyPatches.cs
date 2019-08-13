using System;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using UnityEngine;
using System.Reflection;
using STRINGS;
using System.Diagnostics;

namespace MultiIO
{
    internal class HarmonyPatches
    {
        //Allow MultiIO components to reserve locations for their conduit connections
        [HarmonyPatch(typeof(BuildingDef))]
        [HarmonyPatch("MarkArea")]
        public static class PipedRustDeoxidizer_BuildingDef_MarkArea
        {
            public static void Postfix(int cell, Orientation orientation, GameObject go, BuildingDef __instance)
            {
                MultiOutput multiOut = __instance.BuildingComplete.GetComponent<MultiOutput>();
                MultiInput multiIn = __instance.BuildingComplete.GetComponent<MultiInput>();
                multiOut?.MarkAreas(cell, orientation, go, __instance.MarkOverlappingPorts);
                multiIn?.MarkAreas(cell, orientation, go, __instance.MarkOverlappingPorts);
            }
        }

        //Allow MultiIO components to remove their reservations for conduit connections
        [HarmonyPatch(typeof(BuildingDef))]
        [HarmonyPatch("UnmarkArea")]
        public static class PipedRustDeoxidizer_BuildingDef_UnmarkArea
        {
            public static void Postfix(int cell, Orientation orientation, GameObject go, BuildingDef __instance)
            {
                MultiOutput multiOut = __instance.BuildingComplete.GetComponent<MultiOutput>();
                MultiInput multiIn = __instance.BuildingComplete.GetComponent<MultiInput>();
                multiOut?.UnmarkAreas(cell, orientation, go, __instance.MarkOverlappingPorts);
                multiIn?.UnmarkAreas(cell, orientation, go, __instance.MarkOverlappingPorts);
            }
        }
        //TODO: Patch to prevent a building with a MultiIO component being placed where its ports would overlap with another building's ports
        [HarmonyPatch(typeof(BuildingDef), "AreConduitPortsInValidPositions")]
        public static class PipedRustDeoxidizer_BuildingDef_AreConduitPortsInValidPositions
        {
            public static void Postfix(ref bool __result, BuildingDef __instance, GameObject source_go, int cell, Orientation orientation)
            {
                if (!__result || source_go == null)
                    return;
                List<ConduitIO> portList = MultiIOExtensions.GetAllPortsFromObject(source_go);
                if (portList.Count == 0)
                    return;

                foreach (ConduitIO port in portList)
                {

                }
            }
        }
        //Patch the code that draws icons for a building's input and output ports to include drawing MultiIO ports.
        [HarmonyPatch(typeof(BuildingCellVisualizer), "DrawIcons")]
        public static class PipedRustDeoxidizer_BuildingCellVisualizer_DrawIcons
        {
            public static void Postfix(HashedString mode, BuildingCellVisualizer __instance, Building ___building, BuildingCellVisualizerResources ___resources)
            {
                List<ConduitIO> ports = MultiIOExtensions.GetAllPortsFromObject(___building.gameObject);
                Sprite outputIcon = ___resources.gasOutputIcon;
                Sprite inputIcon = ___resources.gasInputIcon;
                if (OverlayModes.GasConduits.ID == mode)
                {
                    foreach (ConduitIO port in ports)
                    {
                        if (port.ConduitType == ConduitType.Gas)
                        {
                            CellOffset offset = port.CellOffset;
                            int cell = Grid.OffsetCell(___building.GetCell(), ___building.GetRotatedOffset(offset));
                            CallDrawUtilityIcon(__instance, cell, port is InputPort ? inputIcon : outputIcon, ref port.CellVisualizer, port.IconColor, Color.white);
                        }
                    }
                }
                else if (OverlayModes.LiquidConduits.ID == mode)
                {
                    foreach (ConduitIO port in ports)
                    {
                        if (port.ConduitType == ConduitType.Liquid)
                        {
                            CellOffset offset = port.CellOffset;
                            int cell = Grid.OffsetCell(___building.GetCell(), ___building.GetRotatedOffset(offset));
                            CallDrawUtilityIcon(__instance, cell, port is InputPort ? inputIcon : outputIcon, ref port.CellVisualizer, port.IconColor, Color.white);
                        }
                    }
                }
                else if (OverlayModes.SolidConveyor.ID == mode)
                {
                    foreach (ConduitIO port in ports)
                    {
                        if (port.ConduitType == ConduitType.Solid)
                        {
                            CellOffset offset = port.CellOffset;
                            int cell = Grid.OffsetCell(___building.GetCell(), ___building.GetRotatedOffset(offset));
                            CallDrawUtilityIcon(__instance, cell, port is InputPort ? inputIcon : outputIcon, ref port.CellVisualizer, port.IconColor, Color.white);
                        }
                    }
                }

            }


            private static MethodInfo _DrawUtilityIconMethod = null;
            //This is normally a private method that actually draws the Utility Icon. Uses System.Reflection to pull a reference.
            private static MethodInfo DrawUtilityIconMethod
            {
                get
                {
                    //Save the result from this so the method only has to be found once.
                    if (_DrawUtilityIconMethod == null)
                    {
                        //Not using Harmony AccessTools/Traverse because it has issues finding this method. Likely because the third parameter has a ref.
                        //Currently not sure how to explicitly check for refs, so currently only checking for the number of parameters.
                        Type[] parameters = new Type[7] { typeof(int), typeof(Sprite), typeof(GameObject), typeof(Color), typeof(Color), typeof(float), typeof(bool) };
                        MethodInfo[] privateMethods = typeof(BuildingCellVisualizer).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
                        MethodInfo[] filteredMethods = privateMethods.Where(x => x.Name == "DrawUtilityIcon" && x.GetParameters().Length == parameters.Length).ToArray();
                        if (filteredMethods.Length != 1)
                            Debug.LogError($"[MultiIO] Error has occurred. Please send this bug report to the mod developer ->\nError occurred during DrawIcons Harmony patch. Reflection did not return one method with the specified parameter length.");
                        _DrawUtilityIconMethod = filteredMethods[0];
                    }
                    return _DrawUtilityIconMethod;
                }
            }
            private static void CallDrawUtilityIcon(BuildingCellVisualizer instance, int cell, Sprite icon, ref GameObject visualizer, Color tint, Color connectedColor)
            {
                object[] args = new object[7] { cell, icon, visualizer, tint, connectedColor, 1.5f, false };
                DrawUtilityIconMethod.Invoke(instance, args);
                //So that the visualizer ref carries over
                visualizer = (GameObject)args[2];
            }
        }

        [HarmonyPatch(typeof(BuildingCellVisualizer), "DisableIcons")]
        public static class PipedRustDeoxidizer_BuildingCellVisualizer_DisableIcons
        {
            public static void Postfix(BuildingCellVisualizer __instance)
            {
                List<ConduitIO> ports = MultiIOExtensions.GetAllPortsFromObject(__instance.gameObject);

                foreach (ConduitIO port in ports)
                {
                    GameObject visualizer = port.CellVisualizer;
                    if (visualizer != null)
                        visualizer.SetActive(false);
                }
            }
        }

        //When building conduits, they will cause the input/output ports they connect to pulse in size and play a sound.
        //This patch allows MultIO input/output ports to have the same behaviour.
        [HarmonyPatch(typeof(BaseUtilityBuildTool), "CheckForConnection")]
        public static class PipedRustDeoxidizer_BaseUtilityBuildTool_CheckForConnection
        {
            public static void Postfix(int cell, string defName, string soundName, bool fireEvents, ref bool __result)
            {
                //Only need to patch if the result was false and a pipe is being checked (liquid/gas)
                if (__result)
                    return;
                if (!defName.Contains("Conduit"))
                    return;
                Building building = Grid.Objects[cell, 1]?.GetComponent<Building>();
                if (building == null)
                    return;

                ConduitIO port = MultiIOExtensions.GetPortAt(building.gameObject, cell);
                if (port == null)
                    return;

                ConduitType type = port.ConduitType;
                string nameCheck = "";
                if (type == ConduitType.Gas)
                    nameCheck = "Gas";
                else if (type == ConduitType.Liquid)
                    nameCheck = "Liquid";
                else if (type == ConduitType.Solid)
                    nameCheck = "Solid";
                if (nameCheck == "" || !defName.Contains(nameCheck))
                    return;

                BuildingCellVisualizer bcv = building.GetComponent<BuildingCellVisualizer>();
                if (bcv != null)
                {
                    if (fireEvents)
                    {
                        bcv.ConnectedEvent(cell);
                        string sound = GlobalAssets.GetSound(soundName);
                        if (sound != null)
                            KMonoBehaviour.PlaySound(sound);
                    }
                    __result = true;
                }


            }
        }
        //Related to the BaseUtilityBuildTool.CheckForConnection() patch. This specifically causes the port icon to pulse in size when a conduit is attached.
        [HarmonyPatch(typeof(BuildingCellVisualizer), "ConnectedEvent")]
        public static class PipedRustDeoxidizer_BuildingCellVisualizer_ConnectedEvent
        {
            public static void Postfix(BuildingCellVisualizer __instance, int cell)
            {
                ConduitIO port = MultiIOExtensions.GetPortAt(__instance.gameObject, cell);

                if (port != null)
                {
                    GameObject visualizer = port.CellVisualizer;
                    SizePulse pulse = visualizer.AddComponent<SizePulse>();
                    pulse.speed = 20f;
                    pulse.multiplier = 0.75f;
                    pulse.updateWhenPaused = true;
                    pulse.onComplete = (System.Action)Delegate.Combine(pulse.onComplete, (System.Action)delegate { UnityEngine.Object.Destroy(pulse); });
                }
            }
        }
        //When the building is removed, this patch will remove MultiIO ports from showing up on their respective overlays.
        [HarmonyPatch(typeof(BuildingCellVisualizer), "OnCleanUp")]
        public static class PipedRustDeoxidizer_BuildingCellVisualizer_OnCleanUp
        {
            public static void Postfix(BuildingCellVisualizer __instance)
            {
                List<ConduitIO> ports = MultiIOExtensions.GetAllPortsFromObject(__instance.gameObject);
                foreach (ConduitIO outPort in ports)
                {
                    GameObject visualizer = outPort.CellVisualizer;
                    if (visualizer != null)
                        UnityEngine.Object.Destroy(visualizer);
                }
            }
        }
        //Adds a description of input/output requirements to the card that appears in the build menu for any building that uses a MultiIO component
        [HarmonyPatch(typeof(Building), "RequirementDescriptors")]
        public static class PipedRustDeoxidizer_Building_RequirementDescriptors
        {
            public static void Postfix(Building __instance, BuildingDef def, List<Descriptor> __result)
            {
                MultiOutput multiOut = __instance.GetComponent<MultiOutput>();
                if (multiOut != null)
                {
                    int gasOut = 0;
                    int liquidOut = 0;
                    int solidOut = 0;


                    foreach (OutputPort port in multiOut.PortList)
                    {
                        if (!port.RequiresConnection)
                            continue;
                        if (port.ConduitType == ConduitType.Gas)
                            gasOut++;
                        else if (port.ConduitType == ConduitType.Liquid)
                            liquidOut++;
                        else if (port.ConduitType == ConduitType.Solid)
                            solidOut++;
                    }

                    if (gasOut > 0)
                    {
                        Descriptor gasOutDesc = default(Descriptor);
                        string prefix = gasOut > 1 ? $"({gasOut}) " : "";
                        gasOutDesc.SetupDescriptor(prefix + UI.BUILDINGEFFECTS.REQUIRESGASOUTPUT, UI.BUILDINGEFFECTS.TOOLTIPS.REQUIRESGASOUTPUT, Descriptor.DescriptorType.Requirement);
                        __result.Add(gasOutDesc);
                    }
                    if (liquidOut > 0)
                    {
                        Descriptor liquitOutDesc = default(Descriptor);
                        string prefix = liquidOut > 1 ? $"({liquidOut}) " : "";
                        liquitOutDesc.SetupDescriptor(prefix + UI.BUILDINGEFFECTS.REQUIRESLIQUIDOUTPUT, UI.BUILDINGEFFECTS.TOOLTIPS.REQUIRESLIQUIDOUTPUT, Descriptor.DescriptorType.Requirement);
                        __result.Add(liquitOutDesc);
                    }
                    if (solidOut > 0)
                    {
                        Descriptor solidOutDesc = default(Descriptor);
                        string prefix = solidOut > 1 ? $"({solidOut}) " : "";
                        solidOutDesc.SetupDescriptor(prefix + UI.FormatAsLink("Solid Output Pipe", "SOLIDPIPING"), $"Must expel <style=\"KKeywork\">Solid</style> through a {BUILDINGS.PREFABS.SOLIDCONDUIT.NAME} system", Descriptor.DescriptorType.Requirement);
                        __result.Add(solidOutDesc);
                    }
                }
                MultiInput multiIn = __instance.GetComponent<MultiInput>();
                if(multiIn != null)
                {
                    int gasIn = 0;
                    int liquidIn = 0;
                    int solidIn = 0;


                    foreach (InputPort port in multiIn.PortList)
                    {
                        if (!port.RequiresConnection)
                            continue;
                        if (port.ConduitType == ConduitType.Gas)
                            gasIn++;
                        else if (port.ConduitType == ConduitType.Liquid)
                            liquidIn++;
                        else if (port.ConduitType == ConduitType.Solid)
                            solidIn++;
                    }

                    if (gasIn > 0)
                    {
                        Descriptor gasOutDesc = default(Descriptor);
                        string prefix = gasIn > 1 ? $"({gasIn}) " : "";
                        gasOutDesc.SetupDescriptor(prefix + UI.BUILDINGEFFECTS.REQUIRESGASINPUT, UI.BUILDINGEFFECTS.TOOLTIPS.REQUIRESGASINPUT, Descriptor.DescriptorType.Requirement);
                        __result.Add(gasOutDesc);
                    }
                    if (liquidIn > 0)
                    {
                        Descriptor liquitOutDesc = default(Descriptor);
                        string prefix = liquidIn > 1 ? $"({liquidIn}) " : "";
                        liquitOutDesc.SetupDescriptor(prefix + UI.BUILDINGEFFECTS.REQUIRESLIQUIDINPUT, UI.BUILDINGEFFECTS.TOOLTIPS.REQUIRESLIQUIDINPUT, Descriptor.DescriptorType.Requirement);
                        __result.Add(liquitOutDesc);
                    }
                    if (solidIn > 0)
                    {
                        Descriptor solidOutDesc = default(Descriptor);
                        string prefix = solidIn > 1 ? $"({solidIn}) " : "";
                        solidOutDesc.SetupDescriptor(prefix + UI.FormatAsLink("Solid Input Pipe", "SOLIDPIPING"), $"Must receive <style=\"KKeywork\">Solids</style> from a {BUILDINGS.PREFABS.SOLIDCONDUIT.NAME} system", Descriptor.DescriptorType.Requirement);
                        __result.Add(solidOutDesc);
                    }
                }
            }
        } //End Patch
    }
}
