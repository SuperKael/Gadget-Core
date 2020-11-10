using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(ObjectScript))]
    [HarmonyPatch("DropLocal")]
    static class Patch_ObjectScript_DropLocal
    {
        [HarmonyPostfix]
        public static void Postfix(ObjectScript __instance)
        {
            LootTables.DropLoot("object:" + __instance.name + (__instance.id % 50), __instance.transform.position);
        }
    }
}