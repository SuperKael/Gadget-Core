using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Collections;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(MenuScript))]
    [HarmonyPatch("OnConnectedToServer")]
    static class Patch_MenuScript_OnConnectedToServer
    {
        public static readonly MethodInfo OnConnectedToServer = typeof(MenuScript).GetMethod("OnConnectedToServer", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(MenuScript __instance)
        {
            if (GadgetNetwork.connectTime < 0) GadgetNetwork.connectTime = Time.realtimeSinceStartup;
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                if (InstanceTracker.GameScript.gameObject.GetComponent<RPCHooks>() == null) InstanceTracker.GameScript.gameObject.AddComponent<RPCHooks>();
                __instance.StartCoroutine(WaitAndTryAgain(__instance));
                return false;
            }
            else
            {
                if (!GadgetNetwork.MatrixReady) GadgetNetwork.InitializeVanillaIDMatrix();
                return true;
            }
        }

        private static IEnumerator WaitAndTryAgain(MenuScript __instance)
        {
            yield return new WaitUntil(() => GadgetNetwork.MatrixReady || GadgetNetwork.GetTimeSinceConnect() > GadgetNetwork.MatrixTimeout);
            OnConnectedToServer.Invoke(__instance, new object[] { });
        }
    }
}