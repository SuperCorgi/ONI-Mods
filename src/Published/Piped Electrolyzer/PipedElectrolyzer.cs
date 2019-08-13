using UnityEngine;
using KSerialization;
using System.Runtime.Serialization;
using System.Reflection;
namespace PipedElectrolyzer
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class PipedElectrolyzer : StateMachineComponent<PipedElectrolyzer.StatesInstance>
    {
        //[OnSerialized]
        //public void OnSerialized()
        //{
        //    SerializationTemplate template = Manager.GetSerializationTemplate(typeof(PipedElectrolyzer));
        //    foreach(SerializationTemplate.SerializationField f in template.serializableFields)
        //    {
        //        Debugger.AddMessage($"The following field is in the template: {f.field.Name}");
        //    }
        //    Debugger.AddMessage("PipedElectrolyzer was serialized");
        //}

        public class StatesInstance : GameStateMachine<States, StatesInstance, PipedElectrolyzer, object>.GameInstance
        {
            public StatesInstance(PipedElectrolyzer smi)
                : base(smi)
            {

            }
        }

        public class States : GameStateMachine<States, StatesInstance, PipedElectrolyzer>
        {
            public State off;

            public State on;

            public State converting;

            public State overpressure;

            public override void InitializeStates(out BaseState default_state)
            {
                default_state = off;
                root.EventTransition(GameHashes.OperationalChanged, off, (StatesInstance smi) => !smi.master.operational.IsOperational).EventHandler(GameHashes.OnStorageChange, delegate (StatesInstance smi)
                {
                    smi.master.UpdateMeter();
                });


                off.EventTransition(GameHashes.OperationalChanged, on, (StatesInstance smi) => smi.master.operational.IsOperational);


                on.Enter("Waiting", (StatesInstance smi) =>
                {
                    smi.master.operational.SetActive(false, false);
                }).EventTransition(GameHashes.OnStorageChange, converting, (StatesInstance smi) => smi.master.converter.HasEnoughMassToStartConverting());


                converting.Enter("Ready", delegate (StatesInstance smi)
                {
                    smi.master.operational.SetActive(true, false);
                }).Transition(on, (StatesInstance smi) => !smi.master.converter.CanConvertAtAll(), UpdateRate.SIM_200ms).Transition(overpressure, (StatesInstance smi) => !smi.master.RoomForPressure, UpdateRate.SIM_200ms);



                overpressure.Enter("OverPressure", (StatesInstance smi) =>
                {
                    smi.master.operational.SetActive(false, false);
                }).Transition(converting, (StatesInstance smi) => smi.master.RoomForPressure, UpdateRate.SIM_200ms).ToggleStatusItem(Db.Get().BuildingStatusItems.PressureOk, null);


            }
        }

        private bool RoomForPressure
        {
            get
            {
                int cell = Grid.PosToCell(base.transform.GetPosition());
                cell = Grid.CellAbove(cell);
                return !GameUtil.FloodFillCheck(OverPressure, this, cell, 3, true, true);
            }
        }

        [SerializeField]
        public float maxMass = 2.5f;

        [SerializeField]
        public bool hasMeter = true;

        [MyCmpAdd]
        private Storage storage;

        [MyCmpGet]
        private ElementConverter converter;

        [MyCmpReq]
        private Operational operational;

        private MeterController meter;

        protected override void OnSpawn()
        {
            KBatchedAnimController component = this.GetComponent<KBatchedAnimController>();
            if (hasMeter)
                meter = new MeterController((KAnimControllerBase)component, "U2H_meter_target", "meter", Meter.Offset.Behind, Grid.SceneLayer.NoLayer, new Vector3(-0.4f, 0.5f, -0.1f), new string[4]
                {
        "U2H_meter_target",
        "U2H_meter_tank",
        "U2H_meter_waterbody",
        "U2H_meter_level"
                });
            smi.StartSM();
            UpdateMeter();

            Tutorial.Instance.oxygenGenerators.Add(base.gameObject);
        }

        protected override void OnCleanUp()
        {
            Tutorial.Instance.oxygenGenerators.Remove(base.gameObject);
            base.OnCleanUp();
        }

        public void UpdateMeter()
        {
            if (hasMeter)
            {
                float positionPercent = Mathf.Clamp01(storage.MassStored() / storage.capacityKg);
                meter.SetPositionPercent(positionPercent);
            }
        }

        private static bool OverPressure(int cell, PipedElectrolyzer electrolyzer)
        {
            return Grid.Mass[cell] > electrolyzer.maxMass;
        }
    }
}
