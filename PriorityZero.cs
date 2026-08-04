using System;
using System.Collections.Generic;
using HarmonyLib;
using STRINGS;
using UnityEngine;
using UnityEngine.Windows;

namespace QualityOfLifeONI
{
    // A simple custom MonoBehaviour attached to our new 0 button to listen for the Backslash key
    public class PriorityZeroHotkey : MonoBehaviour
    {
        public PriorityButton targetButton;

        void Update()
        {
            // Listen for the Backslash (\) key
            if (UnityEngine.Input.GetKeyDown(KeyCode.Backslash))
            {
                // Only trigger if the priority screen is active, visible, and the button has logic
                if (targetButton != null && targetButton.isActiveAndEnabled && targetButton.onClick != null)
                {
                    targetButton.onClick(targetButton.priority);
                }
            }
        }
    }

    // 1. Intercept InstantiateButtons to append our "0" button (Priority Level 10)
    [HarmonyPatch(typeof(PriorityScreen), "InstantiateButtons")]
    public static class PriorityScreen_InstantiateButtons_Patch
    {
        public static void Postfix(PriorityScreen __instance, Action<PrioritySetting> on_click, bool playSelectionSound)
        {
            // Access the private fields using Harmony's Traverse
            var buttonPrefab_basic = Traverse.Create(__instance).Field("buttonPrefab_basic").GetValue<PriorityButton>();
            var buttons_basic = Traverse.Create(__instance).Field("buttons_basic").GetValue<List<PriorityButton>>();

            if (buttonPrefab_basic == null || buttons_basic == null) return;

            // Reactivate the basic button prefab momentarily so we can clone it
            buttonPrefab_basic.gameObject.SetActive(true);

            // Instantiate the 10th button. 
            // FIX: force_active = true ensures the button actually renders in the UI!
            PriorityButton priorityButton = Util.KInstantiateUI<PriorityButton>(
                buttonPrefab_basic.gameObject,
                buttonPrefab_basic.transform.parent.gameObject,
                true
            );

            // Safely hide the prefab again
            buttonPrefab_basic.gameObject.SetActive(false);

            // Ensure our new button is fully active in the Unity Hierarchy
            priorityButton.gameObject.SetActive(true);

            // Assign our new PriorityLevel 10 logic, but display it visually as "0"
            buttons_basic.Add(priorityButton);
            priorityButton.playSelectionSound = playSelectionSound;
            priorityButton.onClick = on_click;
            priorityButton.text.text = "0";
            priorityButton.priority = new PrioritySetting(PriorityScreen.PriorityClass.basic, 10);

            // Inject our custom Hotkey component to handle the '\' key cleanly
            var hotkeyHandler = priorityButton.gameObject.AddComponent<PriorityZeroHotkey>();
            hotkeyHandler.targetButton = priorityButton;
        }
    }

    // 2. Intercept SetScreenPriority to ensure our 10th button maintains the "0" string when updated
    [HarmonyPatch(typeof(PriorityScreen), "SetScreenPriority")]
    public static class PriorityScreen_SetScreenPriority_Patch
    {
        public static void Postfix(PriorityScreen __instance)
        {
            var buttons_basic = Traverse.Create(__instance).Field("buttons_basic").GetValue<List<PriorityButton>>();
            var button_toggleHigh = Traverse.Create(__instance).Field("button_toggleHigh").GetValue<KToggle>();

            // Klei's base code loops through the list and automatically applies (index + 1) to the text.
            // We need to overwrite the 10th button (index 9) so it stays visually labelled as "0" instead of "10".
            if (buttons_basic != null && buttons_basic.Count == 10)
            {
                PriorityButton button0 = buttons_basic[9];

                // Force the internal text back to "0"
                button0.text.text = "0";

                // Ensure the tooltip reflects "0" and shows our new custom hotkey '\'
                string tooltipFormat = button_toggleHigh != null && button_toggleHigh.isOn
                    ? UI.PRIORITYSCREEN.HIGH
                    : UI.PRIORITYSCREEN.BASIC;

                button0.tooltip.SetSimpleTooltip(string.Format(tooltipFormat, "0") + " (Hotkey: \\)");
            }
        }
    }
}