using HarmonyLib;
using System.Collections.Generic;

namespace MoreAutomations
{
#region Robo-pilot Module
    // 1. Add Output Automation Port to the Robo-Pilot Module Building Def
    [HarmonyPatch(typeof(RoboPilotModuleConfig), nameof(RoboPilotModuleConfig.CreateBuildingDef))]
    public static class RoboPilotModuleConfig_CreateBuildingDef_Patch
    {
        public static void Postfix(ref BuildingDef __result)
        {
            // Create single logic output port at Offset (1, 0)
            __result.LogicOutputPorts = new List<LogicPorts.Port>
            {
                LogicPorts.Port.OutputPort(
                    LogicSwitch.PORT_ID,
                    new CellOffset(1, 0),
                    "Data Bank Full Signal",
                    "Sends a Green signal when Data Banks are fully loaded, otherwise Red signal.",
                    "Data Banks Not Full",
                    false
                )
            };
        }
    }

    // 2. Drive the Logic Signal based on Data Bank Storage
    [HarmonyPatch(typeof(RoboPilotModule), "OnSpawn")]
    public static class RoboPilotModule_OnSpawn_Patch
    {
        public static void Postfix(RoboPilotModule __instance)
        {
            // Subscribe storage change event to update output signal
            __instance.Subscribe((int)GameHashes.OnStorageChange, (data) =>
            {
                UpdateAutomationSignal(__instance);
            });

            // Initial signal update on spawn
            UpdateAutomationSignal(__instance);
        }

        private static void UpdateAutomationSignal(RoboPilotModule module)
        {
            LogicPorts component = module.GetComponent<LogicPorts>();
            if (component != null)
            {
                // Checks if Data Banks stored equals or exceeds capacity (100kg / 100 units)
                bool isFull = module.IsFull(); // Uses built-in IsFull() method

                component.SendSignal(LogicSwitch.PORT_ID, isFull ? 1 : 0);
            }
        }
    }
#endregion
}