using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace MultiIO
{
    /// <summary>
    /// A component that facilities adding multiple input ports to a building while also handling much of their typical logic.
    /// </summary>
    public class MultiInput : MultiIO<InputPort>
    {
        /// <summary>
        /// Adds an InputPort as a child Component to whatever building MultiInput is attached to. Behaves similar to a ConduitConsumer. This function can be used any number of times as long the ports remain valid.
        /// </summary>
        /// <param name="conduitType">The type of conduit this port should attach to.</param>
        /// <param name="offset">The offset (left-right, down-up) of where this port is located. Offsets begin bottom-left.</param>
        /// <param name="iconColor">The color the port icon will appear as.</param>
        /// <param name="consumptionRate">The maximum rate the input port should consume from the connected conduit.</param>
        /// <param name="maximumStore">The maximum amount (KG) the conduit can store into the Storage component of the building.</param>
        /// <param name="toStoreTag">Specifies a tag the input material must have for the port to accept it.</param>
        /// <param name="alwaysConsume">Whether to stop consuming from the input if the building is not operational.</param>
        /// <param name="wrongElementResult">What to do with a material if it does not match the specified toStoreTag param.</param>
        /// <param name="keepZeroMassObject">Whether or not elements with zero mass should remain displayed in storage section of the details screen.</param>
        /// <returns>The InputPort object that was added to the MultiInput.</returns>
        public InputPort AddInputPort(ConduitType conduitType, CellOffset offset, Tag toStoreTag, Color iconColor, float consumptionRate = float.PositiveInfinity, float maximumStore = 0f, bool alwaysConsume = false, InputPort.WrongElementResult wrongElementResult = InputPort.WrongElementResult.Dump, bool keepZeroMassObject = true)
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
        /// <summary>
        /// Adds an InputPort as a child Component to whatever building MultiInput is attached to. Behaves similar to a ConduitConsumer. This function can be used any number of times as long the ports remain valid.
        /// </summary>
        /// <param name="conduitType">The type of conduit this port should attach to.</param>
        /// <param name="offset">The offset (left-right, down-up) of where this port is located. Offsets begin bottom-left.</param>
        /// <param name="consumptionRate">The maximum rate the input port should consume from the connected conduit.</param>
        /// <param name="maximumStore">The maximum amount (KG) the conduit can store into the Storage component of the building.</param>
        /// <param name="toStoreTag">Specifies a tag the input material must have for the port to accept it.</param>
        /// <param name="alwaysConsume">Whether to stop consuming from the input if the building is not operational.</param>
        /// <param name="wrongElementResult">What to do with a material if it does not match the specified toStoreTag param.</param>
        /// <param name="keepZeroMassObject">Whether or not elements with zero mass should remain displayed in storage section of the details screen.</param>
        /// <returns>The InputPort object that was added to the MultiInput.</returns>
        public InputPort AddInputPort(ConduitType conduitType, CellOffset offset, Tag toStoreTag, float consumptionRate = float.PositiveInfinity, float maximumStore = 0f, bool alwaysConsume = false, InputPort.WrongElementResult wrongElementResult = InputPort.WrongElementResult.Dump, bool keepZeroMassObject = true)
        {
            Color iconColor;
            if (PortCount == 0)
                iconColor = PortIconColors.StandardInput;
            else
                iconColor = PortIconColors.SecondaryColor;

            return AddInputPort(conduitType, offset, toStoreTag, iconColor, consumptionRate, maximumStore, alwaysConsume, wrongElementResult, keepZeroMassObject);
        }

        /// <summary>
        /// Creates an inert input port that does not automatically handle Conduit Update behavior. The building must define its own behavior for the input port.
        /// </summary>
        /// <param name="conduitType">The type of conduit this port should attach to.</param>
        /// <param name="offset">The offset (left-right, down-up) of where this port is located. Offsets begin bottom-left.</param>
        /// <param name="iconColor">The color the port icon will appear as.</param>
        /// <returns></returns>
        public InputPort AddInputPortInert(ConduitType conduitType, CellOffset offset, Color iconColor)
        {
            InputPort port = AddInputPort(conduitType, offset, GameTags.Any, iconColor);
            port.UseConduitUpdater = false;
            return port;
        }

        /// <summary>
        /// Creates an inert input port that does not automatically handle Conduit Update behavior. The building must define its own behavior for the input port.
        /// </summary>
        /// <param name="conduitType">The type of conduit this port should attach to.</param>
        /// <param name="offset">The offset (left-right, down-up) of where this port is located. Offsets begin bottom-left.</param>
        /// <returns></returns>
        public InputPort AddInputPortInert(ConduitType conduitType, CellOffset offset)
        {
            Color iconColor;
            if (PortCount == 0)
                iconColor = PortIconColors.StandardInput;
            else
                iconColor = PortIconColors.SecondaryColor;

            InputPort port = AddInputPort(conduitType, offset, GameTags.Any, iconColor);
            port.UseConduitUpdater = false;
            return port;
        }
    }
}
