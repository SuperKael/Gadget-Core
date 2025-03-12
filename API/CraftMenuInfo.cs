using GadgetCore.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace GadgetCore.API
{
    /// <summary>
    /// Represents a craft menu, such as the emblem forge or the alchemy station. Also includes custom craft menus.
    /// </summary>
    public class CraftMenuInfo : MenuInfo
    {
        private static readonly MethodInfo RefreshExpBar = typeof(GameScript).GetMethod("RefreshExpBar", BindingFlags.Public | BindingFlags.Instance);
        private static readonly MethodInfo Crafting = typeof(GameScript).GetMethod("Crafting", BindingFlags.Public | BindingFlags.Instance);
        private static readonly MethodInfo CraftCheck = typeof(GameScript).GetMethod("CraftCheck", BindingFlags.Public | BindingFlags.Instance);

        private static readonly FieldInfo holdingItem = typeof(GameScript).GetField("holdingItem", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo curRecipePage = typeof(GameScript).GetField("curRecipePage", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo craftType = typeof(GameScript).GetField("craftType", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo craftValue = typeof(GameScript).GetField("craftValue", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo crafting = typeof(GameScript).GetField("crafting", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo canCraft = typeof(GameScript).GetField("canCraft", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo interacting = typeof(PlayerScript).GetField("interacting", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo r = typeof(PlayerScript).GetField("r", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// The title to be displayed when this craft menu is open.
        /// </summary>
        public readonly string Title;
        /// <summary>
        /// The description to be displayed when this craft menu is open.
        /// </summary>
        public readonly string Desc;

        /// <summary>
        /// The Texture to be used to display this craft menu.
        /// </summary>
        public virtual Texture MenuTex { get; protected set; }
        /// <summary>
        /// The Texture to be used to display the craft progress bar.
        /// </summary>
        public virtual Texture ProgressBarTex { get; protected set; }
        /// <summary>
        /// The Texture to be used to display the inactive craft button.
        /// </summary>
        public virtual Texture ButtonInactiveTex { get; protected set; }
        /// <summary>
        /// The Texture to be used to display the active craft button.
        /// </summary>
        public virtual Texture ButtonActiveTex { get; protected set; }
        /// <summary>
        /// The Texture to be used to display the selected craft button.
        /// </summary>
        public virtual Texture ButtonSelectTex { get; protected set; }
        /// <summary>
        /// The Material to be used to display this craft menu.
        /// </summary>
        public virtual Material MenuMat { get; protected set; }
        /// <summary>
        /// The Material to be used to display the craft progress bar.
        /// </summary>
        public virtual Material ProgressBarMat { get; protected set; }
        /// <summary>
        /// The Material to be used to display the inactive craft button.
        /// </summary>
        public virtual Material ButtonInactiveMat { get; protected set; }
        /// <summary>
        /// The Material to be used to display the active craft button.
        /// </summary>
        public virtual Material ButtonActiveMat { get; protected set; }
        /// <summary>
        /// The Material to be used to display the selected craft button.
        /// </summary>
        public virtual Material ButtonSelectMat { get; protected set; }
        /// <summary>
        /// The sound effect that plays every time the craft button is pressed.
        /// </summary>
        public virtual AudioClip CraftSound { get; protected set; }
        /// <summary>
        /// The list of validators for determining if an item fits a slot. If any of the validators return true for a slot, then it should be considered valid.
        /// </summary>
        protected internal readonly List<SlotValidator> SlotValidators;
        /// <summary>
        /// The list of validators for determining if the current items in the craft menu form a valid recipe. The validators should be checked in reverse order, and then if one of the validators returns true, then the <see cref="CraftPerformer"/> in the <see cref="CraftPerformers"/> list at the same index as the succesful validator should be used.
        /// </summary>
        protected internal readonly List<CraftValidator> CraftValidators;
        /// <summary>
        /// The list of performers used for performing a crafting recipe, only after the matching validator passes.
        /// </summary>
        protected internal readonly List<CraftPerformer> CraftPerformers;
        /// <summary>
        /// The list of finalizers used when completing a crafting recipe, after the matching validator executes.
        /// </summary>
        protected internal readonly List<CraftFinalizer> CraftFinalizers;
        /// <summary>
        /// The list of Mod IDs that match the registered craft performers. These values will change when ReplaceCraftPerformer is called. May be the same as the entry in <see cref="CraftPerformerInitModIDs"/>
        /// </summary>
        protected internal readonly List<int> CraftPerformerModIDs;
        /// <summary>
        /// The list of Mod IDs that match the registered craft performers when they were first registered. May be the same as the entry in <see cref="CraftPerformerModIDs"/>
        /// </summary>
        protected internal readonly List<int> CraftPerformerInitModIDs;

        /// <summary>
        /// Stores what the state of the recipe will be after it is complete.
        /// </summary>
        protected internal Item[] craftResult;

        /// <summary>
        /// List of IDs representing the outputs of recipes that have been crafted before.
        /// </summary>
        protected internal HashSet<int> unlockedRecipes;

        /// <summary>
        /// The performer index for the recipe currently being crafted.
        /// </summary>
        protected internal int activePerformer = -1;

        /// <summary>
        /// Use to register a custom crafting menu. Using RegisterMenu is unnecessary for crafting menus. Note that all crafting menus must have, at most, 3 ingredient slots. Use OpenCraftMenu to open the menu.
        /// </summary>
        /// <param name="Title">The text to display as the title of the crafting window.</param>
        /// <param name="Desc">The text to display as the description of the crafting window.</param>
        /// <param name="MenuTex">The Texture to use for the crafting window.</param>
        /// <param name="ProgressBarTex">The Texture to use for the crafting progress bar.</param>
        /// <param name="ButtonInactiveTex">The Texture to be used to display the inactive craft button.</param>
        /// <param name="ButtonActiveTex">The Texture to be used to display the active craft button.</param>
        /// <param name="ButtonSelectTex">The Texture to be used to display the selected craft button.</param>
        /// <param name="CraftSound">The sound effect that plays every time the craft button is pressed.</param>
        /// <param name="SlotValidator">Used to check if an item is valid for a slot.</param>
        /// <param name="CraftValidator">Used to check if the items currently in the slots are valid for a recipe. The Item array is of length 4, where the first three Items are the inputs, and the fourth Item is the output.</param>
        /// <param name="CraftPerformer">Used to perform a crafting operation by modifying the contents of the Item array. The Item array is of length 4, where the first three Items are the inputs, and the fourth Item is the output. Should decrement the inputs, and set the output, possibly respecting whatever is already in the output.</param>
        /// <param name="CraftFinalizer">Optionally used to perform any final behavior after the craft is complete. The Item array is of length 4, where the first three Items are the inputs, and the fourth Item is the output.</param>
        /// <param name="Tile">An optional parameter that specifies what Interactive tile should open the menu when interacted with.</param>
        public CraftMenuInfo(string Title, string Desc, Texture MenuTex, Texture ProgressBarTex, Texture ButtonInactiveTex, Texture ButtonActiveTex, Texture ButtonSelectTex, AudioClip CraftSound, SlotValidator SlotValidator, CraftValidator CraftValidator, CraftPerformer CraftPerformer, CraftFinalizer CraftFinalizer = null, TileInfo Tile = null) : base(MenuType.CRAFTING, null, Tile)
        {
            this.Title = Title;
            this.Desc = Desc;
            this.MenuTex = MenuTex;
            this.ProgressBarTex = ProgressBarTex;
            this.ButtonInactiveTex = ButtonInactiveTex;
            this.ButtonActiveTex = ButtonActiveTex;
            this.ButtonSelectTex = ButtonSelectTex;
            this.CraftSound = CraftSound;
            if (SlotValidator != null && CraftValidator != null && CraftPerformer != null)
            {
                SlotValidators = new List<SlotValidator>(1);
                CraftValidators = new List<CraftValidator>(1);
                CraftPerformers = new List<CraftPerformer>(1);
                CraftFinalizers = new List<CraftFinalizer>(1);
                CraftPerformerModIDs = new List<int>(1);
                CraftPerformerInitModIDs = new List<int>(1);
                AddCraftPerformer(SlotValidator, CraftValidator, CraftPerformer, CraftFinalizer);
            }
            else if (SlotValidator != null || CraftValidator != null || CraftPerformer != null)
            {
                throw new InvalidOperationException("If SlotValidator, CraftValidator, or CraftPerformer are null, they must all be null!");
            }
            else
            {
                SlotValidators = new List<SlotValidator>(0);
                CraftValidators = new List<CraftValidator>(0);
                CraftPerformers = new List<CraftPerformer>(0);
                CraftFinalizers = new List<CraftFinalizer>(0);
                CraftPerformerModIDs = new List<int>(0);
                CraftPerformerInitModIDs = new List<int>(0);
            }
        }

        /// <summary>
        /// Use to register a custom crafting menu. Using RegisterMenu is unnecessary for crafting menus. Note that all crafting menus must have, at most, 3 ingredient slots. Use OpenCraftMenu to open the menu.
        /// </summary>
        /// <param name="Title">The text to display as the title of the crafting window.</param>
        /// <param name="Desc">The text to display as the description of the crafting window.</param>
        /// <param name="MenuTex">The Texture to use for the crafting window.</param>
        /// <param name="ProgressBarTex">The Texture to use for the crafting progress bar.</param>
        /// <param name="ButtonInactiveTex">The Texture to be used to display the inactive craft button.</param>
        /// <param name="ButtonActiveTex">The Texture to be used to display the active craft button.</param>
        /// <param name="ButtonSelectTex">The Texture to be used to display the selected craft button.</param>
        /// <param name="CraftSound">The sound effect that plays every time the craft button is pressed.</param>
        /// <param name="PerformerTuple">A <see cref="Tuple{SlotValidator, CraftValidator, CraftPerformer}"/> containing the SlotValidator, CraftValidator, and CraftPerformer.</param>
        /// <param name="CraftFinalizer">Optionally used to perform any final behavior after the craft is complete. The Item array is of length 4, where the first three Items are the inputs, and the fourth Item is the output.</param>
        /// <param name="Tile">An optional parameter that specifies what Interactive tile should open the menu when interacted with.</param>
        public CraftMenuInfo(string Title, string Desc, Texture MenuTex, Texture ProgressBarTex, Texture ButtonInactiveTex, Texture ButtonActiveTex, Texture ButtonSelectTex, AudioClip CraftSound, Tuple<SlotValidator, CraftValidator, CraftPerformer> PerformerTuple, CraftFinalizer CraftFinalizer = null, TileInfo Tile = null) : base(MenuType.CRAFTING, null, Tile)
        {
            this.Title = Title;
            this.Desc = Desc;
            this.MenuTex = MenuTex;
            this.ProgressBarTex = ProgressBarTex;
            this.ButtonInactiveTex = ButtonInactiveTex;
            this.ButtonActiveTex = ButtonActiveTex;
            this.ButtonSelectTex = ButtonSelectTex;
            this.CraftSound = CraftSound;
            if (PerformerTuple != null)
            {
                SlotValidators = new List<SlotValidator>(1);
                CraftValidators = new List<CraftValidator>(1);
                CraftPerformers = new List<CraftPerformer>(1);
                CraftFinalizers = new List<CraftFinalizer>(1);
                CraftPerformerModIDs = new List<int>(1);
                CraftPerformerInitModIDs = new List<int>(1);
                AddCraftPerformer(PerformerTuple.Item1, PerformerTuple.Item2, PerformerTuple.Item3, CraftFinalizer);
            }
            else
            {
                SlotValidators = new List<SlotValidator>(0);
                CraftValidators = new List<CraftValidator>(0);
                CraftPerformers = new List<CraftPerformer>(0);
                CraftFinalizers = new List<CraftFinalizer>(0);
                CraftPerformerModIDs = new List<int>(0);
                CraftPerformerInitModIDs = new List<int>(0);
            }
        }

        /// <summary>
        /// Use to register a custom crafting menu. Using RegisterMenu is unnecessary for crafting menus. Note that all crafting menus must have, at most, 3 ingredient slots. Use OpenCraftMenu to open the menu.
        /// </summary>
        /// <param name="Title">The text to display as the title of the crafting window.</param>
        /// <param name="Desc">The text to display as the description of the crafting window.</param>
        /// <param name="MenuTex">The Texture to use for the crafting window.</param>
        /// <param name="ProgressBarTex">The Texture to use for the crafting progress bar.</param>
        /// <param name="ButtonInactiveTex">The Texture to be used to display the inactive craft button.</param>
        /// <param name="ButtonActiveTex">The Texture to be used to display the active craft button.</param>
        /// <param name="ButtonSelectTex">The Texture to be used to display the selected craft button.</param>
        /// <param name="CraftSound">The sound effect that plays every time the craft button is pressed.</param>
        /// <param name="PerformerTuple">A <see cref="Tuple{SlotValidator, CraftValidator, CraftPerformer, CraftFinalizer}"/> containing the SlotValidator, CraftValidator, CraftPerformer, and CraftFinalizer.</param>
        /// <param name="Tile">An optional parameter that specifies what Interactive tile should open the menu when interacted with.</param>
        public CraftMenuInfo(string Title, string Desc, Texture MenuTex, Texture ProgressBarTex, Texture ButtonInactiveTex, Texture ButtonActiveTex, Texture ButtonSelectTex, AudioClip CraftSound, Tuple<SlotValidator, CraftValidator, CraftPerformer, CraftFinalizer> PerformerTuple, TileInfo Tile = null) : base(MenuType.CRAFTING, null, Tile)
        {
            this.Title = Title;
            this.Desc = Desc;
            this.MenuTex = MenuTex;
            this.ProgressBarTex = ProgressBarTex;
            this.ButtonInactiveTex = ButtonInactiveTex;
            this.ButtonActiveTex = ButtonActiveTex;
            this.ButtonSelectTex = ButtonSelectTex;
            this.CraftSound = CraftSound;
            if (PerformerTuple != null)
            {
                SlotValidators = new List<SlotValidator>(1);
                CraftValidators = new List<CraftValidator>(1);
                CraftPerformers = new List<CraftPerformer>(1);
                CraftFinalizers = new List<CraftFinalizer>(1);
                CraftPerformerModIDs = new List<int>(1);
                CraftPerformerInitModIDs = new List<int>(1);
                AddCraftPerformer(PerformerTuple.Item1, PerformerTuple.Item2, PerformerTuple.Item3, PerformerTuple.Item4);
            }
            else
            {
                SlotValidators = new List<SlotValidator>(0);
                CraftValidators = new List<CraftValidator>(0);
                CraftPerformers = new List<CraftPerformer>(0);
                CraftFinalizers = new List<CraftFinalizer>(0);
                CraftPerformerModIDs = new List<int>(0);
                CraftPerformerInitModIDs = new List<int>(0);
            }
        }

        /// <summary>
        /// Use to register a custom crafting menu. Using RegisterMenu is unnecessary for crafting menus. Note that all crafting menus must have, at most, 3 ingredient slots. Use OpenCraftMenu to open the menu.
        /// </summary>
        /// <param name="Title">The text to display as the title of the crafting window.</param>
        /// <param name="Desc">The text to display as the description of the crafting window.</param>
        /// <param name="MenuMat">The Material to use for the crafting window.</param>
        /// <param name="ProgressBarMat">The Material to use for the crafting progress bar.</param>
        /// <param name="ButtonInactiveMat">The Material to be used to display the inactive craft button.</param>
        /// <param name="ButtonActiveMat">The Material to be used to display the active craft button.</param>
        /// <param name="ButtonSelectMat">The Material to be used to display the selected craft button.</param>
        /// <param name="CraftSound">The sound effect that plays every time the craft button is pressed.</param>
        /// <param name="SlotValidator">Used to check if an item is valid for a slot.</param>
        /// <param name="CraftValidator">Used to check if the items currently in the slots are valid for a recipe. The Item array is of length 4, where the first three Items are the inputs, and the fourth Item is the output.</param>
        /// <param name="CraftPerformer">Used to perform a crafting operation by modifying the contents of the Item array. The Item array is of length 4, where the first three Items are the inputs, and the fourth Item is the output. Should decrement the inputs, and set the output, possibly respecting whatever is already in the output.</param>
        /// <param name="CraftFinalizer">Optionally used to perform any final behavior after the craft is complete. The Item array is of length 4, where the first three Items are the inputs, and the fourth Item is the output.</param>
        /// <param name="Tile">An optional parameter that specifies what Interactive tile should open the menu when interacted with.</param>
        public CraftMenuInfo(string Title, string Desc, Material MenuMat, Material ProgressBarMat, Material ButtonInactiveMat, Material ButtonActiveMat, Material ButtonSelectMat, AudioClip CraftSound, SlotValidator SlotValidator, CraftValidator CraftValidator, CraftPerformer CraftPerformer, CraftFinalizer CraftFinalizer = null, TileInfo Tile = null) : base(MenuType.CRAFTING, null, Tile)
        {
            this.Title = Title;
            this.Desc = Desc;
            this.MenuMat = MenuMat;
            this.ProgressBarMat = ProgressBarMat;
            this.ButtonInactiveMat = ButtonInactiveMat;
            this.ButtonActiveMat = ButtonActiveMat;
            this.ButtonSelectMat = ButtonSelectMat;
            this.CraftSound = CraftSound;
            if (SlotValidator != null && CraftValidator != null && CraftPerformer != null)
            {
                SlotValidators = new List<SlotValidator>(1);
                CraftValidators = new List<CraftValidator>(1);
                CraftPerformers = new List<CraftPerformer>(1);
                CraftFinalizers = new List<CraftFinalizer>(1);
                CraftPerformerModIDs = new List<int>(1);
                CraftPerformerInitModIDs = new List<int>(1);
                AddCraftPerformer(SlotValidator, CraftValidator, CraftPerformer, CraftFinalizer);
            }
            else if (SlotValidator != null || CraftValidator != null || CraftPerformer != null)
            {
                throw new InvalidOperationException("If SlotValidator, CraftValidator, or CraftPerformer are null, they must all be null!");
            }
            else
            {
                SlotValidators = new List<SlotValidator>(0);
                CraftValidators = new List<CraftValidator>(0);
                CraftPerformers = new List<CraftPerformer>(0);
                CraftFinalizers = new List<CraftFinalizer>(0);
                CraftPerformerModIDs = new List<int>(0);
                CraftPerformerInitModIDs = new List<int>(0);
            }
        }

        /// <summary>
        /// Use to register a custom crafting menu. Using RegisterMenu is unnecessary for crafting menus. Note that all crafting menus must have, at most, 3 ingredient slots. Use OpenCraftMenu to open the menu.
        /// </summary>
        /// <param name="Title">The text to display as the title of the crafting window.</param>
        /// <param name="Desc">The text to display as the description of the crafting window.</param>
        /// <param name="MenuMat">The Material to use for the crafting window.</param>
        /// <param name="ProgressBarMat">The Material to use for the crafting progress bar.</param>
        /// <param name="ButtonInactiveMat">The Material to be used to display the inactive craft button.</param>
        /// <param name="ButtonActiveMat">The Material to be used to display the active craft button.</param>
        /// <param name="ButtonSelectMat">The Material to be used to display the selected craft button.</param>
        /// <param name="CraftSound">The sound effect that plays every time the craft button is pressed.</param>
        /// <param name="PerformerTuple">A <see cref="Tuple{SlotValidator, CraftValidator, CraftPerformer}"/> containing the SlotValidator, CraftValidator, and CraftPerformer.</param>
        /// <param name="CraftFinalizer">Optionally used to perform any final behavior after the craft is complete. The Item array is of length 4, where the first three Items are the inputs, and the fourth Item is the output.</param>
        /// <param name="Tile">An optional parameter that specifies what Interactive tile should open the menu when interacted with.</param>
        public CraftMenuInfo(string Title, string Desc, Material MenuMat, Material ProgressBarMat, Material ButtonInactiveMat, Material ButtonActiveMat, Material ButtonSelectMat, AudioClip CraftSound, Tuple<SlotValidator, CraftValidator, CraftPerformer> PerformerTuple, CraftFinalizer CraftFinalizer = null, TileInfo Tile = null) : base(MenuType.CRAFTING, null, Tile)
        {
            this.Title = Title;
            this.Desc = Desc;
            this.MenuMat = MenuMat;
            this.ProgressBarMat = ProgressBarMat;
            this.ButtonInactiveMat = ButtonInactiveMat;
            this.ButtonActiveMat = ButtonActiveMat;
            this.ButtonSelectMat = ButtonSelectMat;
            this.CraftSound = CraftSound;
            if (PerformerTuple != null)
            {
                SlotValidators = new List<SlotValidator>(1);
                CraftValidators = new List<CraftValidator>(1);
                CraftPerformers = new List<CraftPerformer>(1);
                CraftFinalizers = new List<CraftFinalizer>(1);
                CraftPerformerModIDs = new List<int>(1);
                CraftPerformerInitModIDs = new List<int>(1);
                AddCraftPerformer(PerformerTuple.Item1, PerformerTuple.Item2, PerformerTuple.Item3, CraftFinalizer);
            }
            else
            {
                SlotValidators = new List<SlotValidator>(0);
                CraftValidators = new List<CraftValidator>(0);
                CraftPerformers = new List<CraftPerformer>(0);
                CraftFinalizers = new List<CraftFinalizer>(0);
                CraftPerformerModIDs = new List<int>(0);
                CraftPerformerInitModIDs = new List<int>(0);
            }
        }

        /// <summary>
        /// Use to register a custom crafting menu. Using RegisterMenu is unnecessary for crafting menus. Note that all crafting menus must have, at most, 3 ingredient slots. Use OpenCraftMenu to open the menu.
        /// </summary>
        /// <param name="Title">The text to display as the title of the crafting window.</param>
        /// <param name="Desc">The text to display as the description of the crafting window.</param>
        /// <param name="MenuMat">The Material to use for the crafting window.</param>
        /// <param name="ProgressBarMat">The Material to use for the crafting progress bar.</param>
        /// <param name="ButtonInactiveMat">The Material to be used to display the inactive craft button.</param>
        /// <param name="ButtonActiveMat">The Material to be used to display the active craft button.</param>
        /// <param name="ButtonSelectMat">The Material to be used to display the selected craft button.</param>
        /// <param name="CraftSound">The sound effect that plays every time the craft button is pressed.</param>
        /// <param name="PerformerTuple">A <see cref="Tuple{SlotValidator, CraftValidator, CraftPerformer, CraftFinalizer}"/> containing the SlotValidator, CraftValidator, CraftPerformer, and CraftFinalizer.</param>
        /// <param name="Tile">An optional parameter that specifies what Interactive tile should open the menu when interacted with.</param>
        public CraftMenuInfo(string Title, string Desc, Material MenuMat, Material ProgressBarMat, Material ButtonInactiveMat, Material ButtonActiveMat, Material ButtonSelectMat, AudioClip CraftSound, Tuple<SlotValidator, CraftValidator, CraftPerformer, CraftFinalizer> PerformerTuple, TileInfo Tile = null) : base(MenuType.CRAFTING, null, Tile)
        {
            this.Title = Title;
            this.Desc = Desc;
            this.MenuMat = MenuMat;
            this.ProgressBarMat = ProgressBarMat;
            this.ButtonInactiveMat = ButtonInactiveMat;
            this.ButtonActiveMat = ButtonActiveMat;
            this.ButtonSelectMat = ButtonSelectMat;
            this.CraftSound = CraftSound;
            if (PerformerTuple != null)
            {
                SlotValidators = new List<SlotValidator>(1);
                CraftValidators = new List<CraftValidator>(1);
                CraftPerformers = new List<CraftPerformer>(1);
                CraftFinalizers = new List<CraftFinalizer>(1);
                CraftPerformerModIDs = new List<int>(1);
                CraftPerformerInitModIDs = new List<int>(1);
                AddCraftPerformer(PerformerTuple.Item1, PerformerTuple.Item2, PerformerTuple.Item3, PerformerTuple.Item4);
            }
            else
            {
                SlotValidators = new List<SlotValidator>(0);
                CraftValidators = new List<CraftValidator>(0);
                CraftPerformers = new List<CraftPerformer>(0);
                CraftFinalizers = new List<CraftFinalizer>(0);
                CraftPerformerModIDs = new List<int>(0);
                CraftPerformerInitModIDs = new List<int>(0);
            }
        }

        /// <summary>
        /// Use to register a custom crafting menu. Using RegisterMenu is unnecessary for crafting menus. Note that all crafting menus must have, at most, 3 ingredient slots. Use OpenCraftMenu to open the menu.
        /// </summary>
        /// <param name="Title">The text to display as the title of the crafting window.</param>
        /// <param name="Desc">The text to display as the description of the crafting window.</param>
        /// <param name="MenuTex">The Texture to use for the crafting window.</param>
        /// <param name="ProgressBarTex">The Texture to use for the crafting progress bar.</param>
        /// <param name="ButtonInactiveTex">The Texture to be used to display the inactive craft button.</param>
        /// <param name="ButtonActiveTex">The Texture to be used to display the active craft button.</param>
        /// <param name="ButtonSelectTex">The Texture to be used to display the selected craft button.</param>
        /// <param name="CraftSound">The sound effect that plays every time the craft button is pressed.</param>
        /// <param name="SlotValidator">Used to check if an item is valid for a slot.</param>
        /// <param name="CraftValidator">Used to check if the items currently in the slots are valid for a recipe. The Item array is of length 4, where the first three Items are the inputs, and the fourth Item is the output.</param>
        /// <param name="CraftPerformer">Used to perform a crafting operation by modifying the contents of the Item array. The Item array is of length 4, where the first three Items are the inputs, and the fourth Item is the output. Should decrement the inputs, and set the output, possibly respecting whatever is already in the output.</param>
        /// <param name="CraftFinalizer">Optionally used to perform any final behavior after the craft is complete. The Item array is of length 4, where the first three Items are the inputs, and the fourth Item is the output.</param>
        /// <param name="Tile">An optional parameter that specifies what Interactive tile should open the menu when interacted with.</param>
        public CraftMenuInfo(string Title, string Desc, Texture MenuTex, Texture ProgressBarTex, Texture ButtonInactiveTex, Texture ButtonActiveTex, Texture ButtonSelectTex, Task<AudioClip> CraftSound, SlotValidator SlotValidator, CraftValidator CraftValidator, CraftPerformer CraftPerformer, CraftFinalizer CraftFinalizer = null, TileInfo Tile = null) : base(MenuType.CRAFTING, null, Tile)
        {
            this.Title = Title;
            this.Desc = Desc;
            this.MenuTex = MenuTex;
            this.ProgressBarTex = ProgressBarTex;
            this.ButtonInactiveTex = ButtonInactiveTex;
            this.ButtonActiveTex = ButtonActiveTex;
            this.ButtonSelectTex = ButtonSelectTex;

            if (CraftSound != null)
            {
                CraftSound.ContinueWith(task =>
                {
                    this.CraftSound = task.Result;
                });
            }

            if (SlotValidator != null && CraftValidator != null && CraftPerformer != null)
            {
                SlotValidators = new List<SlotValidator>(1);
                CraftValidators = new List<CraftValidator>(1);
                CraftPerformers = new List<CraftPerformer>(1);
                CraftFinalizers = new List<CraftFinalizer>(1);
                CraftPerformerModIDs = new List<int>(1);
                CraftPerformerInitModIDs = new List<int>(1);
                AddCraftPerformer(SlotValidator, CraftValidator, CraftPerformer, CraftFinalizer);
            }
            else if (SlotValidator != null || CraftValidator != null || CraftPerformer != null)
            {
                throw new InvalidOperationException("If SlotValidator, CraftValidator, or CraftPerformer are null, they must all be null!");
            }
            else
            {
                SlotValidators = new List<SlotValidator>(0);
                CraftValidators = new List<CraftValidator>(0);
                CraftPerformers = new List<CraftPerformer>(0);
                CraftFinalizers = new List<CraftFinalizer>(0);
                CraftPerformerModIDs = new List<int>(0);
                CraftPerformerInitModIDs = new List<int>(0);
            }
        }

        /// <summary>
        /// Use to register a custom crafting menu. Using RegisterMenu is unnecessary for crafting menus. Note that all crafting menus must have, at most, 3 ingredient slots. Use OpenCraftMenu to open the menu.
        /// </summary>
        /// <param name="Title">The text to display as the title of the crafting window.</param>
        /// <param name="Desc">The text to display as the description of the crafting window.</param>
        /// <param name="MenuTex">The Texture to use for the crafting window.</param>
        /// <param name="ProgressBarTex">The Texture to use for the crafting progress bar.</param>
        /// <param name="ButtonInactiveTex">The Texture to be used to display the inactive craft button.</param>
        /// <param name="ButtonActiveTex">The Texture to be used to display the active craft button.</param>
        /// <param name="ButtonSelectTex">The Texture to be used to display the selected craft button.</param>
        /// <param name="CraftSound">The sound effect that plays every time the craft button is pressed.</param>
        /// <param name="PerformerTuple">A <see cref="Tuple{SlotValidator, CraftValidator, CraftPerformer}"/> containing the SlotValidator, CraftValidator, and CraftPerformer.</param>
        /// <param name="CraftFinalizer">Optionally used to perform any final behavior after the craft is complete. The Item array is of length 4, where the first three Items are the inputs, and the fourth Item is the output.</param>
        /// <param name="Tile">An optional parameter that specifies what Interactive tile should open the menu when interacted with.</param>
        public CraftMenuInfo(string Title, string Desc, Texture MenuTex, Texture ProgressBarTex, Texture ButtonInactiveTex, Texture ButtonActiveTex, Texture ButtonSelectTex, Task<AudioClip> CraftSound, Tuple<SlotValidator, CraftValidator, CraftPerformer> PerformerTuple, CraftFinalizer CraftFinalizer = null, TileInfo Tile = null) : base(MenuType.CRAFTING, null, Tile)
        {
            this.Title = Title;
            this.Desc = Desc;
            this.MenuTex = MenuTex;
            this.ProgressBarTex = ProgressBarTex;
            this.ButtonInactiveTex = ButtonInactiveTex;
            this.ButtonActiveTex = ButtonActiveTex;
            this.ButtonSelectTex = ButtonSelectTex;

            if (CraftSound != null)
            {
                CraftSound.ContinueWith(task =>
                {
                    this.CraftSound = task.Result;
                });
            }

            if (PerformerTuple != null)
            {
                SlotValidators = new List<SlotValidator>(1);
                CraftValidators = new List<CraftValidator>(1);
                CraftPerformers = new List<CraftPerformer>(1);
                CraftFinalizers = new List<CraftFinalizer>(1);
                CraftPerformerModIDs = new List<int>(1);
                CraftPerformerInitModIDs = new List<int>(1);
                AddCraftPerformer(PerformerTuple.Item1, PerformerTuple.Item2, PerformerTuple.Item3, CraftFinalizer);
            }
            else
            {
                SlotValidators = new List<SlotValidator>(0);
                CraftValidators = new List<CraftValidator>(0);
                CraftPerformers = new List<CraftPerformer>(0);
                CraftFinalizers = new List<CraftFinalizer>(0);
                CraftPerformerModIDs = new List<int>(0);
                CraftPerformerInitModIDs = new List<int>(0);
            }
        }

        /// <summary>
        /// Use to register a custom crafting menu. Using RegisterMenu is unnecessary for crafting menus. Note that all crafting menus must have, at most, 3 ingredient slots. Use OpenCraftMenu to open the menu.
        /// </summary>
        /// <param name="Title">The text to display as the title of the crafting window.</param>
        /// <param name="Desc">The text to display as the description of the crafting window.</param>
        /// <param name="MenuTex">The Texture to use for the crafting window.</param>
        /// <param name="ProgressBarTex">The Texture to use for the crafting progress bar.</param>
        /// <param name="ButtonInactiveTex">The Texture to be used to display the inactive craft button.</param>
        /// <param name="ButtonActiveTex">The Texture to be used to display the active craft button.</param>
        /// <param name="ButtonSelectTex">The Texture to be used to display the selected craft button.</param>
        /// <param name="CraftSound">The sound effect that plays every time the craft button is pressed.</param>
        /// <param name="PerformerTuple">A <see cref="Tuple{SlotValidator, CraftValidator, CraftPerformer, CraftFinalizer}"/> containing the SlotValidator, CraftValidator, CraftPerformer, and CraftFinalizer.</param>
        /// <param name="Tile">An optional parameter that specifies what Interactive tile should open the menu when interacted with.</param>
        public CraftMenuInfo(string Title, string Desc, Texture MenuTex, Texture ProgressBarTex, Texture ButtonInactiveTex, Texture ButtonActiveTex, Texture ButtonSelectTex, Task<AudioClip> CraftSound, Tuple<SlotValidator, CraftValidator, CraftPerformer, CraftFinalizer> PerformerTuple, TileInfo Tile = null) : base(MenuType.CRAFTING, null, Tile)
        {
            this.Title = Title;
            this.Desc = Desc;
            this.MenuTex = MenuTex;
            this.ProgressBarTex = ProgressBarTex;
            this.ButtonInactiveTex = ButtonInactiveTex;
            this.ButtonActiveTex = ButtonActiveTex;
            this.ButtonSelectTex = ButtonSelectTex;

            if (CraftSound != null)
            {
                CraftSound.ContinueWith(task =>
                {
                    this.CraftSound = task.Result;
                });
            }

            if (PerformerTuple != null)
            {
                SlotValidators = new List<SlotValidator>(1);
                CraftValidators = new List<CraftValidator>(1);
                CraftPerformers = new List<CraftPerformer>(1);
                CraftFinalizers = new List<CraftFinalizer>(1);
                CraftPerformerModIDs = new List<int>(1);
                CraftPerformerInitModIDs = new List<int>(1);
                AddCraftPerformer(PerformerTuple.Item1, PerformerTuple.Item2, PerformerTuple.Item3, PerformerTuple.Item4);
            }
            else
            {
                SlotValidators = new List<SlotValidator>(0);
                CraftValidators = new List<CraftValidator>(0);
                CraftPerformers = new List<CraftPerformer>(0);
                CraftFinalizers = new List<CraftFinalizer>(0);
                CraftPerformerModIDs = new List<int>(0);
                CraftPerformerInitModIDs = new List<int>(0);
            }
        }

        /// <summary>
        /// Use to register a custom crafting menu. Using RegisterMenu is unnecessary for crafting menus. Note that all crafting menus must have, at most, 3 ingredient slots. Use OpenCraftMenu to open the menu.
        /// </summary>
        /// <param name="Title">The text to display as the title of the crafting window.</param>
        /// <param name="Desc">The text to display as the description of the crafting window.</param>
        /// <param name="MenuMat">The Material to use for the crafting window.</param>
        /// <param name="ProgressBarMat">The Material to use for the crafting progress bar.</param>
        /// <param name="ButtonInactiveMat">The Material to be used to display the inactive craft button.</param>
        /// <param name="ButtonActiveMat">The Material to be used to display the active craft button.</param>
        /// <param name="ButtonSelectMat">The Material to be used to display the selected craft button.</param>
        /// <param name="CraftSound">The sound effect that plays every time the craft button is pressed.</param>
        /// <param name="SlotValidator">Used to check if an item is valid for a slot.</param>
        /// <param name="CraftValidator">Used to check if the items currently in the slots are valid for a recipe. The Item array is of length 4, where the first three Items are the inputs, and the fourth Item is the output.</param>
        /// <param name="CraftPerformer">Used to perform a crafting operation by modifying the contents of the Item array. The Item array is of length 4, where the first three Items are the inputs, and the fourth Item is the output. Should decrement the inputs, and set the output, possibly respecting whatever is already in the output.</param>
        /// <param name="CraftFinalizer">Optionally used to perform any final behavior after the craft is complete. The Item array is of length 4, where the first three Items are the inputs, and the fourth Item is the output.</param>
        /// <param name="Tile">An optional parameter that specifies what Interactive tile should open the menu when interacted with.</param>
        public CraftMenuInfo(string Title, string Desc, Material MenuMat, Material ProgressBarMat, Material ButtonInactiveMat, Material ButtonActiveMat, Material ButtonSelectMat, Task<AudioClip> CraftSound, SlotValidator SlotValidator, CraftValidator CraftValidator, CraftPerformer CraftPerformer, CraftFinalizer CraftFinalizer = null, TileInfo Tile = null) : base(MenuType.CRAFTING, null, Tile)
        {
            this.Title = Title;
            this.Desc = Desc;
            this.MenuMat = MenuMat;
            this.ProgressBarMat = ProgressBarMat;
            this.ButtonInactiveMat = ButtonInactiveMat;
            this.ButtonActiveMat = ButtonActiveMat;
            this.ButtonSelectMat = ButtonSelectMat;

            if (CraftSound != null)
            {
                CraftSound.ContinueWith(task =>
                {
                    this.CraftSound = task.Result;
                });
            }

            if (SlotValidator != null && CraftValidator != null && CraftPerformer != null)
            {
                SlotValidators = new List<SlotValidator>(1);
                CraftValidators = new List<CraftValidator>(1);
                CraftPerformers = new List<CraftPerformer>(1);
                CraftFinalizers = new List<CraftFinalizer>(1);
                CraftPerformerModIDs = new List<int>(1);
                CraftPerformerInitModIDs = new List<int>(1);
                AddCraftPerformer(SlotValidator, CraftValidator, CraftPerformer, CraftFinalizer);
            }
            else if (SlotValidator != null || CraftValidator != null || CraftPerformer != null)
            {
                throw new InvalidOperationException("If SlotValidator, CraftValidator, or CraftPerformer are null, they must all be null!");
            }
            else
            {
                SlotValidators = new List<SlotValidator>(0);
                CraftValidators = new List<CraftValidator>(0);
                CraftPerformers = new List<CraftPerformer>(0);
                CraftFinalizers = new List<CraftFinalizer>(0);
                CraftPerformerModIDs = new List<int>(0);
                CraftPerformerInitModIDs = new List<int>(0);
            }
        }

        /// <summary>
        /// Use to register a custom crafting menu. Using RegisterMenu is unnecessary for crafting menus. Note that all crafting menus must have, at most, 3 ingredient slots. Use OpenCraftMenu to open the menu.
        /// </summary>
        /// <param name="Title">The text to display as the title of the crafting window.</param>
        /// <param name="Desc">The text to display as the description of the crafting window.</param>
        /// <param name="MenuMat">The Material to use for the crafting window.</param>
        /// <param name="ProgressBarMat">The Material to use for the crafting progress bar.</param>
        /// <param name="ButtonInactiveMat">The Material to be used to display the inactive craft button.</param>
        /// <param name="ButtonActiveMat">The Material to be used to display the active craft button.</param>
        /// <param name="ButtonSelectMat">The Material to be used to display the selected craft button.</param>
        /// <param name="CraftSound">The sound effect that plays every time the craft button is pressed.</param>
        /// <param name="PerformerTuple">A <see cref="Tuple{SlotValidator, CraftValidator, CraftPerformer}"/> containing the SlotValidator, CraftValidator, and CraftPerformer.</param>
        /// <param name="CraftFinalizer">Optionally used to perform any final behavior after the craft is complete. The Item array is of length 4, where the first three Items are the inputs, and the fourth Item is the output.</param>
        /// <param name="Tile">An optional parameter that specifies what Interactive tile should open the menu when interacted with.</param>
        public CraftMenuInfo(string Title, string Desc, Material MenuMat, Material ProgressBarMat, Material ButtonInactiveMat, Material ButtonActiveMat, Material ButtonSelectMat, Task<AudioClip> CraftSound, Tuple<SlotValidator, CraftValidator, CraftPerformer> PerformerTuple, CraftFinalizer CraftFinalizer = null, TileInfo Tile = null) : base(MenuType.CRAFTING, null, Tile)
        {
            this.Title = Title;
            this.Desc = Desc;
            this.MenuMat = MenuMat;
            this.ProgressBarMat = ProgressBarMat;
            this.ButtonInactiveMat = ButtonInactiveMat;
            this.ButtonActiveMat = ButtonActiveMat;
            this.ButtonSelectMat = ButtonSelectMat;

            if (CraftSound != null)
            {
                CraftSound.ContinueWith(task =>
                {
                    this.CraftSound = task.Result;
                });
            }

            if (PerformerTuple != null)
            {
                SlotValidators = new List<SlotValidator>(1);
                CraftValidators = new List<CraftValidator>(1);
                CraftPerformers = new List<CraftPerformer>(1);
                CraftFinalizers = new List<CraftFinalizer>(1);
                CraftPerformerModIDs = new List<int>(1);
                CraftPerformerInitModIDs = new List<int>(1);
                AddCraftPerformer(PerformerTuple.Item1, PerformerTuple.Item2, PerformerTuple.Item3, CraftFinalizer);
            }
            else
            {
                SlotValidators = new List<SlotValidator>(0);
                CraftValidators = new List<CraftValidator>(0);
                CraftPerformers = new List<CraftPerformer>(0);
                CraftFinalizers = new List<CraftFinalizer>(0);
                CraftPerformerModIDs = new List<int>(0);
                CraftPerformerInitModIDs = new List<int>(0);
            }
        }

        /// <summary>
        /// Use to register a custom crafting menu. Using RegisterMenu is unnecessary for crafting menus. Note that all crafting menus must have, at most, 3 ingredient slots. Use OpenCraftMenu to open the menu.
        /// </summary>
        /// <param name="Title">The text to display as the title of the crafting window.</param>
        /// <param name="Desc">The text to display as the description of the crafting window.</param>
        /// <param name="MenuMat">The Material to use for the crafting window.</param>
        /// <param name="ProgressBarMat">The Material to use for the crafting progress bar.</param>
        /// <param name="ButtonInactiveMat">The Material to be used to display the inactive craft button.</param>
        /// <param name="ButtonActiveMat">The Material to be used to display the active craft button.</param>
        /// <param name="ButtonSelectMat">The Material to be used to display the selected craft button.</param>
        /// <param name="CraftSound">The sound effect that plays every time the craft button is pressed.</param>
        /// <param name="PerformerTuple">A <see cref="Tuple{SlotValidator, CraftValidator, CraftPerformer, CraftFinalizer}"/> containing the SlotValidator, CraftValidator, CraftPerformer, and CraftFinalizer.</param>
        /// <param name="Tile">An optional parameter that specifies what Interactive tile should open the menu when interacted with.</param>
        public CraftMenuInfo(string Title, string Desc, Material MenuMat, Material ProgressBarMat, Material ButtonInactiveMat, Material ButtonActiveMat, Material ButtonSelectMat, Task<AudioClip> CraftSound, Tuple<SlotValidator, CraftValidator, CraftPerformer, CraftFinalizer> PerformerTuple, TileInfo Tile = null) : base(MenuType.CRAFTING, null, Tile)
        {
            this.Title = Title;
            this.Desc = Desc;
            this.MenuMat = MenuMat;
            this.ProgressBarMat = ProgressBarMat;
            this.ButtonInactiveMat = ButtonInactiveMat;
            this.ButtonActiveMat = ButtonActiveMat;
            this.ButtonSelectMat = ButtonSelectMat;

            if (CraftSound != null)
            {
                CraftSound.ContinueWith(task =>
                {
                    this.CraftSound = task.Result;
                });
            }

            if (PerformerTuple != null)
            {
                SlotValidators = new List<SlotValidator>(1);
                CraftValidators = new List<CraftValidator>(1);
                CraftPerformers = new List<CraftPerformer>(1);
                CraftFinalizers = new List<CraftFinalizer>(1);
                CraftPerformerModIDs = new List<int>(1);
                CraftPerformerInitModIDs = new List<int>(1);
                AddCraftPerformer(PerformerTuple.Item1, PerformerTuple.Item2, PerformerTuple.Item3, PerformerTuple.Item4);
            }
            else
            {
                SlotValidators = new List<SlotValidator>(0);
                CraftValidators = new List<CraftValidator>(0);
                CraftPerformers = new List<CraftPerformer>(0);
                CraftFinalizers = new List<CraftFinalizer>(0);
                CraftPerformerModIDs = new List<int>(0);
                CraftPerformerInitModIDs = new List<int>(0);
            }
        }

        /// <summary>
        /// Called after this Registry Entry has been registered to its Registry. You should never call this yourself.
        /// </summary>
        protected internal override void PostRegister()
        {
            base.PostRegister();

            if (MenuMat == null)
            {
                MenuMat = new Material(Shader.Find("Unlit/Transparent"))
                {
                    mainTexture = MenuTex
                };
            }
            else
            {
                MenuTex = MenuMat.mainTexture;
            }
            GadgetCoreAPI.AddCustomResource("mat/craftMenu" + ID, MenuMat);
            if (ProgressBarMat == null)
            {
                ProgressBarMat = new Material(Shader.Find("Unlit/Transparent"))
                {
                    mainTexture = ProgressBarTex
                };
            }
            else
            {
                ProgressBarTex = ProgressBarMat.mainTexture;
            }
            GadgetCoreAPI.AddCustomResource("mat/craftBar" + ID, ProgressBarMat);
            if (ButtonInactiveMat == null)
            {
                ButtonInactiveMat = new Material(Shader.Find("Unlit/Transparent"))
                {
                    mainTexture = ButtonInactiveTex
                };
            }
            else
            {
                ButtonInactiveTex = ButtonInactiveMat.mainTexture;
            }
            GadgetCoreAPI.AddCustomResource("mat/craftButtonInactive" + ID, ButtonInactiveMat);
            if (ButtonActiveMat == null)
            {
                ButtonActiveMat = new Material(Shader.Find("Unlit/Transparent"))
                {
                    mainTexture = ButtonActiveTex
                };
            }
            else
            {
                ButtonActiveTex = ButtonActiveMat.mainTexture;
            }
            GadgetCoreAPI.AddCustomResource("mat/craftButtonActive" + ID, ButtonActiveMat);
            if (ButtonSelectMat == null)
            {
                ButtonSelectMat = new Material(Shader.Find("Unlit/Transparent"))
                {
                    mainTexture = ButtonSelectTex
                };
            }
            else
            {
                ButtonSelectTex = ButtonSelectMat.mainTexture;
            }
            GadgetCoreAPI.AddCustomResource("mat/craftButtonSelect" + ID, ButtonSelectMat);
        }

        /// <summary>
        /// Adds the given ID to the list of recipes that have been crafted before.
        /// </summary>
        public virtual void UnlockRecipe(int recipeID)
        {
            unlockedRecipes.Add(recipeID);
            PreviewLabs.PlayerPrefs.SetString("craftMenu" + ID + "unlocks", unlockedRecipes.Select(x => x.ToString()).Concat(","));
        }

        /// <summary>
        /// Returns whether the given recipe is unlocked.
        /// </summary>
        public virtual bool IsRecipeUnlocked(int recipeID)
        {
            return unlockedRecipes.Contains(recipeID);
        }

        /// <summary>
        /// Returns the progress made on the craft bar per hit of the craft button (from 1-100).
        /// </summary>
        protected internal virtual int GetCraftValue(int recipeID)
        {
            return OnCraftButton?.Invoke(recipeID) ?? (IsRecipeUnlocked(recipeID) ? 100 : UnityEngine.Random.Range(10, 30));
        }

        /// <summary>
        /// Indicates whether the quick-crafting menu is enabled for this <see cref="CraftMenuInfo"/>.
        /// </summary>
        protected virtual bool AllowQuickCrafting()
        {
            return false;
        }

        /// <summary>
        /// Opens this CraftMenu
        /// </summary>
        public override IEnumerator OpenMenuRoutine()
        {
            r.GetValue<Rigidbody>(InstanceTracker.PlayerScript).velocity = new Vector3(0f, 0f, 0f);
            if (interacting.GetValue<bool>(InstanceTracker.PlayerScript)) InstanceTracker.PlayerScript.w.SetActive(false);
            if (CanOpenMenu())
            {
                if (GameScript.buildMode)
                {
                    InstanceTracker.GameScript.ExitBuildMode();
                }
                if (GameScript.combatMode)
                {
                    InstanceTracker.GameScript.ExitCM();
                }
                InstanceTracker.GameScript.StartCoroutine(RefreshExpBar.Invoke(InstanceTracker.GameScript, new object[] { }) as IEnumerator);
                InstanceTracker.GameScript.txtCraftName[0].text = Title;
                InstanceTracker.GameScript.txtCraftName[1].text = InstanceTracker.GameScript.txtCraftName[0].text;
                InstanceTracker.GameScript.txtCraftTip[0].text = Desc;
                InstanceTracker.GameScript.txtCraftTip[1].text = InstanceTracker.GameScript.txtCraftTip[0].text;
                InstanceTracker.GameScript.barCraft.GetComponent<Renderer>().material = (Material)Resources.Load("mat/craftBar" + ID);
                craftType.SetValue(InstanceTracker.GameScript, ID);
                craftValue.SetValue(InstanceTracker.GameScript, 0);
                InstanceTracker.GameScript.barCraft.transform.localScale = new Vector3(0f, 0.6f, 1f);
                crafting.SetValue(InstanceTracker.GameScript, true);
                InstanceTracker.GameScript.StartCoroutine(Crafting.Invoke(InstanceTracker.GameScript, new object[] { }) as IEnumerator);
                InstanceTracker.GameScript.menuCraftObj.GetComponent<Renderer>().material = (Material)Resources.Load("mat/craftMenu" + ID); ;
                CraftCheck.Invoke(InstanceTracker.GameScript, new object[] { });
                InvokeOnMenuOpened();
                InstanceTracker.GameScript.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/invOpen"), Menuu.soundLevel / 10f);
                InstanceTracker.GameScript.bRecipe.SetActive(AllowQuickCrafting());
                InstanceTracker.GameScript.menuCraft.SetActive(true);
                InstanceTracker.GameScript.inventoryMain.SetActive(true);
                InstanceTracker.GameScript.CraftCheck();
                GameScript.inventoryOpen = true;
            }
            yield return new WaitForSeconds(0.5f);
            interacting.SetValue(InstanceTracker.PlayerScript, false);
        }

        /// <summary>
        /// Closes this CraftMenu
        /// </summary>
        public override IEnumerator CloseMenuRoutine()
        {
            InstanceTracker.GameScript.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/invClose"), Menuu.soundLevel / 10f);
            InstanceTracker.GameScript.menuCraft.SetActive(false);
            InstanceTracker.GameScript.inventoryMain.SetActive(false);
            InstanceTracker.GameScript.hoverItem.SetActive(false);
            if (holdingItem.GetValue<Item>(InstanceTracker.GameScript).id != 0)
            {
                InstanceTracker.GameScript.InvokeMethod("DropItem");
            }
            InstanceTracker.GameScript.InvokeMethod("DropCraftItems");
            curRecipePage.SetValue(InstanceTracker.GameScript, 0);
            crafting.SetValue(InstanceTracker.GameScript, false);
            canCraft.SetValue(InstanceTracker.GameScript, false);
            Cursor.visible = true;
            GameScript.inventoryOpen = false;
            InvokeOnMenuClosed();
            yield break;
        }

        /// <summary>
        /// Registers another CraftPerformer to this CraftMenuInfo. Returns the index that the performer was registered to.
        /// </summary>
        /// <param name="PerformerTuple">The Tuple containing the SlotValidator, CraftValidator, and CraftPerformer for this performer.</param>
        public int AddCraftPerformer(Tuple<SlotValidator, CraftValidator, CraftPerformer> PerformerTuple)
        {
            return AddCraftPerformer(PerformerTuple.Item1, PerformerTuple.Item2, PerformerTuple.Item3);
        }

        /// <summary>
        /// Registers another CraftPerformer to this CraftMenuInfo. Returns the index that the performer was registered to.
        /// </summary>
        /// <param name="PerformerTuple">The Tuple containing the SlotValidator, CraftValidator, CraftPerformer, and CraftFinalizer for this performer.</param>
        public int AddCraftPerformer(Tuple<SlotValidator, CraftValidator, CraftPerformer, CraftFinalizer> PerformerTuple)
        {
            return AddCraftPerformer(PerformerTuple.Item1, PerformerTuple.Item2, PerformerTuple.Item3, PerformerTuple.Item4);
        }

        /// <summary>
        /// Registers another CraftPerformer to this CraftMenuInfo. Returns the index that the performer was registered to.
        /// </summary>
        /// <param name="SlotValidator">Used to check if an item is valid for a slot.</param>
        /// <param name="CraftValidator">Used to check if the items currently in the slots are valid for a recipe. The Item array is of length 4, where the first three Items are the inputs, and the fourth Item is the output.</param>
        /// <param name="CraftPerformer">Used to perform a crafting operation by modifying the contents of the Item array. The Item array is of length 4, where the first three Items are the inputs, and the fourth Item is the output. Should decrement the inputs, and set the output, possibly respecting whatever is already in the output.</param>
        /// <param name="CraftFinalizer">Optionally used to perform any final behavior after the craft is complete. The Item array is of length 4, where the first three Items are the inputs, and the fourth Item is the output.</param>
        public int AddCraftPerformer(SlotValidator SlotValidator, CraftValidator CraftValidator, CraftPerformer CraftPerformer, CraftFinalizer CraftFinalizer = null)
        {
            if (SlotValidator == null) throw new ArgumentNullException("SlotValidator cannot be null!");
            if (CraftValidator == null) throw new ArgumentNullException("CraftValidator cannot be null!");
            if (CraftPerformer == null) throw new ArgumentNullException("CraftPerformer cannot be null!");
            SlotValidators.Add(SlotValidator);
            CraftValidators.Add(CraftValidator);
            CraftPerformers.Add(CraftPerformer);
            CraftFinalizers.Add(CraftFinalizer);
            CraftPerformerModIDs.Add(Registry.gadgetRegistering);
            CraftPerformerInitModIDs.Add(Registry.gadgetRegistering);
            return CraftPerformers.Count - 1;
        }

        /// <summary>
        /// Replaces an existing CraftPerformer on this CraftMenuInfo.
        /// </summary>
        /// <param name="index">The index of the CraftPerformer to replace</param>
        /// <param name="PerformerTuple">The Tuple containing the SlotValidator, CraftValidator, and CraftPerformer for this performer.</param>
        public int ReplaceCraftPerformer(int index, Tuple<SlotValidator, CraftValidator, CraftPerformer> PerformerTuple)
        {
            return ReplaceCraftPerformer(index, PerformerTuple.Item1, PerformerTuple.Item2, PerformerTuple.Item3);
        }

        /// <summary>
        /// Replaces an existing CraftPerformer on this CraftMenuInfo.
        /// </summary>
        /// <param name="index">The index of the CraftPerformer to replace</param>
        /// <param name="PerformerTuple">The Tuple containing the SlotValidator, CraftValidator, CraftPerformer, and CraftFinalizer for this performer.</param>
        public int ReplaceCraftPerformer(int index, Tuple<SlotValidator, CraftValidator, CraftPerformer, CraftFinalizer> PerformerTuple)
        {
            return ReplaceCraftPerformer(index, PerformerTuple.Item1, PerformerTuple.Item2, PerformerTuple.Item3, PerformerTuple.Item4);
        }

        /// <summary>
        /// Replaces an existing CraftPerformer on this CraftMenuInfo.
        /// </summary>
        /// <param name="index">The index of the CraftPerformer to replace</param>
        /// <param name="SlotValidator">A Func that is used to check if an item is valid for a slot.</param>
        /// <param name="CraftValidator">A Func that is used to check if the items currently in the slots are valid for a recipe. The Item array is of length 4, where the first three Items are the inputs, and the fourth Item is the output.</param>
        /// <param name="CraftPerformer">An Action that is used to perform a crafting operation by modifying the contents of the Item array. The Item array is of length 4, where the first three Items are the inputs, and the fourth Item is the output. Should decrement the inputs, and set the output, possibly respecting whatever is already in the output.</param>
        /// <param name="CraftFinalizer">Used to perform any final behavior after the craft is complete. The Item array is of length 4, where the first three Items are the inputs, and the fourth Item is the output.</param>
        public int ReplaceCraftPerformer(int index, SlotValidator SlotValidator, CraftValidator CraftValidator, CraftPerformer CraftPerformer, CraftFinalizer CraftFinalizer = null)
        {
            SlotValidators[index] = SlotValidator ?? throw new ArgumentNullException("SlotValidator cannot be null!");
            CraftValidators[index] = CraftValidator ?? throw new ArgumentNullException("CraftValidator cannot be null!");
            CraftPerformers[index] = CraftPerformer ?? throw new ArgumentNullException("CraftPerformer cannot be null!");
            CraftFinalizers[index] = CraftFinalizer;
            CraftPerformerModIDs[index] = Registry.gadgetRegistering;
            return CraftPerformers.Count - 1;
        }

        /// <summary>
        /// Removes a CraftPerformer from this CraftMenuInfo, given its index. 
        /// </summary>
        public void RemoveCraftPerformer(int index)
        {
            SlotValidators.RemoveAt(index);
            CraftValidators.RemoveAt(index);
            CraftPerformers.RemoveAt(index);
            CraftFinalizers.RemoveAt(index);
            CraftPerformerModIDs.RemoveAt(index);
            CraftPerformerInitModIDs.RemoveAt(index);
        }

        /// <summary>
        /// Removes all CraftPerformers that were added by the given mod
        /// </summary>
        public void RemoveModCraftPerformers(int modID)
        {
            for (int i = 0;i < CraftPerformerModIDs.Count;i++)
            {
                if (CraftPerformerModIDs[i] == modID)
                {
                    RemoveCraftPerformer(i);
                }
            }
        }

        /// <summary>
        /// Validates an Item for insertion into a specified slot. Returns true if validation was successful and the Item should fit in the given slot, or false otherwise.
        /// </summary>
        public bool ValidateSlot(Item newItem, Item[] items, int slot)
        {
            return SlotValidators.Any(x => x(newItem, items, slot));
        }

        /// <summary>
        /// Generates a standard SlotValidator/CraftValidator/CraftPerformer trio using a simple set of input IDs and an output Item
        /// </summary>
        /// <param name="recipes">An array of recipes consisting of the ingredient IDs, the item output, and the amount of possible random extra output.</param>
        public static Tuple<SlotValidator, CraftValidator, CraftPerformer> CreateSimpleCraftPerformer(params Tuple<int[], Item, int>[] recipes)
        {
            return CreateSimpleCraftPerformer(recipes.Select(x => Tuple.Create(x.Item1, x.Item2, x.Item3, false)).ToArray());
        }

        /// <summary>
        /// Generates a standard SlotValidator/CraftValidator/CraftPerformer trio using a simple set of input IDs and an output Item
        /// </summary>
        /// <param name="recipes">An array of recipes consisting of the ingredient IDs, the item output, the amount of possible random extra output, and whether to randomize the rarity of the output item.</param>
        public static Tuple<SlotValidator, CraftValidator, CraftPerformer> CreateSimpleCraftPerformer(params Tuple<int[], Item, int, bool>[] recipes)
        {
            return Tuple.Create<SlotValidator, CraftValidator, CraftPerformer>((Item newItem, Item[] items, int slot) =>
            {
                return recipes.Any(x => x.Item1.Length > slot && newItem.id == x.Item1[slot]);
            }, (Item[] items) =>
            {
                return recipes.Any(x =>
                {
                    for (int i = 0; i < x.Item1.Length; i++)
                    {
                        if (x.Item1[i] > 0 && (items.Length <= i || items[i] == null || items[i].q < 1 || items[i].id != x.Item1[i])) return false;
                    }
                    return items[items.Length - 1] == null || items[items.Length - 1].q == 0 || (GadgetCoreAPI.CanItemsStack(items[items.Length - 1], x.Item2) && items[items.Length - 1].q + x.Item2.q + x.Item3 <= 9999);
                });
            }, (Item[] items) =>
            {
                foreach (Tuple<int[], Item, int, bool> recipe in recipes)
                {
                    bool recipeValid = true;
                    for (int i = 0; i < recipe.Item1.Length; i++)
                    {
                        if (recipe.Item1[i] > 0 && (items.Length <= i || items[i] == null || items[i].q < 1 || items[i].id != recipe.Item1[i]))
                        {
                            recipeValid = false;
                            break;
                        }
                    }
                    if (recipeValid)
                    {
                        for (int i = 0; i < recipe.Item1.Length; i++)
                        {
                            if (recipe.Item1[i] > 0)
                            {
                                items[i].q--;
                                if (items[i].q <= 0) items[i] = GadgetCoreAPI.EmptyItem();
                            }
                        }
                        if (items[items.Length - 1].id == recipe.Item2.id && items[items.Length - 1].q > 0)
                        {
                            items[items.Length - 1].q += recipe.Item2.q;
                        }
                        else
                        {
                            items[items.Length - 1] = GadgetCoreAPI.CopyItem(recipe.Item2);
                            if (recipe.Item4) items[items.Length - 1].tier = GadgetCoreAPI.GetRandomCraftTier();
                        }
                        if (recipe.Item3 > 0) items[items.Length - 1].q += UnityEngine.Random.Range(0, recipe.Item3 + 1);
                        if (recipe.Item3 < 0) items[items.Length - 1].q -= UnityEngine.Random.Range(0, -recipe.Item3 + 1);
                        break;
                    }
                }
            });
        }

        /// <summary>
        /// Generates a standard SlotValidator/CraftValidator/CraftPerformer trio using a more advanced structure of recipe components.
        /// </summary>
        /// <param name="recipes">An array of recipes represented by <see cref="AdvancedRecipe"/>s.</param>
        public static Tuple<SlotValidator, CraftValidator, CraftPerformer> CreateAdvancedCraftPerformer(params AdvancedRecipe[] recipes)
        {
            return Tuple.Create<SlotValidator, CraftValidator, CraftPerformer>((Item newItem, Item[] items, int slot) =>
            {
                return recipes.Any(x => x.Slots[slot].Type != AdvancedRecipeComponentType.UNUSED && (x.Slots[slot].Type & AdvancedRecipeComponentType.OUTPUT) == AdvancedRecipeComponentType.INPUT && x.Slots[slot].Matches(newItem));
            }, (Item[] items) =>
            {
                return recipes.Any(x =>
                {
                    for (int i = 0; i < x.Slots.Length; i++)
                    {
                        bool isSlotEmpty = items.Length <= i || items[i] == null || items[i].q <= 0;
                        if (x.Slots[i].Type == AdvancedRecipeComponentType.UNUSED && !isSlotEmpty || // Unused slot

                        (x.Slots[i].Type & AdvancedRecipeComponentType.OUTPUT) == AdvancedRecipeComponentType.INPUT ?

                        (isSlotEmpty || items[i].q < x.Slots[i].Item.q + Math.Max(x.Slots[i].QuantityVariation, 0) ||
                        !x.Slots[i].Matches(items[i])) : // Input slot

                        !isSlotEmpty && (!GadgetCoreAPI.CanItemsStack(x.Slots[i].Item, items[i]) || // Output slot
                        items[i].q + x.Slots[i].Item.q + x.Slots[i].QuantityVariation > 9999)) return false;
                    }
                    return true;
                });
            }, (Item[] items) =>
            {
                foreach (AdvancedRecipe recipe in recipes)
                {
                    bool recipeValid = true;
                    for (int i = 0; i < recipe.Slots.Length; i++)
                    {
                        bool isSlotEmpty = items.Length <= i || items[i] == null || items[i].q <= 0;
                        if (recipe.Slots[i].Type == AdvancedRecipeComponentType.UNUSED && !isSlotEmpty || // Unused slot

                           (recipe.Slots[i].Type & AdvancedRecipeComponentType.OUTPUT) == AdvancedRecipeComponentType.INPUT ?

                           (isSlotEmpty || items[i].q < recipe.Slots[i].Item.q + Math.Max(recipe.Slots[i].QuantityVariation, 0) ||
                           !recipe.Slots[i].Matches(items[i])) : // Input slot

                           !isSlotEmpty && (!GadgetCoreAPI.CanItemsStack(recipe.Slots[i].Item, items[i]) || // Output slot
                           items[i].q + recipe.Slots[i].Item.q + recipe.Slots[i].QuantityVariation > 9999))
                        {
                            recipeValid = false;
                            break;
                        }
                    }
                    if (recipeValid)
                    {
                        Item coreItem = new Item(0, 1, 0, 0, 0, new int[3], new int[3]);
                        for (int i = 0; i < recipe.Slots.Length; i++)
                        {
                            switch (recipe.Slots[i].Type)
                            {
                                case AdvancedRecipeComponentType.CORE_INPUT:
                                    coreItem = items[i].CloneItem();
                                    goto case AdvancedRecipeComponentType.INPUT;
                                case AdvancedRecipeComponentType.INPUT:
                                    items[i].q -= recipe.Slots[i].Item.q;
                                    if (recipe.Slots[i].QuantityVariation > 0) items[i].q -= UnityEngine.Random.Range(0, recipe.Slots[i].QuantityVariation + 1);
                                    if (recipe.Slots[i].QuantityVariation < 0) items[i].q += UnityEngine.Random.Range(0, -recipe.Slots[i].QuantityVariation + 1);
                                    if (items[i].q <= 0) items[i] = GadgetCoreAPI.EmptyItem();
                                    break;
                            }
                        }
                        for (int i = 0; i < recipe.Slots.Length; i++)
                        {
                            int oldQuantity = items[i]?.q ?? 0;
                            switch (recipe.Slots[i].Type)
                            {
                                case AdvancedRecipeComponentType.CORE_OUTPUT:
                                    items[i] = coreItem.CloneItem();
                                    items[i].id = recipe.Slots[i].Item.id;
                                    items[i].q = recipe.Slots[i].Item.q + oldQuantity;
                                    items[i].exp += recipe.Slots[i].Item.exp;
                                    items[i].tier += recipe.Slots[i].Item.tier;
                                    if (recipe.Slots[i].QuantityVariation > 0) items[i].q += UnityEngine.Random.Range(0, recipe.Slots[i].QuantityVariation + 1);
                                    if (recipe.Slots[i].QuantityVariation < 0) items[i].q -= UnityEngine.Random.Range(0, -recipe.Slots[i].QuantityVariation + 1);
                                    break;
                                case AdvancedRecipeComponentType.OUTPUT:
                                    items[i] = recipe.Slots[i].Item.CloneItem();
                                    items[i].q += oldQuantity;
                                    if (items[i].tier < 0) items[i].tier = GadgetCoreAPI.GetRandomCraftTier();
                                    if (recipe.Slots[i].QuantityVariation > 0) items[i].q += UnityEngine.Random.Range(0, recipe.Slots[i].QuantityVariation + 1);
                                    if (recipe.Slots[i].QuantityVariation < 0) items[i].q -= UnityEngine.Random.Range(0, -recipe.Slots[i].QuantityVariation + 1);
                                    break;
                            }
                        }
                        break;
                    }
                }
            });
        }

        /// <summary>
        /// Removes all CraftPerformers that were added by the given mod from all CraftMenuInfos
        /// </summary>
        public static void RemoveAllModCraftPerformers(int modID)
        {
            foreach (MenuInfo menu in MenuRegistry.Singleton)
            {
                if (menu is CraftMenuInfo craftMenu) craftMenu.RemoveModCraftPerformers(modID);
            }
        }

        /// <summary>
        /// Given the ID of the recipe being crafted, returns the progress to be added to the craft bar (from 1-100) in a click.
        /// </summary>
        public event Func<int, int> OnCraftButton;

        /// <summary>
        /// A delegate used for determining if an item is valid for a slot.
        /// </summary>
        public delegate bool SlotValidator(Item newItem, Item[] items, int slot);
        /// <summary>
        /// A delegate used for validating crafting recipes.
        /// </summary>
        public delegate bool CraftValidator(Item[] items);
        /// <summary>
        /// A delegate used for performing crafting recipes.
        /// </summary>
        public delegate void CraftPerformer(Item[] items);
        /// <summary>
        /// A delegate used for finalizing crafting recipes.
        /// </summary>
        public delegate void CraftFinalizer(Item[] items);

        /// <summary>
        /// Represents a recipe provided to <see cref="CreateAdvancedCraftPerformer(AdvancedRecipe[])"/>
        /// </summary>
        public struct AdvancedRecipe
        {
            /// <summary>
            /// Array of recipe components for this recipe. Will always be of length 4.
            /// </summary>
            public readonly AdvancedRecipeComponent[] Slots;

            /// <summary>
            /// Constructs a new <see cref="AdvancedRecipe"/> with the four specified slots. An unused slot can be null.
            /// </summary>
            public AdvancedRecipe(AdvancedRecipeComponent Slot1, AdvancedRecipeComponent Slot2, AdvancedRecipeComponent Slot3, AdvancedRecipeComponent Slot4)
            {
                Slots = new[] { Slot1, Slot2, Slot3, Slot4 };
                if (Slots.Count(x => x.Type == AdvancedRecipeComponentType.CORE_INPUT) > 1) throw new InvalidOperationException("An advanced recipe may only contain one CORE_INPUT component!");
                if (Slots.Count(x => x.Type == AdvancedRecipeComponentType.CORE_INPUT) == 0 && Slots.Count(x => x.Type == AdvancedRecipeComponentType.CORE_OUTPUT) > 0) throw new InvalidOperationException("An advanced recipe may not contain a CORE_OUTPUT component without a CORE_INPUT component1!");
            }
        }

        /// <summary>
        /// Represents a component in a recipe provided to <see cref="CreateAdvancedCraftPerformer(AdvancedRecipe[])"/>
        /// </summary>
        public struct AdvancedRecipeComponent
        {
            /// <summary>
            /// Fill unused recipe slots with this.
            /// </summary>
            public static readonly AdvancedRecipeComponent UNUSED = new AdvancedRecipeComponent(null, AdvancedRecipeComponentType.UNUSED);

            /// <summary>
            /// The type of this advanced recipe component.
            /// </summary>
            public readonly AdvancedRecipeComponentType Type;
            /// <summary>
            /// The Item associated with this slot.
            /// </summary>
            public readonly Item Item;
            /// <summary>
            /// Whether or not the Item should be matched purely based on ID.
            /// </summary>
            public readonly bool Fuzzy;
            /// <summary>
            /// The maximum amount by which the quantity of the associated Item may by randomly increased by.
            /// </summary>
            public readonly int QuantityVariation;

            /// <summary>
            /// Constructs a new <see cref="AdvancedRecipeComponent"/>
            /// </summary>
            public AdvancedRecipeComponent(Item item, AdvancedRecipeComponentType type, int quantityVariation = 0)
            {
                if (type != AdvancedRecipeComponentType.UNUSED && (item == null || item.id == 0 || item.q == 0)) throw new InvalidOperationException("Only unused slots may have empty Items!");
                Item = item;
                Type = type;
                Fuzzy = (type & AdvancedRecipeComponentType.CORE_INPUT) == AdvancedRecipeComponentType.CORE_INPUT;
                QuantityVariation = quantityVariation;
            }

            /// <summary>
            /// Constructs a new <see cref="AdvancedRecipeComponent"/>
            /// </summary>
            public AdvancedRecipeComponent(Item item, AdvancedRecipeComponentType type, bool fuzzy, int quantityVariation = 0)
            {
                Item = item;
                Type = type;
                Fuzzy = fuzzy;
                QuantityVariation = quantityVariation;
            }

            /// <summary>
            /// Checks if the given item matches this component's requirement
            /// Can be overridden for more advanced logic
            /// </summary>
            public bool Matches(Item item)
            {
                return Fuzzy ? Item.id == item.id : GadgetCoreAPI.MatchItems(item, Item);
            }
        }

        /// <summary>
        /// Represents the component type of an <see cref="AdvancedRecipeComponent"/>
        /// </summary>
        public enum AdvancedRecipeComponentType
        {
            /// <summary>
            /// Standard input component for a recipe.
            /// </summary>
            INPUT       = 0b00,
            /// <summary>
            /// Core input component for a recipe - there can only be one component of this type.
            /// </summary>
            CORE_INPUT  = 0b01,
            /// <summary>
            /// Standard output component for a recipe.
            /// </summary>
            OUTPUT      = 0b10,
            /// <summary>
            /// Core output component for a recipe - inherits all properties other than ID and quantity from the core input component.
            /// </summary>
            CORE_OUTPUT = 0b11,
            /// <summary>
            /// A component representing an unused slot.
            /// </summary>
            UNUSED      = 0b100
        }
    }
}
