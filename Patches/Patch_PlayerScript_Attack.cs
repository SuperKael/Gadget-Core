using HarmonyLib;
using GadgetCore.API;
using GadgetCore;
using System.Collections;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(PlayerScript))]
    [HarmonyPatch("Attack")]
    static class Patch_PlayerScript_Attack
    {
        [HarmonyPrefix]
        public static bool Prefix(PlayerScript __instance, ref bool ___attacking, ref bool ___canAttack, ref GameScript ___gameScript, ref IEnumerator __result)
        {
            if (ItemRegistry.GetSingleton().HasEntry(GameScript.equippedIDs[0]) && !___attacking && ___canAttack && !___gameScript.combatSwitching && !PlayerScript.beaming)
            {
                __result = ItemRegistry.GetSingleton().GetEntry(GameScript.equippedIDs[0]).InvokeOnAttack(__instance);
                return false;
            }
            return true;
        }
    }
}