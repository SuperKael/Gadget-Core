using HarmonyLib;
using GadgetCore.API;
using System.Collections;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(PlayerScript))]
    [HarmonyPatch("Attack")]
    static class Patch_PlayerScript_Attack
    {
        [HarmonyPrefix]
        public static bool Prefix(PlayerScript __instance, ref bool ___attacking, ref bool ___canAttack, ref GameScript ___gameScript, ref IEnumerator __result)
        {
            if (ItemRegistry.Singleton.HasEntry(GameScript.equippedIDs[0]) && !___attacking && ___canAttack && !___gameScript.combatSwitching && !PlayerScript.beaming && !VanillaItemInfo.Attacking)
            {
                ItemRegistry.Singleton.GetEntry(GameScript.equippedIDs[0]).InvokeOnAttack(__instance);
                __result = GadgetCoreAPI.EmptyEnumerator();
                return false;
            }
            return true;
        }
    }
}