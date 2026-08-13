using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace QualityOfLifeONI
{
    [HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings))]
    public static class LaddersAndDoorsFixed_GeneratedBuildings_Patch
    {
        public static void Postfix()
        {
            string[] ladderIds = new string[]
            {
                "Ladder",             // Regular Ladder
                "LadderFast",         // Plastic Ladder
                "FirePole"            // Fire Pole
            };

            string[] doorIds = new string[]
            {
                "Door",               // Pneumatic Door
                "ManualPressureDoor", // Manual Airlock
                "PressureDoor",       // Mechanized Airlock
                "BunkerDoor",         // Bunker Door
                "InsulatedDoor"       // Insulated Door
            };

            string[] heavyWattWireIds = new string[]
            {
                "HighWattageWire",             // Heavy-Watt Wire
                "WireRefinedHighWattage",      // Heavy-Watt Conductive Wire
                "WireBridgeHighWattage",       // Heavy-Watt Joint Plate
                "WireRefinedBridgeHighWattage" // Heavy-Watt Conductive Joint Plate
            };

            // --- 1. LADDER LOGIC ---
            foreach (string id in ladderIds)
            {
                BuildingDef def = Assets.GetBuildingDef(id);
                if (def != null)
                {
                    def.Entombable = false;

                    // Anywhere allows placing WHITE blueprints over solid tiles/terrain
                    def.BuildLocationRule = BuildLocationRule.Anywhere;

                    if (def.ReplacementCandidateLayers == null)
                        def.ReplacementCandidateLayers = new List<ObjectLayer>();

                    if (def.ReplacementTags == null)
                        def.ReplacementTags = new List<Tag>();

                    // Replacement rules for upgrading existing ladders/fire poles and tiles
                    if (!def.ReplacementCandidateLayers.Contains(ObjectLayer.Building))
                        def.ReplacementCandidateLayers.Add(ObjectLayer.Building);

                    if (!def.ReplacementCandidateLayers.Contains(ObjectLayer.FoundationTile))
                        def.ReplacementCandidateLayers.Add(ObjectLayer.FoundationTile);

                    if (!def.ReplacementTags.Contains(GameTags.Ladders))
                        def.ReplacementTags.Add(GameTags.Ladders);

                    if (!def.ReplacementTags.Contains(GameTags.FloorTiles))
                        def.ReplacementTags.Add(GameTags.FloorTiles);
                }
            }

            // --- 2. DOOR LOGIC ---
            foreach (string id in doorIds)
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

                    if (def.ReplacementCandidateLayers == null)
                        def.ReplacementCandidateLayers = new List<ObjectLayer>();

                    if (def.ReplacementTags == null)
                        def.ReplacementTags = new List<Tag>();

                    if (!def.ReplacementCandidateLayers.Contains(ObjectLayer.FoundationTile))
                        def.ReplacementCandidateLayers.Add(ObjectLayer.FoundationTile);

                    if (!def.ReplacementCandidateLayers.Contains(ObjectLayer.Backwall))
                        def.ReplacementCandidateLayers.Add(ObjectLayer.Backwall);

                    if (!def.ReplacementTags.Contains(GameTags.FloorTiles))
                        def.ReplacementTags.Add(GameTags.FloorTiles);

                    if (!def.ReplacementTags.Contains(GameTags.Backwall))
                        def.ReplacementTags.Add(GameTags.Backwall);
                }
            }

            // --- 3. HEAVY-WATT WIRE & JOINT PLATE LOGIC ---
            foreach (string id in heavyWattWireIds)
            {
                BuildingDef def = Assets.GetBuildingDef(id);
                if (def != null)
                {
                    // Require NotInTiles so ONI enforces tile destruction on completion
                    def.BuildLocationRule = BuildLocationRule.NotInTiles;

                    // Force set ReplacementTile so building over a tile queues a replacement order
                    def.ReplacementLayer = ObjectLayer.ReplacementTile;

                    if (def.ReplacementCandidateLayers == null)
                        def.ReplacementCandidateLayers = new List<ObjectLayer>();

                    if (def.ReplacementTags == null)
                        def.ReplacementTags = new List<Tag>();

                    // Allow replacing solid tiles (FoundationTile) and other Heavy-Watt Wires
                    if (!def.ReplacementCandidateLayers.Contains(ObjectLayer.FoundationTile))
                        def.ReplacementCandidateLayers.Add(ObjectLayer.FoundationTile);

                    if (!def.ReplacementCandidateLayers.Contains(def.ObjectLayer))
                        def.ReplacementCandidateLayers.Add(def.ObjectLayer);

                    if (!def.ReplacementTags.Contains(GameTags.FloorTiles))
                        def.ReplacementTags.Add(GameTags.FloorTiles);

                    if (!def.ReplacementTags.Contains(GameTags.Wires))
                        def.ReplacementTags.Add(GameTags.Wires);
                }
            }
        }
    }
}