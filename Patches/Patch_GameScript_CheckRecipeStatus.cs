using HarmonyLib;
using GadgetCore.API;
using System.Reflection;
using UnityEngine;
using GadgetCore.Util;
using System;
using System.Collections;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("CheckRecipeStatus")]
    static class Patch_GameScript_CheckRecipeStatus
	{
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int a, ref int ___craftType)
        {
            if (MenuRegistry.Singleton.GetEntry(___craftType) is CraftMenuInfo craftMenu)
            {
				if (!craftMenu.unlockedRecipes.Contains(a))
				{
                    craftMenu.UnlockRecipe(a);
					__instance.InvokeMethod("RefreshRecipeUnlock");
					__instance.StartCoroutine(__instance.InvokeMethod<IEnumerator>("RecipeVictory"));
				}
				return false;
            }
            return true;
        }
    }
}