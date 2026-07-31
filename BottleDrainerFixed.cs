using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace QualityOfLifeONI
{
    [HarmonyPatch(typeof(LiquidBottlerConfig), nameof(LiquidBottlerConfig.ConfigureBuildingTemplate))]
    public static class BottleDrainerFixed
    {
        public static void Postfix(GameObject go)
        {
            Storage storage = go.GetComponent<Storage>();
            if(storage != null)
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
