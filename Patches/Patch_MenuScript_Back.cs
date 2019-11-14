using HarmonyLib;
using GadgetCore.API;
using GadgetCore;
using UnityEngine;
using System;
using System.Threading;
using System.Collections;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(MenuScript))]
    [HarmonyPatch("Back")]
    static class Patch_MenuScript_Back
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (Network.isClient || Network.isServer) Network.Disconnect();
        }
    }
}