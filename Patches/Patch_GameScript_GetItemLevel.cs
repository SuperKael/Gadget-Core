using HarmonyLib;
using System.Collections.Generic;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("GetItemLevel")]
    internal static class Patch_GameScript_GetItemLevel
    {
        private static Queue<int> spoofLevels = new Queue<int>();

        public static void SpoofLevel(int level)
        {
            spoofLevels.Enqueue(level);
        }

        [HarmonyPrefix]
        public static bool Prefix(ref int __result)
        {
            if (spoofLevels.Count > 0)
            {
                __result = spoofLevels.Dequeue();
                return false;
            }
            return true;
        }
    }
}