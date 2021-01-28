using HarmonyLib;
using GadgetCore.API;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("CombineItemStorage")]
    static class Patch_GameScript_CombineItemStorage
    {
        public static readonly MethodInfo SwapItemStorage = typeof(GameScript).GetMethod("SwapItemStorage", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int slot, Item[] ___storage, Item ___holdingItem, int ___curStoragePage)
        {
            Item item = ___storage[slot + ___curStoragePage * 30];
            if (!GadgetCoreAPI.CanItemsStack(item, ___holdingItem))
            {
                SwapItemStorage.Invoke(__instance, new object[] { slot });
                return false;
            }
            if (ItemRegistry.Singleton.HasEntry(item.id))
            {
                ItemInfo info = ItemRegistry.Singleton.GetEntry(item.id);
                if ((info.Type & ItemType.NONSTACKING) == ItemType.NONSTACKING)
                {
                    SwapItemStorage.Invoke(__instance, new object[] { slot });
                    return false;
                }
            }
            return true;
        }
    }
}