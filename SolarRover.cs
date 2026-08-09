using System;
using KSerialization;
using Klei.AI;
using UnityEngine;

namespace QualityOfLifeONI
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class RoverSolarCharger : KMonoBehaviour, ISim200ms, IActivationRangeTarget
    {
        // The UI thresholds
        [Serialize]
        private float startChargingPercent = 50f;
        [Serialize]
        private float stopChargingPercent = 95f;

        private bool isCharging;
        private AmountInstance batteryAmount;
        private Effects effects;

        // AI variables
        private RoverSolarRechargeChore rechargeChore;
        private float searchCooldown = 0f;

        // Exposed so the Chore can ask if it should stop charging
        public bool IsChargingExpected => isCharging;

        protected override void OnSpawn()
        {
            base.OnSpawn();
            batteryAmount = this.gameObject.GetAmounts().Get(Db.Get().Amounts.InternalChemicalBattery);
            effects = this.gameObject.GetComponent<Effects>();
        }

        public void Sim200ms(float dt)
        {
            if (batteryAmount == null) return;

            // --- CRASH PREVENTION SAFEGUARDS ---
            // 1. If the Rover is still inside the Lander/Rocket pod, DO NOT RUN.
            if (this.transform.parent != null) return;

            // 2. If the Rover hasn't been assigned to a valid World/Grid cell yet, DO NOT RUN.
            int currentCell = Grid.PosToCell(this.gameObject);
            if (!Grid.IsValidCell(currentCell)) return;

            WorldContainer world = this.gameObject.GetMyWorld();
            if (world == null) return;
            // -----------------------------------

            float currentCyclePercent = GameClock.Instance.GetCurrentCycleAsPercentage();
            float currentBatteryPercent = (batteryAmount.value / batteryAmount.GetMax()) * 100f;
            bool isDaytime = currentCyclePercent >= (5f / 24f) && currentCyclePercent <= (19f / 24f);

            // Toggle logic
            if (isCharging)
            {
                if (currentBatteryPercent >= stopChargingPercent || !isDaytime)
                    isCharging = false;
            }
            else
            {
                if (currentBatteryPercent <= startChargingPercent && isDaytime)
                    isCharging = true;
            }

            // AI CHORE MANAGEMENT & CHARGING logic
            if (isCharging)
            {
                if (searchCooldown > 0f) searchCooldown -= dt;

                // If chore died (couldn't find light) and cooldown is done, create a new one to hunt again
                if ((rechargeChore == null || rechargeChore.isComplete) && searchCooldown <= 0f)
                {
                    rechargeChore = new RoverSolarRechargeChore(this, this);
                    searchCooldown = 5f; // Cooldown to save CPU performance
                }

                // Apply charging if standing in light
                int lux = Grid.LightIntensity[currentCell];
                float watts = Mathf.Clamp((float)lux * 0.00053f, 0f, 380f);

                if (watts > 0)
                {
                    batteryAmount.ApplyDelta(watts * dt);

                    if (effects != null && !effects.HasEffect("ScoutBotCharging"))
                    {
                        effects.Add("ScoutBotCharging", false);
                    }
                    return;
                }
            }
            else
            {
                // Cancel chore when daytime ends or battery is full
                if (rechargeChore != null && !rechargeChore.isComplete)
                {
                    rechargeChore.Cancel("Charging finished or daytime ended");
                    rechargeChore = null;
                }
            }

            // Turn off effect if no active wattage
            if (effects != null && effects.HasEffect("ScoutBotCharging"))
            {
                effects.Remove("ScoutBotCharging");
            }
        }

        // --- IActivationRangeTarget Implementation (For the UI Slider) ---

        // Top Slider in UI (ActivateValue) -> Stop Charging (High threshold, must be >= Bottom)
        public float ActivateValue
        {
            get => stopChargingPercent;
            set
            {
                stopChargingPercent = value;
                // If Stop goes below Start, push Start down with it
                if (stopChargingPercent < startChargingPercent)
                {
                    startChargingPercent = stopChargingPercent;
                }
            }
        }

        // Bottom Slider in UI (DeactivateValue) -> Start Charging (Low threshold, must be <= Top)
        public float DeactivateValue
        {
            get => startChargingPercent;
            set
            {
                startChargingPercent = value;
                // If Start goes above Stop, push Stop up with it
                if (startChargingPercent > stopChargingPercent)
                {
                    stopChargingPercent = startChargingPercent;
                }
            }
        }

        public float MinValue => 0f;
        public float MaxValue => 100f;
        public bool UseWholeNumbers => true;

        public string ActivationRangeTitleText => "Rover Solar Charging Thresholds";

        // Top slider controls Stop Charging, Bottom slider controls Start Charging
        public string ActivateSliderLabelText => "Stop Charging";
        public string DeactivateSliderLabelText => "Start Charging";

        public string ActivateTooltip => "The Rover will stop charging when battery reaches this percentage";
        public string DeactivateTooltip => "The Rover will start solar charging when battery falls below this percentage";
    }
}