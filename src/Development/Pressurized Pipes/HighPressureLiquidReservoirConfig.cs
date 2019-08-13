using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using TUNING;
namespace HighPressurePipes
{
    class HighPressureLiquidReservoirConfig : IBuildingConfig
    {
        public const string ID = "PressurizedLiquidReservoir";

        private const ConduitType CONDUIT_TYPE = ConduitType.Liquid;

        private const int WIDTH = 2;

        private const int HEIGHT = 3;

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
            buildingDef.InputConduitType = ConduitType.Liquid;
            buildingDef.OutputConduitType = ConduitType.Liquid;
            buildingDef.Floodable = false;
            buildingDef.ViewMode = OverlayModes.LiquidConduits.ID;
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
            storage.allowItemRemoval = false;
            storage.storageFilters = STORAGEFILTERS.LIQUIDS;
            storage.capacityKg = 5000f;
            storage.SetDefaultStoredItemModifiers(GasReservoirConfig.ReservoirStoredItemModifiers);
            ConduitConsumer conduitConsumer = go.AddOrGet<ConduitConsumer>();
            conduitConsumer.conduitType = ConduitType.Liquid;
            conduitConsumer.ignoreMinMassCheck = true;
            conduitConsumer.forceAlwaysSatisfied = true;
            conduitConsumer.alwaysConsume = true;
            conduitConsumer.capacityKG = storage.capacityKg;
            ConduitDispenser conduitDispenser = go.AddOrGet<ConduitDispenser>();
            conduitDispenser.conduitType = ConduitType.Liquid;
            conduitDispenser.elementFilter = null;
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.AddOrGetDef<StorageController.Def>();
        }
    }
}
