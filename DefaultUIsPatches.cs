using HarmonyLib;

namespace QualityOfLifeONI
{
    internal static class FilteredToolHelper
    {
        public static void SetActiveFilter(FilteredDragTool tool, string targetFilterKey)
        {
            if (tool == null) return;

            var filters = Traverse.Create(tool).Field("currentFilters").GetValue<ToolParameterMenu.ToggleData[]>();
            if (filters == null) return;

            for (int i = 0; i < filters.Length; i++)
            {
                filters[i].state = (filters[i].name == targetFilterKey)
                    ? ToolParameterMenu.ToggleState.On
                    : ToolParameterMenu.ToggleState.Off;
            }
        }
    }

    [HarmonyPatch(typeof(DigTool), "GetDefaultFilters")]
    public static class DigTool_GetDefaultFilters_Patch
    {
        public static void Postfix(ref ToolParameterMenu.ToggleData[] filters)
        {
            // Use the central config!
            QoLConfig options = ModInit.Config ?? new QoLConfig();

            ToolParameterMenu.ToggleState tilesState = options.DigTiles ? ToolParameterMenu.ToggleState.On : ToolParameterMenu.ToggleState.Off;
            ToolParameterMenu.ToggleState backwallState = options.DigNaturalBackwall ? ToolParameterMenu.ToggleState.On : ToolParameterMenu.ToggleState.Off;
            ToolParameterMenu.ToggleState plantsState = options.DigPlants ? ToolParameterMenu.ToggleState.On : ToolParameterMenu.ToggleState.Off;

            filters = new ToolParameterMenu.ToggleData[]
            {
                new ToolParameterMenu.ToggleData(ToolParameterMenu.FILTERLAYERS.TILES, tilesState, true),
                new ToolParameterMenu.ToggleData(ToolParameterMenu.FILTERLAYERS.NATURALBACKWALL, backwallState, true),
                new ToolParameterMenu.ToggleData(ToolParameterMenu.FILTERLAYERS.UPROOTPLANTS, plantsState, true)
            };
        }
    }

    [HarmonyPatch(typeof(DeconstructTool), "OnPrefabInit")]
    public static class DeconstructTool_OnPrefabInit_Patch
    {
        public static void Postfix(DeconstructTool __instance)
        {
            QoLConfig options = ModInit.Config ?? new QoLConfig();

            string filterKey;
            switch (options.DefaultDeconstructFilter)
            {
                case QoLConfig.DeconstructFilterOptions.PowerWires: filterKey = ToolParameterMenu.FILTERLAYERS.WIRES; break;
                case QoLConfig.DeconstructFilterOptions.LiquidPipes: filterKey = ToolParameterMenu.FILTERLAYERS.LIQUIDCONDUIT; break;
                case QoLConfig.DeconstructFilterOptions.GasPipes: filterKey = ToolParameterMenu.FILTERLAYERS.GASCONDUIT; break;
                case QoLConfig.DeconstructFilterOptions.ConveyorRails: filterKey = ToolParameterMenu.FILTERLAYERS.SOLIDCONDUIT; break;
                case QoLConfig.DeconstructFilterOptions.Buildings: filterKey = ToolParameterMenu.FILTERLAYERS.BUILDINGS; break;
                case QoLConfig.DeconstructFilterOptions.Automation: filterKey = ToolParameterMenu.FILTERLAYERS.LOGIC; break;
                case QoLConfig.DeconstructFilterOptions.BackgroundBuildings: filterKey = ToolParameterMenu.FILTERLAYERS.BACKWALL; break;
                default: filterKey = ToolParameterMenu.FILTERLAYERS.ALL; break;
            }

            FilteredToolHelper.SetActiveFilter(__instance, filterKey);
        }
    }

    [HarmonyPatch(typeof(PrioritizeTool), "OnPrefabInit")]
    public static class PrioritizeTool_OnPrefabInit_Patch
    {
        public static void Postfix(PrioritizeTool __instance)
        {
            QoLConfig options = ModInit.Config ?? new QoLConfig();

            string filterKey;
            switch (options.DefaultPriorityFilter)
            {
                case QoLConfig.PriorityFilterOptions.Construction: filterKey = ToolParameterMenu.FILTERLAYERS.CONSTRUCTION; break;
                case QoLConfig.PriorityFilterOptions.Digging: filterKey = ToolParameterMenu.FILTERLAYERS.DIG; break;
                case QoLConfig.PriorityFilterOptions.Cleaning: filterKey = ToolParameterMenu.FILTERLAYERS.CLEAN; break;
                case QoLConfig.PriorityFilterOptions.Duties: filterKey = ToolParameterMenu.FILTERLAYERS.OPERATE; break;
                default: filterKey = ToolParameterMenu.FILTERLAYERS.ALL; break;
            }

            FilteredToolHelper.SetActiveFilter(__instance, filterKey);
        }
    }
}