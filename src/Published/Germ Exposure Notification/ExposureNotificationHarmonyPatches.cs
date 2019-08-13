using System;
using System.Collections.Generic;
using Harmony;
using Klei.AI;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;


namespace ExposureNotification
{
    public class ExposureNotificationHarmonyPatches
    {
        private class ConfigObject
        {
            public bool ShowLocation = false;
        }

        private class ShowLocationObject
        {
            public Vector3 Pos;
            public MinionIdentity Minion;
            public bool ShowLocation = false;
            public ShowLocationObject(MinionIdentity minion)
            {
                Pos = minion.transform.GetPosition();
                Minion = minion;
            }
        }

        private static LocString DUPE_EXPOSED_TO_GERMS_POPFX = "Exposed to {0}";
        private static LocString DUPE_EXPOSED_TO_GERMS_NOTIFICATION = "Exposed to {0}";
        private static LocString DUPE_EXPOSED_TO_GERMS_TOOLTIP = "The following Duplicants have been exposed to {0}:";
        private static bool MinionsLoaded = false;
        private static bool showLocation = false;

        private static void CreateAndAddNotification(GermExposureMonitor.Instance monitor, string sicknessName)
        {
            string text = string.Format(DUPE_EXPOSED_TO_GERMS_NOTIFICATION, sicknessName);
            Notification.ClickCallback callback = new Notification.ClickCallback(Notification_Callback);
            MinionIdentity minion = monitor.gameObject.GetComponent<MinionIdentity>();
            ShowLocationObject slo = new ShowLocationObject(minion);
            slo.ShowLocation = MinionsLoaded && showLocation;
            Notification notification = new Notification(text, NotificationType.BadMinor, HashedString.Invalid,
                (List<Notification> n, object d) => string.Format(DUPE_EXPOSED_TO_GERMS_TOOLTIP, sicknessName) + n.ReduceMessages(true),
                null, false, 0, callback, slo);
            monitor.gameObject.AddOrGet<Notifier>().Add(notification);
            Action<object> act = null;
            act = x =>
            {
                monitor.gameObject.AddOrGet<Notifier>().Remove(notification);
                monitor.Unsubscribe((int)GameHashes.SleepFinished, act);
                monitor.Unsubscribe((int)GameHashes.DuplicantDied, act);
            };
            monitor.Subscribe((int)GameHashes.SleepFinished, act);
            monitor.Subscribe((int)GameHashes.DuplicantDied, act);
        }


        private static void Notification_Callback(object d)
        {
            ShowLocationObject slo = (ShowLocationObject)d;
            if (slo.ShowLocation)
            {
                CameraController.Instance.CameraGoTo(slo.Pos, 4);
                SelectTool.Instance.Select(slo.Minion.GetComponent<KSelectable>(), true);
            }
            else
            {
                SelectTool.Instance.SelectAndFocus(slo.Minion.transform.GetPosition(), slo.Minion.GetComponent<KSelectable>(), new Vector3(0, 0, 0));
            }
        }

        [HarmonyPatch(typeof(GermExposureMonitor.Instance))]
        [HarmonyPatch("SetExposureState")]
        public static class SetExposureStateHarmonyPatch
        {
            public static void Postfix(string germ_id, GermExposureMonitor.ExposureState exposure_state, GermExposureMonitor.Instance __instance)
            {
                if (exposure_state == GermExposureMonitor.ExposureState.Exposed)
                {
                    GermExposureMonitor.ExposureType exposure_type = null;
                    foreach (GermExposureMonitor.ExposureType type in GermExposureMonitor.exposureTypes)
                    {
                        if (germ_id == type.germ_id)
                        {
                            exposure_type = type;
                            break;
                        }
                    }
                    Sickness sickness = exposure_type?.sickness_id != null ? Db.Get().Sicknesses.Get(exposure_type.sickness_id) : null;
                    string sicknessName = sickness != null ? sickness.Name : "a disease";
                    string text = string.Format(DUPE_EXPOSED_TO_GERMS_POPFX, sicknessName);
                    PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Negative, text, __instance.gameObject.transform, 3f, true);
                    CreateAndAddNotification(__instance, sicknessName);
                }
            }
        }

        [HarmonyPatch(typeof(Game))]
        [HarmonyPatch("Update")]
        public static class RunOnceMinionsLoadedPatch
        {
            public static void Postfix()
            {
                if (MinionsLoaded)
                    return;
                List<MinionIdentity> minions = Components.LiveMinionIdentities?.Items;
                if (minions == null || minions.Count == 0)
                {
                    return;
                }
                try
                {
                    ReadConfigFile();
                }
                catch (Exception)
                {
                    showLocation = false;
                }
                try
                {
                    foreach (MinionIdentity minion in minions)
                    {
                        GermExposureMonitor.Instance monitor = minion.gameObject.GetSMI<GermExposureMonitor.Instance>();
                        foreach (GermExposureMonitor.ExposureType exposure in GermExposureMonitor.exposureTypes)
                        {
                            GermExposureMonitor.ExposureState state = monitor.GetExposureState(exposure.germ_id);
                            if (state == GermExposureMonitor.ExposureState.Contracted || state == GermExposureMonitor.ExposureState.Exposed)
                            {
                                Sickness sickness = Db.Get().Sicknesses.Get(exposure.sickness_id);
                                string sicknessName = sickness != null ? sickness.Name : "a disease";
                                CreateAndAddNotification(monitor, sicknessName);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    //Do nothing. Can't be sure why this error occurred, but hopefully will only happen this time.
                }
                MinionsLoaded = true;
            }
        }

        public static string Test()
        {
            return "";
        }

        public static bool Test(string s)
        {
            return false;
        }

        public static void ReadConfigFile()
        {
            string savePath = SaveLoader.GetActiveSaveFilePath();
            if (savePath == "")
            {
                return;
            }
            FileInfo file = new FileInfo(savePath);
            DirectoryInfo saveFolder = file.Directory;
            if (saveFolder == null)
            {
                return;
            }
            DirectoryInfo rootFolder = null;
            DirectoryInfo modFolder = null;
            DirectoryInfo steamFolder = null;
            DirectoryInfo targetFolder = null;

            rootFolder = saveFolder.Parent;
            if (rootFolder == null)
                return;

            if (rootFolder.Name == "save_files")
                rootFolder = saveFolder.Parent;

            if (rootFolder == null)
                return;

            foreach (DirectoryInfo d in rootFolder.GetDirectories())
            {
                if (d.Name == "mods")
                {
                    modFolder = d;
                    break;
                }
            }

            if (modFolder == null)
            {
                return;
            }
            foreach (DirectoryInfo d in modFolder.GetDirectories())
            {
                if (d.Name == "Steam")
                {
                    steamFolder = d;
                    break;
                }
            }

            if (steamFolder == null)
            {
                return;
            }
            foreach (DirectoryInfo d in steamFolder.GetDirectories())
            {
                if (d.Name == "1720103574")
                {
                    targetFolder = d;
                    break;
                }
            }

            if (targetFolder == null)
            {
                return;
            }
            try
            {
                FileInfo target = null;
                foreach (FileInfo f in targetFolder.GetFiles())
                {
                    if ($"{f.Name}".ToLower() == "config.json")
                    {
                        target = f;
                        break;
                    }
                }
                if (target != null)
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.NullValueHandling = NullValueHandling.Ignore;
                    using (StreamReader sr = new StreamReader(target.FullName))
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        object _result = serializer.Deserialize<ConfigObject>(reader);
                        bool result = _result == null ? false : ((ConfigObject)_result).ShowLocation;
                        showLocation = result;
                    }
                }
                else
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.NullValueHandling = NullValueHandling.Ignore;
                    using (StreamWriter sw = new StreamWriter(targetFolder.FullName + @"/config.json"))
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        ConfigObject co = new ConfigObject();
                        co.ShowLocation = false;
                        serializer.Serialize(writer, co);
                        showLocation = false;
                    }
                }
            }
            catch (Exception)
            {
                showLocation = false;
            }
        }

        [HarmonyPatch(typeof(Game))]
        [HarmonyPatch("Load")]
        public static class ResetMinionsLoadedPatch
        {
            public static void Prefix()
            {
                MinionsLoaded = false;
            }
        }

    }

}
