using HarmonyLib;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace GadgetCore.Patches
{
    [HarmonyPatch]
    static class Patch_Object_Instantiate
    {
        [HarmonyTargetMethods]
        public static MethodBase[] TargetMethods()
        {
            return typeof(UnityEngine.Object).GetMethods().Where(x => x.Name == "Instantiate" && (x.GetMethodBody()?.GetILAsByteArray()?.Length ?? 0) > 0).Select(x => x.IsGenericMethod ? x.MakeGenericMethod(typeof(UnityEngine.Object)) : x).ToArray();
        }

        [HarmonyPrefix]
        public static void Prefix(ref UnityEngine.Object original, ref HideFlags __state)
        {
            if (original != null) __state = original.hideFlags;
        }

        [HarmonyPostfix]
        public static void Postfix(ref UnityEngine.Object __result, ref HideFlags __state)
        {
            if ((__state & HideFlags.HideInInspector) == HideFlags.HideInInspector)
            {
                __result.hideFlags |= ~HideFlags.HideInInspector;
                (__result as GameObject)?.SetActive(true);
            }
        }
    }
}