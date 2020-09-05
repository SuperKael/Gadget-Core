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
            if (Network.isServer)
            {
                RPCHooks.Destroy(viewID);
                return false;
            }
            return true;
        }
    }
}