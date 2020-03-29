using GadgetCore.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GadgetCore.API
{
    /// <summary>
    /// Static class used for tracking custom crafting recipes and menus.
    /// </summary>
    public static class CraftMenus
    {
        internal static List<CustomCraftMenu> customCraftMenus = new List<CustomCraftMenu>();
        private static int lastCrafterID = 3;

        /// <summary>
        /// Use to register a custom crafting menu. Using RegisterMenu is unnecessary for crafting menus. Note that all crafting menus must have, at most, 3 ingredient slots. Use OpenCraftMenu to open the menu.
        /// </summary>
        /// <param name="Title">The text to display as the title of the crafting window.</param>
        /// <param name="Desc">The text to display as the description of the crafting window.</param>
        /// <param name="MenuMat">The material to use for the crafting window.</param>
        /// <param name="ProgressBarMat">The material to use for the crafting progress bar.</param>
        /// <param name="SlotValidator">Used to check if an item is valid for a slot.</param>
        /// <param name="CraftValidator">Used to check if the items currently in the slots are valid for a recipe. The Item array is of length 4, where the first three Items are the inputs, and the fourth Item is the output.</param>
        /// <param name="CraftPerformer">Used to perform a crafting operation by modifying the contents of the Item array. The Item array is of length 4, where the first three Items are the inputs, and the fourth Item is the output. Should decrement the inputs, and set the output, possibly respecting whatever is already in the output.</param>
        /// <param name="Tile">An optional parameter that specifies what Interactive tile should open the menu when interacted with. Shortcut for adding OpenCraftMenuRoutine to the tile's OnInteract event.</param>
        /// <returns></returns>
        public static int RegisterCustomCraftMenu(string Title, string Desc, Material MenuMat, Material ProgressBarMat, SlotValidator SlotValidator, CraftValidator CraftValidator, CraftPerformer CraftPerformer, TileInfo Tile = null)
        {
            int CraftMenuID = ++lastCrafterID;
            customCraftMenus.Add(new CustomCraftMenu(CraftMenuID, Title, Desc, MenuMat, ProgressBarMat, SlotValidator, CraftValidator, CraftPerformer));
            if (Tile != null) Tile.OnInteract += () => GadgetCoreAPI.OpenCraftMenuRoutine(lastCrafterID + 1);
            return CraftMenuID;
        }

        /// <summary>
        /// Returns the registered <see cref="CustomCraftMenu"/> with the given ID. Returns null if there is no registered menu with the given ID.
        /// </summary>
        public static CustomCraftMenu GetCraftMenuByID(int CraftMenuID)
        {
            return customCraftMenus.Count > CraftMenuID ? customCraftMenus[CraftMenuID] : null;
        }

        /// <summary>
        /// Creates a SlotValidator, CraftValidator, and CraftPerformer that fulfil a simple array of recipes based upon the IDs of the inputs, the Item for the output, and an int representing the possible bonus output. The bonus output is used for recipes with a random amount of output - the actual output will be the quantity of the output item, plus randomly up to the bonus output.
        /// </summary>
        public static Tuple<SlotValidator, CraftValidator, CraftPerformer> SimpleCraftPerformer(Tuple<int[], Item, int>[] recipes)
        {
            return Tuple.Create<SlotValidator, CraftValidator, CraftPerformer>((Item item, int slot) =>
            {
                return recipes.Any(x => item.id == x.Item1[slot]);
            }, (Item[] items) =>
            {
                return recipes.Any(x =>
                {
                    for (int i = 0; i < x.Item1.Length; i++)
                    {
                        if (x.Item1[i] > 0 && (items.Length <= i || items[i] == null || items[i].q < 1 || items[i].id != x.Item1[i])) return false;
                    }
                    return items[items.Length - 1] == null || items[items.Length - 1].q == 0 || items[items.Length - 1].id == x.Item2.id;
                });
            }, (Item[] items) =>
            {
                foreach (Tuple<int[], Item, int> recipe in recipes)
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
                            }
                        }
                        if (items[items.Length - 1].id == recipe.Item2.id)
                        {
                            items[items.Length - 1].q += recipe.Item2.q;
                        }
                        else
                        {
                            items[items.Length - 1] = GadgetCoreAPI.CopyItem(recipe.Item2);
                        }
                        if (recipe.Item3 > 0) items[items.Length - 1].q += UnityEngine.Random.Range(0, recipe.Item3 + 1);
                        if (recipe.Item3 < 0) items[items.Length - 1].q -= UnityEngine.Random.Range(0, -recipe.Item3 + 1);
                        break;
                    }
                }
            });
        }
    }

    /// <summary>
    /// A delegate used for determining if an item is valid for a slot.
    /// </summary>
    public delegate bool SlotValidator(Item item, int slot);
    /// <summary>
    /// A delegate used for validating crafting recipes.
    /// </summary>
    public delegate bool CraftValidator(Item[] items);
    /// <summary>
    /// A delegate used for performing crafting recipes.
    /// </summary>
    public delegate void CraftPerformer(Item[] items);
}
