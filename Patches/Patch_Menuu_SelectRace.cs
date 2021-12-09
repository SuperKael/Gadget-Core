using HarmonyLib;
using GadgetCore.API;
using System.Collections;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(Menuu))]
    [HarmonyPatch("SelectRace")]
    static class Patch_Menuu_SelectRace
    {
        [HarmonyPrefix]
        public static bool Prefix(Menuu __instance, ref int ___stuffSelecting, ref IEnumerator __result)
        {
            ___stuffSelecting = 0;
            __instance.stuffChosen.transform.position = new Vector3(__instance.box[Menuu.curRace % CharacterRaceRegistry.PageSize].transform.position.x, __instance.box[Menuu.curRace % CharacterRaceRegistry.PageSize].transform.position.y, -3f);
            __instance.menuStuffSelect.SetActive(true);
            CharacterRaceRegistry.CurrentPage = Menuu.curRace / CharacterRaceRegistry.PageSize + 1;
            __instance.RefreshStuffSelect(Menuu.curRace % CharacterRaceRegistry.PageSize);
            __result = GadgetCoreAPI.EmptyEnumerator();
            return false;
        }
    }
}