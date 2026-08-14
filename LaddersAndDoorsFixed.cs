using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace QualityOfLifeONI
{
    public enum ModBuildingMode
    {
        Vanilla,
        Override,
        Background
    }

    // --- 1. SIDE-SCREEN TOGGLE COMPONENTS ---

    // Toggle 1: OVERRIDE MODE (Handled by ONI's default CheckboxSideScreen)
    public class OverrideModeControl : KMonoBehaviour, ICheckboxControl
    {
        public string CheckboxTitleKey => "Mod Settings";
        public string CheckboxLabel => "OVERRIDE MODE";
        public string CheckboxTooltip => "Replace solid tiles directly when constructing ladders and doors.";

        public bool GetCheckboxValue() => ModeManager.CurrentMode == ModBuildingMode.Override;

        public void SetCheckboxValue(bool value)
        {
            if (value)
                ModeManager.SetMode(ModBuildingMode.Override);
            else if (ModeManager.CurrentMode == ModBuildingMode.Override)
                ModeManager.SetMode(ModBuildingMode.Vanilla);
        }
    }

    // Toggle 2: BACKGROUND MODE (Handled by our Custom BackgroundModeSideScreen)
    public class BackgroundModeControl : KMonoBehaviour
    {
        public string CheckboxLabel => "BACKGROUND MODE";
        public string CheckboxTooltip => "Treat ladders and doors as background structures (like Drywall).";

        public bool GetCheckboxValue() => ModeManager.CurrentMode == ModBuildingMode.Background;

        public void SetCheckboxValue(bool value)
        {
            if (value)
                ModeManager.SetMode(ModBuildingMode.Background);
            else if (ModeManager.CurrentMode == ModBuildingMode.Background)
                ModeManager.SetMode(ModBuildingMode.Vanilla);
        }
    }

    // --- 2. CUSTOM SIDE SCREEN FOR BACKGROUND MODE ---
    public class BackgroundModeSideScreen : SideScreenContent
    {
        public MultiToggle toggle;
        public LocText label;
        public ToolTip tooltip;
        private BackgroundModeControl target;

        protected override void OnSpawn()
        {
            base.OnSpawn();
            if (toggle != null)
            {
                toggle.onClick = (System.Action)Delegate.Combine(toggle.onClick, new System.Action(this.OnClick));
            }
        }

        public override bool IsValidForTarget(GameObject target)
        {
            return target.GetComponent<BackgroundModeControl>() != null;
        }

        public override void SetTarget(GameObject target)
        {
            base.SetTarget(target);
            if (target == null) return;

            this.target = target.GetComponent<BackgroundModeControl>();
            if (this.target == null) return;

            if (label != null) label.text = this.target.CheckboxLabel;
            if (tooltip != null) tooltip.toolTip = this.target.CheckboxTooltip;

            Refresh();
        }

        public override void ClearTarget()
        {
            base.ClearTarget();
            this.target = null;
        }

        private void Refresh()
        {
            if (target != null && toggle != null)
            {
                toggle.ChangeState(target.GetCheckboxValue() ? 1 : 0);
            }
        }

        private void OnClick()
        {
            if (target != null)
            {
                target.SetCheckboxValue(!target.GetCheckboxValue());
                Refresh();
            }
        }
    }

    // --- 3. MODE & STATE MANAGER ---
    public static class ModeManager
    {
        public static ModBuildingMode CurrentMode = ModBuildingMode.Override;

        public static readonly string[] LadderIds = new string[]
        {
            "Ladder",             // Regular Ladder
            "LadderFast",         // Plastic Ladder
            "FirePole"            // Fire Pole
        };

        public static readonly string[] DoorIds = new string[]
        {
            "Door",               // Pneumatic Door
            "ManualPressureDoor", // Manual Airlock
            "PressureDoor",       // Mechanized Airlock
            "BunkerDoor",         // Bunker Door
            "InsulatedDoor"       // Insulated Door
        };

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

        public static void SaveVanillaStates()
        {
            List<string> allIds = new List<string>(LadderIds);
            allIds.AddRange(DoorIds);

            foreach (string id in allIds)
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

        // --- MODE 1: OVERRIDE MODE ---
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

        // --- MODE 2: BACKGROUND MODE ---
        private static void ApplyBackgroundMode()
        {
            List<string> targetIds = new List<string>(LadderIds);
            targetIds.AddRange(DoorIds);

            foreach (string id in targetIds)
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

    // --- 4. HARMONY PATCHES ---

    // Patch 1: Building setup
    [HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings))]
    public static class LaddersAndDoorsFixed_GeneratedBuildings_Patch
    {
        public static void Postfix()
        {
            ModeManager.SaveVanillaStates();
            ModeManager.SetMode(ModBuildingMode.Override);

            List<string> allTargetIds = new List<string>(ModeManager.LadderIds);
            allTargetIds.AddRange(ModeManager.DoorIds);

            foreach (string id in allTargetIds)
            {
                BuildingDef def = Assets.GetBuildingDef(id);
                if (def != null && def.BuildingComplete != null)
                {
                    def.BuildingComplete.AddComponent<OverrideModeControl>();
                    def.BuildingComplete.AddComponent<BackgroundModeControl>();
                }
            }
        }
    }

    // Patch 2: Safe registration of custom side screen UI for BackgroundMode
    [HarmonyPatch(typeof(DetailsScreen), "OnPrefabInit")]
    public static class DetailsScreen_OnPrefabInit_Patch
    {
        public static void Postfix(DetailsScreen __instance)
        {
            // Traverse private field 'sideScreens' safely
            var sideScreens = Traverse.Create(__instance).Field<List<DetailsScreen.SideScreenRef>>("sideScreens").Value;
            if (sideScreens == null) return;

            // Find standard checkbox side screen prefab using type name matching
            DetailsScreen.SideScreenRef originalRef = null;
            foreach (var ssRef in sideScreens)
            {
                if (ssRef.screenPrefab != null && ssRef.screenPrefab.GetType().Name == "CheckboxSideScreen")
                {
                    originalRef = ssRef;
                    break;
                }
            }

            if (originalRef != null && originalRef.screenPrefab != null)
            {
                // Clone the prefab gameobject
                GameObject clonedGo = UnityEngine.Object.Instantiate(originalRef.screenPrefab.gameObject, originalRef.screenPrefab.transform.parent);
                clonedGo.name = "BackgroundModeSideScreen";

                // Destroy original CheckboxSideScreen script on the clone
                var oldComponent = clonedGo.GetComponent(originalRef.screenPrefab.GetType());
                if (oldComponent != null)
                {
                    UnityEngine.Object.DestroyImmediate(oldComponent);
                }

                // Attach custom screen component
                BackgroundModeSideScreen customScreen = clonedGo.AddComponent<BackgroundModeSideScreen>();
                customScreen.toggle = clonedGo.GetComponentInChildren<MultiToggle>();
                customScreen.label = clonedGo.GetComponentInChildren<LocText>();
                customScreen.tooltip = clonedGo.GetComponentInChildren<ToolTip>();

                // Insert into sideScreens list
                sideScreens.Add(new DetailsScreen.SideScreenRef
                {
                    name = "BackgroundModeSideScreen",
                    screenPrefab = customScreen,
                    offset = originalRef.offset,
                    tab = originalRef.tab,
                    screenInstance = null
                });
            }
        }
    }
}