using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using GadgetCore.Loader;
using System.Linq;

namespace GadgetCore.Patches
{
    [HarmonyPatch]
    static class Patch_CustomEntityScript_UpdateMethods
    {
        [HarmonyTargetMethods]
        public static MethodBase[] TargetMethods()
        {
            MethodBase[] methods = GadgetMods.ListAllMods().SelectMany(x => x.Assembly.GetExportedTypes()).Where(x => typeof(CustomEntityScript).IsAssignableFrom(x)).SelectMany(x => new MethodInfo[] { x.GetMethod("Update", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance), x.GetMethod("FixedUpdate", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) }).Where(x => x != null).ToArray();
            return methods != null && methods.Length > 0 ? methods : null;
        }

        [HarmonyPostfix]
        public static void Postfix(CustomEntityScript __instance)
        {
            if (__instance.StunTime > 0) __instance.rigidbody.velocity = Vector3.zero;
        }
    }
}