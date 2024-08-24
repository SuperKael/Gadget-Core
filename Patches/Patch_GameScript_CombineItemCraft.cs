using HarmonyLib;
using GadgetCore.API;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("CombineItemCraft")]
    internal static class Patch_GameScript_CombineItemCraft
    {
        public static readonly MethodInfo SwapItemCraft = typeof(GameScript).GetMethod("SwapItemCraft", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int slot, Item[] ___craft, Item ___holdingItem)
        {
            Item item = ___craft[slot];
            if (!GadgetCoreAPI.CanItemsStack(item, ___holdingItem))
            {
                SwapItemCraft.Invoke(__instance, new object[] { slot });
                return false;
            }
            if (ItemRegistry.Singleton.HasEntry(item.id))
            {
                ItemInfo info = ItemRegistry.Singleton.GetEntry(item.id);
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