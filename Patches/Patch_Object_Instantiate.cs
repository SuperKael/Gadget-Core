using HarmonyLib;
using GadgetCore.API;
using GadgetCore;
using UnityEngine;
using System.Reflection;
using System;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(UnityEngine.Object))]
    [HarmonyPatch("Instantiate")]
    [HarmonyPatch(new Type[] { typeof(UnityEngine.Object), typeof(Vector3), typeof(Quaternion) })]
    static class Patch_Object_Instantiate
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

    [HarmonyPatch(typeof(UnityEngine.Object))]
    [HarmonyPatch("Instantiate")]
    [HarmonyPatch(new Type[] { typeof(GameObject), typeof(Vector3), typeof(Quaternion) })]
    static class Patch_Object_InstantiateGameObject
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