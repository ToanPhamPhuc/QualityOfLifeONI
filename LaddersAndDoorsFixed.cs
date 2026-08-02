using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using System;

namespace LaddersAndDoorsFixed
{
    [HarmonyPatch(typeof(Db), "Initialize")]
    public static class LaddersAndDoorsFixed_Db_Initialize_Patch
    {
        public static void Postfix()
        {
            // Target IDs for vanilla Ladders, Doors, and the commonly modded "InsulatedDoor"
            string[] targetIds = new string[]
            {
                "Ladder",             // Regular Ladder
                "LadderFast",         // Plastic Ladder
                "FirePole",           // Fire Pole
                "Door",               // Pneumatic Door
                "ManualPressureDoor", // Manual Airlock
                "PressureDoor",       // Mechanized Airlock
                "BunkerDoor",         // Bunker Door
                "InsulatedDoor"       // Support for modded Insulated Doors
            };

            // Loop through each ID and patch their BuildingDef rules
            foreach (string id in targetIds)
            {
                BuildingDef def = Assets.GetBuildingDef(id);
                if (def != null)
                {
                    // Treat like Drywall: allows placement anywhere, ignoring solid tile collisions
                    def.BuildLocationRule = BuildLocationRule.Anywhere;

                    // Prevent the building from being disabled ("Entombed") if it overlaps with a solid tile
                    def.Entombable = false;
                }
            }
        }
    }
}