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
                        if (___craft[___craft.Length - 1].q == 0 && craftMenu.IsRecipeUnlocked(craftMenu.craftResult[craftMenu.craftResult.Length - 1].id))
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

        [HarmonyPostfix]
        public static void Postfix(GameScript __instance, Item[] ___craft, ref bool ___canCraft, ref int ___craftType, ref Item ___geartobecrafted, ref Item ___alchemytobecrafted)
        {
            if (!___canCraft)
            {
                switch (___craftType)
                {
                    case 0:
                        if (GadgetCoreAPI.gearForgeRecipes.ContainsKey(Tuple.Create(___craft[0].id, ___craft[1].id, ___craft[2].id)))
                        {
                            ___canCraft = true;
                            __instance.buttonCraft.SendMessage("CanCraft", ___craftType, SendMessageOptions.DontRequireReceiver);
                            ___geartobecrafted = __instance.InvokeMethod<Item>("GetGearCrafted");
                            if (__instance.InvokeMethod<bool>("RecipeCraftedAlready", ___geartobecrafted.id, 0))
                            {
                                __instance.previewObj.GetComponent<Renderer>().material = (Material)Resources.Load("i/i" + ___geartobecrafted.id);
                                __instance.previewObj.SetActive(true);
                            }
                            else
                            {
                                __instance.previewObj.SetActive(false);
                            }
                        }
                        break;
                    case 1:
                        if (GadgetCoreAPI.alchemyStationRecipes.ContainsKey(Tuple.Create(___craft[0].id, ___craft[1].id, ___craft[2].id)))
                        {
                            ___canCraft = true;
                            if (__instance.buttonCraft.activeSelf)
                            {
                                __instance.buttonCraft.SendMessage("CanCraft", ___craftType, SendMessageOptions.DontRequireReceiver);
                            }
                            ___alchemytobecrafted = __instance.InvokeMethod<Item>("GetGearCrafted");
                            if (__instance.InvokeMethod<bool>("RecipeCraftedAlready", ___alchemytobecrafted.id, 0))
                            {
                                __instance.previewObj.GetComponent<Renderer>().material = (Material)Resources.Load("i/i" + ___alchemytobecrafted.id);
                                __instance.previewObj.SetActive(true);
                            }
                            else
                            {
                                __instance.previewObj.SetActive(false);
                            }
                        }
                        break;
                    case 2:
                        if (GadgetCoreAPI.ultimateForgeRecipes.ContainsKey(Tuple.Create(___craft[0].id, ___craft[2].id)))
                        {
                            ___canCraft = true;
                            __instance.buttonCraft.SendMessage("CanCraft", ___craftType, SendMessageOptions.DontRequireReceiver);
                            ___geartobecrafted = __instance.InvokeMethod<Item>("GetUltimateCrafted");
                            if (__instance.InvokeMethod<bool>("RecipeCraftedAlready", ___geartobecrafted.id, 0))
                            {
                                __instance.previewObj.GetComponent<Renderer>().material = (Material)Resources.Load("i/i" + ___geartobecrafted.id);
                                __instance.previewObj.SetActive(true);
                            }
                            else
                            {
                                __instance.previewObj.SetActive(false);
                            }
                        }
                        break;
                    case 3:
                        if ((___craft[0].id > 100 || GadgetCoreAPI.creationMachineRecipes.ContainsKey(___craft[0].id)) &&
                            (___craft[1].id > 100 || GadgetCoreAPI.creationMachineRecipes.ContainsKey(___craft[1].id)) &&
                            (___craft[2].id > 100 || GadgetCoreAPI.creationMachineRecipes.ContainsKey(___craft[2].id)) && ___craft[3].id == 0)
                        {
                            ___canCraft = true;
                            __instance.buttonCraft.SendMessage("CanCraft", ___craftType, SendMessageOptions.DontRequireReceiver);
                            ___geartobecrafted = __instance.InvokeMethod<Item>("GetSlotCrafted");
                            __instance.previewObj.SetActive(false);
                        }
                        break;
                }
            }
        }
    }
}