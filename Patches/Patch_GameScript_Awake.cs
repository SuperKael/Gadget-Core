using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("Awake")]
    internal static class Patch_GameScript_Awake
    {
        [HarmonyPrefix]
        public static void Prefix(GameScript __instance)
        {
            InstanceTracker.GameScript = __instance;
            GadgetCore.GenerateSpriteSheet();
            GadgetUtils.SafeCopyTexture(__instance.TileManager.GetComponent<ChunkWorld>().Texture, 0, 0, 0, 0, 128, 128, GadgetCoreAPI.spriteSheet, 0, 0, 0, 0);
            __instance.TileManager.GetComponent<ChunkWorld>().Texture = GadgetCoreAPI.spriteSheet;
            __instance.WallManager.GetComponent<ChunkWorld>().Texture = GadgetCoreAPI.spriteSheet;
            GameObject gadgetHookScriptHolder = new GameObject("Gadget Hook Script Holder");
            foreach (GadgetInfo mod in Gadgets.ListAllGadgetInfos())
            {
                gadgetHookScriptHolder.AddComponent<GadgetHookScript>().Mod = mod;
            }
        }

        [HarmonyPostfix]
        public static void Postfix(GameScript __instance)
        {
            for (int i = 0; i <= 3; i++)
            {
                GadgetCoreAPI.unlockedVanillaStationRecipes[i] = new HashSet<int>(PreviewLabs.PlayerPrefs.GetString("craftMenu" + i + "unlocks")?.Split(',').Select(x => int.TryParse(x, out int val) ? (int?)val : null).Where(x => x.HasValue).Select(x => x.Value) ?? new int[0]);
            }
            foreach (MenuInfo menu in MenuRegistry.Singleton)
            {
                if (menu is CraftMenuInfo craftMenu)
                {
                    craftMenu.unlockedRecipes = new HashSet<int>(PreviewLabs.PlayerPrefs.GetString("craftMenu" + craftMenu.ID + "unlocks")?.Split(',').Select(x => int.TryParse(x, out int val) ? (int?)val : null).Where(x => x.HasValue).Select(x => x.Value) ?? new int[0]);
                }
            }
            foreach (PlanetInfo planet in PlanetRegistry.Singleton)
            {
                planet.Relics = PreviewLabs.PlayerPrefs.GetInt("planetRelics" + planet.ID);
                if (planet.Relics > 99)
                {
                    planet.Relics = 99;
                    planet.PortalUses = -1;
                }
                else
                {
                    planet.PortalUses = PreviewLabs.PlayerPrefs.GetInt("portalUses" + planet.ID);
                }
            }
        }
    }
}