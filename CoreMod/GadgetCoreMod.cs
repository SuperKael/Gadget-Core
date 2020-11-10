using GadgetCore.API;
using GadgetCore.Util;
using System.IO;
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
            Texture2D crafterItemTex = GadgetCoreAPI.LoadTexture2D("Core Mod/Universal Crafter/item_tex");
            crafterItem = new ItemInfo(ItemType.GENERIC, "Crafter Block", "Used for crafting\nmodded items.", crafterItemTex).Register("Crafter Block");

            Texture2D crafterTileTex = GadgetCoreAPI.LoadTexture2D("Core Mod/Universal Crafter/tile_tex");
            GameObject crafterProp = Object.Instantiate(GadgetCoreAPI.GetPlaceableNPCResource(2101));
            crafterProp.name = "Universal Crafter";
            crafterProp.transform.GetChild(0).GetComponent<MeshRenderer>().material = new Material(Shader.Find("Unlit/Transparent"))
            {
                mainTexture = crafterTileTex
            };

            GameObject animatedObject = crafterProp.transform.GetChild(1).gameObject;
            animatedObject.transform.localPosition = new Vector3(-0.3f, -0.5f, 0.001f);
            AnimationClip clip = new AnimationClip
            {
                name = "itemCrafterTop",
                wrapMode = WrapMode.Loop,
                legacy = true
            };
            clip.SetCurve("", typeof(Transform), "m_LocalPosition.x", new AnimationCurve(new Keyframe(0f, -0.3f, 0f, 0f)));
            clip.SetCurve("", typeof(Transform), "m_LocalPosition.y", new AnimationCurve(new Keyframe(0f, -0.625f, 0f, 0f), new Keyframe(1f, 0.5f, 0f, 0f), new Keyframe(1.25f, -0.625f, 0f, 0f), new Keyframe(1.75f, -0.625f, 0f, 0f)));
            clip.SetCurve("", typeof(Transform), "m_LocalPosition.z", new AnimationCurve(new Keyframe(0f, 0.001f, 0f, 0f)));
            animatedObject.GetComponent<Animation>().RemoveClip("itemForgeTop");
            animatedObject.GetComponent<Animation>().AddClip(clip, "itemCrafterTop");
            animatedObject.GetComponent<Animation>().clip = clip;

            crafterTile = new TileInfo(TileType.INTERACTIVE, crafterTileTex, crafterProp, crafterItem).Register("Crafter Block");

            MenuRegistry.Singleton["Crafter Menu"] = new CraftMenuInfo("Universal Crafter", "Combines required items for custom recipes.",
                GadgetCoreAPI.LoadTexture2D("Core Mod/Universal Crafter/menu_tex"), GadgetCoreAPI.LoadTexture2D("Core Mod/Universal Crafter/bar_tex"),
                GadgetCoreAPI.LoadTexture2D("Core Mod/Universal Crafter/button0_tex"), GadgetCoreAPI.LoadTexture2D("Core Mod/Universal Crafter/button1_tex"), GadgetCoreAPI.LoadTexture2D("Core Mod/Universal Crafter/button2_tex"),
                GadgetCoreAPI.LoadAudioClip("Core Mod/Universal Crafter/craft_au"), null, crafterTile);
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
