using System.Reflection;
using Harmony;

namespace PortableBattery
{
    public class PortableBattery : Battery
    {
        [MyCmpGet]
        private Building building;

        [MyCmpReq]
        private Placeable placeable;

        [MyCmpGet]
        private Pickupable pickupable;

        protected override void OnSpawn()
        {
            Debug.Log($"[PortableBattery] Spawning portable battery");
            base.OnSpawn();
            placeable.OnTake = amount => this.OnTake();
            placeable.OnPlaced = this.OnPlace;
        }

        //When picked up, or toggled into a pickupable state. Remove the battery from the circuit manager, and set it to a disconnected state.
        //Remove the battery from the object layer.
        private Pickupable OnTake()
        {
            Debug.Log("$[PortableBattery] OnTake triggered for PortableBattery");
            if (!placeable.IsPlaced)
                return pickupable;
            this.SetConnectionStatus(CircuitManager.ConnectionStatus.NotConnected);
            Game.Instance.circuitManager.Disconnect(this);
            this.RemoveTag(GameTags.Stored);
            return pickupable;
        }

        //When relocated to a new place. Set the PowerCell variable to the new location. Set the status to unpowered (instead of disconnected) and reconnect the battery to the circuit manager. The game will handle determining the necessary state.
        private void OnPlace(int target)
        {
            this.AddTag(GameTags.Stored);
            PropertyInfo powerCellSetter = AccessTools.Property(typeof(Battery), "PowerCell");
            powerCellSetter.SetValue(this as Battery, building.GetPowerInputCell(), null);
            this.SetConnectionStatus(CircuitManager.ConnectionStatus.Unpowered);
            Game.Instance.circuitManager.Connect(this);
        }
    }
}
