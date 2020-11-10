using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(ChestScript))]
    [HarmonyPatch("DropLocal")]
    static class Patch_ChestScript_DropLocal
    {
        [HarmonyPostfix]
        public static void Postfix(ChestScript __instance)
        {
            if (__instance.isGold)
            {
                LootTables.DropLoot("chest:gold", __instance.transform.position, 0.2f);
            }
            else
            {
                LootTables.DropLoot("chest:basic", __instance.transform.position, 0.2f);
            }
        }
    }
}