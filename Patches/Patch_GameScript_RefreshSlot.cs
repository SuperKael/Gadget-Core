using HarmonyLib;
using GadgetCore.API;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("RefreshSlot")]
    internal static class Patch_GameScript_RefreshSlot
    {
        [HarmonyPrefix]
        public static void Prefix(GameScript __instance, int i, ref Item[] ___inventory)
        {
            if (___inventory[i].q <= 0)
            {
                ___inventory[i].RemoveAllExtraData();
            }
        }

        [HarmonyPostfix]
        public static void Postfix(GameScript __instance, int i, ref Item[] ___inventory)
        {
            if (___inventory[i].q > 0)
            {
                AnimIcon animIcon = (AnimIcon)__instance.invIconBack[i].GetComponent("AnimIcon");
                if ((ItemRegistry.GetTypeByID(___inventory[i].id) & ItemType.LEVELING) != ItemType.LEVELING)
                {
                    __instance.invIconBack[i].SetActive(false);
                    animIcon.anim = 0;
                }
                else
                {
                    __instance.invIconBack[i].GetComponent<Renderer>().material = (Material)Resources.Load("iBack" + ___inventory[i].tier);
                    if (___inventory[i].tier == 3)
                    {
                        animIcon.anim = 1;
                    }
                    else
                    {
                        animIcon.anim = 0;
                    }
                    __instance.invIconBack[i].SetActive(true);
                }
            }
        }
    }
}