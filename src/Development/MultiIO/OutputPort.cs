using System;
using System.Collections.Generic;
using UnityEngine;
using KSerialization;
namespace MultiIO
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class OutputPort : ConduitIO
    {
        ///<summary>
        ///If defined, used to determine which elements to pull from the buildings storage and out into a connected conduit.
        ///</summary>
        [SerializeField]
        public SimHashes[] ElementFilter;
        ///<summary>True: The ElementFilter will act as a blacklist\n
        ///False: The ElementFilter will act as a whitelist</summary>
        [SerializeField]
        public bool InvertElementFilter;
        /// <summary>
        /// <para>Whether or not this port will output if the machine is not currently operating.</para>
        /// </summary>
        [SerializeField]
        public bool AlwaysDispense;
        ///<summary>Default 20f (20KG). Conveyor rails have no built in maximum.</summary>
        [SerializeField]
        protected float SolidOutputMax = 20f;

        protected override Endpoint EndpointType => Endpoint.Source;
        protected override ConduitFlowPriority FlowPriority => ConduitFlowPriority.Last;

        private static readonly Operational.Flag outputConnectedFlag = new Operational.Flag("output_conduit", Operational.Flag.Type.Requirement);
        private static readonly Operational.Flag outputEmptyFlag = new Operational.Flag("output_empty", Operational.Flag.Type.Requirement);
        private bool PreviouslyEmpty = false;
        private bool PreviouslyConnected = false;
        private int ElementOutputOffset = 0;
        private Guid NeedsConduitStatusItemGuid;
        private Guid ConduitBlockedStatusItemGuid;
        private bool isDispensing = false;


        protected override void ConduitTick(float delta)
        {
            UpdateConduitBlockedStatus();
            bool dispensed = false;
            if (!operational.IsOperational && !AlwaysDispense)
            {
                return;
            }
            foreach (GameObject item in storage.items)
            {
                if (item.GetComponent<PrimaryElement>()?.Element.id == SimHashes.Water)
                {
                    item.AddOrGet<Pickupable>();
                }
            }
            PrimaryElement element = FindSuitableElement();

            if (element != null)
            {
                element.KeepZeroMassObject = true;
                IConduitFlow iConduitManager = GetConduitManager();
                if (iConduitManager == null)
                {
                    Debug.LogError($"[MultiIO] OutputPort.ConduitTick(): iConduitManager is null");
                }
                //Solid Conduits do not use the same kind of flow manager, so the code must be separated
                if (ConduitType == ConduitType.Solid)
                {
                    SolidConduitFlow solidManager = iConduitManager as SolidConduitFlow;
                    if (solidManager == null)
                    {
                        Debug.LogError($"[MultiIO] OutputPort.ConduitTick(): solidManager is null");
                    }
                    //Solid conveyor only needs to take elements with a Pikcupable component. The only difference between Water and Bottled Water is a Pikcupable component.
                    Pickupable pickup = element.gameObject.GetComponent<Pickupable>();
                    if (pickup == null)
                        return;
                    if (pickup.PrimaryElement.Mass > SolidOutputMax)
                        pickup = pickup.Take(SolidOutputMax);
                    solidManager.AddPickupable(portCell, pickup);
                    dispensed = true;
                }
                else if (ConduitType == ConduitType.Liquid || ConduitType == ConduitType.Gas)
                {
                    ConduitFlow conduitManager = iConduitManager as ConduitFlow;
                    if (conduitManager == null)
                    {
                        Debug.LogError($"[MutiIO] OutputPort.ConduitTick(): conduitManager is null");
                        return;
                    }
                    float amountMoved = conduitManager.AddElement(portCell, element.ElementID, element.Mass, element.Temperature, element.DiseaseIdx, element.DiseaseCount);
                    if (amountMoved > 0f)
                    {
                        float movedRatio = amountMoved / element.Mass;
                        int movedDisease = (int)(movedRatio * (float)element.DiseaseCount);
                        element.ModifyDiseaseCount(-movedDisease, "ConduitDispenser.ConduitUpdate");
                        element.Mass -= amountMoved;
                        _parent.Trigger((int)GameHashes.OnStorageChange, element.gameObject);
                        dispensed = true;
                    }
                }
            }
            isDispensing = dispensed;
        }


        private PrimaryElement FindSuitableElement()
        {
            List<GameObject> items = storage.items;
            int count = items.Count;
            for (int i = 0; i < count; i++)
            {
                int index = (i + ElementOutputOffset) % count;
                PrimaryElement component = items[index].GetComponent<PrimaryElement>();
                if (component != null && component.Mass > 0f && MatchesConduit(component))
                {

                    if (ElementFilter == null || ElementFilter.Length == 0 || MatchesFilter(component.ElementID))
                    {
                        ElementOutputOffset = (ElementOutputOffset + 1) % count;
                        return component;
                    }
                }
            }
            return null;
        }

        private bool MatchesConduit(PrimaryElement element)
        {
            if (ConduitType == ConduitType.Gas)
                return element.Element.IsGas;
            if (ConduitType == ConduitType.Liquid)
                return element.Element.IsLiquid;
            //All elements can be Pickupable as long as they are in a bottle/canister
            if (ConduitType == ConduitType.Solid)
                return element.GetComponent<Pickupable>() != null;
            return false;
        }
        private bool MatchesFilter(SimHashes element)
        {
            foreach (SimHashes filter in ElementFilter)
            {
                if (element == filter)
                    return !InvertElementFilter;
            }
            return InvertElementFilter;
        }

        protected override void UpdateConduitExistsStatus(bool force = false)
        {
            bool connected = !RequiresConnection || RequireOutputs.IsConnected(portCell, ConduitType);
            //No need to trigger operational/gui change if we would set them to what they already are.
            if (!force && connected == PreviouslyConnected)
                return;

            //Because outputConnectedFlag is a requirement, the machine cannot operate at all unless a conduit is attached to the port
            operational.SetFlag(outputConnectedFlag, connected);
            PreviouslyConnected = connected;
            StatusItem status;
            if (ConduitType == ConduitType.Gas)
                status = Db.Get().BuildingStatusItems.NeedGasOut;
            else if (ConduitType == ConduitType.Liquid)
                status = Db.Get().BuildingStatusItems.NeedLiquidOut;
            else if (ConduitType == ConduitType.Solid)
                status = Db.Get().BuildingStatusItems.NeedSolidOut;
            else
                throw new ArgumentOutOfRangeException("OutputPort.UpdateConduitExistsStatus() Dispenser's conduit type was not a valid option");

            bool guidExists = NeedsConduitStatusItemGuid != Guid.Empty;

            if (connected == guidExists)
            {
                NeedsConduitStatusItemGuid = selectable.ToggleStatusItem(status, NeedsConduitStatusItemGuid, !connected);
            }
        }

        private void UpdateConduitBlockedStatus(bool force = false)
        {
            IConduitFlow flowManager = this.GetConduitManager();
            bool isEmpty = flowManager.IsConduitEmpty(portCell);
            if (force || isEmpty != PreviouslyEmpty)
            {
                operational.SetFlag(outputEmptyFlag, isEmpty);
                PreviouslyEmpty = isEmpty;
                StatusItem conduitBlockedMultiples = Db.Get().BuildingStatusItems.ConduitBlockedMultiples;
                bool guidExists = ConduitBlockedStatusItemGuid != Guid.Empty;
                if (isEmpty == guidExists)
                {
                    ConduitBlockedStatusItemGuid = selectable.ToggleStatusItem(conduitBlockedMultiples, ConduitBlockedStatusItemGuid, !isEmpty);
                }
            }
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            UpdateConduitBlockedStatus(true);
        }



    }
}
