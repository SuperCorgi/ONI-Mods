using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using UnityEngine;
using Database;
namespace HighPressurePipes
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
        private static readonly Color32 PressurizedColor = new Color32(250, 128, 114, 0);
        private static readonly Color32 PressurizedConduitColor = new Color32(66, 15, 12, 255);
        private static readonly Color32 PressurizedConduitKAnimTint = new Color32(255, 60, 60, 255);

        [HarmonyPatch(typeof(GeneratedBuildings))]
        [HarmonyPatch("LoadGeneratedBuildings")]
        public static class PipedElectrolyzer_GeneratedBuildings_LoadGeneratedBuildings
        {
            public static void Prefix()
            {
                string prefix = "STRINGS.BUILDINGS.PREFABS." + PressurizedGasCoduitConfig.ID.ToUpper();
                Strings.Add(prefix + ".NAME", "Pressurized Gas Pipe");
                Strings.Add(prefix + ".DESC", "Yep. Carries a lot of gas.");
                Strings.Add(prefix + ".EFFECT", "Carries a whole lot of gas.");
                ModUtil.AddBuildingToPlanScreen("Oxygen", PressurizedGasCoduitConfig.ID);
            }
        }

        [HarmonyPatch(typeof(Db))]
        [HarmonyPatch("Initialize")]
        public static class PipedElectrolyzer_Db_Initialize
        {
            public static void Prefix()
            {
                List<string> list = new List<string>(Techs.TECH_GROUPING["ImprovedOxygen"]);
                list.Add(PressurizedGasCoduitConfig.ID);
                Techs.TECH_GROUPING["ImprovedOxygen"] = list.ToArray();
            }
        }

        [HarmonyPatch(typeof(OverlayModes.Mode), "ResetDisplayValues", new Type[] { typeof(KBatchedAnimController)})]
        internal static class Patch_OverModesMode_ResetDisplayValues
        {
            internal static void Postfix(KBatchedAnimController controller)
            {
                Conduit conduit = controller.GetComponent<Conduit>();
                if(conduit != null)
                {
                    int cell = Grid.PosToCell(conduit);
                    int layer = IntegrationHelper.layers[(int)conduit.ConduitType];
                    if(IntegrationHelper.GetCapacityAt(cell, layer) != -1f)
                    {
                        controller.TintColour = PressurizedConduitKAnimTint;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Conduit), "OnSpawn")]
        internal static class Patch_Conduit_OnSpawn
        {
            internal static void Postfix(Conduit __instance)
            {
                ConduitType type = __instance.ConduitType;
                if (type != ConduitType.Gas && type != ConduitType.Liquid)
                    return;
                Building building = __instance.GetComponent<Building>();
                int layer = IntegrationHelper.layers[(int)type];
                if (building.Def.PrefabID.StartsWith("Pressurized"))
                {
                    float capacity = type == ConduitType.Gas ? 3f : 30f;
                    IntegrationHelper.MarkConduitCapacity(__instance.Cell, layer, capacity);
                    KAnimControllerBase kAnim = __instance.GetComponent<KAnimControllerBase>();
                    if (kAnim != null)
                    {
                        kAnim.TintColour = PressurizedConduitKAnimTint;
                    }
                    else
                        Debug.LogWarning($"[Pressurized] Conduit.OnSpawn() KAnimControllerBase component was null!");
                }
                else
                {
                    IntegrationHelper.MarkConduitCapacity(__instance.Cell, layer, -1f);
                }
            }
        }

        [HarmonyPatch(typeof(Game), "Update")]
        internal static class Patch_Game_Update
        {
                internal static void Postfix()
                {
                    List<IntegrationHelper.QueueDamage> damages = IntegrationHelper.queueDamages;
                    if (damages.Count > 0)
                    {
                        //Debug.Log($"[Pressurized] We've got a lot of damage to do!");
                        foreach (IntegrationHelper.QueueDamage info in damages)
                        {
                            info.Receiver.Trigger((int)GameHashes.DoBuildingDamage, info.Damage);
                        }
                        damages.Clear();
                    }
                }
        }

        [HarmonyPatch(typeof(Conduit), "OnCleanUp")]
        internal static class Patch_Conduit_OnCleanup
        {
            internal static void Postfix(Conduit __instance)
            {
                ConduitType type = __instance.ConduitType;
                if (type != ConduitType.Gas && type != ConduitType.Liquid)
                    return;
                IntegrationHelper.UnmarkConduitCapacity(__instance.Cell, IntegrationHelper.layers[(int)__instance.ConduitType]);
            }
        }


        [HarmonyPatch(typeof(OverlayModes.ConduitMode), "Update")]
        internal static class Patch_OvererlayModesConduitMode_Update
        {
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
            {
                //Debug.Log($"[Pressurized] Beginning Transpiler for OverModes.ConduitMode.Update()");
                MethodBase patch = AccessTools.Method(typeof(Patch_OvererlayModesConduitMode_Update), nameof(PatchThermalColor));
                int layerTargetIdx = 12;
                int tintColourIdx = 15;
                bool foundVar = false;
                bool foundColour = false;
                bool foundLayer = false;
                LocalVariableInfo layerTargetInfo = null;
                foreach (LocalVariableInfo var in original.GetMethodBody().LocalVariables)
                {
                    Debug.Log(var);
                    if (var.LocalIndex == tintColourIdx)
                    {
                        if (var.LocalType != typeof(Color32))
                            Debug.LogError($"[Pressurized] OverlayModes.ConduitMode.Update() Transpiler -> Local variable signatures did not match expected signatures");
                        else
                            foundColour = true;
                    }
                    else if (var.LocalIndex == layerTargetIdx)
                    {
                        if (var.LocalType != typeof(SaveLoadRoot))
                            Debug.LogError($"[Pressurized] OverlayModes.ConduitMode.Update() Transpiler -> Local variable signatures did not match expected signatures");
                        else
                        {
                            foundLayer = true;
                            layerTargetInfo = var;
                        }
                    }
                }

                foundVar = foundLayer && foundColour;
                foreach (CodeInstruction code in instructions)
                {
                    if (foundVar && code.opcode == OpCodes.Stloc_S && (code.operand as LocalVariableInfo).LocalIndex == tintColourIdx)
                    {
                        yield return new CodeInstruction(OpCodes.Ldloc_S, layerTargetIdx);
                        yield return new CodeInstruction(OpCodes.Call, patch);
                    }
                    yield return code;
                }
            }

            private static Color32 PatchThermalColor(Color32 original, SaveLoadRoot layerTarget)
            {
                Conduit conduit = layerTarget.GetComponent<Conduit>();
                if (conduit == null) //bridges will have no conduit but appear in here
                    return original;
                if (IntegrationHelper.GetCapacityAt(conduit.GetNetworkCell(), IntegrationHelper.layers[(int)conduit.ConduitType]) > 0f)
                    return PressurizedColor;
                return original;
            }
        }



        [HarmonyPatch(typeof(ConduitFlow), "AddElement")]
        internal static class Patch_ConduitFlow_AddElement
        {

            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                foreach (CodeInstruction code in instructions)
                {
                    foreach (CodeInstruction result in IntegrationHelper.AddIntegrationIfNeeded(code, new CodeInstruction(OpCodes.Ldarg_1)))
                    {
                        yield return result;
                    }
                }

            }
        }

        [HarmonyPatch(typeof(ConduitFlow), "UpdateConduit")]
        internal static class Patch_ConduitFlow_UpdateConduit
        {
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                foreach (CodeInstruction code in instructions)
                {
                    foreach (CodeInstruction result in IntegrationHelper.AddIntegrationIfNeeded(code, new CodeInstruction(OpCodes.Ldloc_S, 14), true))
                    {
                        yield return result;
                    }
                }

            }
        }

        [HarmonyPatch(typeof(ConduitFlow), "IsConduitFull")]
        internal static class Patch_ConduitFlow_IsConduitFull
        {

            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                foreach (CodeInstruction code in instructions)
                {
                    foreach (CodeInstruction result in IntegrationHelper.AddIntegrationIfNeeded(code, new CodeInstruction(OpCodes.Ldarg_1)))
                    {
                        yield return result;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(ConduitFlowVisualizer), "AddThermalConductivity")]
        internal static class Patch_ConduitFlowVisualizer_AddThermalConductivity
        {
            private static readonly FieldInfo conduitType = AccessTools.Field(typeof(ConduitFlow), "conduitType");

            internal static void Prefix(ConduitFlowVisualizer __instance, int cell, ref float conductivity, ConduitFlow ___flowManager)
            {
                
                int layer = 0;
                layer = IntegrationHelper.layers[(int)(ConduitType)conduitType.GetValue(___flowManager)];
                if(IntegrationHelper.GetCapacityAt(cell, layer) != -1f)
                {
                    conductivity = 1f;
                }
            }
        }

        [HarmonyPatch(typeof(ConduitFlowVisualizer), "RemoveThermalConductivity")]
        internal static class Patch_ConduitFlowVisualizer_RemoveThermalConductivity
        {
            private static readonly FieldInfo conduitType = AccessTools.Field(typeof(ConduitFlow), "conduitType");

            internal static void Prefix(ConduitFlowVisualizer __instance, int cell, ref float conductivity, ConduitFlow ___flowManager)
            {
                int layer = 0;
                layer = IntegrationHelper.layers[(int)(ConduitType)conduitType.GetValue(___flowManager)];
                if (IntegrationHelper.GetCapacityAt(cell, layer) != -1f)
                {
                    conductivity = 1f;
                }
            }
        }

        [HarmonyPatch(typeof(ConduitFlowVisualizer), "GetCellTintColour")]
        internal static class Patch_ConduitFlowVisualizer_GetCellTintColour
        {
            private static readonly FieldInfo conduitType = AccessTools.Field(typeof(ConduitFlow), "conduitType");

            internal static void Postfix(ConduitFlowVisualizer __instance, int cell, ConduitFlow ___flowManager, bool ___showContents, ref Color32 __result)
            {
                int layer = 0;
                layer = IntegrationHelper.layers[(int)(ConduitType)conduitType.GetValue(___flowManager)];
                if (IntegrationHelper.GetCapacityAt(cell, layer) != -1f)
                {
                    //Debug.Log($"[Pressurized] CellTintColour -> R: {__result.r} G: {__result.g} B: {__result.b} A: {__result.a}");                   
                    __result = ___showContents ? PressurizedColor : PressurizedConduitColor;
                }
            }
        }
    }
}
