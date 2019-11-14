using HarmonyLib;
using GadgetCore.API;
using GadgetCore;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("RefreshQuestSlot")]
    static class Patch_GameScript_RefreshQuestSlot
    {
        [HarmonyPostfix]
        public static void Postfix(GameScript __instance, int i, ref Item[] ___inventoryQuest)
        {
            if (___inventoryQuest[i].q > 0)
            {
                AnimIcon animIcon = (AnimIcon)__instance.invQuestIconBack[i].GetComponent("AnimIcon");
                if ((ItemRegistry.GetTypeByID(___inventoryQuest[i].id) & ItemType.LEVELING) != ItemType.LEVELING)
                {
                    __instance.invQuestIconBack[i].SetActive(false);
                    animIcon.anim = 0;
                }
                else
                {
                    __instance.invQuestIconBack[i].GetComponent<Renderer>().material = (Material)Resources.Load("iBack" + ___inventoryQuest[i].tier);
                    if (___inventoryQuest[i].tier == 3)
                    {
                        animIcon.anim = 1;
                    }
                    else
                    {
                        animIcon.anim = 0;
                    }
                    __instance.invQuestIconBack[i].SetActive(true);
                }
            }
        }
    }
}