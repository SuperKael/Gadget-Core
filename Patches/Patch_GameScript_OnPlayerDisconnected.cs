using HarmonyLib;
using GadgetCore.API;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("OnPlayerDisconnected")]
    internal static class Patch_GameScript_OnPlayerDisconnected
    {
        [HarmonyPrefix]
        public static void Prefix(GameScript __instance, NetworkPlayer pl)
        {
            string name = GadgetNetwork.GetNameByNetworkPlayer(pl);
            if (!string.IsNullOrEmpty(name))
            {
                GadgetConsole.Print($"{name} has left the game.");
            }
        }
    }
}