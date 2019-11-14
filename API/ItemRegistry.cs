using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GadgetCore.API
{
    /// <summary>
    /// This registry is filled with ItemInfos, and is used for registering custom items to the game.
    /// </summary>
    public class ItemRegistry : Registry<ItemRegistry, ItemInfo, ItemType>
    {
        /// <summary>
        /// Gets the name of this registry. Must be constant.
        /// </summary>
        public override string GetRegistryName()
        {
            return "Item";
        }

        /// <summary>
        /// Gets the default type of the given ID, assuming that it is a vanilla ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ItemType GetDefaultTypeByID(int id)
        {
            if (id > 0 && id <= 40)
            {
                if (id <= 10) return ItemType.LOOT | ItemType.ROCK;
                if (id <= 20) return ItemType.LOOT | ItemType.PLANT;
                if (id <= 30) return ItemType.LOOT | ItemType.MONSTER;
                return ItemType.LOOT | ItemType.BUG;
            }
            if (id > 100 && id <= 200)
            {
                if (id <= 110) return ItemType.EMBLEM | ItemType.ROCK;
                if (id <= 120) return ItemType.EMBLEM | ItemType.PLANT;
                if (id <= 130) return ItemType.EMBLEM | ItemType.MONSTER;
                if (id <= 140) return ItemType.EMBLEM | ItemType.BUG;
                return ItemType.EMBLEM | ItemType.OTHER;
            }
            if (id >= 300 && id < 600) return ItemType.WEAPON;
            if (id >= 600 && id < 700) return ItemType.OFFHAND;
            if (id >= 700 && id < 800) return ItemType.HELMET;
            if (id >= 800 && id < 900) return ItemType.ARMOR;
            if (id >= 900 && id < 1000) return ItemType.RING;
            if (id >= 1000 && id < 2000) return ItemType.DROID;
            return ItemType.GENERIC;
        }

        /// <summary>
        /// Gets the type of the specified ID. Applies to vanilla items as well as Gadget items.
        /// </summary>
        public static ItemType GetTypeByID(int id)
        {
            ItemInfo itemInfo = GetSingleton().GetEntry(id);
            return itemInfo != null ? (itemInfo.Type & (ItemType.BASIC_MASK | ItemType.TYPE_MASK)) : ItemRegistry.GetDefaultTypeByID(id);
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
    /// Specifies what type of item this is. These represent a set of flags, so they can be combined and masked using the | and &amp; operators respectively. I.E: 'ItemType.LOOT | ItemType.TIER1 | ItemType.ROCK'. Please note that EQUIPABLE is incompatible with LOOT, EMBLEM, USABLE, and CONSUMABLE, and attempts to combine them will result in unexpected results. Also note that the top 16 bits are flags for general use, and will be ignored by Gadget Core. They can be set using FLAG1-FLAG16
    /// </summary>
    [Flags]
    public enum ItemType : uint
    {
        /// <summary>
        /// There is absolutely nothing special about this item. It can't be used or equipped, and it isn't any sort of loot or other tiered item.
        /// </summary>
        GENERIC     = 0b0000000000000000,
        /// <summary>
        /// This item is loot, as is used in the emblem forge and the alchemy table. Note that a tier and loot type should also be set. Incompatible with EQUIPABLE.
        /// </summary>
        LOOT        = 0b0000000000000010,
        /// <summary>
        /// This item is an emblem, as is used in the gear forge and creation machine. Note that a tier and loot type should also be set. Incompatible with EQUIPABLE.
        /// </summary>
        EMBLEM      = 0b0000000000000011,
        /// <summary>
        /// This item can be used from the hotbar, and doing so will invoke OnUse. Incompatible with EQUIPABLE.
        /// </summary>
        USABLE      = 0b0000000000000100,
        /// <summary>
        /// When this item is used, one of it should be consumed. Implies USABLE.
        /// </summary>
        CONSUMABLE  = 0b0000000000000101,
        /// <summary>
        /// This item can be equipped. Note that this alone does not actually make it equipable, since no equip slot will take it. Incompatible with LOOT, EMBLEM, USABLE, and CONSUMABLE.
        /// </summary>
        EQUIPABLE   = 0b0000000000001000,
        /// <summary>
        /// This item is a weapon, and can as such can be equipped to the weapon slot. Implies EQUIPABLE, NONSTACKING, LEVELING, and MODABLE. Use BASIC_MASK to strip all but the EQUIPABLE implication.
        /// </summary>
        WEAPON      = 0b0000000001111000,
        /// <summary>
        /// This item is an offhand, and can as such can be equipped to the offhand slot. Implies EQUIPABLE, NONSTACKING, LEVELING, and MODABLE. Use BASIC_MASK to strip all but the EQUIPABLE implication.
        /// </summary>
        OFFHAND     = 0b0000000001111001,
        /// <summary>
        /// This item is a helmet, and can as such can be equipped to the helmet slot. Implies EQUIPABLE, NONSTACKING, LEVELING, and MODABLE. Use BASIC_MASK to strip all but the EQUIPABLE implication.
        /// </summary>
        HELMET      = 0b0000000001111010,
        /// <summary>
        /// This item is an armor, and can as such can be equipped to the armor slot. Implies EQUIPABLE, NONSTACKING, LEVELING, and MODABLE. Use BASIC_MASK to strip all but the EQUIPABLE implication.
        /// </summary>
        ARMOR       = 0b0000000001111011,
        /// <summary>
        /// This item is a ring, and can as such can be equipped to a ring slot. Implies EQUIPABLE, NONSTACKING, LEVELING, and MODABLE. Use BASIC_MASK to strip all but the EQUIPABLE implication.
        /// </summary>
        RING        = 0b0000000001111100,
        /// <summary>
        /// This item is a droid, and can as such can be equipped to a droid slot. Implies EQUIPABLE, NONSTACKING, and LEVELING. Use BASIC_MASK to strip all but the EQUIPABLE implication.
        /// </summary>
        DROID       = 0b0000000000111101,
        /// <summary>
        /// This item is a gear mod, and as such can be installed into weapons, offhands, helmets, armors, and rings. Note that this is non-functional in this version of Gadget Core.
        /// </summary>
        MOD         = 0b0000000010000000,

        /// <summary>
        /// This item can stack. Note that this flag is meaningless to set, and is only intended to be used for querying as such: ({Item Type} &amp; ItemType.NONSTACKING) == ItemType.STACKING
        /// </summary>
        STACKING    = 0b0000000000000000,
        /// <summary>
        /// This item cannot stack. If an equipable item does not have this flag set, unexpected behavior may occur.
        /// </summary>
        NONSTACKING = 0b0000000000010000,
        /// <summary>
        /// This item is able to level up. Also causes the item to show the background that displays the item's rarity tier. If a leveling item is able to stack, unexpected behavior may occur.
        /// </summary>
        LEVELING    = 0b0000000000100000,
        /// <summary>
        /// This item is able to have mods installed into it. If a modable item is able to stack, unexpected behavoir may occur. Note that this is non-functional in this version of Gadget Core.
        /// </summary>
        MODABLE     = 0b0000000001000000,

        /// <summary>
        /// This item is tier 1. This is meaningless if the item is not either a LOOT or an EMBLEM.
        /// </summary>
        TIER1       = 0b0000000100000000,
        /// <summary>
        /// This item is tier 2. This is meaningless if the item is not either a LOOT or an EMBLEM.
        /// </summary>
        TIER2       = 0b0000001000000000,
        /// <summary>
        /// This item is tier 3. This is meaningless if the item is not either a LOOT or an EMBLEM.
        /// </summary>
        TIER3       = 0b0000001100000000,
        /// <summary>
        /// This item is tier 4. This is meaningless if the item is not either a LOOT or an EMBLEM.
        /// </summary>
        TIER4       = 0b0000010000000000,
        /// <summary>
        /// This item is tier 5. This is meaningless if the item is not either a LOOT or an EMBLEM.
        /// </summary>
        TIER5       = 0b0000010100000000,
        /// <summary>
        /// This item is tier 6. This is meaningless if the item is not either a LOOT or an EMBLEM.
        /// </summary>
        TIER6       = 0b0000011000000000,
        /// <summary>
        /// This item is tier 7. This is meaningless if the item is not either a LOOT or an EMBLEM.
        /// </summary>
        TIER7       = 0b0000011100000000,
        /// <summary>
        /// This item is tier 8. This is meaningless if the item is not either a LOOT or an EMBLEM.
        /// </summary>
        TIER8       = 0b0000100000000000,
        /// <summary>
        /// This item is tier 9. This is meaningless if the item is not either a LOOT or an EMBLEM.
        /// </summary>
        TIER9       = 0b0000100100000000,
        /// <summary>
        /// This item is tier 10. This is meaningless if the item is not either a LOOT or an EMBLEM.
        /// </summary>
        TIER10      = 0b0000000000000000,

        /// <summary>
        /// This item is a rock material. This is meaningless if the item is not either a LOOT or an EMBLEM.
        /// </summary>
        ROCK        = 0b0000000000000000,
        /// <summary>
        /// This item is a plant material. This is meaningless if the item is not either a LOOT or an EMBLEM. Implies ORGANIC.
        /// </summary>
        PLANT       = 0b0101000000000000,
        /// <summary>
        /// This item is a monster material. This is meaningless if the item is not either a LOOT or an EMBLEM. Implies ORGANIC.
        /// </summary>
        MONSTER     = 0b0110000000000000,
        /// <summary>
        /// This item is a bug material. This is meaningless if the item is not either a LOOT or an EMBLEM. Implies ORGANIC.
        /// </summary>
        BUG         = 0b0111000000000000,
        /// <summary>
        /// This item is some other form of material besides the standard four. This is meaningless if the item is not either a LOOT or an EMBLEM.
        /// </summary>
        OTHER       = 0b1000000000000000,

        /// <summary>
        /// This item is considered to be an organic material, and as such can be placed into the alchemy station if it is also a LOOT.
        /// </summary>
        ORGANIC     = 0b0100000000000000,

        /// <summary>
        /// Generic flag 1. Gadget Core ignores this, so it can be used for whatever you wish.
        /// </summary>
        FLAG1       = 0b00000000000000010000000000000000,
        /// <summary>
        /// Generic flag 2. Gadget Core ignores this, so it can be used for whatever you wish.
        /// </summary>
        FLAG2       = 0b00000000000000100000000000000000,
        /// <summary>
        /// Generic flag 3. Gadget Core ignores this, so it can be used for whatever you wish.
        /// </summary>
        FLAG3       = 0b00000000000001000000000000000000,
        /// <summary>
        /// Generic flag 4. Gadget Core ignores this, so it can be used for whatever you wish.
        /// </summary>
        FLAG4       = 0b00000000000010000000000000000000,
        /// <summary>
        /// Generic flag 5. Gadget Core ignores this, so it can be used for whatever you wish.
        /// </summary>
        FLAG5       = 0b00000000000100000000000000000000,
        /// <summary>
        /// Generic flag 6. Gadget Core ignores this, so it can be used for whatever you wish.
        /// </summary>
        FLAG6       = 0b00000000001000000000000000000000,
        /// <summary>
        /// Generic flag 7. Gadget Core ignores this, so it can be used for whatever you wish.
        /// </summary>
        FLAG7       = 0b00000000010000000000000000000000,
        /// <summary>
        /// Generic flag 8. Gadget Core ignores this, so it can be used for whatever you wish.
        /// </summary>
        FLAG8       = 0b00000000100000000000000000000000,
        /// <summary>
        /// Generic flag 9. Gadget Core ignores this, so it can be used for whatever you wish.
        /// </summary>
        FLAG9       = 0b00000001000000000000000000000000,
        /// <summary>
        /// Generic flag 10. Gadget Core ignores this, so it can be used for whatever you wish.
        /// </summary>
        FLAG10      = 0b00000010000000000000000000000000,
        /// <summary>
        /// Generic flag 11. Gadget Core ignores this, so it can be used for whatever you wish.
        /// </summary>
        FLAG11      = 0b00000100000000000000000000000000,
        /// <summary>
        /// Generic flag 12. Gadget Core ignores this, so it can be used for whatever you wish.
        /// </summary>
        FLAG12      = 0b00001000000000000000000000000000,
        /// <summary>
        /// Generic flag 13. Gadget Core ignores this, so it can be used for whatever you wish.
        /// </summary>
        FLAG13      = 0b00010000000000000000000000000000,
        /// <summary>
        /// Generic flag 14. Gadget Core ignores this, so it can be used for whatever you wish.
        /// </summary>
        FLAG14      = 0b00100000000000000000000000000000,
        /// <summary>
        /// Generic flag 15. Gadget Core ignores this, so it can be used for whatever you wish.
        /// </summary>
        FLAG15      = 0b01000000000000000000000000000000,
        /// <summary>
        /// Generic flag 16. Gadget Core ignores this, so it can be used for whatever you wish.
        /// </summary>
        FLAG16      = 0b10000000000000000000000000000000,

        /// <summary>
        /// A bitmask that filters out the LOOT, EMBLEM, USABLE, CONSUMABLE, and EQUIPABLE flags.
        /// </summary>
        BASIC_MASK  = 0b0000000000001111,
        /// <summary>
        /// A bitmask that filters out the NONSTACKING, LEVELING, MODABLE, and MOD flags.
        /// </summary>
        TYPE_MASK   = 0b0000000011110000,
        /// <summary>
        /// A bitmask that filters out the TIER1* flags.
        /// </summary>
        TIER_MASK   = 0b0000111100000000,
        /// <summary>
        /// A bitmask that filters out the ROCK, PLANT, MONSTER, BUG, OTHER, and ORGANIC flags.
        /// </summary>
        LOOT_MASK   = 0b1111000000000000,
        /// <summary>
        /// A bitmask that filters out the FLAG* generic flags.
        /// </summary>
        FLAGS_MASK  = 0b11111111111111110000000000000000
    }
}
