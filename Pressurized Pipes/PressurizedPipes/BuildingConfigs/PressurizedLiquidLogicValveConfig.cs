using System.Collections.Generic;
using System.Linq;
using System.Text;
using STRINGS;
using TUNING;
using UnityEngine;
//namespace PressurizedPipes
//{
//    public class PressurizedLiquidLogicValveConfig : IBuildingConfig
//    {
//        public const string ID = "AdvancedLiquidLogicValve";

//        private const ConduitType CONDUIT_TYPE = ConduitType.Liquid;

//        public override BuildingDef CreateBuildingDef()
//        {
//            BuildingDef obj = BuildingTemplates.CreateBuildingDef(construction_mass: TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER1, construction_materials: MATERIALS.REFINED_METALS, noise: NOISE_POLLUTION.NOISY.TIER1, id: "LiquidLogicValve", width: 1, height: 2, anim: "valveliquid_logic_kanim", hitpoints: 30, construction_time: 10f, melting_point: 1600f, build_location_rule: BuildLocationRule.Anywhere, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER0, temperature_modification_mass_scale: 0.2f);
//            obj.InputConduitType = ConduitType.Liquid;
//            obj.OutputConduitType = ConduitType.Liquid;
//            obj.Floodable = false;
//            obj.RequiresPowerInput = true;
//            obj.EnergyConsumptionWhenActive = 10f;
//            obj.PowerInputOffset = new CellOffset(0, 1);
//            obj.ViewMode = OverlayModes.LiquidConduits.ID;
//            obj.AudioCategory = "Metal";
//            obj.PermittedRotations = PermittedRotations.R360;
//            obj.UtilityInputOffset = new CellOffset(0, 0);
//            obj.UtilityOutputOffset = new CellOffset(0, 1);
//            obj.LogicInputPorts = new List<LogicPorts.Port>
//        {
//            LogicPorts.Port.InputPort(LogicOperationalController.PORT_ID, new CellOffset(0, 0), STRINGS.BUILDINGS.PREFABS.LIQUIDLOGICVALVE.LOGIC_PORT, STRINGS.BUILDINGS.PREFABS.LIQUIDLOGICVALVE.LOGIC_PORT_ACTIVE, STRINGS.BUILDINGS.PREFABS.LIQUIDLOGICVALVE.LOGIC_PORT_INACTIVE, true, false)
//        };
//            GeneratedBuildings.RegisterWithOverlay(OverlayScreen.LiquidVentIDs, "LiquidLogicValve");
//            return obj;
//        }

//        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
//        {
//            Object.DestroyImmediate(go.GetComponent<BuildingEnabledButton>());
//            BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
//            OperationalValve operationalValve = go.AddOrGet<OperationalValve>();
//            operationalValve.conduitType = ConduitType.Liquid;
//            operationalValve.maxFlow = 30f;
//        }

//        public override void DoPostConfigureComplete(GameObject go)
//        {
//            Object.DestroyImmediate(go.GetComponent<ConduitConsumer>());
//            Object.DestroyImmediate(go.GetComponent<ConduitDispenser>());
//            go.GetComponent<RequireInputs>().SetRequirements(true, false);
//            go.AddOrGet<LogicOperationalController>().unNetworkedValue = 0;
//            go.GetComponent<KPrefabID>().AddTag(GameTags.OverlayInFrontOfConduits, false);
//        }
//    }
//}
