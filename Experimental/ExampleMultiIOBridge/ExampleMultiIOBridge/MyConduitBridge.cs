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
            //Add an inert input and output port to the building. Inert ports do not automatically perform any behavior such as automatically taking in, outtputing, or filtering. The behavior is expected to be implemented by the Building when
            //using inert ports. 
            inputPort = multiIn.AddInputPortInert(conduitType, inputOffset);
            outputPort = multiOut.AddOutputPortInert(conduitType, outputOffset);
            //We can use input or output port, since they both give us the same manager.
            IConduitFlow conduitFlow = inputPort.GetConduitManager();
            //Buildings such as pipes, bridges, valves, and shutoffs have a default priority, meaning they execute after consumers but before dispensers.
            conduitFlow.AddConduitUpdater(OnConduitTick, ConduitFlowPriority.Default);
        }

        //Since our input/output ports are inert, we must define the behavior of the ports ourself.
        //float dt is the amount of time that has passed. typically not used as far as i am aware, likely always just 1 (1 second)
        private void OnConduitTick(float dt)
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
