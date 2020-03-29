using HarmonyLib;
using UnityEngine;
using System.Collections;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("RefreshStoragePage")]
    static class Patch_GameScript_RefreshStoragePage
    {
        public static readonly MethodInfo RefreshSlotStorage = typeof(GameScript).GetMethod("RefreshSlotStorage", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly FieldInfo curStoragePage = typeof(GameScript).GetField("curStoragePage", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int p, ref IEnumerator __result)
        {
            __result = RefreshStoragePage(__instance, p);
            return false;
        }

        private static IEnumerator RefreshStoragePage(GameScript __instance, int p)
        {
            int oldCurStoragePage = (int)curStoragePage.GetValue(__instance);
            curStoragePage.SetValue(__instance, p);
            __instance.storageSelect.transform.position = new Vector3(__instance.storageButton[p].transform.position.x, __instance.storageButton[p].transform.position.y, __instance.storageSelect.transform.position.z);
            for (int i = 0; i < 30; i++)
            {
                RefreshSlotStorage.Invoke(__instance, new object[] { i });
            }
            curStoragePage.SetValue(__instance, oldCurStoragePage);
            yield return null;
            yield break;
        }
    }
}