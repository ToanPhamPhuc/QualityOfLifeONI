using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace QualityOfLifeONI
{
    // --- 1. SIDE-SCREEN TOGGLE COMPONENT ---
    // ONI automatically renders a toggle side-screen for any building containing an ICheckboxControl component.
    public class OverrideModeControl : KMonoBehaviour, ICheckboxControl
    {
        public string CheckboxTitleKey => "Mod Settings";
        public string CheckboxLabel => "OVERRIDE MODE";
        public string CheckboxTooltip => "Toggle tile replacement/override capabilities for ladders and doors.";

        public bool GetCheckboxValue()
        {
            return OverrideManager.IsOverrideEnabled;
        }

        public void SetCheckboxValue(bool value)
        {
            OverrideManager.ToggleOverrideMode(value);
        }
    }

    // --- 2. OVERRIDE STATE MANAGER ---
    public static class OverrideManager
    {
        public static bool IsOverrideEnabled = true;

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

        // Backup vanilla state to safely restore when toggled OFF
        private class SavedState
        {
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
                        BuildLocationRule = def.BuildLocationRule,
                        ObjectLayer = def.ObjectLayer,
                        TileLayer = def.TileLayer,
                        ReplacementLayer = def.ReplacementLayer,
                        ReplacementCandidateLayers = def.ReplacementCandidateLayers != null ? new List<ObjectLayer>(def.ReplacementCandidateLayers) : new List<ObjectLayer>(),
                        ReplacementTags = def.ReplacementTags != null ? new List<Tag>(def.ReplacementTags) : new List<Tag>()
                    };
                }
            }
        }

        public static void ToggleOverrideMode(bool enable)
        {
            IsOverrideEnabled = enable;

            if (enable)
            {
                ApplyModOverrides();
            }
            else
            {
                RestoreVanillaStates();
            }
        }

        public static void ApplyModOverrides()
        {
            // --- LADDER LOGIC ---
            foreach (string id in LadderIds)
            {
                BuildingDef def = Assets.GetBuildingDef(id);
                if (def != null)
                {
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
            }

            // --- DOOR LOGIC ---
            foreach (string id in DoorIds)
            {
                BuildingDef def = Assets.GetBuildingDef(id);
                if (def != null)
                {
                    def.Entombable = false;
                    def.BuildLocationRule = BuildLocationRule.NotInTiles;
                    def.ObjectLayer = ObjectLayer.Backwall;

                    if (def.TileLayer == ObjectLayer.FoundationTile)
                    {
                        def.TileLayer = ObjectLayer.Backwall;
                    }

                    def.ReplacementLayer = ObjectLayer.ReplacementBackwall;

                    if (def.ReplacementCandidateLayers == null) def.ReplacementCandidateLayers = new List<ObjectLayer>();
                    if (def.ReplacementTags == null) def.ReplacementTags = new List<Tag>();

                    if (!def.ReplacementCandidateLayers.Contains(ObjectLayer.FoundationTile)) def.ReplacementCandidateLayers.Add(ObjectLayer.FoundationTile);
                    if (!def.ReplacementCandidateLayers.Contains(ObjectLayer.Backwall)) def.ReplacementCandidateLayers.Add(ObjectLayer.Backwall);

                    if (!def.ReplacementTags.Contains(GameTags.FloorTiles)) def.ReplacementTags.Add(GameTags.FloorTiles);
                    if (!def.ReplacementTags.Contains(GameTags.Backwall)) def.ReplacementTags.Add(GameTags.Backwall);
                }
            }

            // WIP

            //// --- HEAVY-WATT WIRE & JOINT PLATE LOGIC ---
            //foreach (string id in heavyWattWireIds)
            //{
            //    BuildingDef def = Assets.GetBuildingDef(id);
            //    if (def != null)
            //    {
            //        // BuildLocationRule.Tile enables ONI's tile-replacement pipeline:
            //        // allows dragging over tiles and queues automatic tile deconstruction upon completion
            //        def.BuildLocationRule = BuildLocationRule.Tile;

            //        // Assign the construction ghost to the tile replacement layer
            //        def.ReplacementLayer = ObjectLayer.ReplacementTile;

            //        if (def.ReplacementCandidateLayers == null)
            //            def.ReplacementCandidateLayers = new List<ObjectLayer>();

            //        if (def.ReplacementTags == null)
            //            def.ReplacementTags = new List<Tag>();

            //        // Allow replacing solid tiles (FoundationTile) and other wires on the same layer
            //        if (!def.ReplacementCandidateLayers.Contains(ObjectLayer.FoundationTile))
            //            def.ReplacementCandidateLayers.Add(ObjectLayer.FoundationTile);

            //        if (!def.ReplacementCandidateLayers.Contains(def.ObjectLayer))
            //            def.ReplacementCandidateLayers.Add(def.ObjectLayer);

            //        if (!def.ReplacementTags.Contains(GameTags.FloorTiles))
            //            def.ReplacementTags.Add(GameTags.FloorTiles);

            //        if (!def.ReplacementTags.Contains(GameTags.Wires))
            //            def.ReplacementTags.Add(GameTags.Wires);
            //    }
            //}
        }

        private static void RestoreVanillaStates()
        {
            foreach (var kvp in VanillaStates)
            {
                BuildingDef def = Assets.GetBuildingDef(kvp.Key);
                if (def != null)
                {
                    SavedState saved = kvp.Value;
                    def.BuildLocationRule = saved.BuildLocationRule;
                    def.ObjectLayer = saved.ObjectLayer;
                    def.TileLayer = saved.TileLayer;
                    def.ReplacementLayer = saved.ReplacementLayer;
                    def.ReplacementCandidateLayers = new List<ObjectLayer>(saved.ReplacementCandidateLayers);
                    def.ReplacementTags = new List<Tag>(saved.ReplacementTags);
                }
            }
        }
    }

    // --- 3. HARMONY PATCH ---
    [HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings))]
    public static class LaddersAndDoorsFixed_GeneratedBuildings_Patch
    {
        public static void Postfix()
        {
            // 1. Save vanilla states first
            OverrideManager.SaveVanillaStates();

            // 2. Apply custom mod rules
            OverrideManager.ApplyModOverrides();

            // 3. Attach the side-screen component to building prefabs
            List<string> allTargetIds = new List<string>(OverrideManager.LadderIds);
            allTargetIds.AddRange(OverrideManager.DoorIds);

            foreach (string id in allTargetIds)
            {
                BuildingDef def = Assets.GetBuildingDef(id);
                if (def != null && def.BuildingComplete != null)
                {
                    def.BuildingComplete.AddComponent<OverrideModeControl>();
                }
            }
        }
    }
}