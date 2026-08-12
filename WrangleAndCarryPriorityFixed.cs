//using System;
//using System.Collections.Generic;
//using System.Reflection;
//using Database;
//using HarmonyLib;
//using KMod;
//using SanchozzONIMods.Lib;
//using UnityEngine;

//namespace WrangleCarry
//{
//    // Token: 0x02000002 RID: 2
//    internal sealed class WrangleCarryPatches : UserMod2
//    {
//        // Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
//        public override void OnLoad(Harmony harmony)
//        {
//            if (Utils.LogModVersion())
//            {
//                return;
//            }
//            WrangleCarryPatches.@this = this;
//            harmony.Patch(typeof(Db).GetMethod("Initialize"), new HarmonyMethod(typeof(WrangleCarryPatches), "PatchLater", null), null, null, null);
//        }

//        // Token: 0x06000002 RID: 2 RVA: 0x0000209E File Offset: 0x0000029E
//        private static void PatchLater()
//        {
//            Utils.MuteMouthFlapSpeech("anim_rage_kanim", WrangleCarryPatches.rage_anims);
//            WrangleCarryPatches.@this.PatchLater();
//        }

//        // Token: 0x06000003 RID: 3 RVA: 0x000020C0 File Offset: 0x000002C0
//        private static void AddSackSymbolOverride(GameObject dupe, GameObject pickupable)
//        {
//            KAnimControllerBase kanimControllerBase;
//            SymbolOverrideController symbolOverrideController;
//            if (dupe != null && pickupable != null && pickupable.HasTag(GameTags.Creature) && !pickupable.HasTag(GameTags.Robot) && dupe.TryGetComponent<KAnimControllerBase>(out kanimControllerBase) && dupe.TryGetComponent<SymbolOverrideController>(out symbolOverrideController))
//            {
//                KAnim.Build.Symbol symbol = Assets.GetAnim("creature_sack_kanim").GetData().build.GetSymbol("object");
//                symbolOverrideController.AddSymbolOverride("snapTo_chest", symbol, 0);
//                kanimControllerBase.SetSymbolVisiblity("snapTo_chest", true);
//            }
//        }

//        // Token: 0x06000004 RID: 4 RVA: 0x00002160 File Offset: 0x00000360
//        private static void RemoveSackSymbolOverride(GameObject dupe)
//        {
//            KAnimControllerBase kanimControllerBase;
//            SymbolOverrideController symbolOverrideController;
//            if (dupe != null && dupe.TryGetComponent<KAnimControllerBase>(out kanimControllerBase) && dupe.TryGetComponent<SymbolOverrideController>(out symbolOverrideController))
//            {
//                kanimControllerBase.SetSymbolVisiblity("snapTo_chest", false);
//                symbolOverrideController.RemoveSymbolOverride("snapTo_chest", 0);
//            }
//        }

//        // Token: 0x06000005 RID: 5 RVA: 0x000021B0 File Offset: 0x000003B0
//        private static bool IsCritter(GameObject go)
//        {
//            CreatureBrain creatureBrain;
//            return go != null && go.TryGetComponent<CreatureBrain>(out creatureBrain) && !go.HasTag(GameTags.Robot);
//        }

//        // Token: 0x04000001 RID: 1
//        private static WrangleCarryPatches @this;

//        // Token: 0x04000002 RID: 2
//        private const string rage_kanim = "anim_rage_kanim";

//        // Token: 0x04000003 RID: 3
//        private static readonly HashedString[] rage_anims = new HashedString[]
//        {
//            "idle_pre",
//            "rage_pre",
//            "rage_loop",
//            "rage_loop",
//            "rage_pst",
//            "idle_pst"
//        };

//        // Token: 0x04000004 RID: 4
//        private const string chest = "snapTo_chest";

//        // Updates CreatureFetch priority to 9 (9000 internal) after database loads
//        [HarmonyPatch(typeof(Db), "Initialize")]
//        private static class Db_Initialize_PriorityPatch
//        {
//            private static void Postfix()
//            {
//                ChoreType creatureFetch = Db.Get()?.ChoreTypes?.CreatureFetch;
//                if (creatureFetch != null)
//                {
//                    Traverse.Create(creatureFetch).Field("priority").SetValue(9000);
//                }
//            }
//        }

//        // Token: 0x02000006 RID: 6
//        [HarmonyPatch(typeof(ChoreTypes), "Add")]
//        private static class ChoreTypes_Add
//        {
//            // Token: 0x0600001F RID: 31 RVA: 0x00002B97 File Offset: 0x00000D97
//            private static void Prefix(string id, ref bool skip_implicit_priority_change)
//            {
//                if (id == "CreatureFetch")
//                {
//                    skip_implicit_priority_change = true;
//                }
//            }
//        }

//        // Token: 0x02000007 RID: 7
//        [HarmonyPatch]
//        private static class Capturable_OnWork
//        {
//            // Token: 0x06000020 RID: 32 RVA: 0x00002BA9 File Offset: 0x00000DA9
//            private static IEnumerable<MethodBase> TargetMethods()
//            {
//                yield return typeof(Capturable).GetMethod("OnStartWork", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
//                yield return typeof(Capturable).GetMethod("OnStopWork", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
//                yield break;
//            }

//            // Token: 0x06000021 RID: 33 RVA: 0x00002BB4 File Offset: 0x00000DB4
//            private static void Postfix(Capturable __instance)
//            {
//                CreatureBrain creatureBrain;
//                if (__instance.TryGetComponent<CreatureBrain>(out creatureBrain) && creatureBrain.IsRunning())
//                {
//                    creatureBrain.UpdateBrain();
//                }
//            }
//        }

//        // Token: 0x02000008 RID: 8
//        [HarmonyPatch(typeof(Capturable), "OnCompleteWork")]
//        private static class Capturable_OnCompleteWork
//        {
//            // Token: 0x06000022 RID: 34 RVA: 0x00002BDC File Offset: 0x00000DDC
//            private static void Postfix(Capturable __instance, WorkerBase worker)
//            {
//                Pickupable pickupable;
//                if (__instance.TryGetComponent<Pickupable>(out pickupable) && pickupable.IsReachable())
//                {
//                    return;
//                }
//                ChoreProvider target;
//                if (worker != null && worker.TryGetComponent<ChoreProvider>(out target))
//                {
//                    new EmoteChore(target, Db.Get().ChoreTypes.EmoteHighPriority, "anim_rage_kanim", WrangleCarryPatches.rage_anims, null);
//                }
//            }
//        }

//        // Token: 0x02000009 RID: 9
//        [HarmonyPatch(typeof(FetchAreaChore.States), "InitializeStates")]
//        private static class FetchAreaChore_States_InitializeStates
//        {
//            // Token: 0x06000023 RID: 35 RVA: 0x00002C38 File Offset: 0x00000E38
//            private static void Postfix(FetchAreaChore.States __instance)
//            {
//                __instance.delivering.movetostorage.Enter(delegate (FetchAreaChore.StatesInstance smi)
//                {
//                    WrangleCarryPatches.AddSackSymbolOverride(smi.gameObject, smi.sm.deliveryObject.Get(smi));
//                }).Exit(delegate (FetchAreaChore.StatesInstance smi)
//                {
//                    WrangleCarryPatches.RemoveSackSymbolOverride(smi.gameObject);
//                });
//            }
//        }

//        // Token: 0x0200000A RID: 10
//        [HarmonyPatch(typeof(MovePickupableChore.States), "InitializeStates")]
//        private static class MovePickupableChore_States_InitializeStates
//        {
//            // Token: 0x06000024 RID: 36 RVA: 0x00002C9C File Offset: 0x00000E9C
//            private static void Postfix(MovePickupableChore.States __instance)
//            {
//                __instance.approachstorage.Enter(delegate (MovePickupableChore.StatesInstance smi)
//                {
//                    WrangleCarryPatches.AddSackSymbolOverride(smi.sm.deliverer.Get(smi), smi.sm.pickupablesource.Get(smi));
//                }).Exit(delegate (MovePickupableChore.StatesInstance smi)
//                {
//                    WrangleCarryPatches.RemoveSackSymbolOverride(smi.sm.deliverer.Get(smi));
//                });
//            }
//        }

//        // Token: 0x0200000B RID: 11
//        [HarmonyPatch(typeof(MovePickupableChore.States), "IsDeliveryComplete")]
//        private static class MovePickupableChore_States_IsDeliveryComplete
//        {
//            // Token: 0x06000025 RID: 37 RVA: 0x00002CF8 File Offset: 0x00000EF8
//            private static void Postfix(ref bool __result, MovePickupableChore.StatesInstance smi)
//            {
//                if (!__result)
//                {
//                    GameObject gameObject = smi.sm.deliverypoint.Get(smi);
//                    CancellableMove cancellableMove;
//                    if (gameObject != null && gameObject.TryGetComponent<CancellableMove>(out cancellableMove))
//                    {
//                        GameObject nextTarget = cancellableMove.GetNextTarget();
//                        if (nextTarget != null && WrangleCarryPatches.IsCritter(nextTarget) == (smi.master.choreType.IdHash == Db.Get().ChoreTypes.Fetch.IdHash))
//                        {
//                            __result = true;
//                        }
//                    }
//                }
//            }
//        }
//    }
//}