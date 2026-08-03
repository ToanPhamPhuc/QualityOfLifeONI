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
        private float startChargingPercent = 20f;
        [Serialize]
        private float stopChargingPercent = 100f;

        private bool isCharging;
        private AmountInstance batteryAmount;
        private Effects effects;

        protected override void OnSpawn()
        {
            base.OnSpawn();
            // Hook into the Rover's internal chemical battery and effects system
            batteryAmount = this.gameObject.GetAmounts().Get(Db.Get().Amounts.InternalChemicalBattery);
            effects = this.gameObject.GetComponent<Effects>();
        }

        public void Sim200ms(float dt)
        {
            if (batteryAmount == null) return;

            float currentCyclePercent = GameClock.Instance.GetCurrentCycleAsPercentage();
            float currentBatteryPercent = (batteryAmount.value / batteryAmount.GetMax()) * 100f;

            // Calculate if we are between block 5 and 19. 
            // 1 cycle = 24 blocks. Block 5 = 5/24, Block 19 = 19/24.
            bool isDaytime = currentCyclePercent >= (5f / 24f) && currentCyclePercent <= (19f / 24f);

            // State Machine: Check if we should toggle charging ON or OFF
            if (isCharging)
            {
                // Stop if we hit the high threshold, OR if daytime ends
                if (currentBatteryPercent >= stopChargingPercent || !isDaytime)
                {
                    isCharging = false;
                }
            }
            else
            {
                // Start if we drop below the low threshold during daytime
                if (currentBatteryPercent <= startChargingPercent && isDaytime)
                {
                    isCharging = true;
                }
            }

            // Apply charging logic
            if (isCharging)
            {
                int cell = Grid.PosToCell(this.gameObject);
                if (Grid.IsValidCell(cell))
                {
                    int lux = Grid.LightIntensity[cell];

                    // Use the exact Solar Panel formula: lux * 0.00053, capped at 380W 
                    float watts = Mathf.Clamp((float)lux * 0.00053f, 0f, 380f);

                    if (watts > 0)
                    {
                        // Convert watts to joules for this 200ms tick and apply it
                        float joules = watts * dt;
                        batteryAmount.ApplyDelta(joules);

                        // Activate the vanilla Rover charging visual effect 
                        if (effects != null && !effects.HasEffect("ScoutBotCharging"))
                        {
                            effects.Add("ScoutBotCharging", false);
                        }
                        return; // Successfully charged this tick, skip the shut-off below
                    }
                }
            }

            // If we reach here, we are either not charging, or in total darkness. Turn off the effect.
            if (effects != null && effects.HasEffect("ScoutBotCharging"))
            {
                effects.Remove("ScoutBotCharging");
            }
        }

        // --- IActivationRangeTarget Implementation (For the UI Slider) ---

        // Note: The UI slider binds the Left/Bottom slider to ActivateValue and the Right/Top to DeactivateValue.
        public float ActivateValue
        {
            get => startChargingPercent;
            set => startChargingPercent = value;
        }

        public float DeactivateValue
        {
            get => stopChargingPercent;
            set => stopChargingPercent = value;
        }

        public float MinValue => 0f;
        public float MaxValue => 100f;
        public bool UseWholeNumbers => true;

        // Custom text strings for the Rover UI
        public string ActivateTooltip => "The Rover will start solar charging when battery falls below this percentage";
        public string DeactivateTooltip => "The Rover will stop charging when battery reaches this percentage";
        public string ActivationRangeTitleText => "Rover Solar Charging Thresholds";
        public string ActivateSliderLabelText => "Start Charging";
        public string DeactivateSliderLabelText => "Stop Charging";
    }
}