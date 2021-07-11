using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Collections;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("OnServerInitialized")]
    static class Patch_GameScript_OnServerInitialized
    {
        public static readonly MethodInfo OnServerInitialized = typeof(GameScript).GetMethod("OnServerInitialized", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance)
        {
            if (InstanceTracker.GameScript.gameObject.GetComponent<RPCHooks>() == null) InstanceTracker.GameScript.gameObject.AddComponent<RPCHooks>();
            RPCHooks.InitiateGadgetNetwork();
            if (GadgetNetwork.connectTime < 0) GadgetNetwork.connectTime = Time.realtimeSinceStartup;
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(WaitAndTryAgain(__instance));
                return false;
            }
            else
            {
                return true;
            }
        }

        private static IEnumerator WaitAndTryAgain(GameScript __instance)
        {
            yield return new WaitUntil(() => GadgetNetwork.MatrixReady || GadgetNetwork.GetTimeSinceConnect() > GadgetNetwork.MatrixTimeout);
            OnServerInitialized.Invoke(__instance, new object[] { });
        }
    }
}