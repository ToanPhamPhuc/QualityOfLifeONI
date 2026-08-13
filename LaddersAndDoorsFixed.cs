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
            // Separate Ladders and Doors to treat their ObjectLayers differently
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

                    // Use ReplacementTile so the ladder's construction ghost 
                    // knows it is replacing a FoundationTile (Solid Tile)
                    def.ReplacementLayer = ObjectLayer.ReplacementTile;
                    def.ReplacementCandidateLayers = new List<ObjectLayer>
                    {
                        ObjectLayer.FoundationTile
                    };
                    def.ReplacementTags = new List<Tag>
                    {
                        GameTags.FloorTiles
                    };
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

                    // Doors act as Backwalls (stopping space exposure), so we keep your original logic.
                    // This intentionally conflicts with Drywall, which is expected.
                    def.ObjectLayer = ObjectLayer.Backwall;

                    if (def.TileLayer == ObjectLayer.FoundationTile)
                    {
                        def.TileLayer = ObjectLayer.Backwall;
                    }

                    // Doors can replace BOTH Tiles and Drywall
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
        }
    }
}