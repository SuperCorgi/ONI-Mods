using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MultiIO
{
    public static class MultiIOExtensions
    {
        private static Dictionary<int, ConduitIO> portCache = new Dictionary<int, ConduitIO>();

        internal static void RegisterPort(int cell, ConduitIO port)
        {
            if (portCache.ContainsKey(cell))
            {
                Debug.LogWarning($"[MultiIO] MultiIOExtensions.RegisterPort() -> Port registered with a cell that was already cached. Replacing entry.");
                portCache[cell] = port;
            }
            else
            {
                portCache.Add(cell, port);
            }
        }
        internal static void UnregisterPort(int cell)
        {
            if (portCache.ContainsKey(cell))
            {
                portCache.Remove(cell);
            }
            else
            {
                Debug.LogWarning($"[MultiIO] MultiIOExtensions.UnregisterPort() -> Attempted to unregister port for a cell that was not in the cache.");
            }
        }


        public static List<ConduitIO> GetAllPortsFromObject(GameObject obj)
        {
            IEnumerable<ConduitIO> conduits = new List<ConduitIO>();
            List<OutputPort> multiOutputList = obj?.GetComponent<MultiOutput>()?.PortList;
            if (multiOutputList?.Count > 0)
            {
                conduits = conduits.Concat(multiOutputList.Cast<ConduitIO>());
            }
            List<InputPort> multiInputList = obj?.GetComponent<MultiInput>()?.PortList;
            if (multiInputList?.Count > 0)
            {
                conduits = conduits.Concat(multiInputList.Cast<ConduitIO>());
            }
            return conduits.ToList();
        }

        public static ConduitIO GetPortAt(GameObject obj, int cell)
        {
            if (portCache.ContainsKey(cell))
            {
                ConduitIO port = portCache[cell];
                if (port.transform.parent == obj.transform)
                    return port;
            }
            return null;
        }

        public static bool Intersects(ConduitIO port, int origin, Orientation orientation)
        {

            return false;
        }
    }
}
