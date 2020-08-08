using System;
using System.Collections.Generic;
using STRINGS;
using UnityEngine;
using KSerialization;
using System.Runtime.Serialization;
using PressurizedPipes.BuildingConfigs;
namespace PressurizedPipes.Components
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class Insulatable : Workable, IVersion
    {
        private Chore chore;
        private FetchList2 fetchList;
        public float MassToInsulate => info != null ? info.InsulateCost : -1f;
        private float BuildTime => building.Def.ConstructionTime;
        private bool CanInsulate => info != null && info.CanInsulate && MassToInsulate > 0f;
        private PressurizedInfo info => pressurized.Info;
        private bool isInsulated => primaryElement.ElementID == SimHashes.SuperInsulator;
        private float submittedNeedAmount = 0f;
        [Serialize]
        private bool isMarkedForWork = false;
        [MyCmpAdd]
        private Storage storage;
        [MyCmpGet]
        private Conduit conduit;
        [MyCmpGet]
        private PrimaryElement primaryElement;
        [MyCmpReq]
        private Building building;
        [MyCmpReq]
        private Pressurized pressurized;
        private ConduitType ConduitType => conduit.ConduitType;
        [Serialize]
        private string version = "";
        [Serialize]
        private float oldInsulationMass = -1f;
        public string Version
        {
            get
            {
                return version;
            }
            set
            {
                version = value;
            }
        }


        [OnDeserialized]
        private void OnDeserialized()
        {
            //Deserialization occurs before OnSpawn, which is when the version is updated to the most recent version.
            //Check to see if the version has changed. If it has, the mod manager will call OnVersionChange, which should determine what to do depending on what the old version was.
            PressurizedModManager.VersionChangedCheck(this);
        }
        
        public void OnVersionChange()
        {
            //If the old version was not defined from serialization, the mod previously cost 25KG/50KG to insulate gas/liquid pressurized pipes. The new cost is 400KG, but any pipes insulated before the version implementation should still
            //drop however much insulation it cost to previously insulate them.
            if(version == "")
            {
                if (isInsulated)
                {
                    if (building.Def.PrefabID == PressurizedGasConduitConfig.ID)
                        oldInsulationMass = 25f;
                    else if (building.Def.PrefabID == PressurizedLiquidConduitConfig.ID)
                        oldInsulationMass = 50f;
                    Debug.Log($"[Pressurized] Found an insualted Pressurized Pipe saved before versioning. Marking with appropriate old insulation cost: {oldInsulationMass}KG");
                }
            }
        }

        //Configure various fields related to the work, like work type, experience, animation
        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            faceTargetWhenWorking = true;
            synchronizeAnims = false;
            workerStatusItem = Db.Get().DuplicantStatusItems.Building;
            attributeConverter = Db.Get().AttributeConverters.ConstructionSpeed;
            attributeExperienceMultiplier = TUNING.DUPLICANTSTATS.ATTRIBUTE_LEVELING.MOST_DAY_EXPERIENCE;
            minimumAttributeMultiplier = 0.75f;
            skillExperienceSkillGroup = Db.Get().SkillGroups.Building.Id;
            skillExperienceMultiplier = TUNING.SKILLS.MOST_DAY_EXPERIENCE;
            multitoolContext = "build";
            multitoolHitEffectTag = EffectConfigs.BuildSplashId;
            workingPstComplete = null;
            workingPstFailed = null;
            Building building = GetComponent<Building>();
            CellOffset[][] table = OffsetGroups.InvertedStandardTable;
            SetOffsetTable(OffsetGroups.BuildReachabilityTable(building.Def.PlacementOffsets, table, building.Def.ConstructionOffsetFilter));
        }

        //Subscribe listeners as needed
        protected override void OnSpawn()
        {
            base.OnSpawn();
            PressurizedModManager.SetVersion(this);
            Subscribe((int)GameHashes.RefreshUserMenu, OnRefreshUserMenuDelegate);
            Subscribe((int)GameHashes.StatusChange, OnRefreshUserMenuDelegate);
            Subscribe((int)GameHashes.DeconstructComplete, OnDeconstruct);
            if (isMarkedForWork)
            {
                if (!isInsulated)
                {
                    if (!FetchComplete())
                        SetFetchList();
                    else
                        onFetchComplete();
                }
                else
                    OnRemoveInsulate();
            }
        }

        private static readonly EventSystem.IntraObjectHandler<Insulatable> OnRefreshUserMenuDelegate = new EventSystem.IntraObjectHandler<Insulatable>((Action<Insulatable, object>)delegate (Insulatable component, object data)
        {
            component.OnRefreshUserMenu(data);
        });

        private bool FetchComplete()
        {
            PrimaryElement element = storage.FindPrimaryElement(SimHashes.SuperInsulator);
            return element != null && element.Mass >= MassToInsulate;
        }
        private float StoredInsulation()
        {
            PrimaryElement element = storage.FindPrimaryElement(SimHashes.SuperInsulator);
            return element == null ? 0f : element.Mass;
        }

        //Update the button displayed as appropriate
        private void OnRefreshUserMenu(object data)
        {
            if (!CanInsulate)
                return;
            KIconButtonMenu.ButtonInfo button;
            if (gameObject.GetComponent<PrimaryElement>().ElementID != SimHashes.SuperInsulator)
            {
                if (chore == null && fetchList == null)
                    button = new KIconButtonMenu.ButtonInfo("action_move_to_storage", "Insulate", OnInsulate, Action.NumActions, null, null, null, $"Add {UI.FormatAsLink("Insulation", "SUPERINSULATOR")} to the pipe.\n\nPrevents contents from transferring heat to the pipe and its surroundings.\nUses {MassToInsulate.ToString("0")}KG of solid {UI.FormatAsLink("Insulation", "SUPERINSULATOR")}.", true);
                else
                    button = new KIconButtonMenu.ButtonInfo("action_move_to_storage", "Cancel Insulate", OnCancel, Action.NumActions, null, null, null, "Cancel this order to Insulate", true);
            }
            else
            {
                if (chore == null && fetchList == null)
                    button = new KIconButtonMenu.ButtonInfo("action_deconstruct", "Remove Insultation", OnRemoveInsulate, Action.NumActions, null, null, null, $"Remove {UI.FormatAsLink("Insulation", "SUPERINSULATOR")} from this pipe.\nWill return Insulation that was used in this pipe.");
                else
                    button = new KIconButtonMenu.ButtonInfo("action_deconstruct", "Cancel Deinsulation", OnCancel, Action.NumActions, null, null, null, "Cancel this order to remove Insulation", true);
            }
            Game.Instance.userMenu.AddButton(gameObject, button, 2f);
        }

        //Cancel the current activity
        private void OnCancel()
        {

            if (chore == null && fetchList == null)
                return;
            if (fetchList != null)
            {
                fetchList.Cancel("Cancelled by user");
                fetchList = null;
                DropStorage();
                MaterialNeeds.Instance.UpdateNeed(ElementLoader.FindElementByHash(SimHashes.SuperInsulator).tag, 0 - MassToInsulate);
            }
            if (chore != null)
            {
                this.chore.Cancel("Cancelled by user");
                this.chore = null;
                this.ShowProgressBar(false);
                this.workTimeRemaining = this.workTime;
                DropStorage();
            }
            Prioritizable.RemoveRef(gameObject);
            isMarkedForWork = false;
        }

        private void OnRemoveInsulate()
        {
            if (!CanInsulate)
                return;
            chore = GetRemoveChore();
            isMarkedForWork = true;
            Prioritizable.AddRef(gameObject);
        }

        private void OnInsulate()
        {
            if (!CanInsulate)
                return;
            if (DebugHandler.InstantBuildMode)
            {
                OnCompleteWork(null);
                return;
            }
            this.SetWorkTime(BuildTime);
            isMarkedForWork = true;
            //Create the fetch chore to bring the required insulation to the conduit
            SetFetchList();
        }

        private void SetFetchList()
        {
            fetchList = new FetchList2(storage, Db.Get().ChoreTypes.BuildFetch);
            Tag insulation = ElementLoader.FindElementByHash(SimHashes.SuperInsulator).tag;
            float massNeeded = MassToInsulate - StoredInsulation();
            fetchList.Add(insulation, null, null, MassToInsulate);
            MaterialNeeds.Instance.UpdateNeed(insulation, massNeeded);
            submittedNeedAmount = massNeeded;
            fetchList.Submit(onFetchComplete, true);
            Prioritizable.AddRef(gameObject);
        }

        private void onFetchComplete()
        {
            //The required insulation has been gathered. Create the chore to insulate the conduit
            MaterialNeeds.Instance.UpdateNeed(ElementLoader.FindElementByHash(SimHashes.SuperInsulator).tag, 0 - submittedNeedAmount);
            submittedNeedAmount = 0f;
            chore = GetInsulateChore();
            fetchList = null;
        }
        private Chore GetInsulateChore()
        {
            return new WorkChore<Insulatable>(Db.Get().ChoreTypes.Build, this, null, true, null, null, null, true, null, false, false);
        }

        private Chore GetRemoveChore()
        {
            return new WorkChore<Insulatable>(Db.Get().ChoreTypes.Deconstruct, this);
        }

        private void Insulate()
        {
            //The insulation build chore has been complete. Ensure the storage is empty (consuming the insulation inside) and update the simulation to reflect the insulation of the conduit
            chore = null;
            //The completion of the chore does not necessarily consume the insulation that was stored. Setting mass to 0 effectively removes them from storage.
            storage.ConsumeAllIgnoringDisease();
            foreach (GameObject item in storage.items)
                item.GetComponent<PrimaryElement>().Mass = 0f;
            //Override the private variable _Element. The variable acts as a cache when the PrimaryElement.Element is retrieved, and will not notice that ElementID has been changed unless _Element is null
            Harmony.AccessTools.Field(typeof(PrimaryElement), "_Element").SetValue(primaryElement, null);
            primaryElement.ElementID = SimHashes.SuperInsulator;
            ForceRebuild();
            ForceSimUpdate();
            chore = null;
            Prioritizable.RemoveRef(gameObject);
            Game.Instance.userMenu.Refresh(gameObject);
            isMarkedForWork = false;
        }

      

        private void RemoveInsulate()
        {
            Harmony.AccessTools.Field(typeof(PrimaryElement), "_Element").SetValue(primaryElement, null);
            primaryElement.ElementID = SimHashes.Steel;
            ForceRebuild();
            ForceSimUpdate();
            SpawnItemsFromRemoval();
            this.chore = null;
            Prioritizable.RemoveRef(gameObject);
            Game.Instance.userMenu.Refresh(gameObject);
            isMarkedForWork = false;
        }

        private void SpawnItemsFromRemoval()
        {
            //Drop the appropriate amount of insulation material for when insulation has been removed, or when an insulated conduit has been deconstructed
            GameObject go = null;
            int cell = Grid.PosToCell(gameObject);
            Element element = ElementLoader.FindElementByHash(SimHashes.SuperInsulator);
            float insulationMass;
            if (oldInsulationMass > 0f)
            {
                Debug.Log($"[Pressurized] Pressurized Pipe had insulation removed, but was marked for an old insulation mass. Mass returned: {oldInsulationMass}KG");
                insulationMass = oldInsulationMass;
                oldInsulationMass = 0f;
            }
            else
            {
                insulationMass = MassToInsulate;
            }
            go = element.substance.SpawnResource(Grid.CellToPosCBC(cell, Grid.SceneLayer.Ore), insulationMass, primaryElement.Temperature, primaryElement.DiseaseIdx, primaryElement.DiseaseCount, false, false, false);
            TransformExtensions.SetPosition(go.transform, TransformExtensions.GetPosition(go.transform) + (Vector3.up * 0.5f));
            GameComps.Fallers.Add(go, Vector2.zero);
        }

        private void DropStorage()
        {
            //Drop the storage for the chore (which should only contain insulation) when the chore/fetch is cancelled
            List<GameObject> items = storage.items;
            if (items.Count == 0)
                return;
            GameObject go = items[0];
            storage.Drop(go);
            TransformExtensions.SetPosition(go.transform, Grid.CellToPosCBC(Grid.PosToCell(gameObject), Grid.SceneLayer.Ore));
            GameComps.Fallers.Add(go, Vector2.zero);
        }

        private void ForceRebuild()
        {
            //Rebuild the conduit flow network, so that conduit contents transfer no heat.
            ConduitFlow manager = conduit.GetFlowManager();
            if (manager == null)
            {
                Debug.LogError($"[Pressurized] Could not retrieve the conduit flow manager for conduit type: {ConduitType}");
                return;
            }
            //Without forcing the rebuild, the conduit system will not know that this conduit is now made out of insulation. Rebuilds normally occur when conduits are built/destroyed
            manager.ForceRebuildNetworks();
        }

        private void ForceSimUpdate()
        {
            //Update the building temperature simulation, so that the conduit as a building itself transfers no heat.
            //Without this, heat will transfer to/from the conduit's surroudnings from/to the conduit itself (without affecting the conduit contents)
            StructureTemperatureComponents mgr = GameComps.StructureTemperatures;
            HandleVector<int>.Handle handle = mgr.GetHandle(gameObject);
            //Payload contains information such as the building, temperature, and the element the building is made out of
            StructureTemperaturePayload payload = mgr.GetPayload(handle);
            //In this instance, the InternalTemperature represents what the temperature of the building was when the save was first loaded. Temperature reprsents the current temperature
            payload.primaryElement.InternalTemperature = this.primaryElement.Temperature;
            object[] parameters = { payload };
            //The simulation will normally ignore that the payload had its primary element changed except when the game first runs after loading a save.
            //Force the simulation to update its information by invoking UpdateSimState.
            Harmony.AccessTools.Method(typeof(StructureTemperatureComponents), "UpdateSimState").Invoke(GameComps.StructureTemperatures, parameters);
        }

        public override float GetWorkTime()
        {
            return BuildTime;
        }

        protected override void OnStartWork(Worker worker)
        {
            if (isInsulated)
                progressBar.barColor = ProgressBarsConfig.Instance.GetBarColor("DeconstructBar");
            else
                progressBar.barColor = ProgressBarsConfig.Instance.GetBarColor("ProgressBar");

        }

        protected override void OnCompleteWork(Worker worker)
        {
            if (isInsulated)
                RemoveInsulate();
            else
                Insulate();

        }

        private void OnDeconstruct(object data)
        {
            //If a conduit is deconstructed while insulated, the default behavior will only return what was used in the original build recipe.
            //Drop the insulation used in the conduit on deconstruction if the conduit is insulated.
            if (isInsulated)
            {
                SpawnItemsFromRemoval();
            }
        }
    }
}
