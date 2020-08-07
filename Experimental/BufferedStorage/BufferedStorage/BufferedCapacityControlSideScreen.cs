using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace BufferedStorage
{
    public class BufferedCapacityControlSideScreen //: SideScreenContent
    {
        //private BufferStorageLocker target;

        //private IUserControlledCapacity config => target?.GetComponent<IUserControlledCapacity>();

        //[Header("Slider")]
        //[SerializeField]
        //private KSlider bufferSlider;

        //[Header("Slider")]
        //[SerializeField]
        //private KSlider slider;

        //[Header("Number Input")]
        //[SerializeField]
        //private KNumberInputField numberInput;

        //[Header("Number Input")]
        //[SerializeField]
        //private KNumberInputField bufferNumberInput;

        //[SerializeField]
        //private LocText unitsLabel;

        //protected override void OnSpawn()
        //{
        //    base.OnSpawn();
        //    slider.onDrag += delegate
        //    {
        //        ReceivedValueFromSlider(slider.value);
        //    };
        //    slider.onPointerDown += delegate
        //    {
        //        ReceivedValueFromSlider(slider.value);
        //    };
        //    slider.onMove += delegate
        //    {
        //        ReceivedValueFromSlider(slider.value);
        //    };

        //    bufferSlider.onDrag += delegate
        //    {
        //        ReceivedValueFromBufferSlider(bufferSlider.value);
        //    };
        //    bufferSlider.onPointerDown += delegate
        //    {
        //        ReceivedValueFromBufferSlider(bufferSlider.value);
        //    };
        //    bufferSlider.onMove += delegate
        //    {
        //        ReceivedValueFromBufferSlider(bufferSlider.value);
        //    };

        //    numberInput.onEndEdit += delegate
        //    {
        //        ReceivedValueFromInput(numberInput.currentValue);
        //    };

        //    bufferNumberInput.onEndEdit += delegate
        //    {
        //        ReceivedValueFromBufferInput(numberInput.currentValue);
        //    };

        //    numberInput.decimalPlaces = 1;
        //    bufferNumberInput.decimalPlaces = 1;

        //}

      


        //public override bool IsValidForTarget(GameObject target)
        //{
        //    return target.GetComponent<BufferStorageLocker>() != null;
        //}

        //public override void SetTarget(GameObject target)
        //{
        //    if (target == null)
        //    {
        //        Debug.LogError("Invalid gameObject received");
        //        return;
        //    }

        //    this.target = target.GetComponent<BufferStorageLocker>();
        //    if(this.target == null)
        //    {
        //        Debug.LogError("The gameObject received does not contain a BufferStorageLocker component");
        //        return;
        //    }

        //    slider.minValue = config.MinCapacity;
        //    slider.maxValue = config.MaxCapacity;
        //    slider.value = this.target.MaxBuffer;
        //    slider.GetComponentInChildren<ToolTip>();

        //    bufferSlider.minValue = config.MinCapacity;
        //    bufferSlider.maxValue = config.MaxCapacity;
        //    bufferSlider.value = this.target.MinBuffer;
        //    unitsLabel.text = config.CapacityUnits;

        //    numberInput.minValue = config.MinCapacity;
        //    numberInput.minValue = config.MaxCapacity;
        //    numberInput.currentValue = Mathf.Max(config.MinCapacity, Mathf.Min(config.MaxCapacity, this.target.MaxBuffer));
        //    numberInput.Activate();

        //    bufferNumberInput.minValue = config.MinCapacity;
        //    bufferNumberInput.minValue = config.MaxCapacity;
        //    bufferNumberInput.currentValue = Mathf.Max(config.MinCapacity, Mathf.Min(config.MaxCapacity, this.target.MinBuffer));
        //    bufferNumberInput.Activate();

        //    UpdateMaxBufferLabel();
        //    UpdateMinBufferLabel();


        //}

        //private void ReceivedValueFromSlider(float newValue)
        //{
        //    UpdateMaxBuffer(newValue);
        //}

        //private void ReceivedValueFromInput(float newValue)
        //{
        //    UpdateMaxBuffer(newValue);
        //}

        //private void ReceivedValueFromBufferSlider(float newValue)
        //{
        //    UpdateMinBuffer(newValue);
        //}


        //private void ReceivedValueFromBufferInput(float newValue)
        //{
        //    UpdateMinBuffer(newValue);
        //}

        //private void UpdateMaxBuffer(float newValue)
        //{
        //    target.MaxBuffer = newValue;
        //    slider.value = newValue;
        //    UpdateMaxBufferLabel();
        //}

        //private void UpdateMinBuffer(float newValue)
        //{
        //    target.MinBuffer = newValue;
        //    bufferSlider.value = newValue;
        //    UpdateMinBufferLabel();
        //}

        //private void UpdateMaxBufferLabel()
        //{
        //    numberInput.SetDisplayValue(target.MaxBuffer.ToString());
        //}

        //private void UpdateMinBufferLabel()
        //{
        //    bufferNumberInput.SetDisplayValue(target.MinBuffer.ToString());
        //}



    }
}
