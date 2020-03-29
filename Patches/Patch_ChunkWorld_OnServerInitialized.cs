using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Collections;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(ChunkWorld))]
    [HarmonyPatch("OnServerInitialized")]
    static class Patch_ChunkWorld_OnServerInitialized
    {
        public static readonly MethodInfo OnServerInitialized = typeof(ChunkWorld).GetMethod("OnServerInitialized", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(ChunkWorld __instance)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                if (InstanceTracker.GameScript.gameObject.GetComponent<RPCHooks>() == null) InstanceTracker.GameScript.gameObject.AddComponent<RPCHooks>();
                __instance.StartCoroutine(WaitAndTryAgain(__instance));
                return false;
            }
            else
            {
                return true;
            }
        }

        private static IEnumerator WaitAndTryAgain(ChunkWorld __instance)
        {
            yield return new WaitUntil(() => GadgetNetwork.MatrixReady || GadgetNetwork.GetTimeSinceConnect() > GadgetNetwork.MatrixTimeout);
            OnServerInitialized.Invoke(__instance, new object[] { });
        }
    }
}