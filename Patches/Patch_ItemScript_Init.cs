using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Reflection;
using System.Collections;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(ItemScript))]
    [HarmonyPatch("Init")]
    internal static class Patch_ItemScript_Init
    {
        public static readonly MethodInfo Burst = typeof(ItemScript).GetMethod("Burst", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(ItemScript __instance, int[] stats, ref Item ___item, ref Package ___package)
        {
            ___item = GadgetCoreAPI.ConstructItemFromIntArray(stats, true, false);
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