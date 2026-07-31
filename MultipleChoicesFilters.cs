using HarmonyLib;
using KSerialization;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QualityOfLifeONI
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class MultipleChoicesFilters : KMonoBehaviour, ISaveLoadable
    {
        [MyCmpReq]
        private TreeFilterable treeFilterable;

        [MyCmpReq]
        private KSelectable selectable;

        private Guid statusItemGuid;

        protected override void OnSpawn()
        {
            base.OnSpawn();
            treeFilterable.OnFilterChanged += OnFilterChanged;
            UpdateStatusItem();
        }

        protected override void OnCleanUp()
        {
            treeFilterable.OnFilterChanged -= OnFilterChanged;
            base.OnCleanUp();
        }

        private void OnFilterChanged(HashSet<Tag> tags)
        {
            UpdateStatusItem();
        }

        public void UpdateStatusItem()
        {
            HashSet<Tag> tags = treeFilterable.GetTags();
            if (tags == null || tags.Count == 0)
            {
                selectable.RemoveStatusItem(statusItemGuid);
                return;
            }

            // Convert selected tags to localized element/item names
            List<string> names = tags.Select(t => t.ProperName()).ToList();
            string statusText = "Filters: " + string.Join(", ", names);

            // FIX: Changed 'ElementFilter' to 'Filter' (or 'ElementFilterOutput')
            statusItemGuid = selectable.SetStatusItem(
                Db.Get().BuildingStatusItems.Filter,
                statusText
            );
        }
    }

    // 1. Gas Filter
    [HarmonyPatch(typeof(GasFilterConfig), nameof(GasFilterConfig.ConfigureBuildingTemplate))]
    public static class GasFilter_MultiChoice_Patch
    {
        public static void Postfix(GameObject go)
        {
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Filterable>());

            var treeFilter = go.AddOrGet<TreeFilterable>();
            // FIX: Changed 'filterByTag' to 'uiTag'
            treeFilter.uiTag = GameTags.Gas;

            go.AddOrGet<MultipleChoicesFilters>();
        }
    }

    // 2. Liquid Filter
    [HarmonyPatch(typeof(LiquidFilterConfig), nameof(LiquidFilterConfig.ConfigureBuildingTemplate))]
    public static class LiquidFilter_MultiChoice_Patch
    {
        public static void Postfix(GameObject go)
        {
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Filterable>());

            var treeFilter = go.AddOrGet<TreeFilterable>();
            // FIX: Changed 'filterByTag' to 'uiTag'
            treeFilter.uiTag = GameTags.Liquid;

            go.AddOrGet<MultipleChoicesFilters>();
        }
    }

    // 3. Solid Filter
    [HarmonyPatch(typeof(SolidFilterConfig), nameof(SolidFilterConfig.ConfigureBuildingTemplate))]
    public static class SolidFilter_MultiChoice_Patch
    {
        public static void Postfix(GameObject go)
        {
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Filterable>());

            var treeFilter = go.AddOrGet<TreeFilterable>();
            // FIX: Changed 'filterByTag' to 'uiTag'
            treeFilter.uiTag = GameTags.Solid;

            go.AddOrGet<MultipleChoicesFilters>();
        }
    }
}