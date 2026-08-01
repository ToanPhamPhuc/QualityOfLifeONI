using HarmonyLib;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;

namespace QualityOfLifeONI
{
    public class ModInit : KMod.UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            PUtil.InitLibrary();
            new POptions().RegisterOptions(this, typeof(DefaultUIs.ToolFilterOptions));
        }
    }
}