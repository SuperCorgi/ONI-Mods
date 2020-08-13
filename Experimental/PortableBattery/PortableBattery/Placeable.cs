using KSerialization;
using STRINGS;
using UnityEngine;

namespace PortableBattery
{
    [SerializationConfig(MemberSerialization.OptIn)]
    [AddComponentMenu("KMonoBehaviour/scripts/Placeable")]
    public class Placeable : KMonoBehaviour
    {

        //public class MakePickupable : Workable
        //{
        //    protected override void OnSpawn()
        //    {
        //        base.OnSpawn();
        //        this.showProgressBar = true;
        //        this.alwaysShowProgressBar = false;
        //        SetWorkTime(1f);
        //        synchronizeAnims = false;
        //    }
        //}
        [MyCmpReq]
        private KPrefabID prefabId;
        [MyCmpReq]
        public Building building;
        [MyCmpAdd]
        private Pickupable pickupable;
        [MyCmpGet]
        private BuildingCellVisualizer bcv;
        [Serialize]
        private int targetCell = -1;
        [Serialize]
        private bool isPlaced = true;
        public bool IsPlaced
        {
            get
            {
                return isPlaced;
            }
            private set
            {
                isPlaced = value;
            }
        }
        public GameObject previewPrefab;
        private GameObject preview;
        private Chore chore;
        private System.Guid statusItem;
        public System.Action<int> OnPlaced;
        public System.Action OnQueuePlacement;
        public System.Func<float, Pickupable> OnTake;
        public bool ShouldTintWhenMoving = true;

        private static readonly EventSystem.IntraObjectHandler<Placeable> OnRefreshUserMenuDelegate = new EventSystem.IntraObjectHandler<Placeable>(delegate (Placeable component, object data)
        {
            component.OnRefreshUserMenu(data);
        });

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            Subscribe((int)GameHashes.RefreshUserMenu, OnRefreshUserMenuDelegate);
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            prefabId.AddTag(new Tag(prefabId.InstanceID.ToString()), false);
            previewPrefab = building.Def.BuildingPreview;
            GetComponent<Clearable>().isClearable = false;

            if (targetCell != -1)
                QueuePlacement(targetCell);
            if (IsPlaced)
                this.AddTag(GameTags.Stored);
        }

        protected override void OnCleanUp()
        {
            if ((Object)preview != (Object)null)
                preview.DeleteObject();
            base.OnCleanUp();
        }
        private StatusItem GetPendingRelocateStatus()
        {
            return new StatusItem("PendingRelocate", "BUILDING", "status_item_pending_upgrade", StatusItem.IconType.Custom, NotificationType.BadMinor, false, OverlayModes.None.ID);
        }

        //Initiate the relocation process
        public void QueuePlacement(int target)
        {
            targetCell = target;
            Vector3 position = Grid.CellToPosCBC(targetCell, Grid.SceneLayer.Front);
            if ((Object)preview == (Object)null)
            {
                preview = GameUtil.KInstantiate(previewPrefab, position, Grid.SceneLayer.Front, null, 0);
                preview.AddOrGet<Storage>();
                preview.SetActive(true);
            }
            else
            {
                preview.transform.SetPosition(position);
            }
            if (chore != null)
                chore.Cancel("new target");

            if (OnQueuePlacement != null)
                OnQueuePlacement.Invoke();
            pickupable.OnTake = amount => onTake();
            pickupable.OnTake += OnTake;
            chore = GetFetchChore();
        }

        private Pickupable onTake()
        {
            if (!isPlaced)
                return pickupable;

            building.Def.UnmarkArea(Grid.PosToCell(this), building.Orientation, building.Def.ObjectLayer, this.gameObject);
            Grid.Objects[Grid.PosToCell(building), (int)ObjectLayer.Pickupables] = null;
            bcv.DisableIcons();
            statusItem = this.GetComponent<KSelectable>().AddStatusItem(GetPendingRelocateStatus());
            isPlaced = false;

            if (ShouldTintWhenMoving)
            {
                var animController = GetComponent<KBatchedAnimController>();
                animController.TintColour = FilteredStorage.NO_FILTER_TINT;
            }
            RequiresFoundation foundations = GameComps.RequiresFoundations;
            if (foundations.GetHandle(this.gameObject) != HandleVector<int>.Handle.InvalidHandle)
            {
                foundations.Remove(this.gameObject);
                Operational operational = GetComponent<Operational>();
                if (operational != null)
                {
                    operational.SetFlag(RequiresFoundation.solidFoundation, true);
                    GetComponent<KSelectable>().ToggleStatusItem(Db.Get().BuildingStatusItems.MissingFoundation, false, null);
                }

            }
            return pickupable;
        }

        private void OnChoreComplete(Chore completed_chore)
        {
            Place(targetCell);
        }

        public void Place(int target)
        {
            if (preview != null)
                preview.DeleteObject();

            isPlaced = true;
            base.gameObject.transform.SetPosition(Grid.CellToPos(target, CellAlignment.Bottom, Grid.SceneLayer.Building));
            building.Def.MarkArea(Grid.PosToCell(this), building.Orientation, building.Def.ObjectLayer, this.gameObject);
            targetCell = -1;
            this.GetComponent<KSelectable>().RemoveStatusItem(statusItem);
            statusItem = System.Guid.Empty;
            this.bcv.DrawIcons(OverlayScreen.Instance.GetMode());
            ForceSimUpdate();
            BuildLocationRule rule = building.Def.BuildLocationRule;

            if (rule == BuildLocationRule.OnFloor || rule == BuildLocationRule.OnFoundationRotatable)
                GameComps.RequiresFoundations.Add(this.gameObject);

            if (ShouldTintWhenMoving)
            {
                var animController = GetComponent<KBatchedAnimController>();
                animController.TintColour = Color.white;
            }

            if (OnPlaced != null)
                OnPlaced.Invoke(target);
        }

        //The simulation for temperature exchange has the location of this building cached, as does the building itself.
        //Use RefreshCells to update the buildings cache, and pass the payload back to the UpdateSimState method to force a refresh in the simulation
        private void ForceSimUpdate()
        {
            StructureTemperatureComponents mgr = GameComps.StructureTemperatures;
            HandleVector<int>.Handle handle = mgr.GetHandle(gameObject);
            StructureTemperaturePayload payload = mgr.GetPayload(handle);
            Building building = GetComponent<Building>();
            building.RefreshCells();
            payload.building = this.GetComponent<Building>();
            object[] parameters = { payload };
            Harmony.AccessTools.Method(typeof(StructureTemperatureComponents), "UpdateSimState").Invoke(GameComps.StructureTemperatures, parameters);
        }

        private void ForceFoundationCheckUpdate()
        {

        }

        private FetchChore GetFetchChore()
        {
            return new FetchChore(Db.Get().ChoreTypes.Fetch, preview.GetComponent<Storage>(), 1f, new Tag[1]
            {
            new Tag(prefabId.InstanceID.ToString())
            }, null, null, null, true, OnChoreComplete, null, null, FetchOrder2.OperationalRequirement.None, 0);
        }

        //Active the place tool.
        private void OpenPlaceTool()
        {
            PlaceTool.Instance.Activate(this, previewPrefab);
        }

        private void OnRefreshUserMenu(object data)
        {
            KIconButtonMenu.ButtonInfo button = (targetCell == -1) ? new KIconButtonMenu.ButtonInfo("action_deconstruct", UI.USERMENUACTIONS.RELOCATE.NAME, OpenPlaceTool, Action.NumActions, null, null, null, UI.USERMENUACTIONS.RELOCATE.TOOLTIP, true) : new KIconButtonMenu.ButtonInfo("action_deconstruct", UI.USERMENUACTIONS.RELOCATE.NAME_OFF, CancelRelocation, Action.NumActions, null, null, null, UI.USERMENUACTIONS.RELOCATE.TOOLTIP_OFF, true);
            Game.Instance.userMenu.AddButton(base.gameObject, button, 1f);
        }

        private void CancelRelocation()
        {
            if ((Object)preview != (Object)null)
            {
                preview.DeleteObject();
                preview = null;
            }
            targetCell = -1;
        }

    }
}
