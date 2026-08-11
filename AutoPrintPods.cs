using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace QualityOfLifeONI
{
    // Hooks into the Immigration class's 200ms tick update
    [HarmonyPatch(typeof(Immigration), "Sim200ms")]
    public class AutoPrintPods_Immigration_Patch
    {
        private static float afkTimer = 0f;
        private const float AFK_TIMEOUT = 25f; // 25 seconds of AFK time

        public static void Postfix(Immigration __instance, float dt)
        {
            if (__instance == null) return;

            // Check if the pod's 2.5-3 cycle timer has finished
            if (__instance.ImmigrantsAvailable)
            {
                afkTimer += dt;

                // If AFK timer exceeds 25 seconds, print automatically
                if (afkTimer >= AFK_TIMEOUT)
                {
                    AutoPrintPackage(__instance);
                    afkTimer = 0f; // Reset timer for the next cycle
                }
            }
            else
            {
                // Reset the timer if the player manually interacts before 25s
                afkTimer = 0f;
            }
        }

        private static void AutoPrintPackage(Immigration immigration)
        {
            // Filter out Dupes/Bionics by strictly using the carePackages list
            List<CarePackageInfo> validPackages = new List<CarePackageInfo>();
            foreach (CarePackageInfo pkg in immigration.carePackages)
            {
                // Ensure the player has met the requirements (e.g., discovered the element)
                if (pkg.requirement == null || pkg.requirement())
                {
                    validPackages.Add(pkg);
                }
            }

            // Fallback in case no care packages are currently valid
            if (validPackages.Count == 0)
            {
                immigration.EndImmigration();
                return;
            }

            // Select a random care package from the valid list
            CarePackageInfo selectedPackage = validPackages[UnityEngine.Random.Range(0, validPackages.Count)];

            // Find a valid active telepad (Printing Pod or Exobase Mini Pod)
            Telepad targetTelepad = GameUtil.GetActiveTelepad();

            // Fallback if GetActiveTelepad fails: grab the first available pad
            if (targetTelepad == null && Components.Telepads.Count > 0)
            {
                targetTelepad = Components.Telepads.Items[0];
            }

            if (targetTelepad != null)
            {
                // Deliver the chosen package directly at the telepad's location
                selectedPackage.Deliver(targetTelepad.transform.position);
                Debug.Log($"[AutoPrintPods] AFK detected. Automatically printed: {selectedPackage.id}");
            }

            // Tell the game the printing phase is over, resetting the 3-cycle timer
            immigration.EndImmigration();
        }
    }
}