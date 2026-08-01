using HarmonyLib;

namespace BetterSpaceHarvesting
{
    public class AutoEmptyRocketModulesMod : KMod.UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
        }
    }

    // Patch LaunchPad when a rocket lands on it
    [HarmonyPatch(typeof(LaunchPad), "OnRocketLanded")]
    public static class LaunchPad_OnRocketLanded_Patch
    {
        public static void Postfix(LaunchPad __instance, RocketModuleCluster landedRocket)
        {
            if (landedRocket == null)
                return;

            // Get all modules attached to this rocket
            var craftInterface = landedRocket.CraftInterface;
            if (craftInterface == null)
                return;

            foreach (Ref<RocketModuleCluster> moduleRef in craftInterface.ClusterModules)
            {
                RocketModuleCluster module = moduleRef.Get();
                if (module == null)
                    continue;

                // 1. Auto Empty Cargo Bays (CargoBayCluster)
                CargoBayCluster cargoBay = module.GetComponent<CargoBayCluster>();
                if (cargoBay != null)
                {
                    // Triggers the "Empty Storage" button action programmatically
                    Storage storage = cargoBay.GetComponent<Storage>();
                    if (storage != null && !storage.IsEmpty())
                    {
                        storage.DropAll();
                    }
                }

                // 2. Auto Empty Artifact Transport Modules
                ArtifactSelector artifactModule = module.GetComponent<ArtifactSelector>();
                if (artifactModule != null)
                {
                    Storage storage = artifactModule.GetComponent<Storage>();
                    if (storage != null && !storage.IsEmpty())
                    {
                        storage.DropAll();
                    }
                }
            }
        }
    }
}