using KSerialization;
using UnityEngine;

namespace MultiIO
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public abstract class ConduitIO : KMonoBehaviour, ISaveLoadable
    {
        protected GameObject _parent
        {
            get
            {
                return this.transform.parent.gameObject;
            }
            private set
            {
                if (value == null)
                    Debug.LogError($"[MultiIO] ConduitIO.parent.set() -> Passed GameObject was null. A parent must be specified");
                else if (value.transform == null)
                    Debug.LogError($"[MultiIO] ConduitIO.parent.set() -> Passed GameObject had no transform property. Was the GameObject initialized properly?");
                else if (this.transform == null)
                    Debug.LogError($"[MultiIO] ConduitIO.parent.set() -> Attempted to set parent but this instance's transform property was null. Was this instance's gameObject initialized?");
                else
                    this.transform.parent = value.transform;
            }
        }
        /// <summary>
        /// The type of conduits this port will attach to (and the type of elements it will interact with)
        /// </summary>
        public ConduitType ConduitType
        {
            get { return _conduitType; }
            set
            {
                if (_spawned)
                    Debug.LogError($"[MultiIO] ConduitIO.ConduitType.set() -> Attempted to change ConduitType after this component was spawned.");
                else
                    _conduitType = value;
            }
        }
        [SerializeField]
        private ConduitType _conduitType;
        /// <summary>
        /// The offset (left->right, down->up) from the building's origin. Origin begins bottom-left.
        /// </summary>
        public CellOffset CellOffset
        {
            get { return _cellOffset; }
            set
            {
                if (_spawned)
                    Debug.LogError($"[MultiIO] ConduitIO.CellOffset.set() -> Attempted to change CellOffset after this component was spawned.");
                _cellOffset = value;
            }
        }
        [SerializeField]
        private CellOffset _cellOffset;
        [SerializeField]
        protected Color mIconColor;
        /// <summary>
        /// The tint that is applied to the port's icon in its relevant overlay.
        /// </summary>
        public Color IconColor
        {
            get
            {
                return mIconColor == null ? Color.white : mIconColor;
            }
            set
            {
                mIconColor = value;
            }
        }
        /// <summary>
        /// True if a connection to a conduit connection to this port is required for the building to operate.
        /// </summary>
        public bool RequiresConnection
        {
            get
            {
                return _requiresConnection;
            }
            set
            {
                if (_spawned)
                    Debug.LogError($"[MultiIO] ConduitIO.RequiresConnection.set() -> Attempted to change RequiresConnection after this component has spawned.");
                else
                    _requiresConnection = value;
            }
        }
        [SerializeField]
        private bool _requiresConnection = true;

        protected abstract Endpoint EndpointType
        {
            get;
        }
        protected abstract ConduitFlowPriority FlowPriority
        {
            get;
        }

        protected FlowUtilityNetwork.NetworkItem _networkItem;
        protected bool _spawned = false;
        protected Operational operational => _parent.GetComponent<Operational>();
        protected Storage storage => _parent.GetComponent<Storage>();
        protected Building building => _parent.GetComponent<Building>();
        protected KSelectable selectable => _parent.GetComponent<KSelectable>();
        protected HandleVector<int>.Handle partitionerEntry;
        protected int portCell = -1;
        public GameObject CellVisualizer = null;

        protected IConduitFlow GetConduitManager()
        {

            if (ConduitType == ConduitType.Gas)
                return Game.Instance.gasConduitFlow;
            if (ConduitType == ConduitType.Liquid)
                return Game.Instance.liquidConduitFlow;
            if (ConduitType == ConduitType.Solid)
                return Game.Instance.solidConduitFlow;
            Debug.LogError($"[MultiIO] ConduitIO.GetConduitManager() -> Invalid ConduitType, could not find manager. ConduitType: {ConduitType.ToString()}");
            return null;
        }

        public int GetPortCell()
        {
            if (portCell == -1)
            {
                CellOffset roatedOffset = building.GetRotatedOffset(CellOffset);
                int bottomLeftCell = Grid.PosToCell(building.transform.GetPosition());
                portCell = Grid.OffsetCell(bottomLeftCell, roatedOffset);
            }
            return portCell;
        }

        protected ObjectLayer GetConduitObjectLayer()
        {
            if (ConduitType == ConduitType.Gas)
                return ObjectLayer.GasConduit;
            if (ConduitType == ConduitType.Liquid)
                return ObjectLayer.LiquidConduit;
            if (ConduitType == ConduitType.Solid)
                return ObjectLayer.SolidConduit;
            Debug.LogError($"[MultiIO] Could not find ObjectLayer for current ConduitType. ConduitType: {ConduitType}");
            return ObjectLayer.GasConduit;
        }

        protected abstract void UpdateConduitExistsStatus(bool force = false);

        protected abstract void ConduitTick(float delta);

        protected IUtilityNetworkMgr GetNetworkManager()
        {
            if (ConduitType == ConduitType.Solid)
                return Game.Instance.solidConduitSystem;
            if (ConduitType == ConduitType.Liquid)
                return Game.Instance.liquidConduitSystem;
            if (ConduitType == ConduitType.Gas)
                return Game.Instance.gasConduitSystem;
            return null;
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            _spawned = true;
            Debug.Log($"[MultiIO] ConduitIO.OnSpawn() -> ConduitType: {ConduitType.ToString()}  CellOffset: {CellOffset.x + "," + CellOffset.y}");
            portCell = GetPortCell();
            MultiIOExtensions.RegisterPort(portCell, this);
            //Register an event listener for any changes to the grid at the location of this port.
            ScenePartitionerLayer layer = GameScenePartitioner.Instance.objectLayers[(int)GetConduitObjectLayer()];
            partitionerEntry = GameScenePartitioner.Instance.Add("ConduitIO.OnSpawn", _parent, portCell, layer, delegate
            {
                UpdateConduitExistsStatus();
            });
            //Register this conduit to the relevant network. Allows the network to determine flow direction.
            IUtilityNetworkMgr networkManager = GetNetworkManager();
            _networkItem = new FlowUtilityNetwork.NetworkItem(ConduitType, EndpointType, portCell, _parent);
            networkManager.AddToNetworks(portCell, _networkItem, EndpointType != Endpoint.Conduit);

            GetConduitManager().AddConduitUpdater(ConduitTick, FlowPriority);
            UpdateConduitExistsStatus(true);
        }

        protected override void OnCleanUp()
        {
            MultiIOExtensions.UnregisterPort(portCell);
            GetConduitManager().RemoveConduitUpdater(ConduitTick);
            GameScenePartitioner.Instance.Free(ref partitionerEntry);
            IUtilityNetworkMgr networkManager = GetNetworkManager();
            networkManager.RemoveFromNetworks(portCell, _networkItem, EndpointType != Endpoint.Conduit);
        }

    }
}