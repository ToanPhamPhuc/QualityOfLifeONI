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
                    searchCooldown = 5f; // Wait 5 seconds before making another chore to prevent CPU lag
                }

                // Actually apply charge if we are standing in the light
                int cell = Grid.PosToCell(this.gameObject);
                if (Grid.IsValidCell(cell))
                {
                    int lux = Grid.LightIntensity[cell];
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
            }
            else
            {
                // If it hits 100% or night falls, kill the chore so it can go back to work
                if (rechargeChore != null && !rechargeChore.isComplete)
                {
                    rechargeChore.Cancel("Charging finished or daytime ended");
                    rechargeChore = null;
                }
            }

            // Remove visual effect if we aren't actively getting watts
            if (effects != null && effects.HasEffect("ScoutBotCharging"))
            {
                effects.Remove("ScoutBotCharging");
            }
        }

        // --- IActivationRangeTarget Implementation (For the UI Slider) ---
        public float ActivateValue { get => startChargingPercent; set => startChargingPercent = value; }
        public float DeactivateValue { get => stopChargingPercent; set => stopChargingPercent = value; }
        public float MinValue => 0f;
        public float MaxValue => 100f;
        public bool UseWholeNumbers => true;

        public string ActivateTooltip => "The Rover will start solar charging when battery falls below this percentage";
        public string DeactivateTooltip => "The Rover will stop charging when battery reaches this percentage";
        public string ActivationRangeTitleText => "Rover Solar Charging Thresholds";
        public string ActivateSliderLabelText => "Start Charging";
        public string DeactivateSliderLabelText => "Stop Charging";
    }
}