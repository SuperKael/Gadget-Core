using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GadgetCore.API
{
    /// <summary>
    /// Represents a craft menu, such as the emblem forge or the alchemy station. Also includes custom craft menus.
    /// </summary>
    public class CustomCraftMenu
    {
        /// <summary>
        /// The numerical ID of the mod that created this menu. Will be -1 in the case of vanilla menus.
        /// </summary>
        public readonly int ModID;
        /// <summary>
        /// The craft type ID for this menu.
        /// </summary>
        public readonly int CraftMenuID;
        /// <summary>
        /// The title to be displayed when this craft menu is open.
        /// </summary>
        protected internal readonly string Title;
        /// <summary>
        /// The description to be displayed when this craft menu is open.
        /// </summary>
        protected internal readonly string Desc;
        /// <summary>
        /// The material to be used to display this craft menu.
        /// </summary>
        protected internal readonly Material MenuMat;
        /// <summary>
        /// The material to be used to display the craft progress bar.
        /// </summary>
        protected internal readonly Material ProgressBarMat;
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
        /// The list of Mod IDs that match the registered craft performers. These values will change when <see cref="ReplaceCraftPerformer"/> is called. May be the same as the entry in <see cref="CraftPerformerInitModIDs"/>
        /// </summary>
        protected internal readonly List<int> CraftPerformerModIDs;
        /// <summary>
        /// The list of Mod IDs that match the registered craft performers when they were first registered. May be the same as the entry in <see cref="CraftPerformerModIDs"/>
        /// </summary>
        protected internal readonly List<int> CraftPerformerInitModIDs;

        internal CustomCraftMenu(int CraftMenuID, string Title, string Desc, Material MenuMat, Material ProgressBarMat, SlotValidator SlotValidator, CraftValidator CraftValidator, CraftPerformer CraftPerformer)
        {
            this.CraftMenuID = CraftMenuID;
            this.Title = Title;
            this.Desc = Desc;
            this.MenuMat = MenuMat;
            this.ProgressBarMat = ProgressBarMat;
            SlotValidators = new List<SlotValidator>(1);
            CraftValidators = new List<CraftValidator>(1);
            CraftPerformers = new List<CraftPerformer>(1);
            CraftPerformerModIDs = new List<int>(1);
            CraftPerformerInitModIDs = new List<int>(1);
            AddCraftPerformer(SlotValidator, CraftValidator, CraftPerformer);
        }

        /// <summary>
        /// Registers another CraftPerformer to this CraftMenu. Returns the index that the performer was registered to.
        /// </summary>
        /// <param name="SlotValidator">Used to check if an item is valid for a slot.</param>
        /// <param name="CraftValidator">Used to check if the items currently in the slots are valid for a recipe. The Item array is of length 4, where the first three Items are the inputs, and the fourth Item is the output.</param>
        /// <param name="CraftPerformer">Used to perform a crafting operation by modifying the contents of the Item array. The Item array is of length 4, where the first three Items are the inputs, and the fourth Item is the output. Should decrement the inputs, and set the output, possibly respecting whatever is already in the output.</param>
        public int AddCraftPerformer(SlotValidator SlotValidator, CraftValidator CraftValidator, CraftPerformer CraftPerformer)
        {
            if (SlotValidator == null) throw new ArgumentNullException("SlotValidator cannot be null!");
            if (CraftValidator == null) throw new ArgumentNullException("CraftValidator cannot be null!");
            if (CraftPerformer == null) throw new ArgumentNullException("CraftPerformer cannot be null!");
            SlotValidators.Add(SlotValidator);
            CraftValidators.Add(CraftValidator);
            CraftPerformers.Add(CraftPerformer);
            CraftPerformerModIDs.Add(Registry.modRegistering);
            CraftPerformerInitModIDs.Add(Registry.modRegistering);
            return CraftPerformers.Count - 1;
        }

        /// <summary>
        /// Replaces an existing CraftPerformer on this CraftMenu.
        /// </summary>
        /// <param name="SlotValidator">A Func that is used to check if an item is valid for a slot.</param>
        /// <param name="CraftValidator">A Func that is used to check if the items currently in the slots are valid for a recipe. The Item array is of length 4, where the first three Items are the inputs, and the fourth Item is the output.</param>
        /// <param name="CraftPerformer">An Action that is used to perform a crafting operation by modifying the contents of the Item array. The Item array is of length 4, where the first three Items are the inputs, and the fourth Item is the output. Should decrement the inputs, and set the output, possibly respecting whatever is already in the output.</param>
        /// <param name="index">The index of the CraftPerformer to replace</param>
        public int ReplaceCraftPerformer(SlotValidator SlotValidator, CraftValidator CraftValidator, CraftPerformer CraftPerformer, int index)
        {
            SlotValidators[index] = SlotValidator ?? throw new ArgumentNullException("SlotValidator cannot be null!");
            CraftValidators[index] = CraftValidator ?? throw new ArgumentNullException("CraftValidator cannot be null!");
            CraftPerformers[index] = CraftPerformer ?? throw new ArgumentNullException("CraftPerformer cannot be null!");
            CraftPerformerModIDs[index] = Registry.modRegistering;
            return CraftPerformers.Count - 1;
        }

        /// <summary>
        /// Removes a CraftPerformer from this CraftMenu, given its index. 
        /// </summary>
        public void RemoveCraftPerformer(int index)
        {
            SlotValidators.RemoveAt(index);
            CraftValidators.RemoveAt(index);
            CraftPerformers.RemoveAt(index);
            CraftPerformerModIDs.RemoveAt(index);
            CraftPerformerInitModIDs.RemoveAt(index);
        }

        /// <summary>
        /// Validates an Item for insertion into a specified slot. Returns true if validation was succesful and the Item should fit in the given slot, or false otherwise.
        /// </summary>
        public bool ValidateSlot(Item item, int slot)
        {
            return SlotValidators.Any(x => x(item, slot));
        }
    }
}
