using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace QualityOfLifeONI
{
    // Replace 'BottleEmptierConduitConfig' with the exact class name you find in dnSpy!
    [HarmonyPatch(typeof(BottleEmptierConduitLiquidConfig), nameof(BottleEmptierConduitLiquidConfig.ConfigureBuildingTemplate))]
    public static class BottleDrainerFixed
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
}