using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("Die")]
    static class Patch_GameScript_Die
    {
        internal static bool godMode;

        [HarmonyPrefix]
        public static bool Prefix()
        {
            if (godMode)
            {
                GameScript.dead = false;
                GameScript.hp = GameScript.maxhp;
                InstanceTracker.GameScript.UpdateHP();
                return false;
            }
            return true;
        }
    }
}