using HarmonyLib;
using GadgetCore.API;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(Network))]
    [HarmonyPatch("Instantiate")]
    static class Patch_Network_Instantiate
    {
        [HarmonyPrefix]
        public static bool Prefix(ref UnityEngine.Object prefab, Vector3 position, Quaternion rotation, int group, ref UnityEngine.Object __result)
        {
            if (GadgetCoreAPI.resourcePaths.ContainsKey(prefab.GetInstanceID()))
            {
                __result = RPCHooks.Instantiate(GadgetCoreAPI.resourcePaths[prefab.GetInstanceID()], position, rotation, group);
                return false;
            }
            return true;
        }

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