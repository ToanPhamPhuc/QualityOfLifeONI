using System;
using UnityEngine;

namespace QualityOfLifeONI
{
    // A custom AI task that tells the Rover to find light and wait in it
    public class RoverSolarRechargeChore : Chore<RoverSolarRechargeChore.StatesInstance>
    {
        public RoverSolarRechargeChore(IStateMachineTarget target, RoverSolarCharger charger)
            : base(
                  Db.Get().ChoreTypes.Recharge,
                  target,
                  target.GetComponent<ChoreProvider>(),
                  false,
                  (Action<Chore>)null,
                  (Action<Chore>)null,
                  (Action<Chore>)null,
                  PriorityScreen.PriorityClass.personalNeeds, // High priority so it drops other tasks
                  5,
                  false,
                  true,
                  0,
                  false,
                  ReportManager.ReportType.PersonalTime)
        {
            this.smi = new StatesInstance(this, target.gameObject, charger);
        }

        public class StatesInstance : GameStateMachine<States, StatesInstance, RoverSolarRechargeChore, object>.GameInstance
        {
            public RoverSolarCharger charger;

            public StatesInstance(RoverSolarRechargeChore master, GameObject rover, RoverSolarCharger charger) : base(master)
            {
                this.charger = charger;
            }

            // Scans the map to find the best, closest sunlit tile
            public int FindSunlightCell()
            {
                int startCell = Grid.PosToCell(this.gameObject);
                int bestCell = Grid.InvalidCell;
                int maxLux = 0;
                int minDistance = 9999;
                Navigator navigator = this.GetComponent<Navigator>();

                if (navigator == null) return Grid.InvalidCell;

                // Search a 30x30 tile radius around the Rover
                int radius = 30;
                for (int x = -radius; x <= radius; x++)
                {
                    for (int y = -radius; y <= radius; y++)
                    {
                        int cell = Grid.OffsetCell(startCell, x, y);
                        if (Grid.IsValidCell(cell) && Grid.LightIntensity[cell] > 0)
                        {
                            int dist = Math.Abs(x) + Math.Abs(y);

                            // Prioritize brighter cells. If equal brightness, prioritize closer cells.
                            if (Grid.LightIntensity[cell] > maxLux || (Grid.LightIntensity[cell] == maxLux && dist < minDistance))
                            {
                                // Pathfinding check (expensive, so we do it last)
                                if (navigator.CanReach(cell))
                                {
                                    maxLux = Grid.LightIntensity[cell];
                                    minDistance = dist;
                                    bestCell = cell;
                                }
                            }
                        }
                    }
                }
                return bestCell;
            }
        }

        public class States : GameStateMachine<States, StatesInstance, RoverSolarRechargeChore>
        {
            public TargetParameter rover;
            public IntParameter targetCell;

            public State find_light;
            public State moving;
            public State charging;

            public override void InitializeStates(out BaseState default_state)
            {
                default_state = find_light;

                // 1. Find a sunny tile
                find_light
                    .Enter(smi => {
                        int cell = smi.FindSunlightCell();
                        if (cell != Grid.InvalidCell)
                        {
                            smi.sm.targetCell.Set(cell, smi);
                            smi.GoTo(moving);
                        }
                        else
                        {
                            smi.StopSM("No accessible light found");
                        }
                    });

                // 2. Walk to that tile
                moving
                    .MoveTo(smi => smi.sm.targetCell.Get(smi), charging, find_light, false);

                // 3. Stand still and charge
                charging
                    .PlayAnim("idle_default", KAnim.PlayMode.Loop)
                    .Update((smi, dt) => {
                        // If the charger component says we are done, end the task
                        if (smi.charger != null && !smi.charger.IsChargingExpected)
                        {
                            smi.StopSM("Finished charging or daytime ended");
                        }
                        // If the sun moved and we are now in the dark, go find a new spot
                        else if (Grid.LightIntensity[Grid.PosToCell(smi.gameObject)] == 0)
                        {
                            smi.GoTo(find_light);
                        }
                    }, UpdateRate.SIM_200ms);
            }
        }
    }
}