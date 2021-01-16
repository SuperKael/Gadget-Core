using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(ObjectScript))]
    [HarmonyPatch("Awake")]
    static class Patch_ObjectScript_Awake
    {
        [HarmonyPostfix]
        public static void Postfix(ObjectScript __instance)
        {
            if (ObjectRegistry.Singleton.TryGetEntry(__instance.id, out ObjectInfo entry))
            {
                switch (entry.Type)
                {
                    case ObjectType.ORE:
                        __instance.gameObject.name = "ore";
                        break;
                    case ObjectType.TREE:
                        __instance.gameObject.name = "tree";
                        break;
                    case ObjectType.PLANT:
                        __instance.gameObject.name = "plant";
                        break;
                    case ObjectType.BUGSPOT:
                        __instance.gameObject.name = "bugspot";
                        break;
                    default:
                        __instance.gameObject.name = "object";
                        break;
                }
			}
        }
    }
}