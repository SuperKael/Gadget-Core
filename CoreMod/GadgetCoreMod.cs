using GadgetCore.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UModFramework.API;
using UnityEngine;

namespace GadgetCore.CoreMod
{
    [GadgetMod("Gadget Core", LoadPriority: 100, GadgetVersionSpecificity: VersionSpecificity.BUGFIX, RequiredOnClients: false)]
    internal class GadgetCoreMod : GadgetMod
    {
        public ItemInfo crafterItem;
        public TileInfo crafterTile;

        public override void Initialize()
        {
            GadgetCoreAPI.AddCustomResource("mat/menuCrafter", Resources.Load("mat/menuForge"));
            GadgetCoreAPI.AddCustomResource("mat/barCrafter", Resources.Load("mat/barForge"));

            Texture crafterItemTex = GadgetCoreAPI.GetItemMaterial(2101).mainTexture;
            crafterItem = new ItemInfo(ItemType.GENERIC, "Crafter Block", "Used for crafting\nmodded items.", crafterItemTex).Register("Crafter Block");

            Material crafterTileMat = GadgetCoreAPI.GetTileMaterial(2101);
            GameObject crafterProp = GadgetCoreAPI.GetPropResource(2101);
            crafterTile = new TileInfo(TileType.INTERACTIVE, crafterTileMat, crafterProp, crafterItem).Register("Universal Crafter");
        }

        public override string GetModDescription()
        {
            try
            {
                Version version = new Version(GadgetCoreAPI.VERSION);
                string desc = File.ReadAllText(UMFData.ModInfosPath + "/GadgetCore_v" + (version.Revision != 0 ? version.ToString(4) : version.Build != 0 ? version.ToString(3) : version.ToString(2)) + "_ModInfo.txt");
                desc = desc + Environment.NewLine + Environment.NewLine + "Also, Gadget Core comes with a built-in Gadget mod that adds some miscellaneous features, such as a Universal Crafter. It can be safely disabled here in the config menu, although other mods may require its features.";
                return desc;
            }
            catch (FileNotFoundException)
            {
                return "Gadget Core's Mod Info is locked within the Gadget Core zip file! You should extract the zip file in order to gain access to Gadget Core's full feature-set, including UPnP.";
            }
        }
    }
}
