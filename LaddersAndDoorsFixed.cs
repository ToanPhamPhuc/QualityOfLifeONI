using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace LaddersAndDoorsFixed
{
    [HarmonyPatch(typeof(BuildingDef), nameof(BuildingDef.PostProcess))]
    public static class BuildingDef_PostProcess_Patch
    {
        public static void Postfix(BuildingDef __instance)
        {
            if (__instance == null) return;

            // Target Ladder Types: Ladder, Plastic Ladder, Fire Pole
            if (__instance.PrefabID == LadderConfig.ID ||
                __instance.PrefabID == LadderFastConfig.ID ||
                __instance.PrefabID == FirePoleConfig.ID)
            {
                // Allow ladders to be placed directly on solid tile cells
                __instance.BuildLocationRule = BuildLocationRule.Anywhere;

                // Configure replacement settings targeting solid floor tiles
                __instance.ReplacementLayer = ObjectLayer.NumLayers; // Uses default replacement handling

                if (__instance.ReplacementCandidateLayers == null)
                    __instance.ReplacementCandidateLayers = new List<ObjectLayer>();

                if (!__instance.ReplacementCandidateLayers.Contains(ObjectLayer.FoundationTile))
                    __instance.ReplacementCandidateLayers.Add(ObjectLayer.FoundationTile);

                if (__instance.ReplacementTags == null)
                    __instance.ReplacementTags = new List<Tag>();

                if (!__instance.ReplacementTags.Contains(GameTags.FloorTiles))
                    __instance.ReplacementTags.Add(GameTags.FloorTiles);
            }

            // Target Door Types: Manual Airlock, Mechanized Airlock, Bunker Door, Pneumatic Door
            if (__instance.PrefabID == ManualPressureDoorConfig.ID ||
                __instance.PrefabID == PressureDoorConfig.ID ||
                __instance.PrefabID == BunkerDoorConfig.ID ||
                __instance.PrefabID == DoorConfig.ID)
            {
                // Relax build rules so doors can overlap tiles and background walls
                __instance.BuildLocationRule = BuildLocationRule.Anywhere;

                // Configure candidate layers to allow replacing BOTH Floor Tiles and Drywall (Backwall)
                if (__instance.ReplacementCandidateLayers == null)
                    __instance.ReplacementCandidateLayers = new List<ObjectLayer>();

                if (!__instance.ReplacementCandidateLayers.Contains(ObjectLayer.FoundationTile))
                    __instance.ReplacementCandidateLayers.Add(ObjectLayer.FoundationTile);

                if (!__instance.ReplacementCandidateLayers.Contains(ObjectLayer.Backwall))
                    __instance.ReplacementCandidateLayers.Add(ObjectLayer.Backwall);

                // Configure replacement tags matching Tiles and Drywalls
                if (__instance.ReplacementTags == null)
                    __instance.ReplacementTags = new List<Tag>();

                if (!__instance.ReplacementTags.Contains(GameTags.FloorTiles))
                    __instance.ReplacementTags.Add(GameTags.FloorTiles);

                if (!__instance.ReplacementTags.Contains(GameTags.Backwall))
                    __instance.ReplacementTags.Add(GameTags.Backwall);
            }
        }
    }
}