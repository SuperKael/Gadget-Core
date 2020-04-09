using HarmonyLib;
using UnityEngine;
using System;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(MenuScript))]
    [HarmonyPatch("HostGame")]
    static class Patch_MenuScript_HostGame
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
            Network.InitializeServer(GadgetCoreConfig.MaxConnections, ___curHostPort, false);
            if (GadgetCoreConfig.UseUPnP)
            {
                int curHostPort = ___curHostPort;
                try
                {
                    __instance.StartCoroutine(GadgetCore.CoreLib.ForwardPort(curHostPort));
                }
                catch (System.IO.FileNotFoundException)
                {
                    GadgetCore.CoreLogger.Log("Gadget Core was unable to perform UPnP Port Forwarding because Gadget Core's zip file has not been unpacked!");
                }
                catch (Exception e)
                {
                    GadgetCore.CoreLogger.Log("The following error occured while attempting to perform UPnP Port Forwarding:" + Environment.NewLine + e.ToString());
                }
            }
            ___playing = false;
            return false;
        }
    }
}