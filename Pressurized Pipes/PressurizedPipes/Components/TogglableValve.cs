using System;
using System.Linq;
using Harmony;
using System.Reflection;
using UnityEngine;
using KSerialization;

namespace PressurizedPipes.Components
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class TogglableValve : Valve
    {
        //We cannot handle this toggle chore through TogglableValve because Valve already has behavior for handling its own chore.
        //There is no way reasonable for the Valve subclass to know which chore was complete if the chore was within TogglableValve.
        //Instead, create a new component to handle the chore logic.
        private class ToggleJob : Workable
        {
            public Chore chore = null;
            [MyCmpReq]
            private TogglableValve parent;
            [MyCmpGet]
            private KSelectable selectable;
            public void StartToggleChore()
            {
                Debug.Log("Start toggle chore!");
                parent.isMarkedForToggle = true;
                selectable.AddStatusItem(ToggleStatusItem);
                chore = new WorkChore<ToggleJob>(Db.Get().ChoreTypes.Toggle, this);
                Game.Instance.userMenu.Refresh(gameObject);
            }
            private static StatusItem _statusItem = null;
            private static StatusItem ToggleStatusItem
            {
                get
                {
                    if(_statusItem == null)
                    {
                        _statusItem = new StatusItem("TogglingStatus", "BUILDING", "", StatusItem.IconType.Info, NotificationType.BadMinor, false, OverlayModes.None.ID);

                    }
                    return _statusItem;
                }
            }

            public void CancelToggleChore()
            {
                Debug.Log("Cancel toggle chore!");

                parent.isMarkedForToggle = false;
                if (chore != null)
                    chore.Cancel("User cancelled");
                chore = null;
                selectable.RemoveStatusItem(ToggleStatusItem);
                Game.Instance.userMenu.Refresh(gameObject);
            }

            protected override void OnCompleteWork(Worker worker)
            {
                Debug.Log("Toggle Chore complete!");

                if (chore != null)
                {
                    chore.Cancel("Forced complete");
                }
                chore = null;
                parent.isMarkedForToggle = false;
                parent.Toggle();
                selectable.RemoveStatusItem(ToggleStatusItem);
                Game.Instance.userMenu.Refresh(gameObject);
            }

            protected override void OnPrefabInit()
            {
                base.OnPrefabInit();
                //affects how the chore can be reached. without, a dupe must stand directly inside the input location of the valve to interact with the toggle
                SetOffsetTable(OffsetGroups.InvertedStandardTable);
                base.synchronizeAnims = false;
            }

        }
        //Don't use MyCmpReq! Causes the base Valve class to not be able to retrieve the ValveBase component because it also uses MyCmpReq
        private ValveBase valveBase => gameObject.GetComponent<ValveBase>();
        [MyCmpAdd]
        private Storage storage;
        [MyCmpAdd]
        private ToggleJob toggleJob;
        [MyCmpGet]
        private BuildingHP buildingHp;
        [Serialize]
        private bool bufferMode = false;
        [Serialize]
        private bool isMarkedForToggle = false;
        private StatusItem statusItem = null;
        private Action<float> GetBaseConduitUpdate()
        {
            MethodInfo method = AccessTools.Method(typeof(ValveBase), "ConduitUpdate");
            Action<float> function = (Action<float>)Delegate.CreateDelegate(typeof(Action<float>), valveBase, method);
            return function;
        }
        private int _inputCell = -1;
        private int _outputCell = -1;
        private HandleVector<int>.Handle flowAccumulator = HandleVector<int>.InvalidHandle;
        private static FieldInfo inputCellField = AccessTools.Field(typeof(ValveBase), "inputCell");
        private static FieldInfo outputCellField = AccessTools.Field(typeof(ValveBase), "outputCell");

        public int InputCell
        {
            get
            {
                if(_inputCell < 0)
                {
                    _inputCell = (int)inputCellField.GetValue(valveBase);
                }
                return _inputCell;
            }
        }
        public int OutputCell
        {
            get
            {
                if (_outputCell < 0)
                {
                    _outputCell = (int)outputCellField.GetValue(valveBase);
                }
                return _outputCell;
            }
        }

        private static readonly EventSystem.IntraObjectHandler<TogglableValve> OnRefreshUserMenuDelegate = new EventSystem.IntraObjectHandler<TogglableValve>((Action<TogglableValve, object>)delegate (TogglableValve component, object data)
        {
            component.OnRefreshUserMenu(data);
        });

        //Update the button displayed as appropriate
        private void OnRefreshUserMenu(object data)
        {
            KIconButtonMenu.ButtonInfo button;
            string icon = "action_move_to_storage";
            string text;
            string tooltip;
            //If it is already marked for toggling, then the button should display a cancel option!
            if (!isMarkedForToggle)
            {
                if (!bufferMode)
                {
                    text = "Set Buffer";
                    tooltip = "Change the valve mode to buffer. Buffer mode will hold flow until a full packet can be sent of at least the size specified by flow limit.\nCATUION: Valve only stores one element at a time, will dump conflicting elements and become damaged in mixed element environments.";
                }
                else
                {
                    text = "Set Limit";
                    tooltip = "Change the valve mode to limit. Limit mode will behave as a standard valve, limitting the maximum flow.";
                }
            }
            else
            {
                if (!bufferMode)
                {
                    text = "Cancel Set Buffer";
                    tooltip = "Cancel the task to set this valve to Buffer mode";
                }
                else
                {
                    text = "Cancel Set Limit";
                    tooltip = "Cancel the task to set this valve to Limit mode";
                }
            }

            button = new KIconButtonMenu.ButtonInfo(icon, text, OnToggle, Action.NumActions, null, null, null, tooltip, true);
            Game.Instance.userMenu.AddButton(gameObject, button, 2f);
        }

        private void OnToggle()
        {
            //Initiate a toggle chore so that the mode can be toggled between buffer/limit.
            //Requiring a chore eliminates the user from intiating a rather drastic change instantly.
            if (!isMarkedForToggle)
                toggleJob.StartToggleChore();
            else
                toggleJob.CancelToggleChore();
        }

        private void Toggle()
        {
            bufferMode = !bufferMode;
            RefreshStatusItem();
            Game.Instance.userMenu.Refresh(gameObject);
        }

        private void RefreshStatusItem()
        {
            if (bufferMode)
            {
                if (statusItem == null)
                {
                    KSelectable selectable = GetComponent<KSelectable>();
                    statusItem = new StatusItem("BUFFERMODESTATUS", "BUILDING", "", StatusItem.IconType.Info, NotificationType.Neutral, false, OverlayModes.None.ID);
                    selectable.AddStatusItem(statusItem);
                }
            }
            else
            {
                if (statusItem != null)
                {
                    KSelectable selectable = GetComponent<KSelectable>();
                    selectable.RemoveStatusItem(statusItem);
                    statusItem = null;
                }
            }
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            RefreshStatusItem();
            //Remove the ValveBase's default ConduitUpdate and replace it with our own custom handler, so we can implement the buffer mode behavior.
            Conduit.GetFlowManager(valveBase.conduitType).RemoveConduitUpdater(GetBaseConduitUpdate());
            Conduit.GetFlowManager(valveBase.conduitType).AddConduitUpdater(ConduitUpdate);
            Subscribe((int)GameHashes.RefreshUserMenu, OnRefreshUserMenuDelegate);
            Subscribe((int)GameHashes.StatusChange, OnRefreshUserMenuDelegate);
        }
       
        protected override void OnCleanUp()
        {
            base.OnCleanUp();
            Conduit.GetFlowManager(valveBase.conduitType).RemoveConduitUpdater(ConduitUpdate);
        }
        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            //The flowAccumulator handle is set in the ValveBase OnPrefabInit.
            FieldInfo handleInfo = AccessTools.Field(typeof(ValveBase), "flowAccumulator");
            flowAccumulator = (HandleVector<int>.Handle)handleInfo.GetValue(valveBase);
        }

        private void ConduitUpdate(float dt)
        {
            //If the building is broken, nothing normally stops it from operating!
            if (gameObject.GetComponent<BuildingHP>().HitPoints == 0)
            {
                return;
            }
            ConduitFlow manager = Conduit.GetFlowManager(valveBase.conduitType);
            ConduitFlow.Conduit inputConduit = manager.GetConduit(InputCell);
            ConduitFlow.Conduit outputConduit = manager.GetConduit(OutputCell);
            if(!manager.HasConduit(InputCell) || !manager.HasConduit(OutputCell))
            {
                valveBase.UpdateAnim();
            }
            else
            {
                ConduitFlow.ConduitContents inputContents = inputConduit.GetContents(manager);
                if (!bufferMode)
                {
                    float valveFlow = valveBase.CurrentFlow * dt;
                    float maxFlow;
                    float temp;
                    SimHashes element;
                    byte diseaseIdx;
                    float ratio;
                    int disease_count;
                    bool fromStorage = false;
                    Tag storedTag = Tag.Invalid;
                    
                    if (!storage.IsEmpty())
                    {
                        //If there is still mass within the storage but we are not in buffer mode, take nothing in until the storage is emptied! (while still following the limit mode settings)
                        fromStorage = true;
                        GameObject item = storage.items.FirstOrDefault();
                        PrimaryElement storedPrimary = item.GetComponent<PrimaryElement>();
                        maxFlow = Mathf.Min(valveFlow, storedPrimary.Mass);
                        element = storedPrimary.ElementID;
                        temp = storedPrimary.Temperature;
                        diseaseIdx = storedPrimary.DiseaseIdx;
                        ratio = maxFlow / storedPrimary.Mass;
                        disease_count = (int)(ratio * (float)storedPrimary.DiseaseCount);
                        storedTag = storedPrimary.Element.tag;
                    }
                    else
                    {
                        maxFlow = Mathf.Min(inputContents.mass, valveBase.CurrentFlow * dt);
                        element = inputContents.element;
                        temp = inputContents.temperature;
                        diseaseIdx = inputContents.diseaseIdx;
                        ratio = maxFlow / inputContents.mass;
                        disease_count = (int)(ratio * (float)inputContents.diseaseCount);
                    }

                    if (maxFlow > 0f)
                    {
                        float movableMass = manager.AddElement(OutputCell, element, maxFlow, temp, diseaseIdx, disease_count);
                        Game.Instance.accumulators.Accumulate(flowAccumulator, movableMass);
                        if (movableMass > 0f)
                        {
                            //If we took the mass from storage, make sure we use the right function
                            if (!fromStorage)
                                manager.RemoveElement(InputCell, movableMass);
                            else
                                storage.ConsumeIgnoringDisease(storedTag, movableMass);
                        }
                    }
                }
                else
                {
                    float availableInput = inputContents.mass;
                    GameObject storedItem = storage.items.FirstOrDefault();
                    Element storedElement = storedItem?.GetComponent<PrimaryElement>().Element;
                    float storedMass = storedItem != null ? storedItem.GetComponent<PrimaryElement>().Mass : 0f;
                    float maxOutputCapacity = Integration.GetMaxCapacityAt(OutputCell, valveBase.conduitType);

                    //Override the set current flow if the output pipe cannot support a flow that large. This prevents the valve from storing, for example, 3KG, when it can only output 1KG at a time.
                    float minimumOutput = Mathf.Min(maxOutputCapacity, valveBase.CurrentFlow);
                    storage.capacityKg = maxOutputCapacity;
                    
                    float movableToStorage = Mathf.Min(availableInput, storage.RemainingCapacity());
                    if (movableToStorage > 0f)
                    {
                        Element inputElement = ElementLoader.FindElementByHash(inputContents.element);
                        float ratio = movableToStorage / inputContents.mass;
                        int transferredDisease = (int)((float)inputContents.diseaseCount * ratio);
                        if (inputElement == storedElement || storedItem == null)
                        {
                            if (valveBase.conduitType == ConduitType.Gas)
                                storage.AddGasChunk(inputContents.element, movableToStorage, inputContents.temperature, inputContents.diseaseIdx, transferredDisease, false);
                            else
                                storage.AddLiquid(inputContents.element, movableToStorage, inputContents.temperature, inputContents.diseaseIdx, transferredDisease, false);
                            storedMass += movableToStorage;
                            if (storedItem == null)
                                storedElement = inputElement;
                        }
                        else
                        {
                            //The input has a different element than what is in storage! Deal damage and remove however much mass attempted to flow into the valve.
                            Trigger(-794517298, new BuildingHP.DamageSourceInfo
                            {
                                damage = 1,
                                source = STRINGS.BUILDINGS.DAMAGESOURCES.BAD_INPUT_ELEMENT,
                                popString = STRINGS.UI.GAMEOBJECTEFFECTS.DAMAGE_POPS.WRONG_ELEMENT
                            });
                            SimMessages.AddRemoveSubstance(Grid.PosToCell(base.transform.GetPosition()), inputContents.element, CellEventLogger.Instance.ConduitConsumerWrongElement, movableToStorage, inputContents.temperature, inputContents.diseaseIdx, transferredDisease);
                        }
                        manager.RemoveElement(InputCell, movableToStorage);
                    }
                    
                    ConduitFlow.ConduitContents outputContents = outputConduit.GetContents(manager);
                    float initialOutputMass = outputContents.mass; 
                    Element outputElement = ElementLoader.FindElementByHash(outputContents.element);
                    //If we can create a packet of at least size CurrentFlow, including if we combined the valve's output into what is already in the output conduit
                    //Debug.Log($"[TogglableValve] InitialOut: {initialOutputMass}, StoredMass: {storedMass}, MinimumOutput: {minimumOutput}, MaxOutputCapacity: {maxOutputCapacity}, AvailableInput: {availableInput}, MovableToStorage: {movableToStorage}");
                    if(initialOutputMass + storedMass >= minimumOutput&& (storedElement == outputElement || outputElement == null || outputElement.id == SimHashes.Vacuum))
                    {
                        float movableToOut = Mathf.Min(storedMass, maxOutputCapacity - initialOutputMass);
                        if (movableToOut > 0f)
                        {
                            PrimaryElement storedPrimary = storage.items.FirstOrDefault()?.GetComponent<PrimaryElement>();
                            float ratio = movableToOut / storedMass ;
                            int transferredDisease = (int)((float)storedPrimary.DiseaseCount * ratio);
                            float totalMovedOut = manager.AddElement(OutputCell, storedPrimary.ElementID, storedMass, storedPrimary.Temperature, storedPrimary.DiseaseIdx, transferredDisease);
                            Game.Instance.accumulators.Accumulate(flowAccumulator, totalMovedOut);
                            if (totalMovedOut > 0f)
                            {
                                storage.ConsumeIgnoringDisease(storedPrimary.Element.tag, totalMovedOut);
                            }
                        }
                    }
                }
                valveBase.UpdateAnim();
            }
        }
    }
}
