using HarmonyLib;

namespace QualityOfLifeONI // Ensure namespace matches so it can access ModInit easily
{
    public static class BeetaSleepsAtNightMod
    {
        public static bool IsBeetaSleepTime()
        {
            if (GameClock.Instance == null) return false;

            // Read safely from our central config
            int blocks = 3;
            if (ModInit.Config != null)
            {
                blocks = ModInit.Config.SleepBlocks;
            }

            float timeIntoCycle = GameClock.Instance.GetTime() % 600f;
            float sleepThreshold = 600f - (blocks * 25f);

            return timeIntoCycle >= sleepThreshold;
        }
    }

    [HarmonyPatch(typeof(BeeSleepMonitor), "ShouldSleep")]
    public class BeeSleepMonitor_ShouldSleep_Patch
    {
        public static void Postfix(ref bool __result)
        {
            if (BeetaSleepsAtNightMod.IsBeetaSleepTime()) __result = true;
        }
    }

    [HarmonyPatch(typeof(BeeSleepStates), "ShouldWakeUp")]
    public class BeeSleepStates_ShouldWakeUp_Patch
    {
        public static void Postfix(ref bool __result)
        {
            if (BeetaSleepsAtNightMod.IsBeetaSleepTime()) __result = false;
        }
    }
}