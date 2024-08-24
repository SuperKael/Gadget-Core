using HarmonyLib;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(MenuScript))]
    [HarmonyPatch("Back")]
    internal static class Patch_MenuScript_Back
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (Network.isClient || Network.isServer) Network.Disconnect();
        }
    }
}