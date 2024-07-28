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
        public static bool Prefix(ref Object prefab, Vector3 position, Quaternion rotation, int group, ref Object __result)
        {
            lock (GadgetCoreAPI.resourceLock)
            {
                if (prefab != null && GadgetCoreAPI.resourcePaths.ContainsKey(prefab.GetInstanceID()))
                {
                    __result = RPCHooks.InstantiateResource(GadgetCoreAPI.resourcePaths[prefab.GetInstanceID()], position, rotation, group);
                    return false;
                }
            }
            return true;
        }

        [HarmonyPostfix]
        public static void Postfix(ref Object __result)
        {
            if ((__result.hideFlags & HideFlags.HideInInspector) == HideFlags.HideInInspector)
            {
                __result.hideFlags &= ~(HideFlags.HideAndDontSave | HideFlags.HideInInspector);
                (__result as GameObject)?.SetActive(true);
            }
        }
    }
}