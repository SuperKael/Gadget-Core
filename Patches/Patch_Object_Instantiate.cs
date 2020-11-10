using HarmonyLib;
using UnityEngine;
using System;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(UnityEngine.Object))]
    [HarmonyPatch("Instantiate")]
    [HarmonyPatch(new Type[] { typeof(UnityEngine.Object), typeof(Vector3), typeof(Quaternion) })]
    static class Patch_Object_Instantiate
    {
        private static HideFlags flags;

        [HarmonyPrefix]
        public static void Prefix(ref UnityEngine.Object original)
        {
            if (original != null) flags = original.hideFlags & (HideFlags.HideAndDontSave | HideFlags.HideInInspector);
        }

        [HarmonyPostfix]
        public static void Postfix(ref UnityEngine.Object __result)
        {
            if ((flags & HideFlags.HideAndDontSave) == HideFlags.HideAndDontSave)
            {
                if (__result is GameObject && (flags & HideFlags.HideInInspector) != HideFlags.HideInInspector)
                {
                    (__result as GameObject).SetActive(true);
                }
            }
        }
    }

    [HarmonyPatch(typeof(UnityEngine.Object))]
    [HarmonyPatch("Instantiate")]
    [HarmonyPatch(new Type[] { typeof(GameObject), typeof(Vector3), typeof(Quaternion) })]
    static class Patch_Object_InstantiateGameObject
    {
        private static HideFlags flags;

        [HarmonyPrefix]
        public static void Prefix(ref GameObject original)
        {
            if (original != null) flags = original.hideFlags & (HideFlags.HideAndDontSave | HideFlags.HideInInspector);
        }

        [HarmonyPostfix]
        public static void Postfix(ref UnityEngine.Object __result)
        {
            if ((flags & HideFlags.HideAndDontSave) == HideFlags.HideAndDontSave)
            {
                if (__result is GameObject && (flags & HideFlags.HideInInspector) != HideFlags.HideInInspector)
                {
                    (__result as GameObject).SetActive(true);
                }
            }
        }
    }
}