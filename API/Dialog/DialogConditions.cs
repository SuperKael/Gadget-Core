using System;
using System.Linq;

namespace GadgetCore.API.Dialog
{
    /// <summary>
    /// Static class containing commonly-used <see cref="DialogChain"/> trigger conditions.
    /// </summary>
    public static class DialogConditions
    {
        /// <summary>
        /// Combines multiple conditions together, requiring all of them to be met.
        /// </summary>
        public static Func<int, bool> AllConditions(params Func<int, bool>[] conditions)
        {
            return (b) =>
            {
                return conditions.All(x => x(b));
            };
        }

        /// <summary>
        /// Combines multiple conditions together, requiring any of them to be met.
        /// </summary>
        public static Func<int, bool> AnyCondition(params Func<int, bool>[] conditions)
        {
            return (b) =>
            {
                return conditions.Any(x => x(b));
            };
        }

        /// <summary>
        /// Checks for a particular value of the 'b' parameter to the Talk method.
        /// </summary>
        public static Func<int, bool> MatchBValue(int bValue)
        {
            return (b) =>
            {
                return b == bValue;
            };
        }

        /// <summary>
        /// Checks that the player has a particular allegiance and is at a particualar level.
        /// </summary>
        public static Func<int, bool> MatchAllegianceAndLevel(int allegiance, int allegianceLevel)
        {
            return (b) =>
            {
                return Menuu.curAllegiance == allegiance && GameScript.allegianceQuest == allegianceLevel;
            };
        }

        /// <summary>
        /// Checks that the player made a particular story choice (<see cref="GameScript.choice"/>);
        /// </summary>
        public static Func<int, bool> MatchStoryChoice(int choice)
        {
            return (b) =>
            {
                return GameScript.choice == choice;
            };
        }

        /// <summary>
        /// Checks that the player has a particular allegiance.
        /// </summary>
        public static Func<int, bool> MatchAllegiance(int allegiance)
        {
            return (b) =>
            {
                return Menuu.curAllegiance == allegiance;
            };
        }

        /// <summary>
        /// Checks that the player has a particular allegiance level.
        /// </summary>
        public static Func<int, bool> MatchAllegianceLevel(int allegianceLevel)
        {
            return (b) =>
            {
                return GameScript.allegianceQuest == allegianceLevel;
            };
        }

        /// <summary>
        /// Checks that the player has a particular minimum player level.
        /// </summary>
        public static Func<int, bool> MatchMinimumPlayerLevel(int playerLevel)
        {
            return (b) =>
            {
                return InstanceTracker.GameScript.GetPlayerLevel() >= playerLevel;
            };
        }

        /// <summary>
        /// Checks that the player has a particular minimum player level.
        /// </summary>
        public static Func<int, bool> MatchMaximumPlayerLevel(int playerLevel)
        {
            return (b) =>
            {
                return InstanceTracker.GameScript.GetPlayerLevel() <= playerLevel;
            };
        }

        /// <summary>
        /// Checks whether the player has an item.
        /// </summary>
        public static Func<int, bool> HasItem(int itemID, int quantity = 1)
        {
            return (b) =>
            {
                int slot = InstanceTracker.GameScript.ItemExistsSlot(itemID);
                return slot != -1 && GadgetCoreAPI.GetInventory()[slot].q >= quantity;
            };
        }
    }
}
