using HarmonyLib;
using GadgetCore.API;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("DropItem")]
    static class Patch_GameScript_DropItem
	{
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, ref Item ___holdingItem, ref bool ___holdingCombatChip, ref bool ___droppingItem)
        {
			Item item = ___holdingItem;
			___holdingItem = __instance.EmptyItem();
			if (___holdingCombatChip)
			{
				Vector3 position = new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y, 0f);
				GameObject gameObject = (GameObject)Network.Instantiate(Resources.Load("i2"), position, Quaternion.identity, 0);
				gameObject.GetComponent<NetworkView>().RPC("Chip", RPCMode.AllBuffered, new object[]
				{
					item.id
				});
				___droppingItem = false;
				___holdingCombatChip = false;
			}
			else
			{
				int[] array = GadgetCoreAPI.ConstructIntArrayFromItem(item, true, false);
				Vector3 position2 = new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y, 0f);
				GameObject gameObject2 = (GameObject)Network.Instantiate(Resources.Load("i2"), position2, Quaternion.identity, 0);
				gameObject2.GetComponent<NetworkView>().RPC("Init", RPCMode.AllBuffered, new object[]
				{
					array
				});
				___droppingItem = false;
			}
			__instance.RefreshHoldingSlot();
			return false;
        }
    }
}