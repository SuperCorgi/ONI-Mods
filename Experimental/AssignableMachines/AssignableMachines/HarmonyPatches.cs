using System;
using System.Collections.Generic;
using UnityEngine;
using Harmony;
using Database;
namespace AssignableMachines
{
    public class HarmonyPatches
    {
        private static HashSet<string> WarningTable = new HashSet<string>();

        [HarmonyPatch(typeof(ComplexFabricatorWorkable))]
        [HarmonyPatch("OnPrefabInit")]
        public static class ComplexFabricatorWorkable_OnPrefabInit
        {
            public static void Postfix(ComplexFabricatorWorkable __instance)
            {
                TryAddOwnable(__instance.gameObject);
            }
        }

        [HarmonyPatch(typeof(OilRefinery))]
        [HarmonyPatch("OnSpawn")]
        public static class OilRefinery_OnSpawn
        {
            public static void Postfix(OilRefinery __instance)
            {
                TryAddOwnable(__instance.gameObject);
            }
        }

        //[HarmonyPatch(typeof(ManualGenerator))]
        //[HarmonyPatch("OnPrefabInit")]
        //public static class ManualGenerator_OnPrefabInit
        //{
        //    public static void Postfix(ManualGenerator __instance)
        //    {
        //        TryAddOwnable(__instance.gameObject);
        //    }
        //}

        private static void TryAddOwnable(GameObject go)
        {
            string defName = go.GetComponent<Building>().Def.PrefabID;
            if (Db.Get().AssignableSlots.Exists(defName))
            {
                Ownable ownable = go.AddOrGet<Ownable>();
                ownable.slotID = defName;
                ownable.canBePublic = true;
                ownable.AddAutoassignPrecondition(CanAutoAssignTo);
            }
            else if (!WarningTable.Contains(defName))
            {
                Debug.LogWarning($"[AssignableMachines] Cannot assign Ownable! No Database AssignableSlots definition for: {defName}.");
                WarningTable.Add(defName);
            }
        }
        private static bool CanAutoAssignTo(MinionAssignablesProxy min)
        {
            return false;
        }

        [HarmonyPatch(typeof(AssignableSlots), MethodType.Constructor, new Type[] { })]
        public static class AssignableSlots_Constructor
        {
            public static void Postfix(AssignableSlots __instance)
            {
                __instance.Add(new OwnableSlot("RockCrusher", "RockCrusher"));
                __instance.Add(new OwnableSlot("ClothingFabricator", "ClothingFabricator"));
                __instance.Add(new OwnableSlot("ManualGenerator", "ManualGenerator"));
                __instance.Add(new OwnableSlot("Polymerizer", "Polymerizer"));
                __instance.Add(new OwnableSlot("OilRefinery", "OilRefinery"));
                __instance.Add(new OwnableSlot("MetalRefinery", "MetalRefinery"));
                __instance.Add(new OwnableSlot("GlassForge", "GlassForge"));
                __instance.Add(new OwnableSlot("SuitFabricator", "SuitFabricator"));
                __instance.Add(new OwnableSlot("SupermaterialRefinery", "SupermaterialRefinery"));
                __instance.Add(new OwnableSlot("MicrobeMusher", "MicrobeMusher"));
                __instance.Add(new OwnableSlot("GourmetCookingStation", "GourmetCookingStation"));
                __instance.Add(new OwnableSlot("EggCracker", "EggCracker"));
                __instance.Add(new OwnableSlot("CookingStation", "CookingStation"));
                __instance.Add(new OwnableSlot("Apothecary", "Apothecary"));
            }
        }
    }
}