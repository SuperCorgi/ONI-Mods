using Database;
using Harmony;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using System.Linq;
using STRINGS;
namespace PressurizedPipes
{
    internal static class HarmonyPatches
    {
        //MaxMass is used by:
        //ConduitFlow.UpdateConduit
        //ConduitFlow.AddElement
        //ConduitFlow.OnDeserialized
        //ConduitFlow.IsConduitFull
        //ConduitFlow.FreezeConduitContents
        //ConduitFlow.MeltConduitContents
        private static readonly FieldInfo maxMass = AccessTools.Field(typeof(ConduitFlow), "MaxMass");

        //Add the new buildings to the database and building plan screen
        [HarmonyPatch(typeof(GeneratedBuildings))]
        [HarmonyPatch("LoadGeneratedBuildings")]
        public static class HighPressure_GeneratedBuildings_LoadGeneratedBuildings
        {
            public static void Prefix()
            {
                //PRESSURIZED GAS PIPE
                string prefix = "STRINGS.BUILDINGS.PREFABS." + PressurizedGasConduitConfig.ID.ToUpper();
                Strings.Add(prefix + ".NAME", "Pressurized Gas Pipe");
                Strings.Add(prefix + ".DESC", "Able to contain significantly more gas than standard gas pipes.");
                Strings.Add(prefix + ".EFFECT", $"Carries {UI.FormatAsLink("Gas", "ELEMENTS_GAS")} between {UI.FormatAsLink("Outputs", "GASPIPING")} and {UI.FormatAsLink("Intakes", "GASPIPING")}.\n\nCan carry a maximum of 3KG of gasses. Can also connect to regular gas pipes, but may damage pipes with a lower maximum capacity if the flow is too strong, especially to bridges.");
                ModUtil.AddBuildingToPlanScreen("HVAC", PressurizedGasConduitConfig.ID);
                //PRESSURIZED GAS BRIDGE
                prefix = "STRINGS.BUILDINGS.PREFABS." + PressurizedGasConduitBridgeConfig.ID.ToUpper();
                Strings.Add(prefix + ".NAME", "Pressurized Gas Bridge");
                Strings.Add(prefix + ".DESC", "Is able to bridge upwards of 3KG of gas without damaging itself.");
                Strings.Add(prefix + ".EFFECT", $"A gas bridge built with steel and plastics.\n\nCan carry a maximum of 3KG of {UI.FormatAsLink("Gas", "ELEMENTS_LIQUID")}. May damage connected output pipes if too much flow is in the input pipe.");
                ModUtil.AddBuildingToPlanScreen("HVAC", PressurizedGasConduitBridgeConfig.ID);
                //PRESSURIZED LIQUID PIPE
                prefix = "STRINGS.BUILDINGS.PREFABS." + PressurizedLiquidConduitConfig.ID.ToUpper();
                Strings.Add(prefix + ".NAME", "Pressurized Liquid Pipe");
                Strings.Add(prefix + ".DESC", "Able to contain significantly more liquid than standard liquid pipes");
                Strings.Add(prefix + ".EFFECT", $"Carries {UI.FormatAsLink("Liquid", "ELEMENTS_LIQUID")} between {UI.FormatAsLink("Outputs", "LIQUIDPIPING")} and {UI.FormatAsLink("Intakes", "LIQUIDPIPING")}.\n\nCan carry a maximum of 30KG of {UI.FormatAsLink("Liquid", "ELEMENTS_LIQUID")}. Can also connect to regular liquid pipes, but may damage pipes with a lower maximum capacity if the flow is too strong, especially to bridges.");
                ModUtil.AddBuildingToPlanScreen("Plumbing", PressurizedLiquidConduitConfig.ID);
                //PRESSURIZED LIQUID BRIDGE
                prefix = "STRINGS.BUILDINGS.PREFABS." + PressurizedLiquidConduitBridgeConfig.ID.ToUpper();
                Strings.Add(prefix + ".NAME", "Pressurized Liquid Bridge");
                Strings.Add(prefix + ".DESC", "Is able to bridge upwards of 3KG of gas without damaging itself.");
                Strings.Add(prefix + ".EFFECT", $"A liquid bridge built with steel and plastics.\n\nCan carry a maximum of 30KG of {UI.FormatAsLink("Liquid", "ELEMENTS_LIQUID")}. May damage connected output pipes if too much flow is in the input pipe.");
                ModUtil.AddBuildingToPlanScreen("Plumbing", PressurizedLiquidConduitBridgeConfig.ID);
                //PRESSURIZED GAS VALVE
                prefix = "STRINGS.BUILDINGS.PREFABS." + PressurizedGasValveConfig.ID.ToUpper();
                Strings.Add(prefix + ".NAME", "Advanced Gas Valve");
                Strings.Add(prefix + ".DESC", "A gas valve that can fully support the flow of a Pressurized Gas Pipe, with a built-in Buffer mode.");
                Strings.Add(prefix + ".EFFECT", "Can limit flow by up to 3KG. Will not be damaged by strong flows and will not damage connected outputs.\n\nCan also be toggled to \"Buffer\" mode, which will invert the valve's behavior. While in Buffer mode, the valve will only output a packet of at least the size specified by the flow limit.\n\n CATUION: In Buffer mode, the valve will only store up to one gas type. A mixed pipe environment will likely damage the valve.");
                ModUtil.AddBuildingToPlanScreen("HVAC", PressurizedGasValveConfig.ID);
                //PRESSURIZED LIQUID VALVE
                prefix = "STRINGS.BUILDINGS.PREFABS." + PressurizedLiquidValveConfig.ID.ToUpper();
                Strings.Add(prefix + ".NAME", "Advanced Liquid Valve");
                Strings.Add(prefix + ".DESC", "A liquid valve that can fully support the flow of a Pressurized Liquid Pipe, with a built-in Buffer mode.");
                Strings.Add(prefix + ".EFFECT", "Can limit flow by up to 30KG. Will not be damaged by strong flows and will not damage connected outputs.\n\nCan also be toggled to \"Buffer\" mode, which will invert the valve's behavior. While in Buffer mode, the valve will only output a packet of at least the size specified by the flow limit.\n\n CATUION: In Buffer mode, the valve will only store up to one liquid type. A mixed pipe environment will likely damage the valve.");
                ModUtil.AddBuildingToPlanScreen("Plumbing", PressurizedLiquidValveConfig.ID);
                //PRESSURIZED LIQUID RESEVOIR
                //prefix = "STRINGS.BUILDINGS.PREFABS." + HighPressureLiquidReservoirConfig.ID.ToUpper();
                //Strings.Add(prefix + ".NAME", "Pressurized Liquid Resevoir");
                //Strings.Add(prefix + ".DESC", "A pressurized liquid resevoir");
                //Strings.Add(prefix + ".EFFECT", "A pressurized liquid resevoir");
                string statusPrefix = "STRINGS.BUILDING.STATUSITEMS.TOGGLINGSTATUS.";
                Strings.Add(statusPrefix + "NAME", "Mode Switch Errand");
                Strings.Add(statusPrefix + "TOOLTIP", "Mode will be switched once a Duplicate is available.");
                statusPrefix = "STRINGS.BUILDING.STATUSITEMS.BUFFERMODESTATUS.";
                Strings.Add(statusPrefix + "NAME", "Buffer Mode");
                Strings.Add(statusPrefix + "TOOLTIP", "This valve is currently in buffer mode");
            }
        }

        //[HarmonyPatch(typeof(LiquidReservoirConfig), "ConfigureBuildingTemplate")]
        //public static class LiquidResevoirConfig_ConfigureBuildingTemplate
        //{
        //    public static void Postfix(GameObject go)
        //    {
        //        MyUpgradable upgrade = go.AddOrGet<MyUpgradable>();
        //    }
        //}

        //Place the new buildings under appropriate tech groupings (research trees)
        [HarmonyPatch(typeof(Db))]
        [HarmonyPatch("Initialize")]
        public static class HighPressure_Db_Initialize
        {
            public static void Prefix()
            {
                //List<string> list = new List<string>(Techs.TECH_GROUPING["ImprovedOxygen"]);
                //list.Add(PressurizedGasConduitConfig.ID);
                //Techs.TECH_GROUPING["ImprovedOxygen"] = list.ToArray();
                List<string> hvac = new List<string>(Techs.TECH_GROUPING["HVAC"]) { PressurizedGasConduitConfig.ID, PressurizedGasConduitBridgeConfig.ID };
                Techs.TECH_GROUPING["HVAC"] = hvac.ToArray();

                List<string> imprLiqPiping = new List<string>(Techs.TECH_GROUPING["ImprovedLiquidPiping"]) { PressurizedLiquidValveConfig.ID };
                Techs.TECH_GROUPING["ImprovedLiquidPiping"] = imprLiqPiping.ToArray();

                List<string> igp = new List<string>(Techs.TECH_GROUPING["ImprovedGasPiping"]) { PressurizedGasValveConfig.ID };
                Techs.TECH_GROUPING["ImprovedGasPiping"] = igp.ToArray();

                List<string> lt = new List<string>(Techs.TECH_GROUPING["LiquidTemperature"]) { PressurizedLiquidConduitConfig.ID, PressurizedLiquidConduitBridgeConfig.ID };
                (Techs.TECH_GROUPING["LiquidTemperature"]) = lt.ToArray();
            }
        }

        //Modify Gas and Liquid Shutoff to support the triple capacity of the pressurized pipes.
        //Modify Advanced Liquid and Gas Valve, tripling their maximum flow rate as well as the flow ranges of their animations
        //The increased maximum flow is set here to support mod compatibility. As long as this mod is loaded after other mods, this mod will utilize any modifications to the base max flow (i.e. Bigger Capacity by newman55)
        [HarmonyPatch(typeof(ValveBase))]
        [HarmonyPatch("OnSpawn")]
        public static class ValveBase_OnSpawn
        {
            public static void Postfix(ValveBase __instance)
            {
                if (__instance is OperationalValve)
                {
                    __instance.maxFlow *= 3f;
                }
                //else if (__instance.GetComponent<TogglableValve>())
                //{
                //    __instance.maxFlow = 3f;
                //    __instance.CurrentFlow *= 3f;
                //    ValveBase.AnimRangeInfo[] animRanges = __instance.animFlowRanges;
                //    for (int i = 0; i < animRanges.Length; i++)
                //    {
                //        //AnimRangeInfo is a struct, meaning it is CALL BY VALUE, not CALL BY REFERENCE. If the range variable is changed, it does not inherently change the range stored in the array.
                //        ValveBase.AnimRangeInfo range = animRanges[i];
                //        range.minFlow *= __instance.maxFlow;
                //        animRanges[i] = range;
                //    }
                //}
            }
        }

        //Add the Pressurized component to every ConduitBridge during Prefab Initialization
        [HarmonyPatch(typeof(ConduitBridge), "OnPrefabInit")]
        internal static class Patch_ConduitBridge_OnPrefabInit
        {
            internal static void Postfix(ConduitBridge __instance)
            {
                __instance.gameObject.AddOrGet<Pressurized>();
            }
        }

        //Add the Pressurized component to every Conduit during Prefab Initialization
        [HarmonyPatch(typeof(Conduit), "OnPrefabInit")]
        internal static class Patch_Conduit_OnPrefabInit
        {
            internal static void Postfix(Conduit __instance)
            {
                __instance.gameObject.AddOrGet<Pressurized>();
            }
        }

        //Cannot trigger building damage inside of the conduit updates (where the need to damage is discovered). Trigger all damage at the end of each tick safely.
        [HarmonyPatch(typeof(Game), "Update")]
        internal static class Patch_Game_Update
        {
            internal static void Postfix()
            {
                List<Integration.QueueDamage> damages = Integration.queueDamages;
                if (damages.Count > 0)
                {
                    foreach (Integration.QueueDamage info in damages)
                        Integration.DoPressureDamage(info.Receiver);
                    damages.Clear();
                }
            }
        }

        [HarmonyPatch(typeof(Game), "OnLoadLevel")]
        internal static class Patch_Game_OnLoad
        {
            internal static void Postfix()
            {
                Integration.ClearStaticInfo();
            }
        }

        //To change the color of how our pressurized pipes are displayed in their respective overlay
        [HarmonyPatch(typeof(OverlayModes.ConduitMode), "Update")]
        internal static class Patch_OvererlayModesConduitMode_Update
        {
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
            {
                MethodBase patch = AccessTools.Method(typeof(Patch_OvererlayModesConduitMode_Update), nameof(PatchThermalColor));
                //SaveLoadRoot layerTarget;
                int layerTargetIdx = 12;
                //Color32 color;
                int tintColourIdx = 14;
                bool foundVar = false;
                LocalVariableInfo layerTargetInfo = original.GetMethodBody().LocalVariables.FirstOrDefault(x => x.LocalIndex == layerTargetIdx);
                foundVar = layerTargetInfo != default(LocalVariableInfo);
                if (!foundVar)
                    Debug.LogError($"[Pressurized] OverlayModes.ConduitMode.Update() Transpiler -> Local variable signatures did not match expected signatures");

                foreach (CodeInstruction code in instructions)
                {
                    if (foundVar && code.opcode == OpCodes.Stloc_S && (code.operand as LocalVariableInfo)?.LocalIndex == tintColourIdx)
                    {
                        //PatchThermalColor(color, layerTarget)
                        yield return new CodeInstruction(OpCodes.Ldloc_S, layerTargetIdx);
                        yield return new CodeInstruction(OpCodes.Call, patch);
                    }
                    yield return code;
                }
            }
            private static HashSet<int> cells = new HashSet<int>();
            private static HashSet<int> cells2 = new HashSet<int>();
            //Change the overlay tint for the pipe if it is a pressurized pipe.
            private static Color32 PatchThermalColor(Color32 original, SaveLoadRoot layerTarget)
            {
                Pressurized pressurized = layerTarget.GetComponent<Pressurized>();
                if (pressurized != null && pressurized.Info != null && !pressurized.Info.IsDefault)
                    return pressurized.Info.OverlayTint;
                else
                    return original;
            }
        }

        //Modify MaxMass if needed for pressurized pipes when adding elements to a pipe
        [HarmonyPatch(typeof(ConduitFlow), "AddElement")]
        internal static class Patch_ConduitFlow_AddElement
        {

            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                CodeInstruction getCellInstruction = new CodeInstruction(OpCodes.Ldarg_1); //int cell_idx : The first argument of the method being called (Ldarg_0 is the instance (this) reference)
                foreach (CodeInstruction code in instructions)
                {
                    foreach (CodeInstruction result in Integration.AddIntegrationIfNeeded(code, getCellInstruction))
                    {
                        yield return result;
                    }
                }

            }
        }

        //Modify MaxMass if needed for pressurized pipes when update conduits. Also include overpressure integration
        [HarmonyPatch(typeof(ConduitFlow), "UpdateConduit")]
        internal static class Patch_ConduitFlow_UpdateConduit
        {
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                //variable: int cell2;
                //This variable is used for the patch to determine the cell of the conduit being updated. The cell is then used in determining what its MaxMass (max capacity) should be
                CodeInstruction getCellInstruction = new CodeInstruction(OpCodes.Ldloc_S, 13);
                foreach (CodeInstruction code in instructions)
                {
                    foreach (CodeInstruction result in Integration.AddIntegrationIfNeeded(code, getCellInstruction, true))
                    {
                        yield return result;
                    }
                }

            }
        }

        //Modify MaxMass if needed for pressurized pipes when determining if the conduit is full.
        [HarmonyPatch(typeof(ConduitFlow), "IsConduitFull")]
        internal static class Patch_ConduitFlow_IsConduitFull
        {
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                CodeInstruction getCellInstruction = new CodeInstruction(OpCodes.Ldarg_1); //int cell_idx : The first argument of the method being called (Ldarg_0 is the instance (this) reference)
                foreach (CodeInstruction code in instructions)
                {
                    foreach (CodeInstruction result in Integration.AddIntegrationIfNeeded(code, getCellInstruction))
                    {
                        yield return result;
                    }
                }
            }
        }

        //When Deserializing the contents inside of Conduits, the method will normally prevent the deserialized data from being higher than the built-in ConduitFlow MaxMass.
        //Instead, replace the max mass with infinity so the serialized mass will always be used.
        //Must be done this way because OnDeserialized is called before the Conduits are spawned, so no information is available as to what the max mass is supposed to be
        [HarmonyPatch(typeof(ConduitFlow), "OnDeserialized")]
        internal static class Patch_ConduitFlow_OnDeserialized
        {
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                MethodInfo patch = AccessTools.Method(typeof(Patch_ConduitFlow_OnDeserialized), "ReplaceMaxMass");
                foreach (CodeInstruction original in instructions)
                {
                    if (original.opcode == OpCodes.Ldfld && (original.operand as FieldInfo) == maxMass)
                    {
                        yield return original;
                        yield return new CodeInstruction(OpCodes.Call, patch);
                    }
                    else
                        yield return original;
                }
            }

            internal static float ReplaceMaxMass(float original)
            {
                return float.PositiveInfinity;
            }
        }

        //Prevent the game from marking our pipes as radiant or insulated. Otherwise, overlay tints will not properly appear.
        [HarmonyPatch(typeof(ConduitFlowVisualizer), "AddThermalConductivity")]
        internal static class Patch_ConduitFlowVisualizer_AddThermalConductivity
        {
            private static readonly FieldInfo conduitType = AccessTools.Field(typeof(ConduitFlow), "conduitType");

            internal static void Prefix(ConduitFlowVisualizer __instance, int cell, ref float conductivity, ConduitFlow ___flowManager)
            {
                Pressurized pressure = Integration.GetPressurizedAt(cell, (ConduitType)conduitType.GetValue(___flowManager));
                if (!Pressurized.IsDefault(pressure))
                    conductivity = 1f;
            }
        }

        //Prevent the game from attempting to remove our pipes from their list of radiant/insulated pipes, since our pipes will not be in those lists in the first place
        [HarmonyPatch(typeof(ConduitFlowVisualizer), "RemoveThermalConductivity")]
        internal static class Patch_ConduitFlowVisualizer_RemoveThermalConductivity
        {
            private static readonly FieldInfo conduitType = AccessTools.Field(typeof(ConduitFlow), "conduitType");

            internal static void Prefix(ConduitFlowVisualizer __instance, int cell, ref float conductivity, ConduitFlow ___flowManager)
            {
                Pressurized pressure = Integration.GetPressurizedAt(cell, (ConduitType)conduitType.GetValue(___flowManager));
                if (!Pressurized.IsDefault(pressure))
                    conductivity = 1f;
            }
        }

        //Specifically changes the color of the flowing contents that appear in conduits without an overlay
        [HarmonyPatch(typeof(ConduitFlowVisualizer), "GetCellTintColour")]
        internal static class Patch_ConduitFlowVisualizer_GetCellTintColour
        {
            private static readonly FieldInfo conduitType = AccessTools.Field(typeof(ConduitFlow), "conduitType");

            internal static void Postfix(ConduitFlowVisualizer __instance, int cell, ConduitFlow ___flowManager, bool ___showContents, ref Color32 __result)
            {
                Pressurized pressure = Integration.GetPressurizedAt(cell, (ConduitType)conduitType.GetValue(___flowManager));
                if (!Pressurized.IsDefault(pressure))
                    __result = ___showContents ? pressure.Info.FlowOverlayTint : pressure.Info.FlowTint;
            }
        }

        //Integrate overpressure damage when a Gas or Liquid Shutoff has too much pressure in the receiving pipe for the output pipe to handle.
        //The shutoff valves have a built-in limit, but has been overriden with a different harmony patch
        //Patch in through ValveBase and check if the instance is of type OperationalValve (the name in the code for shutoffs)
        [HarmonyPatch(typeof(ValveBase), "ConduitUpdate")]
        internal static class Patch_ValveBase_ConduitUpdate
        {
            private static FieldInfo valveBaseOutputCell;

            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                MethodInfo conduitGetContents = AccessTools.Method(typeof(ConduitFlow.Conduit), "GetContents");
                MethodInfo overPressurePatch = AccessTools.Method(typeof(Patch_ValveBase_ConduitUpdate), nameof(OperationalValveOverPressure));
                valveBaseOutputCell = AccessTools.Field(typeof(ValveBase), "outputCell");
                foreach (CodeInstruction original in instructions)
                {
                    //Integrate patch when the following line is called:
                    //  ConduitFlow.ConduitContents contents = conduit.GetContents(flowManager)
                    //Utilize the contents value to determine if overpressure damage is necessary
                    if (original.opcode == OpCodes.Call && original.operand as MethodInfo == conduitGetContents)
                    {
                        yield return original; //ConduitFlow.ConduitContents contents
                        yield return new CodeInstruction(OpCodes.Ldarg_0); //this (ValveBase)
                        yield return new CodeInstruction(OpCodes.Ldloc_0); //ConduitFlow flowManager
                        yield return new CodeInstruction(OpCodes.Call, overPressurePatch);
                    }
                    else
                        yield return original;
                }
            }

            private static ConduitFlow.ConduitContents OperationalValveOverPressure(ConduitFlow.ConduitContents contents, ValveBase valveBase, ConduitFlow flowManager)
            {
                OperationalValve op;
                if (op = valveBase as OperationalValve)
                {
                    if (op.CurrentFlow > 0f)
                    {
                        int outputCell = (int)valveBaseOutputCell.GetValue(valveBase);
                        GameObject outputObject;
                        float outputCapacity = Integration.GetMaxCapacityWithObject(outputCell, valveBase.conduitType, out outputObject);
                        float inputMass = contents.mass;
                        //If there is greater than 200% of the outputs capacity inside the shutoff valves input pipe, deal overpressure damage 33% of the time.
                        if (inputMass > (outputCapacity * 2) && UnityEngine.Random.Range(0f, 1f) < 0.33f)
                            Integration.DoPressureDamage(outputObject);
                    }
                }
                //since this patch consumed the contents variable on the stack, return the contents back to prevent issues with the next code statement in IL
                return contents;
            }

        }

        //Integrate a max flow rate specifically for ConduitBridges (i.e. Gas Bridge)
        //Normally, a Gas Bridge can move 3KG of gas from one pressurized pipe to another pressurized pipe, since inputs and outputs for buildings have no built in limiter to their flow rate.
        //For ConduitBridges, limit standard bridges to the same standard max flow rate as their respective conduits (1KG for gas bridge and 10KG for liquid bridge).
        [HarmonyPatch(typeof(ConduitBridge), "ConduitUpdate")]
        internal static class Patch_ConduitBridge_ConduitUpdate
        {
            private static FieldInfo bridgeOutputCell;
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                MethodInfo flowManagerGetContents = AccessTools.Method(typeof(ConduitFlow), "GetContents");
                MethodInfo setMaxFlowPatch = AccessTools.Method(typeof(Patch_ConduitBridge_ConduitUpdate), nameof(SetMaxFlow));
                bridgeOutputCell = AccessTools.Field(typeof(ConduitBridge), "outputCell");
                foreach (CodeInstruction original in instructions)
                {
                    if (original.opcode == OpCodes.Callvirt && original.operand as MethodInfo == flowManagerGetContents)
                    {
                        yield return original; //flowManager.GetContents(inputCell)
                        yield return new CodeInstruction(OpCodes.Ldarg_0); //this
                        yield return new CodeInstruction(OpCodes.Ldloc_0); //ConduitFlow flowManager
                        yield return new CodeInstruction(OpCodes.Call, setMaxFlowPatch); //SetMaxFlow(flowManager.GetContents
                    }
                    else
                        yield return original;
                }
            }

            private static ConduitFlow.ConduitContents SetMaxFlow(ConduitFlow.ConduitContents contents, ConduitBridge bridge, ConduitFlow manager)
            {
                //If the bridge is broken, prevent the bridge from operating by limiting what it sees.
                if (bridge.GetComponent<BuildingHP>().HitPoints == 0)
                {
                    //does not actually remove mass from the conduit, just causes the bridge to assume there is no mass available to move.
                    contents.RemoveMass(contents.mass);
                    return contents;
                }

                GameObject outputObject;
                int outputCell = (int)bridgeOutputCell.GetValue(bridge);
                float targetCapacity = Integration.GetMaxCapacityWithObject(outputCell, bridge.type, out outputObject ,false);
                if (outputObject == null)
                    return contents;
                float capacity = Pressurized.GetMaxCapacity(bridge.GetComponent<Pressurized>());

                //If the ConduitBridge is not supposed to support the amount of fluid currently in the contents, only make the bridge's intended max visible
                //Also immediately deal damage if the current contents are higher than 110% of the intended max (110% is set because at 100%, a system with no pressurized pipes would seem to randomly deal damage as if the contents
                //  were barely over 100%
                if (contents.mass > capacity)
                {
                    if (contents.mass > capacity * 1.1)
                        Integration.DoPressureDamage(bridge.gameObject);

                    float initial = contents.mass;
                    float removed = contents.RemoveMass(initial - capacity);
                    float ratio = removed / initial;
                    contents.diseaseCount = (int)((float)contents.diseaseCount * ratio);
                }


                if (contents.mass > targetCapacity * 2 && UnityEngine.Random.Range(0f, 1f) < 0.33f)
                {
                    Integration.DoPressureDamage(outputObject);
                }

                return contents;
            }
        }
    }
}
