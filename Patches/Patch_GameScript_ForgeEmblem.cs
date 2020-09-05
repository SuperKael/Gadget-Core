using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using GadgetCore.Util;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("ForgeEmblem")]
    static class Patch_GameScript_ForgeEmblem
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int slot, ref Item[] ___inventory, ref Item ___holdingItem, ref bool ___emblemAgain, ref int ___slotID)
        {
            bool hasEmblemRecipe = GadgetCoreAPI.emblemForgeRecipes.ContainsKey(___inventory[___slotID].id);
            int lootPerEmblem = hasEmblemRecipe ? GadgetCoreAPI.emblemForgeRecipes[___inventory[___slotID].id].Item2 : 10;
            if (___inventory[___slotID].q >= lootPerEmblem && (hasEmblemRecipe || ___inventory[___slotID].id <= 40))
            {
                __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/emblem"), Menuu.soundLevel / 10f);
                int emblemsToCraft = ___inventory[slot].q / lootPerEmblem;
                ___inventory[slot].q -= emblemsToCraft * 10;
                __instance.InvokeMethod("EmblemForging");
                ___emblemAgain = true;
                ___holdingItem = new Item(hasEmblemRecipe ? GadgetCoreAPI.emblemForgeRecipes[___inventory[___slotID].id].Item1 : __instance.InvokeMethod<int>("GetEmblemID", ___inventory[slot].id), emblemsToCraft, 0, 0, 0, new int[3], new int[3]);
                __instance.InvokeMethod("RefreshSlot", slot);
                __instance.InvokeMethod("RefreshHoldingSlot");
            }
            else
            {
                __instance.InvokeMethod("EmblemForging");
                ___emblemAgain = false;
            }
            return false;
        }
    }
}