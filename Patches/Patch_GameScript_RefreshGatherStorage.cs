using HarmonyLib;
using GadgetCore.API;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("RefreshGatherStorage")]
    internal static class Patch_GameScript_RefreshGatherStorage
    {
        [HarmonyPostfix]
        public static void Postfix(GameScript __instance, ref Item[] ___gatherStorage)
        {
            for (int i = 0; i < 9; i++)
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
}