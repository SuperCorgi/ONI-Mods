using UnityEngine;

namespace PortableBattery
{
    public class PlaceTool : DragTool
    {
        [SerializeField]
        private TextStyleSetting tooltipStyle;

        private GameObject previewObject;

        private Placeable source;

        private ToolTip tooltip;

        public static PlaceTool Instance;

        private bool active;

        [MyCmpAdd]
        BuildToolHoverTextCard hoverText;

        public static void DestroyInstance()
        {
            Instance = null;

        }

        protected override void OnPrefabInit()
        {
            Instance = this;
            tooltip = GetComponent<ToolTip>();
        }

        protected override void OnActivateTool()
        {
            active = true;
            base.OnActivateTool();
            visualizer = GameUtil.KInstantiate(previewObject, Grid.SceneLayer.Front, null, LayerMask.NameToLayer("Place"));
            KBatchedAnimController component = visualizer.GetComponent<KBatchedAnimController>();
            if ((Object)component != (Object)null)
            {
                component.visibilityType = KAnimControllerBase.VisibilityType.Always;
                component.isMovable = true;
            }
            visualizer.SetActive(true);
            ShowToolTip();
            //GetComponent<BuildToolHoverTextCard>().currentDef = null;
            ResourceRemainingDisplayScreen.instance.ActivateDisplay(visualizer);
            if ((Object)component == (Object)null)
            {
                visualizer.SetLayerRecursively(LayerMask.NameToLayer("Place"));
            }
            else
            {
                component.SetLayer(LayerMask.NameToLayer("Place"));
            }
            GridCompositor.Instance.ToggleMajor(true);
        }

        protected override void OnDeactivateTool(InterfaceTool new_tool)
        {
            active = false;
            GridCompositor.Instance.ToggleMajor(false);
            HideToolTip();
            ResourceRemainingDisplayScreen.instance.DeactivateDisplay();
            Object.Destroy(visualizer);
            KMonoBehaviour.PlaySound(GlobalAssets.GetSound(GetDeactivateSound(), false));
            base.OnDeactivateTool(new_tool);
        }

        public void Activate(Placeable source, GameObject previewObject)
        {
            this.source = source;
            this.previewObject = previewObject;

            this.GetComponent<BuildToolHoverTextCard>().currentDef = source.GetComponent<Building>().Def;
            PlayerController.Instance.ActivateTool(this);
        }

        public void Deactivate()
        {
            SelectTool.Instance.Activate();
            source = null;
            previewObject = null;
            ResourceRemainingDisplayScreen.instance.DeactivateDisplay();
        }

        protected override void OnDragTool(int cell, int distFromOrigin)
        {
            if (!((Object)visualizer == (Object)null))
            {
                if (!IsValidPositionForDef(Grid.CellToPos(cell, CellAlignment.Bottom, source.building.Def.SceneLayer)))
                {
                    Debug.Log($"[PortableBattery] Attempted to relocate to an invalid position!");
                    return;
                }
                if (DebugHandler.InstantBuildMode)
                {
                    source.Place(cell);
                }
                else
                {
                    source.QueuePlacement(cell);
                }
                Deactivate();
            }
        }

        protected override Mode GetMode()
        {
            return Mode.Brush;
        }

        private void ShowToolTip()
        {
            ToolTipScreen.Instance.SetToolTip(tooltip);
        }

        private void HideToolTip()
        {
            ToolTipScreen.Instance.ClearToolTip(tooltip);
        }

        public void Update()
        {
            if (active)
            {
                KBatchedAnimController component = visualizer.GetComponent<KBatchedAnimController>();
                if ((Object)component != (Object)null)
                {
                    component.SetLayer(LayerMask.NameToLayer("Place"));
                }
            }
        }

        public override void OnMouseMove(Vector3 cursorPos)
        {
            base.OnMouseMove(cursorPos);
            UpdateVis(cursorPos);
        }

        private void UpdateVis(Vector3 pos)
        {
            bool isValid = IsValidPositionForDef(pos);
            if (visualizer != null)
            {
                Color c = isValid ? Color.white : Color.red;
                KBatchedAnimController controller = visualizer.GetComponent<KBatchedAnimController>();
                if (controller != null)
                    controller.TintColour = c;
                Vector3 vector = Grid.CellToPosCBC(Grid.PosToCell(pos), source.building.Def.SceneLayer);
                visualizer.transform.SetPosition(vector);
                transform.SetPosition(vector - (Vector3.up * 0.5f));
            }
        }

        private bool IsValidPositionForDef(Vector3 pos)
        {
            string fail_reason;
            return source.building.Def.IsValidPlaceLocation(visualizer, pos, source.building.Orientation, out fail_reason);
        }

        public override string GetDeactivateSound()
        {
            return "HUD_Click_Deselect";
        }
    }
}
