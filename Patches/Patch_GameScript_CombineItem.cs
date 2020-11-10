using HarmonyLib;
using GadgetCore.API;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("CombineItem")]
    static class Patch_GameScript_CombineItem
    {
        public static readonly MethodInfo SwapItem = typeof(GameScript).GetMethod("SwapItem", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int slot, Item[] ___inventory, Item ___holdingItem)
        {
            Item item = ___inventory[slot];
            if (!GadgetCoreAPI.CanItemsStack(item, ___holdingItem))
            {
                SwapItem.Invoke(__instance, new object[] { slot });
                return false;
            }
            if (ItemRegistry.Singleton.HasEntry(item.id))
            {
                ItemInfo info = ItemRegistry.Singleton.GetEntry(item.id);
                if ((info.Type & ItemType.NONSTACKING) == ItemType.NONSTACKING)
                {
                    SwapItem.Invoke(__instance, new object[] { slot });
                    return false;
                }
            }
            return true;
        }
    }
}