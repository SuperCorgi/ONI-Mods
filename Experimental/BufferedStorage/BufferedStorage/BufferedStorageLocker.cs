using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KSerialization;
using UnityEngine;
namespace BufferedStorage
{
    public class BufferStorageLocker : StorageLocker
    {
        [Serialize]
        public float maxStoragePercent = 0.15f;

        [Serialize]
        private float _threshold;

        public float Threshold
        {
            get
            {
                return _threshold;
            }
            set
            {
                _threshold = value;
            }
        }

        public ThresholdScreenLayoutType LayoutType => ThresholdScreenLayoutType.SliderBar;

        public LocString Title => "Storage Buffer";
        public LocString ThresholdValueName => "Percent";
        public LocString ThresholdValueUnits()
        {
            return "%";
        }

        public float GetRangeMinInputField()
        {
            return 0f;
        }

        public float GetRangeMaxInputField()
        {
            return 100f;
        }

        public float ProcessedSliderValue(float input)
        {
            return (float)Mathf.RoundToInt(input);
        }

        public float ProcessedInputValue(float input)
        {
            return (float)Mathf.RoundToInt(input);
        }

        public override float UserMaxCapacity
        {
            get
            {
                return base.UserMaxCapacity;
            }

            set
            {
                base.UserMaxCapacity = value;
                //GetComponent<Storage>().storageFullMargin = value * maxStoragePercent;
            }
        }



        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
        }

        public int SliderDecimalPlaces(int idex)
        {
            return 1;
        }

        public float GetSliderMin(int index)
        {
            return 0;
        }

        public float GetSliderMax(int index)
        {
            return 100;
        }

    }
}
