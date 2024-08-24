using HarmonyLib;
using GadgetCore.API;
using GadgetCore.Util;
using System.Collections;
using System.Linq;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("CheckRecipeStatus")]
    internal static class Patch_GameScript_CheckRecipeStatus
	{
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int a, int q, ref int ___craftType)
        {
            if (MenuRegistry.Singleton.GetEntry(___craftType) is CraftMenuInfo craftMenu)
            {
				if (!craftMenu.IsRecipeUnlocked(a))
				{
                    craftMenu.UnlockRecipe(a);
					__instance.InvokeMethod("RefreshRecipeUnlock");
					__instance.StartCoroutine(__instance.InvokeMethod<IEnumerator>("RecipeVictory"));
				}
				return false;
            }
            else if (___craftType >= 0 && ___craftType <= 3 && ItemRegistry.Singleton.GetEntry(a) is ItemInfo item && !(item is VanillaItemInfo))
            {
                if (!GadgetCoreAPI.unlockedVanillaStationRecipes[___craftType].Contains(a))
                {
                    GadgetCoreAPI.unlockedVanillaStationRecipes[___craftType].Add(a);
                    PreviewLabs.PlayerPrefs.SetString("craftMenu" + ___craftType + "unlocks", GadgetCoreAPI.unlockedVanillaStationRecipes[___craftType].Select(x => x.ToString()).Concat(","));
                    __instance.InvokeMethod("RefreshRecipeUnlock");
                    __instance.StartCoroutine(__instance.InvokeMethod<IEnumerator>("RecipeVictory"));
                }
                return false;
            }
            return true;
        }
    }
}