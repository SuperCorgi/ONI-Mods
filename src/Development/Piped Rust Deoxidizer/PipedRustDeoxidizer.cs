using KSerialization;
using System.Collections.Generic;
using UnityEngine;

namespace PipedRustDeoxidizer
{
    public class PipedRustDeoxidizer : StateMachineComponent<PipedRustDeoxidizer.StatesInstance>
    {
        public class StatesInstance : GameStateMachine<States, StatesInstance, PipedRustDeoxidizer, object>.GameInstance
        {
            public StatesInstance(PipedRustDeoxidizer smi)
                : base(smi)
            {

            }
        }

        //public class State : GameStateMachine<States, StatesInstance, PipedRustDeoxidizer, object>.State
        //{

        //}


        public class States : GameStateMachine<States, StatesInstance, PipedRustDeoxidizer>
        {

            public State off;
            public State on;
            public State converting;
            public State overpressure;

            public override void InitializeStates(out BaseState default_state)
            {
                default_state = this.off;

                this.root.EventTransition(GameHashes.OperationalChanged, this.off, (smi => !smi.master.operational.IsOperational));
                this.off.EventTransition(GameHashes.OperationalChanged, this.on, (smi => smi.master.operational.IsOperational));
                this.on.EventTransition(GameHashes.OnStorageChange, this.converting, (smi => smi.master.converter.CanConvertAtAll()));
                this.converting.Enter("Ready", (smi => smi.master.operational.SetActive(true))).
                    EventTransition(GameHashes.OnStorageChange, on, smi => !ShouldConvertAtAll(smi)).
                    Exit("Ready", (smi => smi.master.operational.SetActive(false)));
            }
        }

        private static bool ShouldConvertAtAll(StatesInstance smi)
        {
            bool canConvert = smi.master.converter.CanConvertAtAll();
            return canConvert;
            //Storage storage = smi.master.storage;
            //List<GameObject> items = storage.items;
            //foreach(GameObject item in items)
            //{
            //    PrimaryElement element = item.GetComponent<PrimaryElement>();
            //    if(element != null && element.Mass > 1f)
            //    {
            //        if (element.ElementID == SimHashes.Oxygen || element.ElementID == SimHashes.ChlorineGas)
            //        {
            //            return false;
            //        }
            //    }
            //}
            //return true;
        }

        [SerializeField]
        public float maxMass = 2.5f;
        [MyCmpAdd]
        private Storage storage;
        [MyCmpGet]
        private ElementConverter converter;
        [MyCmpReq]
        private Operational operational;
        private MeterController meter;

        protected override void OnSpawn()
        {
            this.smi.StartSM();
            Tutorial.Instance.oxygenGenerators.Add(this.gameObject);
        }
        protected override void OnCleanUp()
        {
            Tutorial.Instance.oxygenGenerators.Remove(this.gameObject);
            base.OnCleanUp();
        }

        private bool RoomForPressure
        {
            get
            {
                int start_cell = Grid.CellAbove(Grid.PosToCell(this.transform.GetPosition()));
                return !GameUtil.FloodFillCheck<PipedRustDeoxidizer>(OverPressure, this, start_cell, 3, true, true);
            }
        }

        private static bool OverPressure(int cell, PipedRustDeoxidizer rustDeoxidizer)
        {
            return (double)Grid.Mass[cell] > rustDeoxidizer.maxMass;
        }

    }
}
