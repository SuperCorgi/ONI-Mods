using System.Collections.Generic;
using TUNING;
using UnityEngine;

namespace HighPressurePipes
{
    public class HighPressureGasResevoirConfig : IBuildingConfig
    {



        public const string ID = "PressurizedGasReservoir";

        private const ConduitType CONDUIT_TYPE = ConduitType.Gas;

        private const int WIDTH = 5;

        private const int HEIGHT = 3;

        public static readonly List<Storage.StoredItemModifier> ReservoirStoredItemModifiers = new List<Storage.StoredItemModifier>
        {
            Storage.StoredItemModifier.Hide,
            Storage.StoredItemModifier.Seal
        };

        public override BuildingDef CreateBuildingDef()
        {
            string id = ID;
            int width = 5;
            int height = 3;
            string kanim = "gasstorage_kanim";
            int hitpoints = 100;
            float construction_time = 200f;
            float[] construction_mass = { BUILDINGS.CONSTRUCTION_MASS_KG.TIER3[0], BUILDINGS.CONSTRUCTION_MASS_KG.TIER3[0] };
            string[] construction_material = { SimHashes.Steel.ToString(), MATERIALS.PLASTIC };
            float meltingPoint = 800f;
            BuildLocationRule buildRule = BuildLocationRule.OnFloor;
            EffectorValues decor = BUILDINGS.DECOR.PENALTY.TIER4;
            EffectorValues noise = NOISE_POLLUTION.NOISY.TIER0;
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(id, width, height, kanim, hitpoints, construction_time, construction_mass, construction_material, meltingPoint, buildRule, decor, noise);
            buildingDef.InputConduitType = ConduitType.Gas;
            buildingDef.OutputConduitType = ConduitType.Gas;
            buildingDef.Floodable = false;
            buildingDef.ViewMode = OverlayModes.GasConduits.ID;
            buildingDef.AudioCategory = "HollowMetal";
            buildingDef.UtilityInputOffset = new CellOffset(1, 2);
            buildingDef.UtilityOutputOffset = new CellOffset(0, 0);
            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.AddOrGet<Reservoir>();
            Storage storage = BuildingTemplates.CreateDefaultStorage(go);
            storage.showDescriptor = true;
            storage.storageFilters = STORAGEFILTERS.GASES;
            storage.capacityKg = 150f;
            storage.SetDefaultStoredItemModifiers(ReservoirStoredItemModifiers);
            ConduitConsumer conduitConsumer = go.AddOrGet<ConduitConsumer>();
            conduitConsumer.conduitType = ConduitType.Gas;
            conduitConsumer.ignoreMinMassCheck = true;
            conduitConsumer.forceAlwaysSatisfied = true;
            conduitConsumer.alwaysConsume = true;
            conduitConsumer.capacityKG = storage.capacityKg;
            ConduitDispenser conduitDispenser = go.AddOrGet<ConduitDispenser>();
            conduitDispenser.conduitType = ConduitType.Gas;
            conduitDispenser.elementFilter = null;
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.AddOrGetDef<StorageController.Def>();
        }

    }
}
