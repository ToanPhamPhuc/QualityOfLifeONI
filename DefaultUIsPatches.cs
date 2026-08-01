using HarmonyLib;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;

namespace DefaultUIs
{
    public class DefaultToolFiltersMod : KMod.UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            PUtil.InitLibrary();
            new POptions().RegisterOptions(this, typeof(ToolFilterOptions));
        }
    }

    // Helper method to set active filters on FilteredDragTool.currentFilters
    internal static class FilteredToolHelper
    {
        public static void SetActiveFilter(FilteredDragTool tool, string targetFilterKey)
        {
            if (tool == null)
                return;

            // Retrieve the protected 'currentFilters' array via Traverse
            var filters = Traverse.Create(tool).Field("currentFilters").GetValue<ToolParameterMenu.ToggleData[]>();
            if (filters == null)
                return;

            for (int i = 0; i < filters.Length; i++)
            {
                filters[i].state = (filters[i].name == targetFilterKey)
                    ? ToolParameterMenu.ToggleState.On
                    : ToolParameterMenu.ToggleState.Off;
            }
        }
    }

    // 1. Patch DigTool's default filters
    [HarmonyPatch(typeof(DigTool), "GetDefaultFilters")]
    public static class DigTool_GetDefaultFilters_Patch
    {
        public static void Postfix(ref ToolParameterMenu.ToggleData[] filters)
        {
            ToolFilterOptions options = POptions.ReadSettings<ToolFilterOptions>() ?? new ToolFilterOptions();

            ToolParameterMenu.ToggleState tilesState = options.DigTiles
                ? ToolParameterMenu.ToggleState.On
                : ToolParameterMenu.ToggleState.Off;

            ToolParameterMenu.ToggleState backwallState = options.DigNaturalBackwall
                ? ToolParameterMenu.ToggleState.On
                : ToolParameterMenu.ToggleState.Off;

            ToolParameterMenu.ToggleState plantsState = options.DigPlants
                ? ToolParameterMenu.ToggleState.On
                : ToolParameterMenu.ToggleState.Off;

            filters = new ToolParameterMenu.ToggleData[]
            {
                new ToolParameterMenu.ToggleData(ToolParameterMenu.FILTERLAYERS.TILES, tilesState, true),
                new ToolParameterMenu.ToggleData(ToolParameterMenu.FILTERLAYERS.NATURALBACKWALL, backwallState, true),
                new ToolParameterMenu.ToggleData(ToolParameterMenu.FILTERLAYERS.UPROOTPLANTS, plantsState, true)
            };
        }
    }

    // 2. Patch Deconstruct Tool Defaults
    [HarmonyPatch(typeof(DeconstructTool), "OnPrefabInit")]
    public static class DeconstructTool_OnPrefabInit_Patch
    {
        public static void Postfix(DeconstructTool __instance)
        {
            ToolFilterOptions options = POptions.ReadSettings<ToolFilterOptions>() ?? new ToolFilterOptions();

            string filterKey;
            switch (options.DefaultDeconstructFilter)
            {
                case ToolFilterOptions.DeconstructFilterOptions.PowerWires:
                    filterKey = ToolParameterMenu.FILTERLAYERS.WIRES;
                    break;
                case ToolFilterOptions.DeconstructFilterOptions.LiquidPipes:
                    filterKey = ToolParameterMenu.FILTERLAYERS.LIQUIDCONDUIT;
                    break;
                case ToolFilterOptions.DeconstructFilterOptions.GasPipes:
                    filterKey = ToolParameterMenu.FILTERLAYERS.GASCONDUIT;
                    break;
                case ToolFilterOptions.DeconstructFilterOptions.ConveyorRails:
                    filterKey = ToolParameterMenu.FILTERLAYERS.SOLIDCONDUIT;
                    break;
                case ToolFilterOptions.DeconstructFilterOptions.Buildings:
                    filterKey = ToolParameterMenu.FILTERLAYERS.BUILDINGS;
                    break;
                case ToolFilterOptions.DeconstructFilterOptions.Automation:
                    filterKey = ToolParameterMenu.FILTERLAYERS.LOGIC;
                    break;
                case ToolFilterOptions.DeconstructFilterOptions.BackgroundBuildings:
                    filterKey = ToolParameterMenu.FILTERLAYERS.BACKWALL;
                    break;
                default:
                    filterKey = ToolParameterMenu.FILTERLAYERS.ALL;
                    break;
            }

            FilteredToolHelper.SetActiveFilter(__instance, filterKey);
        }
    }

    // 3. Patch Priority Tool Defaults
    [HarmonyPatch(typeof(PrioritizeTool), "OnPrefabInit")]
    public static class PrioritizeTool_OnPrefabInit_Patch
    {
        public static void Postfix(PrioritizeTool __instance)
        {
            ToolFilterOptions options = POptions.ReadSettings<ToolFilterOptions>() ?? new ToolFilterOptions();

            string filterKey;
            switch (options.DefaultPriorityFilter)
            {
                case ToolFilterOptions.PriorityFilterOptions.Construction:
                    filterKey = ToolParameterMenu.FILTERLAYERS.CONSTRUCTION;
                    break;
                case ToolFilterOptions.PriorityFilterOptions.Digging:
                    filterKey = ToolParameterMenu.FILTERLAYERS.DIG;
                    break;
                case ToolFilterOptions.PriorityFilterOptions.Cleaning:
                    filterKey = ToolParameterMenu.FILTERLAYERS.CLEAN;
                    break;
                case ToolFilterOptions.PriorityFilterOptions.Duties:
                    filterKey = ToolParameterMenu.FILTERLAYERS.OPERATE;
                    break;
                default:
                    filterKey = ToolParameterMenu.FILTERLAYERS.ALL;
                    break;
            }

            FilteredToolHelper.SetActiveFilter(__instance, filterKey);
        }
    }
}