using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Collections;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(Menuu))]
    [HarmonyPatch("WipeShip")]
    static class Patch_Menuu_WipeShip
    {
        [HarmonyPrefix]
        public static bool Prefix(ref IEnumerator __result)
        {
			__result = WipeShip();
            return false;
        }

        private static IEnumerator WipeShip()
        {
			PreviewLabs.PlayerPrefs.SetInt("ship", 0);
			PreviewLabs.PlayerPrefs.SetInt("wall", 0);
			for (int c = 0; c < 6; c++)
			{
				if (PreviewLabs.PlayerPrefs.GetInt(c + "lifetime") > 0)
				{
					for (int i = 0; i < 42; i++)
					{
						int id = PreviewLabs.PlayerPrefs.GetInt(string.Concat(new object[]
						{
						c,
						string.Empty,
						i,
						"id"
						}));
						if (id >= 2000 && id < ItemRegistry.Singleton.GetIDStart())
						{
							PreviewLabs.PlayerPrefs.SetInt(string.Concat(new object[]
							{
							c,
							string.Empty,
							i,
							"id"
							}), 0);
						}
					}
				}
			}
			for (int i = 0; i < 180; i++)
			{
				int id = PreviewLabs.PlayerPrefs.GetInt("storage" + i + "id");
				if (id >= 2000 && id < ItemRegistry.Singleton.GetIDStart())
				{
					PreviewLabs.PlayerPrefs.SetInt("storage" + i + "id", 0);
				}
			}
			yield return null;
			PreviewLabs.PlayerPrefs.Flush();
			Application.Quit();
			yield break;
		}
    }
}