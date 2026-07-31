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

        protected override void OnSpawn()
        {
            base.OnSpawn();

            // Subscribe to TreeFilterable filter change events
            treeFilterable.OnFilterChanged += OnFilterChanged; 

            // Safely update status item on spawn
            UpdateStatusItem();
        }

        protected override void OnCleanUp()
        {
            if (treeFilterable != null)
            {
                treeFilterable.OnFilterChanged -= OnFilterChanged; 
            }
            base.OnCleanUp();
        }

        private void OnFilterChanged(HashSet<Tag> tags)
        {
            UpdateStatusItem();
        }

        public void UpdateStatusItem()
        {
            if (treeFilterable == null || selectable == null)
                return;

            // Access AcceptedTags safely from TreeFilterable
            var tags = treeFilterable.AcceptedTags; 
            if (tags == null || tags.Count == 0)
            {
                if (statusItemGuid != Guid.Empty)
                {
                    statusItemGuid = selectable.RemoveStatusItem(statusItemGuid);
                }
                return;
            }

            // Build display status string safely without calling GetTagsAsStatus before storage init
            List<string> tagNames = new List<string>();
            foreach (Tag tag in tags)
            {
                if (tag.IsValid)
                {
                    tagNames.Add(tag.ProperName());
                }
            }

            if (tagNames.Count == 0)
                return;

            string statusText = "Filters: " + string.Join(", ", tagNames);

            // Remove previous status item before setting new one
            if (statusItemGuid != Guid.Empty)
            {
                selectable.RemoveStatusItem(statusItemGuid);
            }

            statusItemGuid = selectable.AddStatusItem(
                Db.Get().BuildingStatusItems.NoStorageFilterSet,
                statusText
            );
        }
    }

    // Helper method to setup Storage + TreeFilterable without crashing
    public static class FilterSetupHelper
    {
        public static void ConfigureTreeFilter(GameObject go, Tag categoryTag)
        {
            // Ensure a Storage component exists and set its filter category
            Storage storage = go.AddOrGet<Storage>();
            if (storage.storageFilters == null)
            {
                storage.storageFilters = new List<Tag>();
            }
            if (!storage.storageFilters.Contains(categoryTag))
            {
                storage.storageFilters.Add(categoryTag);
            }

            // Add TreeFilterable and MultipleChoicesFilters
            go.AddOrGet<TreeFilterable>(); 
            go.AddOrGet<MultipleChoicesFilters>();
        }
    }

    // 1. Gas Filter
    [HarmonyPatch(typeof(GasFilterConfig), nameof(GasFilterConfig.ConfigureBuildingTemplate))]
    public static class GasFilter_MultiChoice_Patch
    {
        public static void Postfix(GameObject go)
        {
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Filterable>());
            UnityEngine.Object.DestroyImmediate(go.GetComponent<ElementFilter>());

            FilterSetupHelper.ConfigureTreeFilter(go, GameTags.Gas);
        }
    }

    // 2. Liquid Filter
    [HarmonyPatch(typeof(LiquidFilterConfig), nameof(LiquidFilterConfig.ConfigureBuildingTemplate))]
    public static class LiquidFilter_MultiChoice_Patch
    {
        public static void Postfix(GameObject go)
        {
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Filterable>());
            UnityEngine.Object.DestroyImmediate(go.GetComponent<ElementFilter>());

            FilterSetupHelper.ConfigureTreeFilter(go, GameTags.Liquid);
        }
    }

    // 3. Solid Filter
    [HarmonyPatch(typeof(SolidFilterConfig), nameof(SolidFilterConfig.ConfigureBuildingTemplate))]
    public static class SolidFilter_MultiChoice_Patch
    {
        public static void Postfix(GameObject go)
        {
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Filterable>());
            UnityEngine.Object.DestroyImmediate(go.GetComponent<ElementFilter>());

            FilterSetupHelper.ConfigureTreeFilter(go, GameTags.Solid);
        }
    }
}