using System;
using System.Collections.Generic;
using System.Linq;
using STRINGS;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using UnityEngine;

namespace HighPressurePipes
{
    public static class IntegrationHelper
    {
        public static class Tuning
        {
            public static readonly float PressurizedGasConduitCapacity = 3f;
            public static readonly float PressurizedLiquidConduitCapacity = 30f;
            public static readonly float IndustrialGasConduitCapacity = 10f;
            public static readonly float IndustrialLiquidConduitCapacity = 100f;
        }

        private static readonly FieldInfo maxMass = AccessTools.Field(typeof(ConduitFlow), "MaxMass");
        private static readonly FieldInfo conduitType = AccessTools.Field(typeof(ConduitFlow), "conduitType");
        private static readonly MethodBase patch = AccessTools.Method(typeof(IntegrationHelper), nameof(IntegratePressurized));
        private static readonly MethodBase overpressurePatch = AccessTools.Method(typeof(IntegrationHelper), nameof(IntegrateOverpressure));
        private static readonly MethodBase conduitContentsAddMass = AccessTools.Method(typeof(ConduitFlow.ConduitContents), "AddMass");

        internal static IEnumerable<CodeInstruction> AddIntegrationIfNeeded(CodeInstruction original, CodeInstruction toGetCell, bool isUpdateConduit = false)
        {
            if (original.opcode == OpCodes.Ldfld && original.operand == maxMass)
            {
                yield return original;
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldfld, conduitType);
                yield return toGetCell;
                yield return new CodeInstruction(OpCodes.Call, patch);

            }
            else if (isUpdateConduit && original.opcode == OpCodes.Call && original.operand == conduitContentsAddMass)
            {
                yield return original;
                yield return new CodeInstruction(OpCodes.Ldloc_2); //gridenode grid_node
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldfld, maxMass); //this.MaxMass
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldfld, conduitType); //this.conduitType
                yield return toGetCell; //int cell2
                yield return new CodeInstruction(OpCodes.Call, overpressurePatch);
            }
            else
                yield return original;
        }
        internal static int[] layers = { 0, 12, 16, 0 };


        private static void IntegrateOverpressure(ConduitFlow.GridNode sender, float standardMax, ConduitType conduitType, int cell)
        {
            float senderMass = sender.contents.mass;
            float receiverMax = IntegratePressurized(standardMax, conduitType, cell);
            if (senderMass >= receiverMax * 2f)
            {
                //Debug.Log($"That's a lotta pressure!");
                    GameObject receiver = Grid.Objects[cell, layers[(int)conduitType]];
                BuildingHP.DamageSourceInfo damage = new BuildingHP.DamageSourceInfo
                {
                    damage = 1,
                    source = BUILDINGS.DAMAGESOURCES.LIQUID_PRESSURE,
                    popString = STRINGS.UI.GAMEOBJECTEFFECTS.DAMAGE_POPS.LIQUID_PRESSURE
                };
                queueDamages.Add(new QueueDamage(damage, receiver));
                //receiver.Trigger((int)GameHashes.DoBuildingDamage, damage);
            }
        }
        internal class QueueDamage
        {
            public BuildingHP.DamageSourceInfo Damage;
            public GameObject Receiver;

            public QueueDamage(BuildingHP.DamageSourceInfo dmg, GameObject rcvr)
            {
                Damage = dmg;
                Receiver = rcvr;
            }
        }


        internal static List<QueueDamage> queueDamages = new List<QueueDamage>();

        private static float IntegratePressurized(float standardMax, ConduitType conduitType, int cell)
        {
            Vector2I pos = new Vector2I(cell, layers[(int)conduitType]);
            if (CapacityDict.ContainsKey(pos) && CapacityDict[pos] != -1)
                return CapacityDict[pos];
            return standardMax;
        }

        private static Dictionary<Vector2I, float> CapacityDict = new Dictionary<Vector2I, float>();
        internal static void MarkConduitCapacity(int cell, int layer, float capacity)
        {
            if (layer == 0)
                return;
            Vector2I pos = new Vector2I(cell, layer);
            if (CapacityDict.ContainsKey(pos))
            {
                Debug.LogError($"[PressurizedPipes] IntegrationHelper.MarkConduitCapacity() -> Attempted to mark capacity at a position that is already marked: [{cell},{layer}]");
            }
            else
            {
                //Debug.Log($"[PressurizedPipes] Marked [{cell},{layer}] with a capacity of {capacity}KG");
                CapacityDict.Add(pos, capacity);
            }
        }
        internal static void UnmarkConduitCapacity(int cell, int layer)
        {
            Vector2I pos = new Vector2I(cell, layer);
            if (!CapacityDict.ContainsKey(pos))
            {
                Debug.LogError($"[PressurizedPipes] IntegrationHelper.MarkConduitCapacity() -> Attempted to mark capacity at a position that is already marked: [{cell},{layer}]");
            }
            else
            {
                //Debug.Log($"[PressurizedPipes] Unmarked [{cell},{layer}] from capacity cache");
                CapacityDict.Remove(pos);
            }
        }

        public static float GetCapacityAt(int cell, int layer)
        {
            Vector2I pos = new Vector2I(cell, layer);
            if (CapacityDict.ContainsKey(pos))
                return CapacityDict[pos];
            else
                return -1f;
        }
    }
}
