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

        private Guid statusItemGuid = Guid.Empty;

        public Tag filterCategory; // Assign category per building type

        protected override void OnSpawn()
        {
            base.OnSpawn();

            // Safe to call UpdateFilters here because the building is spawned
            if (filterCategory.IsValid)
            {
                treeFilterable.UpdateFilters(new HashSet<Tag> { filterCategory }); //[cite: 1]
            }

            treeFilterable.OnFilterChanged += OnFilterChanged; //[cite: 1]
            UpdateStatusItem();
        }

        protected override void OnCleanUp()
        {
            if (treeFilterable != null)
            {
                treeFilterable.OnFilterChanged -= OnFilterChanged; //[cite: 1]
            }
            base.OnCleanUp();
        }

        private void OnFilterChanged(HashSet<Tag> tags)
        {
            UpdateStatusItem();
        }

        public void UpdateStatusItem()
        {
            HashSet<Tag> tags = treeFilterable.GetTags(); //[cite: 1]
            if (tags == null || tags.Count == 0)
            {
                if (statusItemGuid != Guid.Empty)
                {
                    statusItemGuid = selectable.RemoveStatusItem(statusItemGuid);
                }
                return;
            }

            // Get standard tag status string directly from TreeFilterable
            string statusText = treeFilterable.GetTagsAsStatus(6); //[cite: 1]

            // Clear old status before setting a new one
            if (statusItemGuid != Guid.Empty)
            {
                selectable.RemoveStatusItem(statusItemGuid);
            }

            // Use AddStatusItem to register status items
            statusItemGuid = selectable.AddStatusItem(
                Db.Get().BuildingStatusItems.NoStorageFilterSet, //[cite: 2]
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

            go.AddOrGet<TreeFilterable>();
            var multiFilter = go.AddOrGet<MultipleChoicesFilters>();
            multiFilter.filterCategory = GameTags.Gas;
        }
    }

    // 2. Liquid Filter
    [HarmonyPatch(typeof(LiquidFilterConfig), nameof(LiquidFilterConfig.ConfigureBuildingTemplate))]
    public static class LiquidFilter_MultiChoice_Patch
    {
        public static void Postfix(GameObject go)
        {
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Filterable>());

            go.AddOrGet<TreeFilterable>();
            var multiFilter = go.AddOrGet<MultipleChoicesFilters>();
            multiFilter.filterCategory = GameTags.Liquid;
        }
    }

    // 3. Solid Filter
    [HarmonyPatch(typeof(SolidFilterConfig), nameof(SolidFilterConfig.ConfigureBuildingTemplate))]
    public static class SolidFilter_MultiChoice_Patch
    {
        public static void Postfix(GameObject go)
        {
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Filterable>());

            go.AddOrGet<TreeFilterable>();
            var multiFilter = go.AddOrGet<MultipleChoicesFilters>();
            multiFilter.filterCategory = GameTags.Solid;
        }
    }
}