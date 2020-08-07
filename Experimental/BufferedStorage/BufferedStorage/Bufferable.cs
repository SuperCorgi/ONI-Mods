using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSerialization;
namespace BufferedStorage
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class Bufferable : KMonoBehaviour, ISaveLoadable, ISingleSliderControl, ISliderControl
    {
        [Serialize]
        private float minimumDelivery = 0.5f;
        public string SliderTitleKey => "STRINGS.UI.SIDESCREENS.BUFFERABLE.TITLE";
        public string SliderUnits => "KG";

        public float MinimumDelivery
        {
            get
            {
                return minimumDelivery;
            }
            set
            {
                minimumDelivery = value;
                storage.storageFullMargin = minimumDelivery;
                //Trigger OnStorageChange to update meter animation and fetch logic
                locker.Trigger((int)GameHashes.OnStorageChange);
            }
        }

        [MyCmpGet]
        StorageLocker locker;
        [MyCmpGet]
        Storage storage;



        protected override void OnSpawn()
        {
            base.OnSpawn();
            MinimumDelivery = minimumDelivery;
        }

        //private void OnStorageChange(object data)
        //{
        //    if (!inBufferMode && storage.MassStored() + storage.storageFullMargin > storage.capacityKg)
        //    {
        //        inBufferMode = true;
        //        UpdateThresholds();
        //    }
        //    else if (inBufferMode)
        //    {
        //        if(storage.RemainingCapacity() > BufferThreshold * locker.UserMaxCapacity)
        //        {
        //            inBufferMode = false;
        //            UpdateThresholds();
        //        }
        //    }
        //}

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
            return 4000f;
        }

        public float GetSliderValue(int index)
        {
            return MinimumDelivery;
        }

        public void SetSliderValue(float percent, int index)
        {
            //Debug.Log($"[Bufferable] Slider percent: {percent}");
            MinimumDelivery = percent;
            //Debug.Log($"[Bufferable] MarginFull: {storage.storageFullMargin}, UserCapacity: {locker.UserMaxCapacity}, AmountStored: {locker.AmountStored}");
        }

        public string GetSliderTooltipKey(int index)
        {
            return "STRINGS.UI.SIDESCREENS.BUFFERABLE.TOOLTIP";
        }

        public string GetSliderTooltip()
        {
            return "Set the storage margin.";
        }


    }
}
