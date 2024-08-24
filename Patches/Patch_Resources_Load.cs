using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Reflection;
using System.Linq;

namespace GadgetCore.Patches
{
    [HarmonyPatch]
    internal static class Patch_Resources_Load
    {
        [HarmonyTargetMethods]
        public static MethodBase[] TargetMethods()
        {
            return typeof(Resources).GetMethods().Where(x => x.Name == "Load" && (x.GetMethodBody()?.GetILAsByteArray()?.Length ?? 0) > 0).Select(x => x.IsGenericMethod ? x.MakeGenericMethod(typeof(UnityEngine.Object)) : x).ToArray();
        }

        [HarmonyPrefix]
        public static bool Prefix(string path, ref UnityEngine.Object __result)
        {
            if (GadgetCoreAPI.IsCustomResourceRegistered(path))
            {
                lock (GadgetCoreAPI.resourceLock)
                {
                    __result = GadgetCoreAPI.resources[path];
                }
                if (__result is AudioClip au && !au.LoadAudioData()) GadgetCore.CoreLogger.LogWarning("Loading custom AudioClip resource \"" + path + "\" that is in loadState " + au.loadState);
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        public static void Postfix(string path, ref UnityEngine.Object __result)
        {
            if (__result == null)
            {
                UnityEngine.Object fallbackResource = GetFallbackResource(path);
                if (fallbackResource != null) __result = fallbackResource;
            }
        }

        private static UnityEngine.Object GetFallbackResource(string path)
        {
            if (path.StartsWith("i/i") || path.StartsWith("cc/cc")) return GadgetCoreAPI.MissingItemMaterial;
            return null;
        }
    }
}