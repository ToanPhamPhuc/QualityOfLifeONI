using HarmonyLib;

namespace QualityOfLifeONI
{
    [HarmonyPatch(typeof(SeedPlantingMonitor.Instance), nameof(SeedPlantingMonitor.Instance.RefreshSearchTime))]
    public static class FastPipPlanting
    {
        public static bool Prefix(SeedPlantingMonitor.Instance __instance)
        {
            // Set next search time to 1-3 seconds from now instead of 60-300 seconds!
            __instance.nextSearchTime = UnityEngine.Time.time + UnityEngine.Random.Range(1f, 5f);

            return false; // Skip vanilla RefreshSearchTime
        }
    }
}