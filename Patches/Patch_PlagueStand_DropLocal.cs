using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(PlagueStand))]
    [HarmonyPatch("DropLocal")]
    internal static class Patch_PlagueStand_DropLocal
    {
        [HarmonyPostfix]
        public static void Postfix(PlagueStand __instance)
        {
            LootTables.DropLoot("object:" + __instance.name.Split(' ', '(')[0], __instance.transform.position);
        }
    }
}