using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using System;

namespace LaddersAndDoorsFixed
{
    // Fix: Inject into LoadGeneratedBuildings where Assets.BuildingDefs is fully populated
    [HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings))]
    public static class LaddersAndDoorsFixed_GeneratedBuildings_Patch
    {
        public static void Postfix()
        {
            string[] targetIds = new string[]
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

            foreach (string id in targetIds)
            {
                BuildingDef def = Assets.GetBuildingDef(id);
                if (def != null)
                {
                    // 1. Prevent disabling when overlapping with solid tiles
                    def.Entombable = false;

                    // 2. Adopt Drywall's Build Location Rule
                    def.BuildLocationRule = BuildLocationRule.NotInTiles;

                    // 3. Move them to the Backwall object layer so they don't conflict with FoundationTiles (Solid Tiles)
                    def.ObjectLayer = ObjectLayer.Backwall;

                    // Note: We deliberately DO NOT change 'SceneLayer' or 'ForegroundLayer'. 
                    // This ensures Doors and Ladders still render in front of pipes/drywall visually!

                    // 4. If it's a Door, it normally uses TileLayer = FoundationTile. Nullify this conflict.
                    if (def.TileLayer == ObjectLayer.FoundationTile)
                    {
                        def.TileLayer = ObjectLayer.Backwall;
                    }

                    // 5. Adopt Drywall's Replacement Logic
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