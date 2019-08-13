using TUNING;
using UnityEngine;

namespace PipedElectrolyzer
{
    public class PipedElectrolyzerConfig : IBuildingConfig
    {
        public const string ID = "PipedElectrolyzer";

        public const float WATER2OXYGEN_RATIO = 0.888f;

        public const float OXYGEN_TEMPERATURE = 343.15f;

        public override BuildingDef CreateBuildingDef()
        {
            string id = ID;
            int width = 2;
            int height = 2;
            string anim = "electrolyzer_kanim";
            int hitpoints = 30;
            float construction_time = 30f;
            float[] tier = BUILDINGS.CONSTRUCTION_MASS_KG.TIER3;
            string[] all_metals = MATERIALS.ALL_METALS;
            float melting_point = 800f;
            BuildLocationRule build_location_rule = BuildLocationRule.OnFloor;
            EffectorValues tier2 = NOISE_POLLUTION.NOISY.TIER3;

            BuildingDef def = BuildingTemplates.CreateBuildingDef(id, width, height, anim, hitpoints, construction_time, tier, all_metals, melting_point, build_location_rule, BUILDINGS.DECOR.PENALTY.TIER1, tier2, 0.2f);
            def.RequiresPowerInput = true;
            def.PowerInputOffset = new CellOffset(1, 0);
            def.EnergyConsumptionWhenActive = 480f;
            def.ExhaustKilowattsWhenActive = 0.5f;
            def.SelfHeatKilowattsWhenActive = 1.5f;
            def.ViewMode = OverlayModes.Oxygen.ID;
            def.AudioCategory = "HollowMetal";
            def.InputConduitType = ConduitType.Liquid;
            def.UtilityInputOffset = new CellOffset(0, 0);
            def.OutputConduitType = ConduitType.Gas;
            def.UtilityOutputOffset = new CellOffset(1, 1);
            return def;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
            PipedElectrolyzer electrolyzer = go.AddOrGet<PipedElectrolyzer>();
            electrolyzer.maxMass = 1.8f;
            electrolyzer.hasMeter = true;

            ConduitConsumer conduitConsumer = go.AddOrGet<ConduitConsumer>();
            conduitConsumer.conduitType = ConduitType.Liquid;
            conduitConsumer.consumptionRate = 1f;
            conduitConsumer.capacityTag = ElementLoader.FindElementByHash(SimHashes.Water).tag;
            conduitConsumer.wrongElementResult = ConduitConsumer.WrongElementResult.Dump;

            ConduitDispenser dispenser = go.AddOrGet<ConduitDispenser>();
            dispenser.conduitType = ConduitType.Gas;
            dispenser.invertElementFilter = false;
            dispenser.elementFilter = new SimHashes[1]
            {
                SimHashes.Oxygen
            };

            Storage storage = go.AddOrGet<Storage>();
            storage.capacityKg = 2f;
            storage.showInUI = true;

            ElementConverter elementConverter = go.AddOrGet<ElementConverter>();
            elementConverter.consumedElements = new ElementConverter.ConsumedElement[1]
            {
            new ElementConverter.ConsumedElement(new Tag("Water"), 1f)
            };

            elementConverter.outputElements = new ElementConverter.OutputElement[2]
            {
            new ElementConverter.OutputElement(WATER2OXYGEN_RATIO, SimHashes.Oxygen, OXYGEN_TEMPERATURE, true, 0f, 1f, false, 1f, byte.MaxValue, 0),
            new ElementConverter.OutputElement(0.111999989f, SimHashes.Hydrogen, OXYGEN_TEMPERATURE, false, 0f, 1f, false, 1f, byte.MaxValue, 0)
            };

            Prioritizable.AddRef(go);
        }

        public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
        {
            GeneratedBuildings.RegisterLogicPorts(go, LogicOperationalController.INPUT_PORTS_1_1);
        }

        public override void DoPostConfigureUnderConstruction(GameObject go)
        {
            GeneratedBuildings.RegisterLogicPorts(go, LogicOperationalController.INPUT_PORTS_1_1);
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            GeneratedBuildings.RegisterLogicPorts(go, LogicOperationalController.INPUT_PORTS_1_1);
            go.AddOrGet<LogicOperationalController>();
            go.AddOrGetDef<PoweredActiveController.Def>();
        }
    }
}
