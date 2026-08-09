using System;
using System.Collections.Generic;
using HarmonyLib;

namespace QualityOfLifeONI
{
    #region Robo-pilot Module fixed

    // Helper to add extension methods for RoboPilotModule.cs
    public static class RoboPilotModuleExtensions
    {
        // 1. New method to check if data bank is at least 80kg or full (100kg)
        public static bool IsEnough(this RoboPilotModule module)
        {
            if (module == null) return false;
            bool isFull = module.IsFull();
            bool isEnough = module.GetDataBanksStored() >= 80f; // Fixed: use correct method
            return isFull || isEnough;
                
        }
    }

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
                    "Data Bank Signal",
                    "Sends a Green signal when Data Banks are at least 80/100kg, otherwise Red signal.",
                    "Data Banks required value not met",
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

            ScheduleRecurringSignalUpdate(__instance);
        }

        // Method to checking logic signal every 5 seconds
        private static void ScheduleRecurringSignalUpdate(RoboPilotModule module)
        {
            if (module == null || module.gameObject == null) return;
            UpdateAutomationSignal(module);
            GameScheduler.Instance.Schedule("RoboPilotSignalUpdate", 5f, _ => ScheduleRecurringSignalUpdate(module));
        }
        public static void UpdateAutomationSignal(RoboPilotModule module)
        {
            if (module == null) return;

            LogicPorts component = module.GetComponent<LogicPorts>();
            if (component != null)
            {
                bool isEnough = module.IsEnough();
                component.SendSignal(LogicSwitch.PORT_ID, isEnough ? 1 : 0);
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

/*    #region Cargo Bay, Artifact Transport Modules

    public static class CargoBayAutomationHelper
    {
        public const string PORT_ID = "CargoBayEmptyInputPort";

        // Helper to attach input port definition safely
        public static void AddAutomationPort(BuildingDef def)
        {
            if (def == null) return;

            if (def.LogicInputPorts == null)
                def.LogicInputPorts = new List<LogicPorts.Port>();

            // Check if port already exists to avoid duplicate port crashes
            if (def.LogicInputPorts.Exists(p => p.id == PORT_ID)) return;

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

        // Drop all contents inside storage safely
        public static void EmptyModuleStorage(GameObject go)
        {
            if (go == null) return;

            // Target Storage components on the module
            Storage[] storages = go.GetComponents<Storage>();
            if (storages != null)
            {
                foreach (Storage storage in storages)
                {
                    if (storage != null && storage.Count > 0)
                    {
                        storage.DropAll(false, false, default, true);
                    }
                }
            }
        }
    }

    // --- 1. ADD INPUT PORTS TO BUILDING DEFS ---

    // Solid Cargo Bay
    [HarmonyPatch(typeof(SolidCargoBayClusterConfig), nameof(SolidCargoBayClusterConfig.CreateBuildingDef))]
    public static class SolidCargoBayClusterConfig_CreateBuildingDef_Patch
    {
        public static void Postfix(ref BuildingDef __result) => CargoBayAutomationHelper.AddAutomationPort(__result);
    }

    // Liquid Cargo Bay
    [HarmonyPatch(typeof(LiquidCargoBayClusterConfig), nameof(LiquidCargoBayClusterConfig.CreateBuildingDef))]
    public static class LiquidCargoBayClusterConfig_CreateBuildingDef_Patch
    {
        public static void Postfix(ref BuildingDef __result) => CargoBayAutomationHelper.AddAutomationPort(__result);
    }

    // Gas Cargo Bay
    [HarmonyPatch(typeof(GasCargoBayClusterConfig), nameof(GasCargoBayClusterConfig.CreateBuildingDef))]
    public static class GasCargoBayClusterConfig_CreateBuildingDef_Patch
    {
        public static void Postfix(ref BuildingDef __result) => CargoBayAutomationHelper.AddAutomationPort(__result);
    }

    // Large Cargo Bay
    [HarmonyPatch(typeof(SpecialCargoBayClusterConfig), nameof(SpecialCargoBayClusterConfig.CreateBuildingDef))]
    public static class SpecialCargoBayClusterConfig_CreateBuildingDef_Patch
    {
        public static void Postfix(ref BuildingDef __result) => CargoBayAutomationHelper.AddAutomationPort(__result);
    }

    // Artifact Transport Module
    [HarmonyPatch(typeof(ArtifactCargoBayConfig), nameof(ArtifactCargoBayConfig.CreateBuildingDef))]
    public static class ArtifactCargoBayConfig_CreateBuildingDef_Patch
    {
        public static void Postfix(ref BuildingDef __result) => CargoBayAutomationHelper.AddAutomationPort(__result);
    }


    // --- 2. SAFE LOGIC EVENT HANDLING ---

    [HarmonyPatch(typeof(LogicPorts), "OnLogicEvent")]
    public static class LogicPorts_OnLogicEvent_Patch
    {
        public static void Postfix(LogicPorts __instance, HashedString portID, int newValue)
        {
            try
            {
                if (__instance == null || portID == null) return;

                // Compare string representation to prevent HashedString comparison crashes
                if (portID.ToString() == CargoBayAutomationHelper.PORT_ID)
                {
                    // Check if GREEN signal (1)
                    if (LogicCircuitNetwork.IsBitActive(0, newValue))
                    {
                        CargoBayAutomationHelper.EmptyModuleStorage(__instance.gameObject);
                    }
                }
            }
            catch
            {
                // Catch any unexpected edge-case exceptions during network ticks
            }
        }
    }

    #endregion
*/}