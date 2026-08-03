using System;
using UnityEngine;

namespace QualityOfLifeONI
{
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
                  PriorityScreen.PriorityClass.personalNeeds, // High priority so it interrupts work
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

            // Scans the ENTIRE current planetoid for the accessible cell with the highest Lux
            public int FindBestPlanetoidSunlightCell()
            {
                // Get the world/asteroid the Rover is currently standing on
                WorldContainer world = this.gameObject.GetMyWorld();
                if (world == null) return Grid.InvalidCell;

                Navigator navigator = this.GetComponent<Navigator>();
                if (navigator == null) return Grid.InvalidCell;

                int startCell = Grid.PosToCell(this.gameObject);
                Grid.CellToXY(startCell, out int startX, out int startY);

                // Planetoid boundaries
                Vector2 minVec = world.minimumBounds;
                Vector2 maxVec = world.maximumBounds;

                int minX = (int)minVec.x;
                int minY = (int)minVec.y;
                int maxX = (int)maxVec.x;
                int maxY = (int)maxVec.y;

                int maxLuxFound = 0;
                int bestCell = Grid.InvalidCell;
                int minDistance = int.MaxValue;

                // Scan every cell within the active planetoid
                for (int x = minX; x <= maxX; x++)
                {
                    for (int y = minY; y <= maxY; y++)
                    {
                        int cell = Grid.XYToCell(x, y);
                        if (!Grid.IsValidCell(cell)) continue;

                        int lux = Grid.LightIntensity[cell];
                        if (lux <= 0) continue;

                        // Case 1: Found a spot with strictly GREATER Lux than anything reachable so far
                        if (lux > maxLuxFound)
                        {
                            // Pathfinding is expensive, so we only check it when we find a better Lux candidate
                            if (navigator.CanReach(cell))
                            {
                                maxLuxFound = lux;
                                bestCell = cell;
                                minDistance = Math.Abs(startX - x) + Math.Abs(startY - y);
                            }
                        }
                        // Case 2: Found a spot with EQUAL maximum Lux (e.g. top surface), pick the closest one
                        else if (lux == maxLuxFound && maxLuxFound > 0)
                        {
                            int dist = Math.Abs(startX - x) + Math.Abs(startY - y);
                            if (dist < minDistance)
                            {
                                if (navigator.CanReach(cell))
                                {
                                    bestCell = cell;
                                    minDistance = dist;
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

                // 1. Planetoid-wide search for peak Lux
                find_light
                    .Enter(smi => {
                        int cell = smi.FindBestPlanetoidSunlightCell();
                        if (cell != Grid.InvalidCell)
                        {
                            smi.sm.targetCell.Set(cell, smi);
                            smi.GoTo(moving);
                        }
                        else
                        {
                            // If no accessible light exists on the planet, fail gracefully
                            smi.StopSM("No accessible light found on planetoid");
                        }
                    });

                // 2. Walk to the highest-Lux tile
                moving
                    .MoveTo(smi => smi.sm.targetCell.Get(smi), charging, find_light, false);

                // 3. Stand still and charge
                charging
                    .PlayAnim("idle_default", KAnim.PlayMode.Loop)
                    .Update((smi, dt) => {
                        int currentCell = Grid.PosToCell(smi.gameObject);

                        // Stop if fully charged, day ended, or battery UI threshold met
                        if (smi.charger != null && !smi.charger.IsChargingExpected)
                        {
                            smi.StopSM("Finished charging or daytime ended");
                        }
                        // If light dropped (e.g. shadow cast, night fall, tile blocked), re-evaluate the best spot
                        else if (Grid.LightIntensity[currentCell] == 0)
                        {
                            smi.GoTo(find_light);
                        }
                    }, UpdateRate.SIM_200ms);
            }
        }
    }
}