using UnityEngine;
using System.Collections.Generic;
using MultiIO;

namespace ExampleMultiIOBridge
{
    public class MyConduitBridge : KMonoBehaviour, IBridgedNetworkItem
    {
        private static readonly CellOffset inputOffset = new CellOffset(0, 0);
        private static readonly CellOffset outputOffset = new CellOffset(0, 2);

        [SerializeField]
        public ConduitType conduitType;

        [MyCmpAdd]
        public MultiInput multiIn;

        [MyCmpAdd]
        public MultiOutput multiOut;

        public InputPort inputPort = null;
        public OutputPort outputPort = null;


        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            //Add an input port to the building. It does not have an intake limit specified, but our ConduitUpdater will handle that logic anyway. It can not store any mass inside of the building, and can accept any type.
            inputPort = multiIn.AddInputPort(conduitType, inputOffset, float.PositiveInfinity, 0f, GameTags.Any, false, InputPort.WrongElementResult.Store, false);
            //Add an output port to the building. It is specified to not always attempt to dispense, though again our custom updater will handle that logic
            outputPort = multiOut.AddOutputPort(conduitType, outputOffset, false);
            //When the input port is updating, have it use our own custom ConduitTick function.
            inputPort.ChangeConduitUpdater(OnConduitTick);
            //The input port will handle ConduitTick logic, so the output port will do nothing
            outputPort.ChangeConduitUpdater(x => { });
        }
        //We don't need the input port in this case since it is already stored in this instance
        private void OnConduitTick(InputPort input)
        {
            //The ConduitFlow task is an overarching flow manager for a specific conduit type. If our bridge is a liquid bridge, we will get the liquid manager.
            ConduitFlow flowManager = Conduit.GetFlowManager(conduitType);
            //If there is a pipe connected to the location of the input port, and a pipe connected to the location of the output port
            if (flowManager.HasConduit(inputPort.GetPortCell()) && flowManager.HasConduit(outputPort.GetPortCell()))
            {
                //Get the contents of the input pipe
                ConduitFlow.ConduitContents contents = flowManager.GetContents(inputPort.GetPortCell());
                if(contents.mass > 0f)
                {
                    //The AddElement method will attempt to move as much fluid from the input to the output as it can, and will return the amount successfully moved (if any).
                    //This method also handles things such as merging disease amounts and temperature based on how much is moved
                    float amountMOved = flowManager.AddElement(outputPort.GetPortCell(), contents.element, contents.mass, contents.temperature, contents.diseaseIdx, contents.diseaseCount);
                    if(amountMOved > 0f)
                    {
                        //RemoveElement, similar to AddElement, automatically reduces the disease count (if any germs are present)
                        flowManager.RemoveElement(inputPort.GetPortCell(), amountMOved);
                        
                    }
                }
            }
        }




        public void AddNetworks(ICollection<UtilityNetwork> networks)
        {
            IUtilityNetworkMgr networkManager = Conduit.GetNetworkManager(conduitType);
        }

        public bool IsConnectedToNetworks(ICollection<UtilityNetwork> networks)
        {
            return false;
        }

        public int GetNetworkCell()
        {
            return -1;
        }

    }
}
