using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace QualityOfLifeONI
{
    [HarmonyPatch(typeof(BottleEmptierConfig), nameof(BottleEmptierConfig.ConfigureBuildingTemplate))]
    public static class BottleEmptierFixed
    {
        public static void Postfix(GameObject go)
        {
            Storage storage = go.GetComponent<Storage>();
            if (storage != null)
            {
                storage.SetDefaultStoredItemModifiers(new List<Storage.StoredItemModifier>
                {
                    Storage.StoredItemModifier.Hide,
                    Storage.StoredItemModifier.Seal,
                    Storage.StoredItemModifier.Insulate 
                });
            }   
        }
    }

    [HarmonyPatch(typeof(ManualGeneratorConfig), "CreateBuildingDef")]
    class ManualGeneratorConfig_CreateBuildingDef
    {
        public static void Postfix(BuildingDef __result)
        {
            __result.GeneratorWattageRating = 600f;
        }
    }
}
