using HarmonyLib;
using UnityEngine;

namespace QualityOfLifeONI
{
    [HarmonyPatch(typeof(ScoutRoverConfig), "OnPrefabInit")]
    public class ScoutRover_AddSolarCharger_Patch
    {
        public static void Postfix(GameObject inst)
        {
            // Attach our new logic script to the Rover prefab
            inst.AddOrGet<RoverSolarCharger>();
        }
    }
}