using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using GadgetCore.Util;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("GetSlotCrafted")]
    internal static class Patch_GameScript_GetSlotCrafted
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, ref Item[] ___craft, ref Item __result)
        {
			int num = Random.Range(0, 100);
			int num2 = 0;
			int num3;
			if (num < 5)
			{
				num3 = 0;
			}
			else if (num < 10)
			{
				num3 = 1;
			}
			else if (num < 15)
			{
				num3 = 2;
			}
			else
			{
				num3 = 99;
			}
			int id4 = 0;
			int q = 1;
			if (num3 != 99)
			{
				int id5 = ___craft[num3].id;
				if (GadgetCoreAPI.creationMachineRecipes.TryGetValue(id5, out Tuple<Item, int> recipe))
                {
					__result = GadgetCoreAPI.CopyItem(recipe.Item1);
					__result.q += Random.Range(0, recipe.Item2 + 1);
				}
				else
                {
					switch (id5)
					{
						case 104:
							id4 = 495;
							break;
						case 105:
							id4 = 41;
							break;
						case 106:
							id4 = 599;
							num2 = 3;
							break;
						case 114:
							id4 = 906;
							break;
						case 115:
							id4 = 886;
							break;
						case 116:
							id4 = 647;
							num2 = 3;
							break;
						case 124:
							id4 = 596;
							break;
						case 125:
							id4 = 646;
							break;
						case 126:
							id4 = 549;
							num2 = 3;
							break;
						case 134:
							id4 = 907;
							break;
						case 135:
							id4 = 786;
							break;
						case 136:
							id4 = 499;
							num2 = 3;
							break;
					}
					if (num2 == 0)
					{
						num2 = __instance.InvokeMethod<int>("GetRandomTier");
					}
					__result = new Item(id4, q, 0, num2, 0, new int[3], new int[3]);
				}
			}
			else
			{
				__result = GenerateRandomJunk(__instance);
			}
			return false;
		}

        private static Item GenerateRandomJunk(GameScript instance)
        {
			int id = 0;
			int q = 1;
			int num = Random.Range(0, 65);
			if (num < 25)
			{
				int num5 = Random.Range(0, 4);
				if (num5 == 0)
				{
					id = Random.Range(101, 107);
				}
				else if (num5 == 1)
				{
					id = Random.Range(111, 117);
				}
				else if (num5 == 2)
				{
					id = Random.Range(121, 127);
				}
				else if (num5 == 3)
				{
					id = Random.Range(131, 137);
				}
			}
			else if (num < 40)
			{
				id = 52;
				q = Random.Range(500, 3001);
			}
			else
			{
				int num6 = Random.Range(0, 4);
				if (num6 == 0)
				{
					id = Random.Range(1, 7);
				}
				else if (num6 == 1)
				{
					id = Random.Range(11, 17);
				}
				else if (num6 == 2)
				{
					id = Random.Range(21, 27);
				}
				else if (num6 == 3)
				{
					id = Random.Range(31, 37);
				}
				q = Random.Range(3, 15);
			}
			return new Item(id, q, 0, instance.InvokeMethod<int>("GetRandomTier"), 0, new int[3], new int[3]);
		}
    }
}