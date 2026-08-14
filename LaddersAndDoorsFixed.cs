using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace QualityOfLifeONI
{
    // --- 0. MOD ENTRY POINT ---
    //public class ModInit : KMod.UserMod2
    //{
    //    public override void OnLoad(Harmony harmony)
    //    {
    //        base.OnLoad(harmony);
    //        harmony.PatchAll(); // Ensures all Harmony patches below are registered!
    //    }
    //}

    public enum ModBuildingMode
    {
        Vanilla,
        Override,
        Background
    }

    // --- 1. MODE & STATE MANAGER ---
    public static class ModeManager
    {
        public static ModBuildingMode CurrentMode = ModBuildingMode.Override;

        public static readonly HashSet<string> TargetBuildingIds = new HashSet<string>
        {
            "Ladder",             // Regular Ladder
            "LadderFast",         // Plastic Ladder
            "FirePole",           // Fire Pole
            "Door",               // Pneumatic Door
            "ManualPressureDoor", // Manual Airlock
            "PressureDoor",       // Mechanized Airlock
            "BunkerDoor",         // Bunker Door
            "InsulatedDoor"       // Insulated Door
        };

        private static readonly string[] LadderIds = new string[] { "Ladder", "LadderFast", "FirePole" };
        private static readonly string[] DoorIds = new string[] { "Door", "ManualPressureDoor", "PressureDoor", "BunkerDoor", "InsulatedDoor" };

        private class SavedState
        {
            public bool Entombable;
            public BuildLocationRule BuildLocationRule;
            public ObjectLayer ObjectLayer;
            public ObjectLayer TileLayer;
            public ObjectLayer ReplacementLayer;
            public List<ObjectLayer> ReplacementCandidateLayers;
            public List<Tag> ReplacementTags;
        }

        private static readonly Dictionary<string, SavedState> VanillaStates = new Dictionary<string, SavedState>();

        public static bool IsTargetBuilding(string id) => !string.IsNullOrEmpty(id) && TargetBuildingIds.Contains(id);

        public static void SaveVanillaStates()
        {
            foreach (string id in TargetBuildingIds)
            {
                BuildingDef def = Assets.GetBuildingDef(id);
                if (def != null && !VanillaStates.ContainsKey(id))
                {
                    VanillaStates[id] = new SavedState
                    {
                        Entombable = def.Entombable,
                        BuildLocationRule = def.BuildLocationRule,
                        ObjectLayer = def.ObjectLayer,
                        TileLayer = def.TileLayer,
                        ReplacementLayer = def.ReplacementLayer,
                        ReplacementCandidateLayers = def.ReplacementCandidateLayers != null ? new List<ObjectLayer>(def.ReplacementCandidateLayers) : null,
                        ReplacementTags = def.ReplacementTags != null ? new List<Tag>(def.ReplacementTags) : null
                    };
                }
            }
        }

        public static void SetMode(ModBuildingMode mode)
        {
            CurrentMode = mode;
            RestoreVanillaStates();

            switch (mode)
            {
                case ModBuildingMode.Override:
                    ApplyOverrideMode();
                    break;
                case ModBuildingMode.Background:
                    ApplyBackgroundMode();
                    break;
                case ModBuildingMode.Vanilla:
                default:
                    break;
            }
        }

        private static void ApplyOverrideMode()
        {
            foreach (string id in LadderIds)
            {
                BuildingDef def = Assets.GetBuildingDef(id);
                if (def == null) continue;

                def.Entombable = false;
                def.BuildLocationRule = BuildLocationRule.NotInTiles;
                def.ReplacementLayer = ObjectLayer.ReplacementTile;

                if (def.ReplacementCandidateLayers == null) def.ReplacementCandidateLayers = new List<ObjectLayer>();
                if (def.ReplacementTags == null) def.ReplacementTags = new List<Tag>();

                if (!def.ReplacementCandidateLayers.Contains(ObjectLayer.FoundationTile)) def.ReplacementCandidateLayers.Add(ObjectLayer.FoundationTile);
                if (!def.ReplacementCandidateLayers.Contains(ObjectLayer.Building)) def.ReplacementCandidateLayers.Add(ObjectLayer.Building);

                if (!def.ReplacementTags.Contains(GameTags.FloorTiles)) def.ReplacementTags.Add(GameTags.FloorTiles);
                if (!def.ReplacementTags.Contains(GameTags.Ladders)) def.ReplacementTags.Add(GameTags.Ladders);
            }

            foreach (string id in DoorIds)
            {
                BuildingDef def = Assets.GetBuildingDef(id);
                if (def == null) continue;

                def.Entombable = false;
                def.BuildLocationRule = BuildLocationRule.NotInTiles;
                def.ObjectLayer = ObjectLayer.Backwall;

                if (def.TileLayer == ObjectLayer.FoundationTile)
                    def.TileLayer = ObjectLayer.Backwall;

                def.ReplacementLayer = ObjectLayer.ReplacementBackwall;

                if (def.ReplacementCandidateLayers == null) def.ReplacementCandidateLayers = new List<ObjectLayer>();
                if (def.ReplacementTags == null) def.ReplacementTags = new List<Tag>();

                if (!def.ReplacementCandidateLayers.Contains(ObjectLayer.FoundationTile)) def.ReplacementCandidateLayers.Add(ObjectLayer.FoundationTile);
                if (!def.ReplacementCandidateLayers.Contains(ObjectLayer.Backwall)) def.ReplacementCandidateLayers.Add(ObjectLayer.Backwall);

                if (!def.ReplacementTags.Contains(GameTags.FloorTiles)) def.ReplacementTags.Add(GameTags.FloorTiles);
                if (!def.ReplacementTags.Contains(GameTags.Backwall)) def.ReplacementTags.Add(GameTags.Backwall);
            }
        }

        private static void ApplyBackgroundMode()
        {
            foreach (string id in TargetBuildingIds)
            {
                BuildingDef def = Assets.GetBuildingDef(id);
                if (def == null) continue;

                def.Entombable = false;
                def.BuildLocationRule = BuildLocationRule.NotInTiles;
                def.ObjectLayer = ObjectLayer.Backwall;

                if (def.TileLayer == ObjectLayer.FoundationTile)
                {
                    def.TileLayer = ObjectLayer.Backwall;
                }

                def.ReplacementLayer = ObjectLayer.ReplacementBackwall;
                def.ReplacementCandidateLayers = new List<ObjectLayer>
                {
                    ObjectLayer.FoundationTile,
                    ObjectLayer.Backwall
                };
                def.ReplacementTags = new List<Tag>
                {
                    GameTags.FloorTiles,
                    GameTags.Backwall
                };
            }
        }

        private static void RestoreVanillaStates()
        {
            foreach (var kvp in VanillaStates)
            {
                BuildingDef def = Assets.GetBuildingDef(kvp.Key);
                if (def == null) continue;

                SavedState saved = kvp.Value;
                def.Entombable = saved.Entombable;
                def.BuildLocationRule = saved.BuildLocationRule;
                def.ObjectLayer = saved.ObjectLayer;
                def.TileLayer = saved.TileLayer;
                def.ReplacementLayer = saved.ReplacementLayer;
                def.ReplacementCandidateLayers = saved.ReplacementCandidateLayers != null ? new List<ObjectLayer>(saved.ReplacementCandidateLayers) : null;
                def.ReplacementTags = saved.ReplacementTags != null ? new List<Tag>(saved.ReplacementTags) : null;
            }
        }
    }

    // --- 2. BOTTOM-RIGHT TOOL MENU CONTROLLER ---
    public class LaddersDoorsModeTool : InterfaceTool
    {
        public static LaddersDoorsModeTool Instance;

        private ToolParameterMenu.ToggleData[] currentFilters;

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            Instance = this;
        }

        protected override void OnActivateTool()
        {
            base.OnActivateTool();

            // Hide priority numbers overlay when selecting mode
            if (ToolMenu.Instance != null && ToolMenu.Instance.PriorityScreen != null)
            {
                ToolMenu.Instance.PriorityScreen.Show(false);
            }

            BuildFilters();
            ToolMenu.Instance.toolParameterMenu.PopulateMenu(this.currentFilters);
            ToolMenu.Instance.toolParameterMenu.onParametersChanged += OnParametersChanged;
        }

        protected override void OnDeactivateTool(InterfaceTool new_tool)
        {
            if (ToolMenu.Instance != null && ToolMenu.Instance.toolParameterMenu != null)
            {
                ToolMenu.Instance.toolParameterMenu.onParametersChanged -= OnParametersChanged;
                ToolMenu.Instance.toolParameterMenu.ClearMenu();
            }

            base.OnDeactivateTool(new_tool);
        }

        private void BuildFilters()
        {
            currentFilters = new ToolParameterMenu.ToggleData[]
            {
                new ToolParameterMenu.ToggleData(
                    "Override Mode",
                    ModeManager.CurrentMode == ModBuildingMode.Override ? ToolParameterMenu.ToggleState.On : ToolParameterMenu.ToggleState.Off,
                    false
                ),
                new ToolParameterMenu.ToggleData(
                    "Background Mode",
                    ModeManager.CurrentMode == ModBuildingMode.Background ? ToolParameterMenu.ToggleState.On : ToolParameterMenu.ToggleState.Off,
                    false
                ),
                new ToolParameterMenu.ToggleData(
                    "Vanilla Mode",
                    ModeManager.CurrentMode == ModBuildingMode.Vanilla ? ToolParameterMenu.ToggleState.On : ToolParameterMenu.ToggleState.Off,
                    false
                )
            };
        }

        private void OnParametersChanged()
        {
            if (currentFilters == null) return;

            foreach (var filter in currentFilters)
            {
                if (filter.state == ToolParameterMenu.ToggleState.On)
                {
                    if (filter.name == "Override Mode" && ModeManager.CurrentMode != ModBuildingMode.Override)
                    {
                        ModeManager.SetMode(ModBuildingMode.Override);
                        break;
                    }
                    else if (filter.name == "Background Mode" && ModeManager.CurrentMode != ModBuildingMode.Background)
                    {
                        ModeManager.SetMode(ModBuildingMode.Background);
                        break;
                    }
                    else if (filter.name == "Vanilla Mode" && ModeManager.CurrentMode != ModBuildingMode.Vanilla)
                    {
                        ModeManager.SetMode(ModBuildingMode.Vanilla);
                        break;
                    }
                }
            }

            // Refresh menu options visually
            BuildFilters();
            ToolMenu.Instance.toolParameterMenu.PopulateMenu(this.currentFilters);
        }
    }

    // --- 3. HARMONY PATCHES ---

    // Safety Patch to prevent crashes when comparing unbound actions
    [HarmonyPatch(typeof(GameInputMapping), nameof(GameInputMapping.CompareActionKeyCodes))]
    public static class GameInputMapping_CompareActionKeyCodes_Patch
    {
        public static bool Prefix(Action a, Action b, ref bool __result)
        {
            if (a == Action.NumActions || a == Action.Invalid ||
                b == Action.NumActions || b == Action.Invalid)
            {
                __result = false;
                return false; // Skip original method execution to avoid assertion crash
            }

            return true;
        }
    }

    // Save initial vanilla states upon building definition load
    [HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings))]
    public static class GeneratedBuildings_LoadGeneratedBuildings_Patch
    {
        public static void Postfix()
        {
            ModeManager.SaveVanillaStates();
            ModeManager.SetMode(ModBuildingMode.Override);
        }
    }

    // Register custom interface tool to PlayerController
    [HarmonyPatch(typeof(PlayerController), "OnPrefabInit")]
    public static class PlayerController_OnPrefabInit_Patch
    {
        public static void Postfix(PlayerController __instance)
        {
            GameObject toolGo = new GameObject("LaddersDoorsModeTool");
            toolGo.transform.SetParent(__instance.gameObject.transform);

            LaddersDoorsModeTool customTool = toolGo.AddComponent<LaddersDoorsModeTool>();

            // Safely resize the InterfaceTool[] array directly
            var arr = __instance.tools;
            if (arr == null)
            {
                __instance.tools = new InterfaceTool[] { customTool };
            }
            else
            {
                var newArr = new InterfaceTool[arr.Length + 1];
                Array.Copy(arr, newArr, arr.Length);
                newArr[arr.Length] = customTool;
                __instance.tools = newArr;
            }
        }
    }

    // Inject tool icon button into bottom right ToolMenu
    [HarmonyPatch(typeof(ToolMenu), "CreateBasicTools")]
    public static class ToolMenu_CreateBasicTools_Patch
    {
        public static void Postfix(ToolMenu __instance)
        {
            if (__instance.basicTools != null)
            {
                __instance.basicTools.Add(ToolMenu.CreateToolCollection(
                    "Ladders & Doors Mode",
                    "action_repair",
                    Action.Invalid, // FIXED: Use Action.Invalid instead of Action.NumActions
                    "LaddersDoorsModeTool",
                    "Switches mode for Ladders and Doors",
                    false
                ));
            }
        }
    }
}