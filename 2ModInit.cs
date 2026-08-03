using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;

namespace QualityOfLifeONI
{
    public class ModInit : UserMod2
    {
        // Centralized config instance
        public static QoLConfig Config;

        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);

            PUtil.InitLibrary();

            // Register only the ONE master config class
            new POptions().RegisterOptions(this, typeof(QoLConfig));

            // Load it into memory at startup
            Config = POptions.ReadSettings<QoLConfig>();
            if (Config == null)
            {
                Config = new QoLConfig();
            }
        }
    }

    // 1. Add Strings and register it to the Build Menu
    [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
    public class SelfTimerDoor_Registration_Patch
    {
        public static void Prefix()
        {
            // Add names and descriptions
            Strings.Add("STRINGS.BUILDINGS.PREFABS.SELFTIMERPNEUMATICDOOR.NAME", "(Beta) Self-Timer Pneumatic Door");
            Strings.Add("STRINGS.BUILDINGS.PREFABS.SELFTIMERPNEUMATICDOOR.DESC", "An internal door with an integrated cycle timer.");
            Strings.Add("STRINGS.BUILDINGS.PREFABS.SELFTIMERPNEUMATICDOOR.EFFECT", "Automatically opens and locks according to the time of day, completely bypassing the need for automation wire.");

            // Add to the "Base" build menu category
            ModUtil.AddBuildingToPlanScreen("Base", SelfTimerPneumaticDoorConfig.ID);
        }
    }

    // 2. Add it to the Tech Tree so Dupes can research it
    [HarmonyPatch(typeof(Db), "Initialize")]
    public class SelfTimerDoor_TechTree_Patch
    {
        public static void Postfix()
        {
            // Unlocks alongside the standard Pneumatic Door
            Db.Get().Techs.Get("AnimalControl").unlockedItemIDs.Add(SelfTimerPneumaticDoorConfig.ID);
        }
    }
}