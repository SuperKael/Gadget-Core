using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Collections;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(SpawnBlock))]
    [HarmonyPatch("OnConnectedToServer")]
    internal static class Patch_SpawnBlock_OnConnectedToServer
    {
        public static readonly MethodInfo OnConnectedToServer = typeof(SpawnBlock).GetMethod("OnConnectedToServer", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(SpawnBlock __instance)
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

        private static IEnumerator WaitAndTryAgain(SpawnBlock __instance)
        {
            yield return new WaitUntil(() => GadgetNetwork.MatrixReady || GadgetNetwork.GetTimeSinceConnect() > GadgetNetwork.MatrixTimeout);
            OnConnectedToServer.Invoke(__instance, new object[] { });
        }
    }
}