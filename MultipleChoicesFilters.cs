using HarmonyLib;
using KSerialization;
using System;
using System.Collections.Generic;
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
                // Remove the existing status item using the stored Guid
                statusItemGuid = selectable.RemoveStatusItem(statusItemGuid);
                return;
            }

            // Get standard tag status string directly from TreeFilterable
            string statusText = treeFilterable.GetTagsAsStatus(6);

            // If it's already added, remove the old one before re-adding,
            // or use AddStatusItem to register/update it.
            if (statusItemGuid != Guid.Empty)
            {
                selectable.RemoveStatusItem(statusItemGuid);
            }

            // Use AddStatusItem instead of SetStatusItem
            statusItemGuid = selectable.AddStatusItem(
                Db.Get().BuildingStatusItems.NoStorageFilterSet,
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
            treeFilter.UpdateFilters(new HashSet<Tag> { GameTags.Gas });

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
            treeFilter.UpdateFilters(new HashSet<Tag> { GameTags.Liquid });

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
            treeFilter.UpdateFilters(new HashSet<Tag> { GameTags.Solid });

            go.AddOrGet<MultipleChoicesFilters>();
        }
    }
}