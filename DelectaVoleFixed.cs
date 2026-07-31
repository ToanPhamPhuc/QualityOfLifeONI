using HarmonyLib;
using Klei.AI;
using System.Collections.Generic;
using UnityEngine;

namespace FixShoveVoles
{
    // 1. Fix Standard Shove Vole to lay 100% regular eggs
    [HarmonyPatch(typeof(MoleConfig), nameof(MoleConfig.CreatePrefab))]
    public static class MoleConfig_CreatePrefab_Patch
    {
        public static void Postfix(GameObject __result)
        {
            FertilityMonitor.Def fertilityDef = __result.GetDef<FertilityMonitor.Def>();
            if (fertilityDef != null)
            {
                fertilityDef.initialBreedingWeights = new List<FertilityMonitor.BreedingChance>
                {
                    new FertilityMonitor.BreedingChance { egg = "MoleEgg".ToTag(), weight = 1f },
                    new FertilityMonitor.BreedingChance { egg = "MoleDelicacyEgg".ToTag(), weight = 0f }
                };
            }
        }
    }

    // 2. Fix Delecta Shove Vole (Normal stomach, normal starvation rate)
    [HarmonyPatch(typeof(MoleDelicacyConfig), nameof(MoleDelicacyConfig.CreatePrefab))]
    public static class MoleDelicacyConfig_CreatePrefab_Patch
    {
        public static void Postfix(GameObject __result)
        {
            // Fix Calories/Stomach on the prefab's trait definition
            Modifiers modifiers = __result.GetComponent<Modifiers>();
            if (modifiers != null)
            {
                Trait trait = Db.Get().traits.Get("MoleDelicacyBaseTrait");
                if (trait != null)
                {
                    // Modify max stomach size to match standard Mole (48,000,000 kcal capacity)
                    AttributeModifier maxCalories = trait.SelfModifiers.Find(m => m.AttributeId == Db.Get().Amounts.Calories.maxAttribute.Id);
                    if (maxCalories != null)
                    {
                        maxCalories.SetValue(MoleTuning.STANDARD_STOMACH_SIZE);
                    }
                }
            }
        }
    }
}