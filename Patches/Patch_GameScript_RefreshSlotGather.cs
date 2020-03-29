using HarmonyLib;
using GadgetCore.API;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("RefreshSlotGather")]
    static class Patch_GameScript_RefreshSlotGather
    {
        [HarmonyPrefix]
        public static void Prefix(GameScript __instance, int i, ref Item[] ___gatherStorage)
        {
            if (___gatherStorage[i].q <= 0)
            {
                ___gatherStorage[i].RemoveAllExtraData();
            }
        }

        [HarmonyPostfix]
        public static void Postfix(GameScript __instance, int i, ref Item[] ___gatherStorage)
        {
            AnimIcon animIcon = (AnimIcon)__instance.gatherStorageIconBack[i].GetComponent("AnimIcon");
            if ((ItemRegistry.GetTypeByID(___gatherStorage[i].id) & ItemType.LEVELING) != ItemType.LEVELING)
            {
                __instance.gatherStorageIconBack[i].SetActive(false);
                animIcon.anim = 0;
            }
            else
            {
                __instance.gatherStorageIconBack[i].GetComponent<Renderer>().material = (Material)Resources.Load("iBack" + ___gatherStorage[i].tier);
                if (___gatherStorage[i].tier == 3)
                {
                    animIcon.anim = 1;
                }
                else
                {
                    animIcon.anim = 0;
                }
                __instance.gatherStorageIconBack[i].SetActive(true);
            }
        }
    }
}