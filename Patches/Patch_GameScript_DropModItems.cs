using HarmonyLib;
using GadgetCore.API;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("DropModItems")]
    static class Patch_GameScript_DropModItems
	{
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, ref Item[] ___modSlot)
        {
			if (___modSlot[0].id > 0)
			{
				int[] array = GadgetCoreAPI.ConstructIntArrayFromItem(___modSlot[0], true, false);
				___modSlot[0] = __instance.EmptyItem();
				GameObject gameObject = (GameObject)Network.Instantiate(Resources.Load("i"), MenuScript.player.transform.position, Quaternion.identity, 0);
				gameObject.GetComponent<NetworkView>().RPC("Init", RPCMode.AllBuffered, new object[]
				{
					array
				});
				__instance.RefreshSlotMod(0);
			}
			return false;
        }
    }
}