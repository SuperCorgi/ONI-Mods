﻿using TUNING;
using UnityEngine;
using PressurizedPipes.Components;
namespace PressurizedPipes.BuildingConfigs
{
    public class PressurizedGasValveConfig : IBuildingConfig
    {
        public const string ID = "PressurizedGasValve";

        private const ConduitType CONDUIT_TYPE = ConduitType.Gas;

        public override BuildingDef CreateBuildingDef()
        {
            string id = ID;
            int width = 1;
            int height = 2;
            string anim = "valvegas_advanced_kanim";
            int hitpoints = 30;
            float construction_time = 30f;
            float[] tIER = { BUILDINGS.CONSTRUCTION_MASS_KG.TIER1[0], BUILDINGS.CONSTRUCTION_MASS_KG.TIER1[0] }; //50KG, 50KG
            string[] constructionMaterial = { SimHashes.Steel.ToString(), MATERIALS.PLASTIC };
            float melting_point = 1600f;
            BuildLocationRule build_location_rule = BuildLocationRule.Anywhere;
            EffectorValues tIER2 = NOISE_POLLUTION.NOISY.TIER1;
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(id, width, height, anim, hitpoints, construction_time, tIER, constructionMaterial, melting_point, build_location_rule, BUILDINGS.DECOR.PENALTY.TIER1, tIER2);
            buildingDef.InputConduitType = CONDUIT_TYPE;
            buildingDef.OutputConduitType = CONDUIT_TYPE;
            buildingDef.Floodable = false;
            buildingDef.ViewMode = OverlayModes.GasConduits.ID;
            buildingDef.AudioCategory = "Metal";
            buildingDef.PermittedRotations = PermittedRotations.R360;
            buildingDef.UtilityInputOffset = new CellOffset(0, 0);
            buildingDef.UtilityOutputOffset = new CellOffset(0, 1);
            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            GeneratedBuildings.MakeBuildingAlwaysOperational(go);
            BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
            ValveBase valveBase = go.AddOrGet<ValveBase>();
            valveBase.conduitType = CONDUIT_TYPE;
            //Begin with a maxFlow of 1KG as if the pressurized integration was not a part of this building. The proper flow will be set through Harmony on spawn.
            valveBase.maxFlow = 3f;
            valveBase.animFlowRanges = new ValveBase.AnimRangeInfo[3]
            {
            new ValveBase.AnimRangeInfo((0.25f * 3f), "lo"),
            new ValveBase.AnimRangeInfo((0.5f * 3f), "med"),
            new ValveBase.AnimRangeInfo((0.75f * 3f), "hi")
            };
            go.AddOrGet<TogglableValve>();
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
