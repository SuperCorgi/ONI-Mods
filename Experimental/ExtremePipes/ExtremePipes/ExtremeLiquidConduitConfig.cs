using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TUNING;
using UnityEngine;
using HighPressurePipes;
namespace ExtremePipes
{
    class ExtremeLiquidConduitConfig : IBuildingConfig
    {
        public const string ID = "ExtremeLiquidConduit";

        public override BuildingDef CreateBuildingDef()
        {
            string id = ID;
            int width = 1;
            int height = 1;
            string anim = "utilities_liquid_radiant_kanim";
            int hitpoints = 10;
            float construction_time = 90f;
            float[] tIER = { BUILDINGS.CONSTRUCTION_MASS_KG.TIER3[0], BUILDINGS.CONSTRUCTION_MASS_KG.TIER3[0] }; //200KG, 200KG
            string[] constructionMaterial = { SimHashes.Steel.ToString(), MATERIALS.PLASTIC };
            float melting_point = 1600f;
            BuildLocationRule build_location_rule = BuildLocationRule.Anywhere;
            EffectorValues nONE = NOISE_POLLUTION.NONE;
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(id, width, height, anim, hitpoints, construction_time, tIER, constructionMaterial, melting_point, build_location_rule, BUILDINGS.DECOR.PENALTY.TIER1, nONE);
            buildingDef.Floodable = false;
            buildingDef.Overheatable = false;
            buildingDef.Entombable = false;
            buildingDef.ViewMode = OverlayModes.LiquidConduits.ID;
            buildingDef.ThermalConductivity = .0537f;
            buildingDef.ObjectLayer = ObjectLayer.LiquidConduit;
            buildingDef.TileLayer = ObjectLayer.LiquidConduitTile;
            buildingDef.ReplacementLayer = ObjectLayer.ReplacementLiquidConduit;
            buildingDef.AudioCategory = "Metal";
            buildingDef.AudioSize = "small";
            buildingDef.BaseTimeUntilRepair = -1f;
            buildingDef.UtilityInputOffset = new CellOffset(0, 0);
            buildingDef.UtilityOutputOffset = new CellOffset(0, 0);
            buildingDef.SceneLayer = Grid.SceneLayer.LiquidConduits;
            buildingDef.isKAnimTile = true;
            buildingDef.isUtility = true;
            buildingDef.DragBuild = true;
            buildingDef.ReplacementTags = new List<Tag>();
            buildingDef.ReplacementTags.Add(GameTags.Pipes);
            GeneratedBuildings.RegisterWithOverlay(OverlayScreen.LiquidVentIDs, ID);
            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            GeneratedBuildings.MakeBuildingAlwaysOperational(go);
            BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
            Conduit conduit = go.AddOrGet<Conduit>();
            conduit.type = ConduitType.Liquid;
            go.AddOrGet<Tintable>();
            Tintable.AddToTintTable(ID, new Color32(255, 0, 0, 255));
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.GetComponent<Building>().Def.BuildingUnderConstruction.GetComponent<Constructable>().isDiggingRequired = false;
            go.AddComponent<EmptyConduitWorkable>();
            KAnimGraphTileVisualizer kAnimGraphTileVisualizer = go.AddComponent<KAnimGraphTileVisualizer>();
            kAnimGraphTileVisualizer.connectionSource = KAnimGraphTileVisualizer.ConnectionSource.Liquid;
            kAnimGraphTileVisualizer.isPhysicalBuilding = true;
            go.GetComponent<KPrefabID>().AddTag(GameTags.Pipes);
            LiquidConduitConfig.CommonConduitPostConfigureComplete(go);
        }

        public override void DoPostConfigureUnderConstruction(GameObject go)
        {
            KAnimGraphTileVisualizer kAnimGraphTileVisualizer = go.AddComponent<KAnimGraphTileVisualizer>();
            kAnimGraphTileVisualizer.connectionSource = KAnimGraphTileVisualizer.ConnectionSource.Liquid;
            kAnimGraphTileVisualizer.isPhysicalBuilding = false;
        }
    }
}
