using System.Collections.Generic;
using HarmonyLib;

namespace MoreAutomations
{
#region Robo-pilot Module fixed

    // 1. Add Logic Output Port at Offset (1, 0) - Center Tile
    [HarmonyPatch(typeof(RoboPilotModuleConfig), nameof(RoboPilotModuleConfig.CreateBuildingDef))]
    public static class RoboPilotModuleConfig_CreateBuildingDef_Patch
    {
        public static void Postfix(ref BuildingDef __result)
        {
            __result.LogicOutputPorts = new List<LogicPorts.Port>
            {
                LogicPorts.Port.OutputPort(
                    LogicSwitch.PORT_ID,
                    new CellOffset(-1, 0), // (1, 0) = Center, (2, 0) = Right edge
                    "Data Bank Full Signal",
                    "Sends a Green signal when Data Banks are fully loaded (100 kg), otherwise Red signal.",
                    "Data Banks Not Full",
                    false
                )
            };
        }
    }

    // 2. Control Automation Output Signal
    [HarmonyPatch(typeof(RoboPilotModule), "OnSpawn")]
    public static class RoboPilotModule_OnSpawn_Patch
    {
        public static void Postfix(RoboPilotModule __instance)
        {
            // Update signal whenever storage changes
            __instance.Subscribe((int)GameHashes.OnStorageChange, _ => UpdateAutomationSignal(__instance));

            // FIX: Delay the initial automation signal update by 1 frame upon loading a save
            // to ensure the LogicPorts network has finished connecting.
            __instance.StartCoroutine(DeferredInitialSignalCheck(__instance));
        }

        private static System.Collections.IEnumerator DeferredInitialSignalCheck(RoboPilotModule module)
        {
            yield return null; // Wait for next frame
            UpdateAutomationSignal(module);
        }

        public static void UpdateAutomationSignal(RoboPilotModule module)
        {
            if (module == null) return;

            LogicPorts component = module.GetComponent<LogicPorts>();
            if (component != null)
            {
                bool isFull = module.IsFull();
                component.SendSignal(LogicSwitch.PORT_ID, isFull ? 1 : 0);
            }
        }
    }

    // 3. Override Data Bank Request Logic to ALWAYS target 100/100 kg
    [HarmonyPatch(typeof(RoboPilotModule), "RequestDataBanksForDestination")]
    public static class RoboPilotModule_RequestDataBanks_Patch
    {
        public static void Postfix(RoboPilotModule __instance)
        {
            ManualDeliveryKG delivery = __instance.GetComponent<ManualDeliveryKG>();
            Storage storage = __instance.GetComponent<Storage>();

            if (delivery != null && storage != null)
            {
                float missingMass = storage.Capacity() - storage.UnitsStored();

                if (missingMass > 0f)
                {
                    delivery.refillMass = storage.Capacity();
                }
            }
        }
    }
#endregion
}