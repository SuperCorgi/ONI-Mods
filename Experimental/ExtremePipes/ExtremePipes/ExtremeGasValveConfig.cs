using TUNING;
using UnityEngine;
using HighPressurePipes;
namespace ExtremePipes
{
    public class ExtremeGasValveConfig : IBuildingConfig
    {
        public const string ID = "ExtremeGasValve";

        private const ConduitType CONDUIT_TYPE = ConduitType.Gas;

        public override BuildingDef CreateBuildingDef()
        {
            string id = ID;
            int width = 1;
            int height = 2;
            string anim = "valvegas_kanim";
            int hitpoints = 30;
            float construction_time = 90f;
            float[] tIER = { BUILDINGS.CONSTRUCTION_MASS_KG.TIER3[0], BUILDINGS.CONSTRUCTION_MASS_KG.TIER3[0] }; //200KG, 200KG
            string[] constructionMaterial = { SimHashes.Steel.ToString(), MATERIALS.PLASTIC };
            float melting_point = 1600f;
            BuildLocationRule build_location_rule = BuildLocationRule.Anywhere;
            EffectorValues tIER2 = NOISE_POLLUTION.NOISY.TIER1;
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(id, width, height, anim, hitpoints, construction_time, tIER, constructionMaterial, melting_point, build_location_rule, BUILDINGS.DECOR.PENALTY.TIER2, tIER2);
            buildingDef.InputConduitType = CONDUIT_TYPE;
            buildingDef.OutputConduitType = CONDUIT_TYPE;
            buildingDef.Floodable = false;
            buildingDef.ViewMode = OverlayModes.GasConduits.ID;
            buildingDef.AudioCategory = "Metal";
            buildingDef.PermittedRotations = PermittedRotations.R360;
            buildingDef.UtilityInputOffset = new CellOffset(0, 0);
            buildingDef.UtilityOutputOffset = new CellOffset(0, 1);
            buildingDef.RequiresPowerInput = true;
            buildingDef.PowerInputOffset = new CellOffset(0, 0);
            buildingDef.EnergyConsumptionWhenActive = 30f;
            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            GeneratedBuildings.MakeBuildingAlwaysOperational(go);
            BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
            ValveBase valveBase = go.AddOrGet<ValveBase>();
            valveBase.conduitType = CONDUIT_TYPE;
            valveBase.maxFlow = 10f;
            valveBase.animFlowRanges = new ValveBase.AnimRangeInfo[3]
            {
            new ValveBase.AnimRangeInfo((10f * 0.25f), "lo"),
            new ValveBase.AnimRangeInfo((10f * 0.5f), "med"),
            new ValveBase.AnimRangeInfo((10f * 0.75f), "hi")
            };
            go.AddOrGet<Valve>();
            Tintable tint = go.AddOrGet<Tintable>();
            Tintable.AddToTintTable(ID, new Color32(255, 40, 40, 255));
            Workable workable = go.AddOrGet<Workable>();
            workable.workTime = 5f;
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            Object.DestroyImmediate(go.GetComponent<RequireInputs>());
            Object.DestroyImmediate(go.GetComponent<ConduitConsumer>());
            Object.DestroyImmediate(go.GetComponent<ConduitDispenser>());
            go.AddOrGet<BuildingComplete>().isManuallyOperated = true;
        }
    }
}
