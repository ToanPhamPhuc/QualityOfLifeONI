using System;
using System.Collections.Generic;
using HarmonyLib;
using STRINGS;
using UnityEngine;

namespace PriorityZeroMod
{
    // Custom MonoBehaviour attached to our button to listen for the Backslash key
    public class PriorityZeroHotkey : MonoBehaviour
    {
        public PriorityButton targetButton;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Backslash))
            {
                if (targetButton != null && targetButton.isActiveAndEnabled && targetButton.onClick != null)
                {
                    targetButton.onClick(targetButton.priority);
                }
            }
        }
    }

    // 1. Intercept InstantiateButtons to append our "!" button
    [HarmonyPatch(typeof(PriorityScreen), "InstantiateButtons")]
    public static class PriorityScreen_InstantiateButtons_Patch
    {
        public static void Postfix(PriorityScreen __instance, Action<PrioritySetting> on_click, bool playSelectionSound)
        {
            var buttonPrefab_basic = Traverse.Create(__instance).Field("buttonPrefab_basic").GetValue<PriorityButton>();
            var buttons_basic = Traverse.Create(__instance).Field("buttons_basic").GetValue<List<PriorityButton>>();

            if (buttonPrefab_basic == null || buttons_basic == null) return;

            // Prevent adding duplicate buttons if InstantiateButtons runs multiple times
            if (buttons_basic.Exists(b => b != null && b.priority.priority_class == PriorityScreen.PriorityClass.high)) return;

            // Reactivate the basic button prefab momentarily so we can clone it
            buttonPrefab_basic.gameObject.SetActive(true);

            PriorityButton priorityButton = Util.KInstantiateUI<PriorityButton>(
                buttonPrefab_basic.gameObject,
                buttonPrefab_basic.transform.parent.gameObject,
                true
            );

            buttonPrefab_basic.gameObject.SetActive(false);
            priorityButton.gameObject.SetActive(true);

            buttons_basic.Add(priorityButton);
            priorityButton.playSelectionSound = playSelectionSound;
            priorityButton.onClick = on_click;
            priorityButton.text.text = "!";

            // FIX: Use PriorityClass.high with value 9!
            // Value 9 keeps it in the valid 1-9 range (preventing Chore constructor crashes),
            // while PriorityClass.high ranks it strictly above basic 1-9 and below topPriority (Yellow Alert).
            priorityButton.priority = new PrioritySetting(PriorityScreen.PriorityClass.high, 9);

            var hotkeyHandler = priorityButton.gameObject.AddComponent<PriorityZeroHotkey>();
            hotkeyHandler.targetButton = priorityButton;
        }
    }

    // 2. Intercept SetScreenPriority to ensure our button maintains the "!" text and proper setting
    [HarmonyPatch(typeof(PriorityScreen), "SetScreenPriority")]
    public static class PriorityScreen_SetScreenPriority_Patch
    {
        public static void Postfix(PriorityScreen __instance)
        {
            var buttons_basic = Traverse.Create(__instance).Field("buttons_basic").GetValue<List<PriorityButton>>();
            var button_toggleHigh = Traverse.Create(__instance).Field("button_toggleHigh").GetValue<KToggle>();

            if (buttons_basic != null && buttons_basic.Count >= 10)
            {
                PriorityButton exclamationButton = buttons_basic[9];

                if (exclamationButton != null)
                {
                    // Ensure the priority stays as High, 9
                    exclamationButton.priority = new PrioritySetting(PriorityScreen.PriorityClass.high, 9);

                    // Force the visual text back to "!"
                    exclamationButton.text.text = "!";

                    string tooltipFormat = button_toggleHigh != null && button_toggleHigh.isOn
                        ? UI.PRIORITYSCREEN.HIGH
                        : UI.PRIORITYSCREEN.BASIC;

                    exclamationButton.tooltip.SetSimpleTooltip(string.Format(tooltipFormat, "!") + " (Hotkey: \\)");
                }
            }
        }
    }
}