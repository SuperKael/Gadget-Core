using GadgetCore.API;
using UnityEngine;

namespace GadgetCore.CoreMod
{
    /// <summary>
    /// This is the internal Gadget used by GadgetCore for adding relevant (optional) features.
    /// </summary>
    [Gadget("Gadget Core", LoadPriority: 1000, GadgetVersionSpecificity: VersionSpecificity.BUGFIX, RequiredOnClients: false)]
    public sealed class GadgetCoreMod : Gadget
    {
        /// <summary>
        /// The item for the Universal Crafter
        /// </summary>
        public ItemInfo crafterItem;
        /// <summary>
        /// The tile for the Universal Crafter
        /// </summary>
        public TileInfo crafterTile;

        /// <summary>
        /// Called during gadget initialization. All data registration should be done from this method.
        /// </summary>
        protected internal override void Initialize()
        {
            GadgetCoreAPI.AddCustomResource("mat/menuCrafter", Resources.Load("mat/menuForge"));
            GadgetCoreAPI.AddCustomResource("mat/barCrafter", Resources.Load("mat/barForge"));

            Texture crafterItemTex = GadgetCoreAPI.GetItemMaterial(2101).mainTexture;
            crafterItem = new ItemInfo(ItemType.GENERIC, "Crafter Block", "Used for crafting\nmodded items.", crafterItemTex).Register("Crafter Block");

            Material crafterTileMat = GadgetCoreAPI.GetTileMaterial(2101);
            GameObject crafterProp = GadgetCoreAPI.GetPropResource(2101);
            crafterTile = new TileInfo(TileType.INTERACTIVE, crafterTileMat, crafterProp, crafterItem).Register("Universal Crafter");
        }

        /// <summary>
        /// Returns this mod's description.
        /// </summary>
        public override string GetModDescription()
        {
            return GetDesc();
        }

        /// <summary>
        /// Returns GadgetCore's description.
        /// </summary>
        public static string GetDesc()
        {
            return "Gadget Core is a powerful library for loading Roguelands mods.\n\n" +
                   "In addition to acting as a mod loader, it also provides innumerable features for mods to use to help in modifying Roguelands with maximum efficiency.";
        }
    }
}
