using System;
using UnityEngine;
using STRINGS;
using KSerialization;
namespace MultiIO
{
    //Plans to continue modifying the Solid InputPort behaviour for ConduitTick.
    [SerializationConfig(MemberSerialization.OptIn)]
    public class InputPort : ConduitIO
    {
        public enum WrongElementResult
        {
            Destroy,
            Dump,
            Store
        }

        ///<summary>
        ///Defines what should occur when the conduit provides an element that does not match StoreTag. Solid ports will always drop.
        /// </summary>
        [SerializeField]
        public WrongElementResult WrongElement;
        /// <summary>
        /// A tag that defines what the input port will accept into storage.
        /// </summary>
        [SerializeField]
        public Tag StoreTag = GameTags.Any;
        /// <summary>
        /// The maximum amount in storage (in KG) before the input port stops inputting. Only counts what StoreTag is set to.
        /// </summary>
        [SerializeField]
        public float MaximumStore;
        /// <summary>
        /// True if the input port should operate even when the building is not operating.
        /// </summary>
        [SerializeField]
        public bool AlwaysConsume;
        /// <summary>
        /// The maximum rate (in KG) this port can draw from conduits. If not defined, no limit.
        /// </summary>
        [SerializeField]
        public float ConsumptionRate = float.PositiveInfinity;
        /// <summary>
        /// True if the storage should keep an entry of the inputted element if it is reduced to zero.
        /// </summary>
        [SerializeField]
        public bool KeepZeroMassObject = false;

        public SimHashes LastConsumedElement = SimHashes.Vacuum;

        protected override ConduitFlowPriority FlowPriority => ConduitFlowPriority.First;
        protected override Endpoint EndpointType => Endpoint.Sink;
        private bool PreviouslyConnected = false;
        private static readonly Operational.Flag inputConnectedFlag = new Operational.Flag("input_conduit", Operational.Flag.Type.Requirement);
        private Guid NeedsConduitStatusItemGuid;


        protected override void UpdateConduitExistsStatus(bool force = false)
        {
            bool connected = !RequiresConnection || RequireOutputs.IsConnected(portCell, ConduitType);
            //No need to trigger operational/gui change if we are changing to what they already are.
            if ((!force && connected == PreviouslyConnected))
                return;

            //Because outputConnectedFlag is a requirement, the machine cannot operate at all unless a conduit is attached to the port
            operational.SetFlag(inputConnectedFlag, connected);
            StatusItem status;
            if (ConduitType == ConduitType.Gas)
                status = Db.Get().BuildingStatusItems.NeedGasIn;
            else if (ConduitType == ConduitType.Liquid)
                status = Db.Get().BuildingStatusItems.NeedLiquidIn;
            else if (ConduitType == ConduitType.Solid)
                status = Db.Get().BuildingStatusItems.NeedSolidIn;
            else
                throw new ArgumentOutOfRangeException("InputPort.UpdateConduitExistsStatus() InputPort's conduit type was not a valid option");

            bool guidExists = NeedsConduitStatusItemGuid != Guid.Empty;

            if (connected == guidExists)
            {
                status.allowMultiples = true;
                NeedsConduitStatusItemGuid = selectable.ToggleStatusItem(status, NeedsConduitStatusItemGuid, !connected, new Tuple<ConduitType, Tag>(ConduitType, StoreTag));
            }
            PreviouslyConnected = connected;

        }
        protected override void ConduitTick(float delta)
        {
            if (!AlwaysConsume && !operational.IsOperational)
                return;
            IConduitFlow conduitFlow = GetConduitManager();
            if (ConduitType != ConduitType.Solid)
            {
                ConduitFlow mngr = conduitFlow as ConduitFlow;
                ConduitFlow.ConduitContents contents = mngr.GetContents(portCell);
                if (contents.mass <= 0)
                    return;
                Element element = ElementLoader.FindElementByHash(contents.element);
                bool matchesTag = StoreTag == GameTags.Any || element.HasTag(StoreTag);
                float rateAmount = ConsumptionRate * delta;
                float maxTake = 0f;
                float storageContains = storage.MassStored();
                float storageLeft = storage.capacityKg - storageContains;
                float portContains = StoreTag == GameTags.Any ? storageContains : storage.GetMassAvailable(StoreTag);
                float portLeft = MaximumStore - portContains;
                maxTake = Mathf.Min(storageLeft, portLeft);
                maxTake = Mathf.Min(rateAmount, maxTake);
                float removed = 0f;
                if (maxTake > 0f)
                {
                    ConduitFlow.ConduitContents removedContents = mngr.RemoveElement(portCell, maxTake);
                    removed = removedContents.mass;
                    LastConsumedElement = removedContents.element;
                    float ratio = removed / contents.mass;
                    if (!matchesTag)
                    {
                        BuildingHP.DamageSourceInfo damage = new BuildingHP.DamageSourceInfo
                        {
                            damage = 1,
                            source = BUILDINGS.DAMAGESOURCES.BAD_INPUT_ELEMENT,
                            popString = UI.GAMEOBJECTEFFECTS.DAMAGE_POPS.WRONG_ELEMENT
                        };
                        Trigger((int)GameHashes.DoBuildingDamage, damage);
                        if (WrongElement == WrongElementResult.Dump)
                        {
                            int buildingCell = Grid.PosToCell(_parent.transform.GetPosition());
                            SimMessages.AddRemoveSubstance(buildingCell, contents.element, CellEventLogger.Instance.ConduitConsumerWrongElement, removed, contents.temperature, contents.diseaseIdx, contents.diseaseIdx);
                            return;
                        }
                    }
                    if (ConduitType == ConduitType.Gas)
                    {
                        if (!element.IsGas)
                            Debug.LogWarning($"[MultIO] Gas input port attempted to consume non gass: {element.id.ToString()}");
                        else
                            storage.AddGasChunk(element.id, removed, contents.temperature, contents.diseaseIdx, contents.diseaseCount, KeepZeroMassObject, false);
                    }
                    else if (ConduitType == ConduitType.Liquid)
                    {
                        if (!element.IsLiquid)
                            Debug.LogWarning($"[MultIO] Liquid input port attempted to consume non liquid: {element.id.ToString()}");
                        else
                            storage.AddLiquid(element.id, removed, contents.temperature, contents.diseaseIdx, contents.diseaseCount, KeepZeroMassObject, false);
                    }

                }
            }
            else
            {
                SolidConduitFlow mngr = conduitFlow as SolidConduitFlow;
                SolidConduitFlow.ConduitContents contents = mngr.GetContents(portCell);
                if (contents.pickupableHandle.IsValid() && (AlwaysConsume || operational.IsOperational))
                {
                    float stored = StoreTag == GameTags.Any ? storage.MassStored() : storage.GetMassAvailable(StoreTag);
                    float maxStorage = Mathf.Min(storage.capacityKg, MaximumStore);
                    float availableStorage = Mathf.Max(0f, maxStorage - stored);
                    if (availableStorage > 0f)
                    {
                        Pickupable tmp = mngr.GetPickupable(contents.pickupableHandle);
                        bool matchesTag = StoreTag == GameTags.Any || tmp.HasTag(StoreTag);
                        if (matchesTag)
                        {
                            if (tmp.PrimaryElement.Mass <= stored || tmp.PrimaryElement.Mass > maxStorage)
                            {
                                Pickupable take = mngr.RemovePickupable(portCell);
                                if (take != null)
                                {
                                    storage.Store(take.gameObject, true);
                                }
                            }
                        }
                        else
                        {
                            Pickupable take = mngr.RemovePickupable(portCell);
                            take.transform.SetPosition(Grid.CellToPos(portCell));
                            //TODO: Add a PopFX. Likely will not do damage.
                        }
                    }
                }
            }
        }

    }
}
