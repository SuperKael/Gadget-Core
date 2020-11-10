using HarmonyLib;
using GadgetCore.API;
using System.Reflection;
using UnityEngine;
using GadgetCore.Util;
using System;
using System.Linq;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("CraftCheck")]
    static class Patch_GameScript_CraftCheck
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, Item[] ___craft, ref bool ___canCraft, ref int ___craftType)
        {
            if (MenuRegistry.Singleton.GetEntry(___craftType) is CraftMenuInfo craftMenu)
            {
                bool validated = false;
                for (int i = craftMenu.CraftValidators.Count - 1; i >= 0; i--)
                {
                    if (craftMenu.CraftValidators[i](___craft))
                    {
                        validated = true;
                        craftMenu.activePerformer = i;
                        craftMenu.craftResult = ___craft.Select(x => GadgetCoreAPI.CopyItem(x)).ToArray();
                        craftMenu.CraftPerformers[i](craftMenu.craftResult);
                        ___canCraft = true;
                        __instance.buttonCraft.SendMessage("CanCraft", ___craftType, SendMessageOptions.DontRequireReceiver);
                        if (___craft[___craft.Length - 1].q == 0 && craftMenu.unlockedRecipes.Contains(craftMenu.craftResult[craftMenu.craftResult.Length - 1].id))
                        {
                            __instance.previewObj.GetComponent<Renderer>().material = (Material)Resources.Load("i/i" + craftMenu.craftResult[craftMenu.craftResult.Length - 1].id);
                            __instance.previewObj.SetActive(true);
                        }
                        else
                        {
                            __instance.previewObj.SetActive(false);
                        }
                        break;
                    }
                }
                if (!validated)
                {
                    __instance.previewObj.SetActive(false);
                    ___canCraft = false;
                    __instance.buttonCraft.SendMessage("CantCraft", ___craftType, SendMessageOptions.DontRequireReceiver);
                }
                return false;
            }
            return true;
        }
    }
}