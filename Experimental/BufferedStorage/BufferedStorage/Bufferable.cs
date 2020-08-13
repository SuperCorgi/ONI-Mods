using KSerialization;
using UnityEngine;
//20
namespace BufferedStorage
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class Bufferable : KMonoBehaviour, ISaveLoadable, ISingleSliderControl, ISliderControl
    {
        [Serialize]
        private float minimumDelivery = 0f;
        public string SliderTitleKey => "STRINGS.UI.SIDESCREENS.BUFFERABLE.TITLE";
        public string SliderUnits => "%";

        public float MinimumDelivery
        {
            get
            {
                return minimumDelivery;
            }
            set
            {
                minimumDelivery = Mathf.Clamp(value, 0f, 100f);
                storage.storageFullMargin = Mathf.Max((minimumDelivery / 100) * storage.capacityKg, 0.5f);
                //Trigger OnStorageChange to update meter animation and fetch logic
                locker.Trigger((int)GameHashes.OnStorageChange);
            }
        }

        [MyCmpGet]
        StorageLocker locker;
        [MyCmpGet]
        Storage storage;

        private void UpdateStorageMargin()
        {
            storage.storageFullMargin = Mathf.Max(minimumDelivery / 100) * locker.UserMaxCapacity;
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            MinimumDelivery = minimumDelivery;
        }

        public int SliderDecimalPlaces(int idex)
        {
            return 0;
        }

        public float GetSliderMin(int index)
        {
            return 0;
        }

        public float GetSliderMax(int index)
        {
            return 100f;
        }

        public float GetSliderValue(int index)
        {
            return MinimumDelivery;
        }

        public void SetSliderValue(float percent, int index)
        {
            MinimumDelivery = percent;
        }

        public string GetSliderTooltipKey(int index)
        {
            return "STRINGS.UI.SIDESCREENS.BUFFERABLE.TOOLTIP";
        }

        public string GetSliderTooltip()
        {
            return "Set the minimum delivery (based on max capacity)";
        }


    }
}
