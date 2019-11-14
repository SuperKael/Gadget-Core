using HarmonyLib;
using GadgetCore.API;
using GadgetCore;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("RefreshHoldingSlot")]
    static class Patch_GameScript_RefreshHoldingSlot
    {
        [HarmonyPostfix]
        public static void Postfix(GameScript __instance, bool ___holdingCombatChip, Item ___holdingItem)
        {
            if (!___holdingCombatChip && ___holdingItem.id != 0 && ___holdingItem.q > 0)
            {
                AnimIcon animIcon = (AnimIcon)__instance.holdingItemBack.GetComponent("AnimIcon");
                if ((ItemRegistry.GetTypeByID(___holdingItem.id) & ItemType.LEVELING) != ItemType.LEVELING)
                {
                    __instance.holdingItemBack.SetActive(false);
                    animIcon.anim = 0;
                }
                else
                {
                    __instance.holdingItemBack.GetComponent<Renderer>().material = (Material)Resources.Load("iBack" + ___holdingItem.tier);
                    if (___holdingItem.tier == 3)
                    {
                        animIcon.anim = 1;
                    }
                    else
                    {
                        animIcon.anim = 0;
                    }
                    __instance.holdingItemBack.SetActive(true);
                }
            }
        }
    }
}