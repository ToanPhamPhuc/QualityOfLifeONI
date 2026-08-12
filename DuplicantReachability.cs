using HarmonyLib;

namespace DuplicantReachability
{
    // 1. Fix Deconstruction Reachability
    [HarmonyPatch(typeof(Deconstructable), "OnSpawn")]
    public class Deconstructable_OnSpawn_Patch
    {
        public static void Postfix(Deconstructable __instance)
        {
            if (__instance == null) return;

            Building building = __instance.GetComponent<Building>();
            if (building == null || building.Def == null) return;

            if (building.Def.WidthInCells <= 1 && building.Def.HeightInCells <= 1) return;

            CellOffset[] placementOffsets = AccessTools.PropertyGetter(typeof(Deconstructable), "placementOffsets")
                ?.Invoke(__instance, null) as CellOffset[];

            if (placementOffsets == null) return;

            CellOffset[][] offsetTable = OffsetGroups.BuildReachabilityTable(
                placementOffsets,
                OffsetGroups.InvertedStandardTableWithCorners,
                building.Def.ConstructionOffsetFilter
            );

            __instance.SetOffsetTable(offsetTable);
        }
    }

    // 2. Fix Construction & Material Supply Delivery Reachability
    [HarmonyPatch(typeof(Constructable), "OnSpawn")]
    public class Constructable_OnSpawn_Patch
    {
        public static void Postfix(Constructable __instance)
        {
            if (__instance == null) return;

            BuildingUnderConstruction buildingUC = __instance.GetComponent<BuildingUnderConstruction>();
            if (buildingUC == null || buildingUC.Def == null) return;

            // Expand reachability table for all multi-cell ghosts/blueprints using corners
            CellOffset[][] offsetTable = OffsetGroups.BuildReachabilityTable(
                buildingUC.Def.PlacementOffsets,
                OffsetGroups.InvertedStandardTableWithCorners,
                buildingUC.Def.ConstructionOffsetFilter
            );

            // Update Constructable offset table
            __instance.SetOffsetTable(offsetTable);

            // Update Workable offset table (for construction work)
            Workable workable = __instance.GetComponent<Workable>();
            if (workable != null)
            {
                workable.SetOffsetTable(offsetTable);
            }
        }
    }
}