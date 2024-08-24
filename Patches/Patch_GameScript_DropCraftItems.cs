using HarmonyLib;
using GadgetCore.API;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("DropCraftItems")]
    internal static class Patch_GameScript_DropCraftItems
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, ref Item[] ___craft)
        {
			for (int i = 0; i < 4; i++)
			{
				if (___craft[i].id > 0)
				{
					int[] array = GadgetCoreAPI.ConstructIntArrayFromItem(___craft[i], true, false);
					___craft[i] = __instance.EmptyItem();
					GameObject gameObject = (GameObject)Network.Instantiate(Resources.Load("i"), MenuScript.player.transform.position, Quaternion.identity, 0);
					gameObject.GetComponent<NetworkView>().RPC("Init", RPCMode.AllBuffered, new object[]
					{
						array
					});
					__instance.RefreshSlotCraft(i);
				}
			}
			return false;
        }
    }
}