using UnityEngine;
using KSerialization;
namespace MultiIO
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class MultiOutput : MultiIO<OutputPort>
    {
        /// <summary>
        /// Adds an OutputPort as a child Component to whatever building MultiOutput is attached to. This function can be used any number of times as long the ports remain valid. Returns a reference to the OutputPort object.
        /// </summary>
        /// <param name="conduitType">The type of conduit this port should attach to.</param>
        /// <param name="offset">The offset (left-right, down-up) of where this port is located. Offsets begin bottom-left.</param>
        /// <param name="iconColor">The color the port icon will appear as.</param>
        /// <param name="alwaysDispense">Whether or not this port should output from storage even if the machine is not operating.</param>
        /// <param name="elementFilter">If defined, will filter which elements to put through this port.</param>
        /// <param name="invertElementFilter">If the ElementFilter is defined, true will make the filter act as a blacklist, false will make the filter act as a whitelist.</param>
        /// <returns>A reference to the OutputPort that has already been attached as a child component.</returns>
        public OutputPort AddOutputPort(ConduitType conduitType, CellOffset offset, Color iconColor, bool alwaysDispense = false, SimHashes[] elementFilter = null, bool invertElementFilter = false)
        {
            //Debug.Log($"[MultiIO] Adding output port to building template");
            iconColor.a = 1f;
            OutputPort port = base.AddIOPort(conduitType, offset, iconColor);
            port.AlwaysDispense = alwaysDispense;
            port.ElementFilter = elementFilter;
            port.InvertElementFilter = invertElementFilter;
            count++;
            return port;
            //Debug.Log($"[MultiIO] Number of output ports now: {count}");
        }

        /// <summary>
        /// Adds an OutputPort as a child Component to whatever building MultiOutput is attached to. This function can be used any number of times as long the ports remain valid.
        /// </summary>
        /// <param name="conduitType">The type of conduit this port should attach to.</param>
        /// <param name="offset">The offset (left-right, down-up) of where this port is located. Offsets begin bottom-left.</param>
        /// <param name="alwaysDispense">Whether or not this port should output from storage even if the machine is not operating.</param>
        /// <param name="elementFilter">If defined, will filter which elements to put through this port.</param>
        /// <param name="invertElementFilter">If the ElementFilter is defined, true will make the filter act as a blacklist, false will make the filter act as a whitelist.</param>
        /// <returns>A reference to the OutputPort that has already been attached as a child component.</returns>
        public OutputPort AddOutputPort(ConduitType conduitType, CellOffset offset, bool alwaysDispense = true, SimHashes[] elementFilter = null, bool invertElementFilter = false)
        {
            Color iconColor;
            if (PortCount == 0)
                iconColor = PortIconColors.StandardOutput;
            else
                iconColor = PortIconColors.SecondaryColor;

            return AddOutputPort(conduitType, offset, iconColor, alwaysDispense, elementFilter, invertElementFilter);
        }

    }
}