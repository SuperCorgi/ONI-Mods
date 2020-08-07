using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace MultiIO
{
    public class MultiInput : MultiIO<InputPort>
    {
        public InputPort AddInputPort(ConduitType conduitType, CellOffset offset, Color iconColor, float consumptionRate, float maximumStore, Tag toStoreTag, bool alwaysConsume, InputPort.WrongElementResult wrongElementResult = InputPort.WrongElementResult.Dump, bool keepZeroMassObject = true)
        {
            iconColor.a = 1f;
            InputPort port = base.AddIOPort(conduitType, offset, iconColor);
            port.ConsumptionRate = consumptionRate;
            port.MaximumStore = maximumStore;
            port.StoreTag = toStoreTag;
            port.AlwaysConsume = alwaysConsume;
            port.WrongElement = wrongElementResult;
            port.KeepZeroMassObject = keepZeroMassObject;
            count++;
            return port;
        }

        public InputPort AddInputPort(ConduitType conduitType, CellOffset offset, float consumptionRate, float maximumStore, Tag toStoreTag, bool alwaysConsume, InputPort.WrongElementResult wrongElementResult = InputPort.WrongElementResult.Dump, bool keepZeroMassObject = true)
        {
            Color iconColor;
            if (PortCount == 0)
                iconColor = PortIconColors.StandardInput;
            else
                iconColor = PortIconColors.SecondaryColor;

            return AddInputPort(conduitType, offset, iconColor, consumptionRate, maximumStore, toStoreTag, alwaysConsume, wrongElementResult, keepZeroMassObject);
        }
    }
}
