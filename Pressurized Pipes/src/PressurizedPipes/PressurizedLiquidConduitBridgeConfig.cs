using UnityEngine;
using TUNING;

namespace PressurizedPipes
{
    public class PressurizedLiquidConduitBridgeConfig : IBuildingConfig
    {
        public const string ID = "PressurizedLiquidConduitBridge";

        private const ConduitType CONDUIT_TYPE = ConduitType.Liquid;

        public override BuildingDef CreateBuildingDef()
        {
            string id = ID;
            int width = 3;
            int height = 1;
            string anim = "utilityliquidbridge_pressurized_kanim";
            int hitpoints = 10;
            float construction_time = 45f;
            float[] tIER = { BUILDINGS.CONSTRUCTION_MASS_KG.TIER1[0], BUILDINGS.CONSTRUCTION_MASS_KG.TIER1[0] }; //50KG, 50KG
            string[] constructionMaterial = { SimHashes.Steel.ToString(), MATERIALS.PLASTIC };
            float melting_point = 1600f;
            BuildLocationRule build_location_rule = BuildLocationRule.Conduit;
            EffectorValues nONE = NOISE_POLLUTION.NONE;
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(id, width, height, anim, hitpoints, construction_time, tIER, constructionMaterial, melting_point, build_location_rule, BUILDINGS.DECOR.PENALTY.TIER1, nONE);
            buildingDef.ObjectLayer = ObjectLayer.LiquidConduitConnection;
            buildingDef.SceneLayer = Grid.SceneLayer.LiquidConduitBridges;
            buildingDef.InputConduitType = ConduitType.Liquid;
            buildingDef.OutputConduitType = ConduitType.Liquid;
            buildingDef.Floodable = false;
            buildingDef.Entombable = false;
            buildingDef.Overheatable = false;
            buildingDef.ViewMode = OverlayModes.LiquidConduits.ID;
            buildingDef.AudioCategory = "Metal";
            buildingDef.AudioSize = "small";
            buildingDef.BaseTimeUntilRepair = -1f;
            buildingDef.PermittedRotations = PermittedRotations.R360;
            buildingDef.UtilityInputOffset = new CellOffset(-1, 0);
            buildingDef.UtilityOutputOffset = new CellOffset(1, 0);
            buildingDef.ThermalConductivity = .0537f;
            GeneratedBuildings.RegisterWithOverlay(OverlayScreen.LiquidVentIDs, ID);
            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            GeneratedBuildings.MakeBuildingAlwaysOperational(go);
            BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
            ConduitBridge conduitBridge = go.AddOrGet<ConduitBridge>();
            conduitBridge.type = ConduitType.Liquid;
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            Object.DestroyImmediate(go.GetComponent<RequireInputs>());
            Object.DestroyImmediate(go.GetComponent<ConduitConsumer>());
            Object.DestroyImmediate(go.GetComponent<ConduitDispenser>());
        }
    }
}
