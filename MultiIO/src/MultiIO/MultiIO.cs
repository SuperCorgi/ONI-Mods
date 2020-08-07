using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KSerialization;
namespace MultiIO
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public abstract class MultiIO<T> : KMonoBehaviour, ISaveLoadable where T : ConduitIO
    {
        /// <summary>
        /// A list containing all ports associated to this instance. This instance is tied as the parent GameObject to the ports.
        /// </summary>
        public List<T> PortList {
            get
            {
                if (_dirtyPortList)
                {
                    _portList = GetComponentsInChildren<T>().ToList();
                    _dirtyPortList = false;
                }
                return _portList;
            }
        }
        private List<T> _portList = new List<T>();
        private bool _dirtyPortList = true;

        protected int PortCount
        {
            get
            {
                return count;
            }
        }

        [SerializeField]
        protected int count = 0;


        [MyCmpReq]
        protected Operational operational;

        [MyCmpReq]
        protected Storage storage;


        protected T AddIOPort(ConduitType type, CellOffset offset, Color iconColor)
        {
            GameObject obj = new GameObject();
            T comp = obj.AddComponent<T>();
            comp.ConduitType = type;
            comp.CellOffset = offset;
            comp.IconColor = iconColor;
            //Allows the port component to be retrievable by searching Components in children. Better practice than having the multiples of the component attached to one GameObject
            comp.transform.parent = this.transform;
            _dirtyPortList = true;
            return comp;
        }


        protected override void OnSpawn()
        {
            base.OnSpawn();
        }
        protected override void OnCleanUp()
        {
            base.OnCleanUp();
        }

        private ObjectLayer GetConduitObjectLayer(ConduitType type)
        {
            if (type == ConduitType.Gas)
                return ObjectLayer.GasConduit;
            if (type == ConduitType.Liquid)
                return ObjectLayer.LiquidConduit;
            if (type == ConduitType.Solid)
                return ObjectLayer.SolidConduit;
            Debug.LogWarning($"[MultiIO] Could not find ObjectLayer for current ConduitType. Is the ConduitType None?");
            return ObjectLayer.GasConduit;
        }
        //In essence, marks all the locations on a layer specific to each port so that other ports cannot be placed in the same location. 
        //(i.e. A gas bridge's input/output cannot be in the same cell of one of these ports if they are a gas port)
        internal void MarkAreas(int cell, Orientation orientation, GameObject go, Action<GameObject, GameObject> overlapCallback)
        {
            List<T> ports = PortList;
            foreach (T port in ports)
            {
                ObjectLayer layer = Grid.GetObjectLayerForConduitType(port.ConduitType);
                CellOffset rotatedOffset = Rotatable.GetRotatedCellOffset(port.CellOffset, orientation);
                int portCell = Grid.OffsetCell(cell, rotatedOffset);
                overlapCallback(Grid.Objects[portCell, (int)layer], go);
                Grid.Objects[portCell, (int)layer] = go;
            }

        }
        //Frees space for the other ports to be placed
        internal void UnmarkAreas(int cell, Orientation orientation, GameObject go, Action<GameObject, GameObject> overlapCallback)
        {
            List<T> ports = PortList;
            foreach (T port in ports)
            {
                ObjectLayer layer = Grid.GetObjectLayerForConduitType(port.ConduitType); //Grid.GetObjectLayerForConduitType(port.ConduitType)
                CellOffset offset = Rotatable.GetRotatedCellOffset(port.CellOffset, orientation);
                int portCell = Grid.OffsetCell(cell, offset);
                if (Grid.Objects[portCell, (int)layer] == go)
                    Grid.Objects[portCell, (int)layer] = null;
            }
        }
        /// <summary>
        /// Returns a ConduitType list with the type of all ports associated to this instance.
        /// </summary>
        public List<ConduitType> GetPortTypes()
        {
            return PortList.Select(x => x.ConduitType).ToList();
        }
        /// <summary>
        /// Returns a list of all the (integer) cells for the ports associated to this instance.
        /// </summary>
        public List<int> GetPortCells()
        {
            List<int> cells = new List<int>();
            foreach (T port in PortList)
            {
                int mCell = port.GetPortCell();
                if (mCell != -1)
                    cells.Add(mCell);
            }
            return cells;
        }
        /// <summary>
        /// Returns the port located at the specified cell if there is one, otherwise returns null.
        /// </summary>
        /// <param name="cell">The integer location that the cell can found at on the Grid.</param>
        public T GetPortAt(int cell)
        {
            foreach (T d in PortList)
            {
                if (d.GetPortCell() == cell)
                    return d;
            }
            return null;
        }
    }
}
