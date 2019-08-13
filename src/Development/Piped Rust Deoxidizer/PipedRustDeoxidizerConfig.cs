using TUNING;
using UnityEngine;
using MultiIO;
namespace PipedRustDeoxidizer
{
    public class PipedRustDeoxidizerConfig : IBuildingConfig
    {
        public const string ID = "PipedRustDeoxidizer";
        private const float RUST_KG_CONSUMPTION_RATE = 0.75f;
        private const float SALT_KG_CONSUMPTION_RATE = 0.25f;
        private const float RUST_KG_PER_REFILL = 585f;
        private const float SALT_KG_PER_REFILL = 195f;
        private const float TOTAL_CONSUMPTION_RATE = 1f;
        private const float IRON_CONVERSION_RATIO = 0.4f;
        private const float OXYGEN_CONVERSION_RATIO = 0.57f;
        private const float CHLORINE_CONVERSION_RATIO = 0.03f;
        private const float POWER_CONSUMPTION = 0f;
        private const float EXHAUST_HEAT = 0.25f;
        private const float SELF_HEAT = 1.125f;
        public const float OXYGEN_TEMPERATURE = 348.15f;

        public override BuildingDef CreateBuildingDef()
        {
            string id = ID;
            int width = 2;
            int height = 3;
            string anim = "rust_deoxidizer_kanim";
            int hitpoints = 30;
            float construction_time = 40f;
            float[] material_mass = new float[]
            {
                BUILDINGS.CONSTRUCTION_MASS_KG.TIER3[0], //Refined Metal -> 200KG
                BUILDINGS.CONSTRUCTION_MASS_KG.TIER2[0]  //Plastic -> 100KG
            };
            string[] construction_material = new string[]
            {
                MATERIALS.REFINED_METALS[0],
                MATERIALS.PLASTICS[0]
            };
            float melting_point = 800f;
            BuildLocationRule build_location_rule = BuildLocationRule.OnFloor;
            EffectorValues tieR3_2 = NOISE_POLLUTION.NOISY.TIER3;
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(id, width, height, anim, hitpoints, construction_time, material_mass, construction_material, melting_point, build_location_rule, BUILDINGS.DECOR.PENALTY.TIER1, tieR3_2, 0.2f);
            buildingDef.RequiresPowerInput = true;
            buildingDef.PowerInputOffset = new CellOffset(1, 0);
            buildingDef.EnergyConsumptionWhenActive = POWER_CONSUMPTION;
            buildingDef.ExhaustKilowattsWhenActive = EXHAUST_HEAT;
            buildingDef.SelfHeatKilowattsWhenActive = SELF_HEAT;
            buildingDef.ViewMode = OverlayModes.Oxygen.ID;
            buildingDef.AudioCategory = "HollowMetal";


            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery, false);
            go.AddOrGet<PipedRustDeoxidizer>().maxMass = 1.8f;

            Storage storage = go.AddOrGet<Storage>();
            storage.SetDefaultStoredItemModifiers(Storage.StandardSealedStorage);
            storage.showInUI = true;

            MultiOutput multiOut = go.AddOrGet<MultiOutput>();
            SimHashes[] filter = new SimHashes[1] { SimHashes.Oxygen };
            multiOut.AddOutputPort(ConduitType.Gas, new CellOffset(1, 1), PortIconColors.Oxygen, false, filter, false);
            filter = new SimHashes[1] { SimHashes.ChlorineGas };
            multiOut.AddOutputPort(ConduitType.Gas, new CellOffset(0, 1), PortIconColors.ChlorineGas, false, filter, false);

            //MultiInput multiIn = go.AddOrGet<MultiInput>();
            //InputPort in1 = multiIn.AddInputPort(ConduitType.Gas, new CellOffset(1, 0), PortIconColors.NaturalGas, 1f, 2f, new Tag("Methane"), true);
            //InputPort in2 = multiIn.AddInputPort(ConduitType.Gas, new CellOffset(0, 0), PortIconColors.Hydrogen, 1f, 3f, new Tag("Hydrogen"), true);
            //in1.RequiresConnection = false;

            ManualDeliveryKG manualDeliveryKG = go.AddOrGet<ManualDeliveryKG>();
            manualDeliveryKG.SetStorage(storage);
            manualDeliveryKG.requestedItemTag = new Tag("Rust");
            manualDeliveryKG.capacity = 585f;
            manualDeliveryKG.refillMass = 193.05f;
            manualDeliveryKG.choreTypeIDHash = Db.Get().ChoreTypes.FetchCritical.IdHash;
            ManualDeliveryKG manualDeliveryKG2 = go.AddComponent<ManualDeliveryKG>();
            manualDeliveryKG2.SetStorage(storage);
            manualDeliveryKG2.requestedItemTag = new Tag("Salt");
            manualDeliveryKG2.capacity = 195f;
            manualDeliveryKG2.refillMass = 64.3500061f;
            manualDeliveryKG2.allowPause = true;
            manualDeliveryKG2.choreTypeIDHash = Db.Get().ChoreTypes.FetchCritical.IdHash;

            ElementConverter elementConverter = go.AddOrGet<ElementConverter>();
            //elementConverter.consumedElements = new ElementConverter.ConsumedElement[2]
            //{
            //    new ElementConverter.ConsumedElement(new Tag("Methane"), 0.1f),
            //    new ElementConverter.ConsumedElement(new Tag("Hydrogen"), 0.1f)
            //};
            elementConverter.consumedElements = new ElementConverter.ConsumedElement[2]
            {
                new ElementConverter.ConsumedElement(new Tag("Rust"), RUST_KG_CONSUMPTION_RATE),
                new ElementConverter.ConsumedElement(new Tag("Salt"), SALT_KG_CONSUMPTION_RATE)
            };

            elementConverter.outputElements = new ElementConverter.OutputElement[3]
            {
                new ElementConverter.OutputElement(1.8f, SimHashes.Oxygen, OXYGEN_TEMPERATURE, false, true, 0.0f, 1f, 1f, byte.MaxValue, 0),
                new ElementConverter.OutputElement(1f, SimHashes.ChlorineGas, OXYGEN_TEMPERATURE, false, true, 0.0f, 1f, 1f, byte.MaxValue, 0),
                new ElementConverter.OutputElement(IRON_CONVERSION_RATIO, SimHashes.IronOre, OXYGEN_TEMPERATURE, false, true, 0.0f, 1f, 1f, byte.MaxValue, 0)
            };
            //elementConverter.outputElements = new ElementConverter.OutputElement[3]
            //{
            //    new ElementConverter.OutputElement(OXYGEN_CONVERSION_RATIO, SimHashes.Oxygen, OXYGEN_TEMPERATURE, false, true, 0.0f, 1f, 1f, byte.MaxValue, 0),
            //    new ElementConverter.OutputElement(CHLORINE_CONVERSION_RATIO, SimHashes.ChlorineGas, OXYGEN_TEMPERATURE, false, true, 0.0f, 1f, 1f, byte.MaxValue, 0),
            //    new ElementConverter.OutputElement(IRON_CONVERSION_RATIO, SimHashes.IronOre, OXYGEN_TEMPERATURE, false, true, 0.0f, 1f, 1f, byte.MaxValue, 0)
            //};
            ElementDropper elementDropper = go.AddComponent<ElementDropper>();
            elementDropper.emitMass = 24f;
            elementDropper.emitTag = SimHashes.IronOre.CreateTag();
            elementDropper.emitOffset = new Vector3(0.0f, 1f, 0.0f);
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
            go.AddOrGet<BuildingNotificationButton>();
        }
    }
}
