using HarmonyLib;

namespace QualityOfLifeONI
{
    [HarmonyPatch(typeof(Sublimates), nameof(Sublimates.Sim200ms))]
    public static class LiquidBottler_StopOffGas_Patch
    {
        public static bool Prefix(Sublimates __instance)
        {
            Pickupable pickupable = __instance.GetComponent<Pickupable>();

            if (pickupable != null && pickupable.storage != null)
            {
                // Check the KPrefabID tag on the building's GameObject
                KPrefabID buildingPrefabID = pickupable.storage.GetComponent<KPrefabID>();

                if (buildingPrefabID != null)
                {
                    // Check if the storage belongs to the Liquid Bottler
                    if (buildingPrefabID.IsPrefabID("LiquidBottler"))
                    {
                        return false; // Blocks off-gassing!
                    }
                }
            }

            return true; // Allow normal off-gassing everywhere else
        }
    }
}