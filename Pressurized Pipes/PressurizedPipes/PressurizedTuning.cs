using System.Collections.Generic;
using UnityEngine;
using PressurizedPipes.BuildingConfigs;
namespace PressurizedPipes
{
    public class PressurizedInfo
    {   
        public float Capacity;
        public float IncreaseMultiplier;
        public bool IsDefault;
        public Color32 OverlayTint; //The tint applied to the overlay appearance of the sprite
        public Color32 FlowTint; //The tint applied to the flowing sprites within the conduits
        public Color32 FlowOverlayTint; //The tint tint applied to the overlay appearance of the flowing sprites
        public bool CanInsulate = false;
        public float InsulateCost = -1f;
    }

    public static class PressurizedTuning
    {
        public static PressurizedInfo GetPressurizedInfo(string id)
        {
            if (PressurizedLookup.ContainsKey(id))
                return PressurizedLookup[id];
            else
                return PressurizedLookup[""];
        }

        public static bool TryAddPressurizedInfo(string id, PressurizedInfo info)
        {
            if (PressurizedLookup.ContainsKey(id))
            {
                Debug.LogWarning($"[Pressurized] PressurizedTuning.TryAddPressurizedInfo(string, PressurizedInfo) -> Attempted to add an id that already exists.");
                return false;
            }
            else if (info == null || info.IsDefault == true || info.Capacity <= 0f)
            {
                Debug.LogWarning($"[Pressurized] PressurizedTuning.TryAddPressurizedInfo(string, PressurizedInfo) -> PressurizedInfo argument was invalid. Must not be null, have a Capacity > 0, and IsDefault must be false.");
                return false;
            }
            PressurizedLookup.Add(id, info);
            return true;
        }

        private static Dictionary<string, PressurizedInfo> PressurizedLookup = new Dictionary<string, PressurizedInfo>()
        {
            {
                PressurizedGasConduitConfig.ID,
                new PressurizedInfo()
                {
                    Capacity = 3f,
                    IncreaseMultiplier = 3f,
                    OverlayTint = new Color32(201, 80, 142, 0),
                    FlowTint = new Color32(210, 85, 0, 255),
                    FlowOverlayTint = new Color32(201, 160, 160, 0),
                    IsDefault = false,
                    CanInsulate = true,
                    InsulateCost = 400f
                }
            },
            {

                PressurizedLiquidConduitConfig.ID,
                new PressurizedInfo()
                {
                    Capacity = 30f,
                    IncreaseMultiplier = 3f,
                    OverlayTint = new Color32(201, 80, 142, 0),
                    FlowTint = new Color32(210, 85, 0, 255),
                    FlowOverlayTint = new Color32(201, 160, 160, 0),
                    IsDefault = false,
                    CanInsulate = true,
                    InsulateCost = 400f

                }
            },
            {
                PressurizedGasConduitBridgeConfig.ID,
                new PressurizedInfo()
                {
                    Capacity = 3f,
                    IncreaseMultiplier = 3f,
                    OverlayTint = new Color32(201, 80, 142, 0),
                    FlowTint = new Color32(255, 255, 255, 255),
                    FlowOverlayTint = new Color32(0, 0, 0, 0),
                    IsDefault = false,
                    CanInsulate = false
                }
            },
            {
                PressurizedLiquidConduitBridgeConfig.ID,
                new PressurizedInfo()
                {
                    Capacity = 30f,
                    IncreaseMultiplier = 3f,
                    OverlayTint = new Color32(201, 80, 142, 0),
                    FlowTint = new Color32(255, 255, 255, 255),
                    FlowOverlayTint = new Color32(0, 0, 0, 0),
                    IsDefault = false,
                    CanInsulate = false
                }
            },
            {
                "",
                new PressurizedInfo()
                {
                    Capacity = -1f,
                    IncreaseMultiplier = 1f,
                    FlowTint = new Color32(255, 255, 255, 255),
                    IsDefault = true,
                    CanInsulate = false
                }
            }
        };


    }
}
