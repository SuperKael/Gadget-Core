using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System.Linq;

namespace GadgetCore.Patches
{
    [HarmonyPatch]
    internal static class Patch_Object_Instantiate
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
                __result.hideFlags &= ~(HideFlags.HideAndDontSave | HideFlags.HideInInspector);
                /*if (__result is GameObject obj)
                {
                    foreach (Component comp in obj.GetComponents(typeof(Component)))
                    {
                        if (!(comp is Transform) && !(comp is Rigidbody)) obj.ReplaceComponent(comp.GetType(), comp.GetType());
                    }
                }*/
                (__result as GameObject)?.SetActive(true);
            }
        }
    }
}