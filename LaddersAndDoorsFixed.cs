using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace LaddersAndDoorsFixed
{
    // 1. Configure the Building Rules (BuildLocationRule & Replacement Settings)
    [HarmonyPatch(typeof(BuildingDef), nameof(BuildingDef.PostProcess))]
    public static class BuildingDef_PostProcess_Patch
    {
        public static void Postfix(BuildingDef __instance)
        {
            if (__instance == null) return;

            // Target Ladder Types
            if (__instance.PrefabID == LadderConfig.ID ||
                __instance.PrefabID == LadderFastConfig.ID ||
                __instance.PrefabID == FirePoleConfig.ID)
            {
                __instance.BuildLocationRule = BuildLocationRule.Anywhere;
                __instance.ReplacementLayer = ObjectLayer.NumLayers; // Handled by replacement tags

                __instance.ReplacementCandidateLayers ??= new List<ObjectLayer>();
                if (!__instance.ReplacementCandidateLayers.Contains(ObjectLayer.FoundationTile))
                    __instance.ReplacementCandidateLayers.Add(ObjectLayer.FoundationTile);

                __instance.ReplacementTags ??= new List<Tag>();
                if (!__instance.ReplacementTags.Contains(GameTags.FloorTiles))
                    __instance.ReplacementTags.Add(GameTags.FloorTiles);
            }

            // Target Door Types
            if (__instance.PrefabID == ManualPressureDoorConfig.ID ||
                __instance.PrefabID == PressureDoorConfig.ID ||
                __instance.PrefabID == BunkerDoorConfig.ID ||
                __instance.PrefabID == DoorConfig.ID)
            {
                __instance.BuildLocationRule = BuildLocationRule.Anywhere;

                __instance.ReplacementCandidateLayers ??= new List<ObjectLayer>();
                if (!__instance.ReplacementCandidateLayers.Contains(ObjectLayer.FoundationTile))
                    __instance.ReplacementCandidateLayers.Add(ObjectLayer.FoundationTile);
                if (!__instance.ReplacementCandidateLayers.Contains(ObjectLayer.Backwall))
                    __instance.ReplacementCandidateLayers.Add(ObjectLayer.Backwall);

                __instance.ReplacementTags ??= new List<Tag>();
                if (!__instance.ReplacementTags.Contains(GameTags.FloorTiles))
                    __instance.ReplacementTags.Add(GameTags.FloorTiles);
                if (!__instance.ReplacementTags.Contains(GameTags.Backwall))
                    __instance.ReplacementTags.Add(GameTags.Backwall);
            }
        }
    }

    // 2. Override the "Must be built in unoccupied space" check
    [HarmonyPatch(typeof(BuildingDef), nameof(BuildingDef.IsValidBuildLocation), new[] { typeof(GameObject), typeof(int), typeof(Orientation) })]
    public static class BuildingDef_IsValidBuildLocation_Patch
    {
        public static bool Prefix(BuildingDef __instance, GameObject source_new_building, int cell, Orientation orientation, ref string fail_reason, ref bool __result)
        {
            // Check if this is one of our modded Ladders or Doors
            bool isTargetBuilding = __instance.PrefabID == LadderConfig.ID ||
                                   __instance.PrefabID == LadderFastConfig.ID ||
                                   __instance.PrefabID == FirePoleConfig.ID ||
                                   __instance.PrefabID == ManualPressureDoorConfig.ID ||
                                   __instance.PrefabID == PressureDoorConfig.ID ||
                                   __instance.PrefabID == BunkerDoorConfig.ID ||
                                   __instance.PrefabID == DoorConfig.ID;

            if (!isTargetBuilding) return true; // Run original game logic for all other buildings

            // Get the object occupying the cell foundation/tile layer
            GameObject existingObject = Grid.Objects[cell, (int)ObjectLayer.FoundationTile];

            // If there's a tile here that matches our replacement tag (like Mesh Tile, Solid Tile, etc.)
            if (existingObject != null)
            {
                KPrefabID existingKPrefab = existingObject.GetComponent<KPrefabID>();
                if (existingKPrefab != null && existingKPrefab.HasAnyTags(__instance.ReplacementTags))
                {
                    // Force ONI to consider this placement valid!
                    __result = true;
                    fail_reason = null;
                    return false; // Skip vanilla validity check
                }
            }

            return true; // Fallback to standard check if no replaceable tile is present
        }
    }
}