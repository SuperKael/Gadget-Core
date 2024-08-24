using HarmonyLib;
using UnityEngine;
using System;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(MenuScript))]
    [HarmonyPatch("HostGame")]
    internal static class Patch_MenuScript_HostGame
    {
        [HarmonyPrefix]
        public static bool Prefix(MenuScript __instance, ref int ___curHostPort, ref bool ___playing)
        {
            MonoBehaviour.print("Hosting Game");
            try
            {
                ___curHostPort = int.Parse(__instance.txtHostPort[0].text);
            }
            catch (Exception)
            {
                ___curHostPort = 7777;
                __instance.txtHostPort[0].text = string.Empty;
                __instance.txtHostPort[1].text = __instance.txtHostPort[0].text;
            }
            Network.InitializeServer(GadgetCoreConfig.MaxConnections, ___curHostPort, GadgetCoreConfig.UseUPnP);
            ___playing = false;
            return false;
        }
    }
}