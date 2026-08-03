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
}