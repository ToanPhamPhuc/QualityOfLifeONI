using HarmonyLib;
using UnityEngine;

namespace QualityOfLifeONI
{
    [HarmonyPatch(typeof(EntombVulnerable), "CheckEntombed")]
    public class AutoDigEntombedBuildings_Patch
    {
        public static void Postfix(EntombVulnerable __instance)
        {
            // We only want to trigger this if the building is currently marked as entombed.
            if (__instance.GetEntombed)
            {
                // Grab the OccupyArea component to find which cells the building sits on
                OccupyArea area = __instance.GetComponent<OccupyArea>();
                if (area != null)
                {
                    // Calculate the building's root cell
                    int rootCell = Grid.PosToCell(__instance.gameObject);

                    // Loop through every tile this building occupies
                    foreach (CellOffset offset in area.OccupiedCellsOffsets)
                    {
                        int cell = Grid.OffsetCell(rootCell, offset);

                        // If the cell is valid and currently solid (entombing the building)
                        if (Grid.IsValidCell(cell) && Grid.Solid[cell])
                        {
                            // In ONI, solid tiles (like regolith/sand) are stored in the Solid ObjectLayer
                            GameObject solidProp = Grid.Objects[cell, (int)ObjectLayer.SolidConduitTile];
                            if (solidProp != null)
                            {
                                // Check if it's already marked for digging to prevent duplicating errands
                                if (solidProp.GetComponent<Diggable>() == null)
                                {
                                    // AddOrGet the Diggable component. This simulates the player 
                                    // dragging the "G" (Dig) tool over the tile, creating a dig chore.
                                    solidProp.AddOrGet<Diggable>();

                                    // Optional: Force a high priority so dupes rescue the building immediately.
                                    // You can tweak these numbers based on your preference.
                                    Prioritizable prioritizable = solidProp.GetComponent<Prioritizable>();
                                    if (prioritizable != null)
                                    {
                                        prioritizable.SetMasterPriority(new PrioritySetting(PriorityScreen.PriorityClass.high, 5));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}