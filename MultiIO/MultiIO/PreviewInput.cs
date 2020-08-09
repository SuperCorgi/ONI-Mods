using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiIO
{
    public class PreviewInput : InputPort
    {
        //As a preview input, there should be no defined behavior for this method! The OnSpawn override should not call this anyway.
        protected override void ConduitTick(float delta)
        {
            return;
        }

        //As a preview input, there should be no defined behavior for this method! The OnSpawn override should not call this anyway.
        public override void UpdateConduitExistsStatus(bool force = false)
        {
            return;
        }

        //This preview input can constantly change positions, while a standard InputPort is not expected to change.
        public override int GetPortCell()
        {
            CellOffset roatedOffset = building.GetRotatedOffset(CellOffset);
            int bottomLeftCell = Grid.PosToCell(building.transform.GetPosition());
            portCell = Grid.OffsetCell(bottomLeftCell, roatedOffset);
            return portCell;
        }


        protected override void OnSpawn()
        {
            //_spawned = true;
            Debug.Log($"[MultiIO] PreviewInput.OnSpawn() -> ConduitType: {ConduitType.ToString()}  CellOffset: {CellOffset.x + "," + CellOffset.y}");
            portCell = GetPortCell();
            //MultiIOExtensions.RegisterPort(portCell, this);
            //Register an event listener for any changes to the grid at the location of this port.
            //ScenePartitionerLayer layer = GameScenePartitioner.Instance.objectLayers[(int)GetConduitObjectLayer()];
            //partitionerEntry = GameScenePartitioner.Instance.Add("ConduitIO.OnSpawn", _parent, portCell, layer, delegate
            //{
            //    UpdateConduitExistsStatus();
            //});
            //Register this conduit to the relevant network. Allows the network to determine flow direction.
            //IUtilityNetworkMgr networkManager = GetNetworkManager();
            //_networkItem = new FlowUtilityNetwork.NetworkItem(ConduitType, EndpointType, portCell, _parent);
            //networkManager.AddToNetworks(portCell, _networkItem, EndpointType != Endpoint.Conduit);

            //if (UseConduitUpdater)
            //{
            //    GetConduitManager().AddConduitUpdater(ConduitTick, FlowPriority);
            //}

            //UpdateConduitExistsStatus(true);
        }
    }
}
