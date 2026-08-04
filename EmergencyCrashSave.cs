using HarmonyLib;
using System;
using System.IO;

namespace QualityOfLifeONI
{
    // Hook directly into the method KCrashReporter uses to display the crash dialog
    [HarmonyPatch(typeof(KCrashReporter), "ShowDialog")]
    public static class KCrashReporter_ShowDialog_Patch
    {
        // A flag to ensure we only save once per crash event to prevent cascades
        private static bool hasEmergencySaved = false;

        public static void Prefix()
        {
            if (hasEmergencySaved) return;

            try
            {
                // Ensure the game is actually running a simulation and the save system exists
                if (SaveLoader.Instance != null && Game.Instance != null)
                {
                    Debug.Log("[Emergency Crash Save] Game crash detected via ShowDialog! Attempting emergency save...");

                    // Get the player's standard save directory
                    string saveFolder = SaveLoader.GetSavePrefixAndCreateFolder();

                    // Create a unique file specifically for the crash so we don't corrupt their main save
                    string filename = Path.Combine(saveFolder, "Emergency_Crash_Save.sav");

                    // Force the save. 
                    // Arg 2 (isAutoSave) = false
                    // Arg 3 (updateSavePointer) = false -> Prevents this file from overwriting the "Continue" button on the main menu
                    SaveLoader.Instance.Save(filename, false, false);

                    Debug.Log("[Emergency Crash Save] SUCCESS! Saved to: " + filename);
                    hasEmergencySaved = true;
                }
            }
            catch (Exception e)
            {
                // If the crash is so severe that it broke the SaveLoader itself, catch it safely
                // so we don't prevent the crash screen from doing its job.
                Debug.LogWarning("[Emergency Crash Save] FAILED to save during crash. The game state was too corrupted. Error: " + e.Message);
            }
        }
    }
}