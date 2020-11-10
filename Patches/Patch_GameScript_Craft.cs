using HarmonyLib;
using GadgetCore.API;
using System.Reflection;
using UnityEngine;
using GadgetCore.Util;
using System;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("Craft")]
    static class Patch_GameScript_Craft
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, Item[] ___craft, ref bool ___canCraft, ref int ___craftType, ref int ___craftValue)
        {
            if (MenuRegistry.Singleton.GetEntry(___craftType) is CraftMenuInfo craftMenu)
            {
				if (___canCraft)
				{
					__instance.GetComponent<AudioSource>().PlayOneShot(craftMenu.CraftSound, Menuu.soundLevel / 10f);
					Camera.main.GetComponent<Animation>().Play("craft");
					__instance.sparks.Play();
					__instance.buttonCraft.GetComponent<Animation>().Play();
					___craftValue += craftMenu.GetCraftValue(craftMenu.craftResult[craftMenu.craftResult.Length - 1].id);
					if (___craftValue > 100)
					{
						___craftValue = 100;
					}
					__instance.barCraft.transform.localScale = new Vector3(___craftValue / 100f * 11.5f, 0.6f, 1f);
					if (___craftValue >= 100)
					{
						__instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/create"), Menuu.soundLevel / 10f);
						___canCraft = false;
						___craftValue = 0;
						__instance.sparks2.Play();
						__instance.buttonCraft.SendMessage("CantCraft", ___craftType);
						if (___craft.Length >= craftMenu.craftResult.Length) __instance.previewObj.SetActive(false);
						for (int i = 0; i < Math.Min(___craft.Length, craftMenu.craftResult.Length); i++)
						{
							___craft[i] = craftMenu.craftResult[i];
						}
						if (craftMenu.activePerformer >= 0) craftMenu.CraftFinalizers[craftMenu.activePerformer]?.Invoke(___craft);
						for (int i = 0; i < Math.Min(___craft.Length, craftMenu.craftResult.Length); i++)
						{
							__instance.InvokeMethod("RefreshSlotCraft", i);
						}
						__instance.InvokeMethod("CheckRecipeStatus", ___craft[___craft.Length - 1].id, 0);
						__instance.barCraft.transform.localScale = new Vector3(0f, 0.6f, 1f);
						craftMenu.activePerformer = -1;
						__instance.InvokeMethod("CraftCheck");
					}
				}
				return false;
            }
            return true;
        }
    }
}