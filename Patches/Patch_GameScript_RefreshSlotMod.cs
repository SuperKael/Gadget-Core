using HarmonyLib;
using GadgetCore.API;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("RefreshSlotMod")]
    internal static class Patch_GameScript_RefreshSlotMod
    {
        [HarmonyPrefix]
        public static void Prefix(GameScript __instance, int i, ref Item[] ___modSlot)
        {
            if (___modSlot[i].q <= 0)
            {
                ___modSlot[i].RemoveAllExtraData();
            }
        }

        [HarmonyPostfix]
        public static void Postfix(GameScript __instance, int i, ref Item[] ___modSlot)
        {
            AnimIcon animIcon = (AnimIcon)__instance.modIconBack[i].GetComponent("AnimIcon");
            if ((ItemRegistry.GetTypeByID(___modSlot[i].id) & ItemType.LEVELING) != ItemType.LEVELING)
            {
                __instance.modIconBack[i].SetActive(false);
                animIcon.anim = 0;
            }
            else
            {
                __instance.modIconBack[i].GetComponent<Renderer>().material = (Material)Resources.Load("iBack" + ___modSlot[i].tier);
                if (___modSlot[i].tier == 3)
                {
                    animIcon.anim = 1;
                }
                else
                {
                    animIcon.anim = 0;
                }
                __instance.modIconBack[i].SetActive(true);
            }
        }
    }
}