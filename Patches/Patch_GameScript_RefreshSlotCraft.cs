using HarmonyLib;
using GadgetCore.API;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("RefreshSlotCraft")]
    static class Patch_GameScript_RefreshSlotCraft
    {
        [HarmonyPrefix]
        public static void Prefix(GameScript __instance, int i, ref Item[] ___craft)
        {
            if (___craft[i].q <= 0)
            {
                ___craft[i].RemoveAllExtraData();
            }
        }

        [HarmonyPostfix]
        public static void Postfix(GameScript __instance, int i, ref Item[] ___craft)
        {
            if (___craft[i].q > 0)
            {
                AnimIcon animIcon = (AnimIcon)__instance.craftIconBack[i].GetComponent("AnimIcon");
                if ((ItemRegistry.GetTypeByID(___craft[i].id) & ItemType.LEVELING) != ItemType.LEVELING)
                {
                    __instance.craftIconBack[i].SetActive(false);
                    animIcon.anim = 0;
                }
                else
                {
                    __instance.craftIconBack[i].GetComponent<Renderer>().material = (Material)Resources.Load("iBack" + ___craft[i].tier);
                    MonoBehaviour.print("(GadgetCore Patched) inviconback tier " + ___craft[i].tier);
                    if (___craft[i].tier == 3)
                    {
                        animIcon.anim = 1;
                    }
                    else
                    {
                        animIcon.anim = 0;
                    }
                    __instance.craftIconBack[i].SetActive(true);
                }
            }
        }
    }
}