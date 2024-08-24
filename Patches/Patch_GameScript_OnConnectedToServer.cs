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
    internal static class Patch_GameScript_OnConnectedToServer
    {
        public static readonly MethodInfo OnConnectedToServer = typeof(GameScript).GetMethod("OnConnectedToServer", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance)
        {
            if (GadgetNetwork.MatrixReadyOrTimedOut)
            {
                if (!GadgetNetwork.MatrixTimedOut ||
                    !Gadgets.ListAllGadgetInfos().Any(x => x.Attribute.RequiredOnClients)) return true;
                GadgetCore.CoreLogger.LogWarning("Disconnecting from server due to timout! You can try raising NetworkTimeout in the config.");
                Network.Disconnect();
                return false;
            }
            RPCHooks.InitiateGadgetNetwork();
            __instance.StartCoroutine(WaitAndTryAgain(__instance));
            return false;
        }

        private static IEnumerator WaitAndTryAgain(GameScript __instance)
        {
            yield return new WaitUntil(() => GadgetNetwork.MatrixReady || GadgetNetwork.GetTimeSinceConnect() > GadgetNetwork.MatrixTimeout);
            OnConnectedToServer.Invoke(__instance, new object[] { });
        }
    }
}