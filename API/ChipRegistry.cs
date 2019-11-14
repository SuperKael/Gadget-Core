using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GadgetCore.API
{
    /// <summary>
    /// This registry is filled with ChipInfos, and is used for registering custom chips to the game.
    /// </summary>
    public class ChipRegistry : Registry<ChipRegistry, ChipInfo, ChipType>
    {
        /// <summary>
        /// Gets the name of this registry. Must be constant.
        /// </summary>
        public override string GetRegistryName()
        {
            return "Chip";
        }

        /// <summary>
        /// Gets the ID that modded IDs should start at for this registry. May be 0 if the vanilla game does not use IDs for this type of thing.
        /// </summary>
        public override int GetIDStart()
        {
            return 10000;
        }
    }

    /// <summary>
    /// Specifies what type of chip this is. If you wish for your chip to grant both passive and active effects, use ACTIVE.
    /// </summary>
    public enum ChipType
    {
        /// <summary>
        /// This chip is passive, and cannot be activated.
        /// </summary>
        PASSIVE,
        /// <summary>
        /// This chip is active, and can be activated. Note that active chips can still grant passive effects, if you desire.
        /// </summary>
        ACTIVE
    }
}
