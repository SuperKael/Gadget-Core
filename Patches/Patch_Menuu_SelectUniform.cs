using HarmonyLib;
using GadgetCore.API;
using System.Collections;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(Menuu))]
    [HarmonyPatch("SelectUniform")]
    static class Patch_Menuu_SelectUniform
    {
        [HarmonyPrefix]
        public static bool Prefix(Menuu __instance, ref int ___stuffSelecting, ref IEnumerator __result)
        {
            ___stuffSelecting = 2;
            __instance.stuffChosen.transform.position = new Vector3(__instance.box[Menuu.curUniform % CharacterRaceRegistry.PageSize].transform.position.x, __instance.box[Menuu.curUniform % CharacterRaceRegistry.PageSize].transform.position.y, -3f);
            __instance.menuStuffSelect.SetActive(true);
            CharacterUniformRegistry.CurrentPage = Menuu.curUniform / CharacterUniformRegistry.PageSize + 1;
            __instance.RefreshStuffSelect(Menuu.curUniform);
            __result = GadgetCoreAPI.EmptyEnumerator();
            return false;
        }
    }
}