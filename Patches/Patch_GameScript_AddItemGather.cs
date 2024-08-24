using HarmonyLib;
using GadgetCore.API;
using System.Collections;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("AddItemGather")]
    internal static class Patch_GameScript_AddItemGather
    {
        public static readonly MethodInfo RefreshSlotGather = typeof(GameScript).GetMethod("RefreshSlotGather", BindingFlags.Public | BindingFlags.Instance);
        public static readonly MethodInfo RefreshGatherStorage = typeof(GameScript).GetMethod("RefreshGatherStorage", BindingFlags.Public | BindingFlags.Instance);
        public static readonly FieldInfo gatherStorage = typeof(GameScript).GetField("gatherStorage", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int id, ref IEnumerator __result)
        {
            if (ItemRegistry.Singleton.HasEntry(id))
            {
                __result = AddItemGather(__instance, id);
                return false;
            }
            return true;
        }

        private static IEnumerator AddItemGather(GameScript script, int id)
        {
            Item item = new Item(id, 1, 0, 0, 0, new int[3], new int[3]);
            bool flag = false;
            Item[] gatherStorage = Patch_GameScript_AddItemGather.gatherStorage.GetValue(script) as Item[];
            if ((ItemRegistry.GetTypeByID(id) & ItemType.NONSTACKING) == ItemType.STACKING)
            {
                for (int i = 0; i < 9; i++)
                {
                    if (gatherStorage[i].id == id && gatherStorage[i].q < 9999)
                    {
                        gatherStorage[i].q++;
                        if (script.menuGather.activeSelf)
                        {
                            RefreshSlotGather.Invoke(script, new object[] { i });
                        }
                        flag = true;
                        break;
                    }
                }
            }
            if (!flag)
            {
                for (int i = 0; i < 9; i++)
                {
                    if (gatherStorage[i].id == 0)
                    {
                        gatherStorage[i] = item;
                        if (script.menuGather.activeSelf)
                        {
                            RefreshSlotGather.Invoke(script, new object[] { i });
                        }
                        break;
                    }
                }
            }
            if (script.menuGather.activeSelf)
            {
                RefreshGatherStorage.Invoke(script, new object[] { });
            }
            return null;
        }
    }
}