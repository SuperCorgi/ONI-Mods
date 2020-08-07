using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using UnityEngine;
namespace BufferedStorage
{
    public class HarmonyPatches
    {
        //[HarmonyPatch(typeof(HydroponicFarmConfig))]
        //[HarmonyPatch("ConfigureBuildingTemplate")]
        //public static class StorageLockerSpawn
        //{
        //    public static void Postfix(GameObject go)
        //    {
        //        Storage storage = go.GetComponent<Storage>();
        //        storage.SetDefaultStoredItemModifiers(Storage.StandardInsulatedStorage);
        //    }
        //}

        //[HarmonyPatch(typeof(GasFilterConfig))]
        //[HarmonyPatch("CreateBuildingDef")]
        //public static class GasFilterLowerPowerCost
        //{
        //    public static void Postfix(BuildingDef __result)
        //    {
        //        __result.EnergyConsumptionWhenActive = 40f;
        //    }
        //}

        //[HarmonyPatch(typeof(InsulatedGasConduitConfig))]
        //[HarmonyPatch("CreateBuildingDef")]
        //public static class InsulatedGasPipeBetterInsulation
        //{
        //    public static void Postfix(BuildingDef __result)
        //    {
        //        __result.ThermalConductivity /= 2;
        //    }
        //}

        //[HarmonyPatch(typeof(GantryConfig))]
        //[HarmonyPatch("CreateBuildingDef")]
        //public static class LowerGantryPowerCost
        //{
        //    public static void Postfix(BuildingDef __result)
        //    {
        //        __result.EnergyConsumptionWhenActive = 240f;
        //    }
        //}

        [HarmonyPatch(typeof(StorageLockerConfig))]
        [HarmonyPatch("ConfigureBuildingTemplate")]
        public static class ModifyStorageMarginFull
        {
            public static void Postfix(GameObject go, Tag prefab_tag)
            {
                go.AddOrGet<Bufferable>();
               // Storage storage = go.AddOrGet<Storage>();
                //storage.storageFullMargin = 100f;
            }
        }

        [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
        public static class BufferedStorageLocker_GeneratedBuildings
        {
            public static void Prefix()
            {
                //Strings.Add("STRINGS.BUILDINGS.PREFABS.BUFFERSTORAGELOCKER.NAME", "Storage Buffer");
                //Strings.Add("STRINGS.BUILDINGS.PREFABS.BUFFERSTORAGELOCKER.DESC", "Stores large amounts of materials. Will stop delivering when nearly full");
                //Strings.Add("STRINGS.BUILDINGS.PREFABS.BUFFERSTORAGELOCKER.EFFECT", "Will stop delivering when nearly full");
                Strings.Add("STRINGS.UI.SIDESCREENS.BUFFERABLE.TOOLTIP", "Set the storage margin.");
                Strings.Add("STRINGS.UI.SIDESCREENS.BUFFERABLE.TITLE", "Storage Margin");

                //ModUtil.AddBuildingToPlanScreen("Base", "BufferStorageLocker");
            }
        }


    }
}
