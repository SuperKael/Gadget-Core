using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using GadgetCore.Util;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("PrismItem")]
    static class Patch_GameScript_PrismItem
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int slot, ref Item[] ___inventory, ref Item ___holdingItem, ref bool ___prismAgain, ref int ___slotID)
        {
            bool hasPrismRecipe = GadgetCoreAPI.prismForgeRecipes.ContainsKey(___inventory[___slotID].id);
            int lootPerPrism = hasPrismRecipe ? GadgetCoreAPI.prismForgeRecipes[___inventory[___slotID].id].Item2 : 10;
            if (___inventory[___slotID].q >= lootPerPrism && (hasPrismRecipe || (___inventory[___slotID].id >= 86 && ___inventory[___slotID].id <= 88)))
            {
                __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/glitter4"), Menuu.soundLevel / 10f);
                int prismsToCraft = ___inventory[slot].q / lootPerPrism;
                ___inventory[slot].q -= prismsToCraft * 10;
                __instance.InvokeMethod("Prismer");
                ___prismAgain = false;
                ___holdingItem = new Item(hasPrismRecipe ? GadgetCoreAPI.prismForgeRecipes[___inventory[___slotID].id].Item1 : ___inventory[slot].id + 3, prismsToCraft, 0, 0, 0, new int[3], new int[3]);
                __instance.InvokeMethod("RefreshSlot", slot);
                __instance.InvokeMethod("RefreshHoldingSlot");
            }
            else
            {
                __instance.InvokeMethod("Prismer");
                ___prismAgain = false;
            }
            return false;
        }
    }
}