using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(Resources))]
    [HarmonyPatch("Load")]
    [HarmonyPatch(new Type[] { typeof(string) })]
    static class Patch_Resources_Load
    {
        [HarmonyPrefix]
        public static bool Prefix(string path, ref UnityEngine.Object __result)
        {
            if (GadgetCoreAPI.IsCustomResourceRegistered(path))
            {
                __result = GadgetCoreAPI.resources[path];
                return false;
            }
            return true;
        }
    }
}