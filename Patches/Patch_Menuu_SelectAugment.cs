using HarmonyLib;
using GadgetCore.API;
using System.Collections;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(Menuu))]
    [HarmonyPatch("SelectAugment")]
    static class Patch_Menuu_SelectAugment
    {
        [HarmonyPrefix]
        public static bool Prefix(Menuu __instance, ref int ___stuffSelecting, ref IEnumerator __result)
        {
            ___stuffSelecting = 1;
            __instance.stuffChosen.transform.position = new Vector3(__instance.box[Menuu.curAugment % CharacterRaceRegistry.PageSize].transform.position.x, __instance.box[Menuu.curAugment % CharacterRaceRegistry.PageSize].transform.position.y, -3f);
            __instance.menuStuffSelect.SetActive(true);
            CharacterAugmentRegistry.CurrentPage = Menuu.curAugment / CharacterAugmentRegistry.PageSize + 1;
            __instance.RefreshStuffSelect(Menuu.curAugment);
            __result = GadgetCoreAPI.EmptyEnumerator();
            return false;
        }
    }
}