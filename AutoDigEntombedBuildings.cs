using HarmonyLib;
using UnityEngine;

namespace QualityOfLifeONI
{
    public static class AutoDigHelper
    {
        /// <summary>
        /// Places a Dig errand on the specified cell if it is solid and diggable.
        /// </summary>
        public static void TryPlaceDig(int cell)
        {
            if (!Grid.IsValidCell(cell)) return;

            // 1. Cell must be solid and diggable (hardness < 255 excludes Neutronium)
            if (Grid.Solid[cell] && Grid.Element[cell].hardness < 255)
            {
                // 2. Check if a dig errand already exists on this cell
                if (Grid.Objects[cell, (int)ObjectLayer.DigPlacer] == null)
                {
                    // 3. Temporarily bypass foundation state so DigTool can place on building cells
                    bool wasFoundation = Grid.Foundation[cell];
                    Grid.Foundation[cell] = false;

                    // 4. Place native ONI dig errand
                    GameObject digGO = DigTool.PlaceDig(cell, 0);

                    // 5. Restore original foundation state
                    Grid.Foundation[cell] = wasFoundation;

                    // 6. Set master priority (High Priority - Level 5)
                    if (digGO != null)
                    {
                        Prioritizable prioritizable = digGO.GetComponent<Prioritizable>();
                        if (prioritizable != null)
                        {
                            prioritizable.SetMasterPriority(new PrioritySetting(PriorityScreen.PriorityClass.high, 5));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks if a cell contains a building on valid ObjectLayers.
        /// </summary>
        public static bool HasBuilding(int cell)
        {
            if (!Grid.IsValidCell(cell)) return false;

            return Grid.Objects[cell, (int)ObjectLayer.Building] != null
                || Grid.Objects[cell, (int)ObjectLayer.AttachableBuilding] != null;
        }
    }

    // --- PATCH 1: Triggers when falling Sand / Snow / Regolith lands and solidifies ---
    [HarmonyPatch(typeof(UnstableGroundManager), "RemoveFromPending")]
    public static class UnstableGroundManager_RemoveFromPending_Patch
    {
        public static void Postfix(int cell)
        {
            if (AutoDigHelper.HasBuilding(cell))
            {
                AutoDigHelper.TryPlaceDig(cell);
            }
        }
    }

    // --- PATCH 2: Triggers when Meteors, Ice, or Spawns entomb a building/entity ---
    [HarmonyPatch(typeof(EntombVulnerable), "CheckEntombed")]
    public static class EntombVulnerable_CheckEntombed_Patch
    {
        public static void Postfix(EntombVulnerable __instance)
        {
            if (__instance == null || !__instance.GetEntombed) return;

            OccupyArea area = __instance.GetComponent<OccupyArea>();
            int rootCell = Grid.PosToCell(__instance.gameObject);

            CellOffset[] offsets = (area != null)
                ? area.OccupiedCellsOffsets
                : new CellOffset[] { new CellOffset(0, 0) };

            foreach (CellOffset offset in offsets)
            {
                int cell = Grid.OffsetCell(rootCell, offset);
                AutoDigHelper.TryPlaceDig(cell);
            }
        }
    }
}