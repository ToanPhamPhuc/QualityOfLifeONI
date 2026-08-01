/*using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace BetterSpaceHarvesting
{
    //public class AutoEmptyRocketModulesMod : KMod.UserMod2
    //{
    //    public override void OnLoad(Harmony harmony)
    //    {
    //        base.OnLoad(harmony);
    //    }
    //}

    [HarmonyPatch(typeof(LaunchPad), "OnSpawn")]
    public static class LaunchPad_OnSpawn_Patch
    {
        public static void Postfix(LaunchPad __instance)
        {
            // Subscribe to the RocketLanded event trigger on the LaunchPad
            __instance.Subscribe((int)GameHashes.RocketLanded, (data) =>
            {
                if (data is RocketModuleCluster landedRocket)
                {
                    EmptyRocketModules(landedRocket);
                }
            });
        }

        private static void EmptyRocketModules(RocketModuleCluster landedRocket)
        {
            if (landedRocket == null || landedRocket.CraftInterface == null)
                return;

            // Iterate through modules in the rocket cluster
            foreach (Ref<RocketModuleCluster> moduleRef in landedRocket.CraftInterface.ClusterModules)
            {
                RocketModuleCluster module = moduleRef.Get();
                if (module == null) continue;

                // 1. Empty Cargo Bays
                CargoBayCluster cargoBay = module.GetComponent<CargoBayCluster>();
                if (cargoBay != null)
                {
                    Storage storage = cargoBay.GetComponent<Storage>();
                    if (storage != null && storage.Count > 0)
                    {
                        storage.DropAll(false, false, default, true);
                    }
                }

                // 2. Empty Artifact Transport Modules
                ArtifactSelector artifactModule = module.GetComponent<ArtifactSelector>();
                if (artifactModule != null)
                {
                    Storage storage = artifactModule.GetComponent<Storage>();
                    if (storage != null && storage.Count > 0)
                    {
                        storage.DropAll(false, false, default, true);
                    }
                }
            }
        }
    }
}*/