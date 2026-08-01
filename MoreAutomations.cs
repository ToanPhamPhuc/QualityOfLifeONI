using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using static STRINGS.BUILDINGS.PREFABS;

namespace MoreAutomations
{
#region Robo-pilot Module fixed

    // 1. Add Logic Output Port at Offset (0, 1)
    [HarmonyPatch(typeof(RoboPilotModuleConfig), nameof(RoboPilotModuleConfig.CreateBuildingDef))]
    public static class RoboPilotModuleConfig_CreateBuildingDef_Patch
    {
        public static void Postfix(ref BuildingDef __result)
        {
            __result.LogicOutputPorts = new List<LogicPorts.Port>
            {
                LogicPorts.Port.OutputPort(
                    LogicSwitch.PORT_ID,
                    new CellOffset(0, 1),
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
            if (__instance == null) return;

            // Update signal whenever storage changes
            __instance.Subscribe((int)GameHashes.OnStorageChange, _ => UpdateAutomationSignal(__instance));

            // FIX: Use GameScheduler to delay signal update by 0.2 seconds after spawn/load
            GameScheduler.Instance.Schedule("DeferredRoboPilotSignal", 0.2f, _ => UpdateAutomationSignal(__instance));
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

#region Cargo Bay, Artifact Transport Modules

    public static class CargoBayAutomationHelper
    {
        public const string PORT_ID = "CargoBayEmptyInputPort";

        // Helper to attach input port definition
        public static void AddAutomationPort(BuildingDef def)
        {
            if (def == null) return;

            if (def.LogicInputPorts == null)
                def.LogicInputPorts = new List<LogicPorts.Port>();

            def.LogicInputPorts.Add(
                LogicPorts.Port.InputPort(
                    PORT_ID,
                    new CellOffset(0, 1),
                    "Empty Storage",
                    "A GREEN signal instantly empties all contents onto the ground.",
                    "A RED signal does nothing.",
                    false
                )
            );
        }

        // Helper to handle incoming logic signal
        public static void OnLogicEvent(GameObject go, int data)
        {
            // Checking if the port signal received is GREEN (1)
            if (LogicCircuitNetwork.IsBitActive(0, data))
            {
                Storage storage = go.GetComponent<Storage>();
                if (storage != null && storage.Count > 0)
                {
                    storage.DropAll(false, false, default, true);
                }
            }
        }
    }

    // --- 1. ADD INPUT PORTS TO BUILDING DEFS ---

    // Special Large Cargo Bay Config
    [HarmonyPatch(typeof(SpecialCargoBayClusterConfig), nameof(SpecialCargoBayClusterConfig.CreateBuildingDef))]
    public static class SpecialCargoBayClusterConfig_CreateBuildingDef_Patch
    {
        public static void Postfix(ref BuildingDef __result) => CargoBayAutomationHelper.AddAutomationPort(__result);
    }

    // --- 2. LISTEN FOR LOGIC SIGNALS ON SPAWN ---

    // Cargo Bay Cluster Component (Handles Small/Liquid/Gas/Solid Cargo Bays)
    [HarmonyPatch(typeof(CargoBayCluster), "OnSpawn")]
    public static class CargoBayCluster_OnSpawn_Patch
    {
        public static void Postfix(CargoBayCluster __instance)
        {
            if (__instance == null) return;

            __instance.Subscribe((int)GameHashes.LogicEvent, (data) =>
            {
                if (data is LogicValueChanged valueChanged && valueChanged.portID == CargoBayAutomationHelper.PORT_ID)
                {
                    CargoBayAutomationHelper.OnLogicEvent(__instance.gameObject, valueChanged.newValue);
                }
            });
        }
    }

    // Artifact Transport Module Component
    [HarmonyPatch(typeof(ArtifactSelector), "OnSpawn")]
    public static class ArtifactSelector_OnSpawn_Patch
    {
        public static void Postfix(ArtifactSelector __instance)
        {
            if (__instance == null) return;

            __instance.Subscribe((int)GameHashes.LogicEvent, (data) =>
            {
                if (data is LogicValueChanged valueChanged && valueChanged.portID == CargoBayAutomationHelper.PORT_ID)
                {
                    CargoBayAutomationHelper.OnLogicEvent(__instance.gameObject, valueChanged.newValue);
                }
            });
        }
    }

#endregion
}