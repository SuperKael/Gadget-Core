using HarmonyLib;
using System;
using System.Reflection;

namespace GadgetCore.Patches
{
    /// <summary>
    /// Dirty hack, nothing to see here...
    /// </summary>
    [HarmonyPatch(typeof(Type))]
    [HarmonyPatch("GetMethod")]
    [HarmonyPatch(new[] { typeof(string), typeof(BindingFlags) })]
    internal static class Patch_Type_GetMethod
    {
        private static readonly string RoguelandsAssemblyName = typeof(GameScript).AssemblyQualifiedName;

        [HarmonyPrefix]
        public static void Prefix(Type __instance, ref BindingFlags bindingAttr)
        {
            if (__instance.AssemblyQualifiedName == RoguelandsAssemblyName)
            {
                bindingAttr |= BindingFlags.Public;
            }
        }
    }
}