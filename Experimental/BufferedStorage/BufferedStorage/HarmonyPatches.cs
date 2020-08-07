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
        [HarmonyPatch(typeof(StorageLockerConfig))]
        [HarmonyPatch("ConfigureBuildingTemplate")]
        public static class ModifyStorageMarginFull
        {
            public static void Postfix(GameObject go, Tag prefab_tag)
            {
                go.AddOrGet<Bufferable>();
            }
        }

        [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
        public static class BufferedStorageLocker_GeneratedBuildings
        {
            public static void Prefix()
            {
                Strings.Add("STRINGS.UI.SIDESCREENS.BUFFERABLE.TOOLTIP", "Set the storage margin.");
                Strings.Add("STRINGS.UI.SIDESCREENS.BUFFERABLE.TITLE", "Storage Margin");
            }
        }


    }
}
