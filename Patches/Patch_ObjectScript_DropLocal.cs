using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(ObjectScript))]
    [HarmonyPatch("DropLocal")]
    internal static class Patch_ObjectScript_DropLocal
    {
        [HarmonyPrefix]
        public static bool Prefix(ObjectScript __instance)
        {
            if (ObjectRegistry.Singleton.TryGetEntry(__instance.id, out ObjectInfo entry))
            {
                entry.DropItem(__instance.transform.position);
                return false;
            }
            else
            {
                return true;
            }
        }

        [HarmonyPostfix]
        public static void Postfix(ObjectScript __instance)
        {
            LootTables.DropLoot("object:" + __instance.name + (__instance.id % 50), __instance.transform.position);
        }
    }
}