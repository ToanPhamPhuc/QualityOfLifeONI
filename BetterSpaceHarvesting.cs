using HarmonyLib;

namespace BetterSpaceHarvesting
{
    // AutoEmptyRocketModulesMod removed from here to prevent duplicate UserMod2 error!

    // Patch LaunchPad when a rocket lands on it
    [HarmonyPatch(typeof(LaunchPad), "OnRocketLanded")]
    public static class LaunchPad_OnRocketLanded_Patch
    {
        public static void Postfix(LaunchPad __instance, LaunchableRocketCluster landedRocket)
        {
            if (landedRocket == null)
                return;

            Clustercraft craft = landedRocket.GetComponent<Clustercraft>();
            if (craft == null || craft.ModuleInterface == null)
                return;

            var modules = new System.Collections.Generic.List<Ref<RocketModuleCluster>>(craft.ModuleInterface.ClusterModules);

            foreach (Ref<RocketModuleCluster> moduleRef in modules)
            {
                RocketModuleCluster module = moduleRef.Get();
                if (module == null)
                    continue;

                // 1. Auto Empty Cargo Bays (CargoBayCluster)
                CargoBayCluster cargoBay = module.GetComponent<CargoBayCluster>();
                if (cargoBay != null)
                {
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