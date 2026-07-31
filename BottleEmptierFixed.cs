using HarmonyLib;
using System.Collections.Generic;

namespace QualityOfLifeONI
{//WIP
    //// This targets both the manual Bottle Emptier and the Piped Bottle Drainer automatically!
    //[HarmonyPatch(typeof(BottleEmptier), "OnSpawn")]
    //public static class BottleEmptier_OnSpawn_Patch
    //{
    //    public static void Postfix(BottleEmptier __instance)
    //    {
    //        Storage storage = __instance.GetComponent<Storage>();
    //        if (storage != null)
    //        {
    //            // Force the storage to seal items so Sublimates won't off-gas Polluted Water
    //            storage.SetDefaultStoredItemModifiers(new List<Storage.StoredItemModifier>
    //            {
    //                Storage.StoredItemModifier.Hide,
    //                Storage.StoredItemModifier.Seal,
    //                Storage.StoredItemModifier.Insulate
    //            });

    //            // Apply it immediately to any items currently inside the building
    //            foreach (var item in storage.items)
    //            {
    //                if (item != null)
    //                {
    //                    var sublimates = item.GetComponent<Sublimates>();
    //                    if (sublimates != null)
    //                    {
    //                        // Sublimates checks if storage has Seal; forcing it stopped here
    //                        storage.Drop(item, true); // re-adds or updates storage flags
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}

    [HarmonyPatch(typeof(ManualGeneratorConfig), "CreateBuildingDef")]
    class ManualGeneratorConfig_CreateBuildingDef
    {
        public static void Postfix(BuildingDef __result)
        {
            __result.GeneratorWattageRating = 600f;
        }
    }
}
