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

        /// <summary>
        /// Creates an inert output port that does not automatically handle Conduit Update behavior. The building must define its own behavior for the output port.
        /// </summary>
        /// <param name="conduitType">The type of conduit this port should attach to.</param>
        /// <param name="offset">The offset (left-right, down-up) of where this port is located. Offsets begin bottom-left.</param>
        /// <param name="iconColor">The color the port icon will appear as.</param>
        /// <returns></returns>
        public OutputPort AddOutputPortInert(ConduitType conduitType, CellOffset offset, Color iconColor)
        {
            OutputPort port = AddOutputPort(conduitType, offset, iconColor);
            port.UseConduitUpdater = false;
            return port;
        }

        /// <summary>
        /// Creates an inert output port that does not automatically handle Conduit Update behavior. The building must define its own behavior for the output port.
        /// </summary>
        /// <param name="conduitType">The type of conduit this port should attach to.</param>
        /// <param name="offset">The offset (left-right, down-up) of where this port is located. Offsets begin bottom-left.</param>
        /// <returns></returns>
        public OutputPort AddOutputPortInert(ConduitType conduitType, CellOffset offset)
        {
            OutputPort port = AddOutputPort(conduitType, offset);
            port.UseConduitUpdater = false;
            return port;
        }


        /// <summary>
        /// For DoPostConfigurePreview and DoPostUnderConstruction, adds a port specification that can appear during building previews. Implements no port functionality and acts as a visual placeholder until the building has been created.
        /// </summary>
        /// <param name="conduitType">The type of conduit this port should attach to.</param>
        /// <param name="offset">The offset (left-right, down-up) of where this port is located. Offsets begin bottom-left.</param>
        /// <param name="iconColor">The color the port icon will appear as.</param>
        /// <returns>A reference to the PreviewOutput port. Not typically needed.</returns>
        public PreviewOutput AddPreviewOutputPort(ConduitType conduitType, CellOffset offset, Color iconColor)
        {
            iconColor.a = 1f;
            GameObject obj = new GameObject();
            PreviewOutput port = obj.AddComponent<PreviewOutput>(); port.ConduitType = conduitType;
            port.CellOffset = offset;
            port.IconColor = iconColor;
            base.AddIOPort(port);
            count++;
            return port;
        }
    }
}