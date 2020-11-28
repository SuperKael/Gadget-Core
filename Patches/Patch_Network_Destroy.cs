using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Reflection;
using System;

namespace GadgetCore.Patches
{
    [HarmonyPatch]
    static class Patch_Network_Destroy
    {
        [HarmonyTargetMethod]
        public static MethodBase TargetMethod()
        {
            return typeof(Network).GetMethod("Destroy", new Type[] { typeof(NetworkViewID) });
        }

        [HarmonyPrefix]
        public static bool Prefix(NetworkViewID viewID)
        {
            NetworkView view = NetworkView.Find(viewID);
            if (Network.isServer && view != null && (view.hideFlags |= (HideFlags)64) != 0)
            {
                view.hideFlags &= ~(HideFlags)64;
                RPCHooks.Destroy(viewID);
                return false;
            }
            return true;
        }
    }
}