using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Collections;
using System.Linq;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("OnPlayerConnected")]
    static class Patch_GameScript_OnPlayerConnected
    {
        [HarmonyPrefix]
        public static void Prefix(GameScript __instance, NetworkPlayer pl)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(WaitAndTryAgain(__instance, pl));
            }
        }

        private static IEnumerator WaitAndTryAgain(GameScript __instance, NetworkPlayer pl)
        {
            yield return new WaitUntil(() => GadgetNetwork.MatrixReady || GadgetNetwork.GetTimeSinceConnect() > GadgetNetwork.MatrixTimeout);
            VerifyCompatible(pl);
        }

        private static void VerifyCompatible(NetworkPlayer pl)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() > GadgetNetwork.MatrixTimeout && Gadgets.ListAllGadgetInfos().Any(x => x.Attribute.RequiredOnClients))
            {
                Network.CloseConnection(pl, true);
            }
        }
    }
}