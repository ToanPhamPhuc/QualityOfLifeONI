using HarmonyLib;

namespace DuplicantReachability
{
    [HarmonyPatch(typeof(Deconstructable), "OnSpawn")]
    public class Deconstructable_OnSpawn_Patch
    {
        public static void Postfix(Deconstructable __instance)
        {
            if (__instance == null) return;

            Building building = __instance.GetComponent<Building>();
            if (building == null || building.Def == null) return;

            // Skip 1x1 structures and background tiles
            if (building.Def.WidthInCells <= 1 && building.Def.HeightInCells <= 1) return;

            // Fetch placement offsets via private property reflection
            CellOffset[] placementOffsets = AccessTools.PropertyGetter(typeof(Deconstructable), "placementOffsets")
                .Invoke(__instance, null) as CellOffset[];

            if (placementOffsets == null) return;

            // Recalculate build reachability table using corners (allows reaching boxed buildings)
            CellOffset[][] offsetTable = OffsetGroups.BuildReachabilityTable(
                placementOffsets,
                OffsetGroups.InvertedStandardTableWithCorners,
                building.Def.ConstructionOffsetFilter
            );

            __instance.SetOffsetTable(offsetTable);
        }
    }
}