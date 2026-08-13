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

            // --- 1. LADDER LOGIC ---
            foreach (string id in ladderIds)
            {
                BuildingDef def = Assets.GetBuildingDef(id);
                if (def != null)
                {
                    def.Entombable = false;
                    def.BuildLocationRule = BuildLocationRule.NotInTiles;

                    // Allows the construction ghost to render over existing items
                    def.ReplacementLayer = ObjectLayer.ReplacementTile;

                    if (def.ReplacementCandidateLayers == null)
                        def.ReplacementCandidateLayers = new List<ObjectLayer>();

                    if (def.ReplacementTags == null)
                        def.ReplacementTags = new List<Tag>();

                    // Allow replacing Solid Tiles (FoundationTile) AND existing Ladders/Poles (Building)
                    if (!def.ReplacementCandidateLayers.Contains(ObjectLayer.FoundationTile))
                        def.ReplacementCandidateLayers.Add(ObjectLayer.FoundationTile);

                    if (!def.ReplacementCandidateLayers.Contains(ObjectLayer.Building))
                        def.ReplacementCandidateLayers.Add(ObjectLayer.Building);

                    // Allow replacing Tiles (FloorTiles) AND other Ladders/Poles (Ladders)
                    if (!def.ReplacementTags.Contains(GameTags.FloorTiles))
                        def.ReplacementTags.Add(GameTags.FloorTiles);

                    if (!def.ReplacementTags.Contains(GameTags.Ladders))
                        def.ReplacementTags.Add(GameTags.Ladders);
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
        }
    }
}