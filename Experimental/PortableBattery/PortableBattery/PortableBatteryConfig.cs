using UnityEngine;
using TUNING;

namespace PortableBattery
{
    public class PortableBatteryConfig : BaseBatteryConfig
    {
        public const string ID = "PortableBattery";


        public override BuildingDef CreateBuildingDef()
        {
            BuildingDef result = CreateBuildingDef(ID, 1, 1, 30, "gas_element_sensor_kanim", 5f, new float[] { 100f }, MATERIALS.REFINED_METALS, 1600f, 1f, 1f, BUILDINGS.DECOR.PENALTY.TIER0, NOISE_POLLUTION.NOISY.TIER1);
            result.SceneLayer = Grid.SceneLayer.Building;
            SoundEventVolumeCache.instance.AddVolume("batterysm_kanim", "Battery_rattle", NOISE_POLLUTION.NOISY.TIER1);
            return result;
        }
        public override void DoPostConfigureComplete(GameObject go)
        {
            go.AddOrGet<Placeable>();
            PortableBattery battery = go.AddOrGet<PortableBattery>();
            battery.capacity = 10000f;
            battery.joulesLostPerSecond = 2f;
        }
    }
}
