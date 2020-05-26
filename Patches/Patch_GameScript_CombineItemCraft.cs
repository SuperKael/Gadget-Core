using HarmonyLib;
using GadgetCore.API;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("CombineItemCraft")]
    static class Patch_GameScript_CombineItemCraft
    {
        public static readonly MethodInfo SwapItemCraft = typeof(GameScript).GetMethod("SwapItemCraft", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int slot, Item[] ___craft, Item ___holdingItem)
        {
            Item item = ___craft[slot];
            if (item.corrupted != ___holdingItem.corrupted)
            {
                SwapItemCraft.Invoke(__instance, new object[] { slot });
                return false;
            }
            if (ItemRegistry.GetSingleton().HasEntry(item.id))
            {
                ItemInfo info = ItemRegistry.GetSingleton().GetEntry(item.id);
                if ((info.Type & ItemType.NONSTACKING) == ItemType.NONSTACKING)
                {
                    SwapItemCraft.Invoke(__instance, new object[] { slot });
                    return false;
                }
            }
            return true;
        }
    }
}