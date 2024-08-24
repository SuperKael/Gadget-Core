using System;
using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Reflection;
using System.Collections;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(ItemScript))]
    [HarmonyPatch("InitL")]
    internal static class Patch_ItemScript_InitL
    {
        public static readonly MethodInfo Burst = typeof(ItemScript).GetMethod("Burst", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(ItemScript __instance, int[] stats, ref Item ___item, ref Package ___package)
        {
            if (stats[1] > 9999)
            {
                if (stats[0] == 52)
                {
                    int trophies = (stats[1] - 1) / 9999;
                    stats[1] -= trophies * 9999;
                    if (trophies > 0) GadgetCoreAPI.SpawnItemLocal(__instance.transform.position, new Item(59, trophies, 0, 0, 0, new int[3], new int[3]));
                }
                else
                {
                    int extraStacks = (stats[1] - 1) / 9999;
                    stats[1] -= extraStacks * 9999;
                    int[] extraStackArray = new int[stats.Length];
                    Array.Copy(stats, extraStackArray, stats.Length);
                    extraStackArray[1] = 9999;
                    for (int i = 0; i < extraStacks; i++)
                    {
                        GadgetCoreAPI.SpawnItemLocal(__instance.transform.position, GadgetCoreAPI.ConstructItemFromIntArray(extraStackArray));
                    }
                }
            }
            ___item = GadgetCoreAPI.ConstructItemFromIntArray(stats);
            ___package = new Package(___item, __instance.gameObject, __instance.localItem);
            __instance.b.GetComponent<Renderer>().material = (Material)Resources.Load("i/i" + ___item.id);
            AnimIcon animIcon = (AnimIcon)__instance.back.GetComponent("AnimIcon");
            if ((ItemRegistry.GetTypeByID(___item.id) & ItemType.LEVELING) != ItemType.LEVELING)
            {
                __instance.back.SetActive(false);
                animIcon.anim = 0;
            }
            else
            {
                __instance.back.GetComponent<Renderer>().material = (Material)Resources.Load("iBack" + ___item.tier);
                if (___item.tier == 3)
                {
                    animIcon.anim = 1;
                }
                else
                {
                    animIcon.anim = 0;
                }
                __instance.back.SetActive(true);
            }
            __instance.StartCoroutine(Burst.Invoke(__instance, new object[] { }) as IEnumerator);
            return false;
        }
    }
}