using HarmonyLib;
using GadgetCore.API;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("RefreshSlotStorage")]
    static class Patch_GameScript_RefreshSlotStorage
    {
        [HarmonyPrefix]
        public static void Prefix(GameScript __instance, int i, ref Item[] ___storage, ref int ___curStoragePage)
        {
            int num = i + (___curStoragePage * 30);
            if (___storage[num].q <= 0)
            {
                ___storage[num].RemoveAllExtraData();
            }
        }

        [HarmonyPostfix]
        public static void Postfix(GameScript __instance, int i, ref Item[] ___storage, ref int ___curStoragePage)
        {
            int num = i + (___curStoragePage * 30);
            AnimIcon animIcon = (AnimIcon)__instance.storageIconBack[i].GetComponent("AnimIcon");
            if ((ItemRegistry.GetTypeByID(___storage[num].id) & ItemType.LEVELING) != ItemType.LEVELING)
            {
                __instance.storageIconBack[i].SetActive(false);
                animIcon.anim = 0;
            }
            else
            {
                __instance.storageIconBack[i].GetComponent<Renderer>().material = (Material)Resources.Load("iBack" + ___storage[num].tier);
                if (___storage[num].tier == 3)
                {
                    animIcon.anim = 1;
                }
                else
                {
                    animIcon.anim = 0;
                }
                __instance.storageIconBack[i].SetActive(true);
            }
        }
    }
}