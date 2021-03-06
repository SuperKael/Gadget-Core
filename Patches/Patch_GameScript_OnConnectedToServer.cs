using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Linq;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("OnConnectedToServer")]
    static class Patch_GameScript_OnConnectedToServer
    {
        public static readonly MethodInfo OnConnectedToServer = typeof(GameScript).GetMethod("OnConnectedToServer", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                if (InstanceTracker.GameScript.gameObject.GetComponent<RPCHooks>() == null) InstanceTracker.GameScript.gameObject.AddComponent<RPCHooks>();
                RPCHooks.InitiateGadgetNetwork();
                __instance.StartCoroutine(WaitAndTryAgain(__instance));
                return false;
            }
            else
            {
                if (GadgetNetwork.GetTimeSinceConnect() > GadgetNetwork.MatrixTimeout && Gadgets.ListAllGadgetInfos().Any(x => x.Attribute.RequiredOnClients))
                {
                    GadgetCore.CoreLogger.LogWarning("Disconnecting from server due to timout! You can try raising NetworkTimeout in the config.");
                    Network.Disconnect();
                    return false;
                }
                return true;
            }
        }

        private static IEnumerator WaitAndTryAgain(GameScript __instance)
        {
            yield return new WaitUntil(() => GadgetNetwork.MatrixReady || GadgetNetwork.GetTimeSinceConnect() > GadgetNetwork.MatrixTimeout);
            OnConnectedToServer.Invoke(__instance, new object[] { });
        }
    }
}