using System;
using System.Collections.Generic;
using Harmony;
using Klei.AI;
using UnityEngine;


namespace ExposureNotification
{
    public class ExposureNotificationHarmonyPatches
    {

        private static LocString DUPE_EXPOSED_TO_GERMS_POPFX = "Exposed to {0}";
        private static LocString DUPE_EXPOSED_TO_GERMS_NOTIFICATION = "Exposed to {0}";
        private static LocString DUPE_EXPOSED_TO_GERMS_TOOLTIP = "The following Duplicants have been exposed to {0}:";
        private static bool MinionsLoaded = false;

        private static void CreateAndAddNotification(GermExposureMonitor.Instance monitor, string sicknessName)
        {
            string text = string.Format(DUPE_EXPOSED_TO_GERMS_NOTIFICATION, sicknessName);
            Notification.ClickCallback callback = new Notification.ClickCallback(Notification_Callback);
            MinionIdentity minion = monitor.gameObject.GetComponent<MinionIdentity>();
            Notification notification = new Notification(text, NotificationType.BadMinor, HashedString.Invalid,
                (List<Notification> n, object d) => string.Format(DUPE_EXPOSED_TO_GERMS_TOOLTIP, sicknessName) + n.ReduceMessages(true),
                null, false, 0, callback, minion);
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
            MinionIdentity minion = (MinionIdentity)d;
            SelectTool.Instance.SelectAndFocus(minion.transform.GetPosition(), minion.GetComponent<KSelectable>(), new Vector3(0, 0, 0));
        }

        [HarmonyPatch(typeof(GermExposureMonitor.Instance))]
        [HarmonyPatch("SetExposureState")]
        public static class SetExposureStateHarmonyPatch
        {
            public static void Postfix(string germ_id, GermExposureMonitor.ExposureState exposure_state, GermExposureMonitor.Instance __instance)
            {
                if (exposure_state == GermExposureMonitor.ExposureState.Exposed)
                {
                    ExposureType exposure_type = null;

                    foreach (ExposureType type in TUNING.GERM_EXPOSURE.TYPES)
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
                    foreach (MinionIdentity minion in minions)
                    {
                        GermExposureMonitor.Instance monitor = minion.gameObject.GetSMI<GermExposureMonitor.Instance>();
                        foreach (ExposureType exposure in TUNING.GERM_EXPOSURE.TYPES)
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

    }

}
