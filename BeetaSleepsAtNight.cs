using HarmonyLib;

namespace BeetaSleepsAtNight
{
    // Patches the monitor that dictates when a Beeta decides to go to sleep
    [HarmonyPatch(typeof(BeeSleepMonitor), nameof(BeeSleepMonitor.ShouldSleep))]
    public class BeeSleepMonitor_ShouldSleep_Patch
    {
        public static void Postfix(BeeSleepMonitor.Instance smi, ref bool __result)
        {
            // If the game clock says it's nighttime, override the result to true so they sleep
            if (GameClock.Instance != null && GameClock.Instance.IsNighttime())
            {
                __result = true;
            }
        }
    }

    // Patches the state machine that determines when a Beeta is allowed to wake up
    [HarmonyPatch(typeof(BeeSleepStates), nameof(BeeSleepStates.ShouldWakeUp))]
    public class BeeSleepStates_ShouldWakeUp_Patch
    {
        public static void Postfix(BeeSleepStates.Instance smi, ref bool __result)
        {
            // If the bee is about to wake up (e.g., CO2 exposure reached 0) but it is STILL nighttime,
            // override the result to false so they stay asleep until morning.
            if (GameClock.Instance != null && GameClock.Instance.IsNighttime())
            {
                __result = false;
            }
        }
    }
}