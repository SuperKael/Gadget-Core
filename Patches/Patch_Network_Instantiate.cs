using HarmonyLib;
using GadgetCore.API;
using GadgetCore;
using UnityEngine;
using System.Reflection;
using System;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(Network))]
    [HarmonyPatch("Instantiate")]
    static class Patch_Network_Instantiate
    {
        [HarmonyPostfix]
        public static void Postfix(ref UnityEngine.Object __result)
        {
            if ((__result.hideFlags & HideFlags.HideAndDontSave) == HideFlags.HideAndDontSave)
            {
                if (__result is GameObject && (__result.hideFlags & HideFlags.HideInInspector) != HideFlags.HideInInspector)
                {
                    (__result as GameObject).SetActive(true);
                }
                __result.hideFlags &= ~(HideFlags.HideAndDontSave | HideFlags.HideInInspector);
            }
        }
    }
}