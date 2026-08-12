using HarmonyLib;

namespace DuplicantReachability
{
    // Fix Deconstruction Reachability for Boxed Buildings
    [HarmonyPatch(typeof(Deconstructable), "OnSpawn")]
    public class Deconstructable_OnSpawn_Patch
    {
        public static void Postfix(Deconstructable __instance)
        {
            if (__instance == null) return;

            Building building = __instance.GetComponent<Building>();
            if (building == null || building.Def == null) return;

            // Skip 1x1 structures and single-tile items
            if (building.Def.WidthInCells <= 1 && building.Def.HeightInCells <= 1) return;

            CellOffset[] placementOffsets = AccessTools.PropertyGetter(typeof(Deconstructable), "placementOffsets")
                ?.Invoke(__instance, null) as CellOffset[];

            if (placementOffsets == null) return;

            // Apply corner-inclusive reachability table
            CellOffset[][] offsetTable = OffsetGroups.BuildReachabilityTable(
                placementOffsets,
                OffsetGroups.InvertedStandardTableWithCorners,
                building.Def.ConstructionOffsetFilter
            );

            __instance.SetOffsetTable(offsetTable);
        }
    }

    // Fix Construction / Building Reachability for Boxed Blueprints
    [HarmonyPatch(typeof(Constructable), "OnSpawn")]
    public class Constructable_OnSpawn_Patch
    {
        public static void Postfix(Constructable __instance)
        {
            if (__instance == null) return;

            BuildingUnderConstruction buildingUC = __instance.GetComponent<BuildingUnderConstruction>();
            if (buildingUC == null || buildingUC.Def == null) return;

            // Skip 1x1 structures and single-tile items
            if (buildingUC.Def.WidthInCells <= 1 && buildingUC.Def.HeightInCells <= 1) return;

            // Calculate reachability table using corners for build errand ghosts
            CellOffset[][] offsetTable = OffsetGroups.BuildReachabilityTable(
                buildingUC.Def.PlacementOffsets,
                OffsetGroups.InvertedStandardTableWithCorners,
                buildingUC.Def.ConstructionOffsetFilter
            );

            __instance.SetOffsetTable(offsetTable);
        }
    }
}