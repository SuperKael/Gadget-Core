using HarmonyLib;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("AddChest")]
    internal static class Patch_GameScript_AddChest
    {
        [HarmonyPrefix]
        public static void Prefix(GameScript __instance, int ___curChest, ref int[,] ___CHESTLOOT, ref int[] ___depth)
        {
            if (___depth[___curChest] >= ___CHESTLOOT.GetLength(1))
            {
                int[,] newCHESTLOOT = new int[___CHESTLOOT.GetLength(0), ___CHESTLOOT.GetLength(1) * 2];
                for (int c = 0; c < ___CHESTLOOT.GetLength(0); c++)
                {
                    for (int d = 0; d < ___CHESTLOOT.GetLength(1); d++)
                    {
                        newCHESTLOOT[c, d] = ___CHESTLOOT[c, d];
                    }
                }
                ___CHESTLOOT = newCHESTLOOT;
            }
        }
    }
}