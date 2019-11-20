using System;
using System.Collections;
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
        public static ItemType GetDefaultTypeByID(int ID)
        {
            if (ID > 0 && ID <= 40)
            {
                if (ID <= 10) return ItemType.LOOT | ItemType.ROCK;
                if (ID <= 20) return ItemType.LOOT | ItemType.PLANT;
                if (ID <= 30) return ItemType.LOOT | ItemType.MONSTER;
                return ItemType.LOOT | ItemType.BUG;
            }
            if (ID > 100 && ID <= 200)
            {
                if (ID <= 110) return ItemType.EMBLEM | ItemType.ROCK;
                if (ID <= 120) return ItemType.EMBLEM | ItemType.PLANT;
                if (ID <= 130) return ItemType.EMBLEM | ItemType.MONSTER;
                if (ID <= 140) return ItemType.EMBLEM | ItemType.BUG;
                return ItemType.EMBLEM | ItemType.OTHER;
            }
            if (ID >= 300 && ID < 600) return ItemType.WEAPON;
            if (ID >= 600 && ID < 700) return ItemType.OFFHAND;
            if (ID >= 700 && ID < 800) return ItemType.HELMET;
            if (ID >= 800 && ID < 900) return ItemType.ARMOR;
            if (ID >= 900 && ID < 1000) return ItemType.RING;
            if (ID >= 1000 && ID < 2000) return ItemType.DROID;
            switch (ID)
            {
                case 41:
                    return ItemType.CONSUMABLE;
                case 44:
                    return ItemType.CONSUMABLE;
                case 46:
                    return ItemType.CONSUMABLE;
                case 47:
                    return ItemType.CONSUMABLE;
                case 48:
                    return ItemType.CONSUMABLE;
                case 50:
                    return ItemType.CONSUMABLE;
                case 54:
                    return ItemType.CONSUMABLE;
                case 55:
                    return ItemType.CONSUMABLE;
                case 56:
                    return ItemType.CONSUMABLE;
                case 58:
                    return ItemType.CONSUMABLE;
                case 60:
                    return ItemType.CONSUMABLE;
                case 61:
                    return ItemType.CONSUMABLE;
                case 62:
                    return ItemType.CONSUMABLE;
                case 63:
                    return ItemType.CONSUMABLE;
                case 64:
                    return ItemType.CONSUMABLE;
                case 65:
                    return ItemType.CONSUMABLE;
                case 66:
                    return ItemType.CONSUMABLE;
                case 67:
                    return ItemType.CONSUMABLE;
                case 68:
                    return ItemType.CONSUMABLE;
                case 69:
                    return ItemType.CONSUMABLE;
                case 70:
                    return ItemType.CONSUMABLE;
                case 71:
                    return ItemType.CONSUMABLE;
                case 72:
                    return ItemType.CONSUMABLE;
                case 73:
                    return ItemType.CONSUMABLE;
                case 74:
                    return ItemType.CONSUMABLE;
                default:
                    return ItemType.GENERIC;
            }
        }

        /// <summary>
        /// Gets the default weapon scaling of the given ID, assuming it is the ID of a vanilla weapon. Does not include any special-case scaling that varies depending on external factors such as the player's current health.
        /// </summary>
        public static float[] GetDefaultWeaponScalingByID(int ID)
        {
            float[] scaling = new float[6];
            if (ID < 300 || ID >= 600) return scaling;
            else if (ID < 400) scaling[1] += 1;
            else if (ID < 500) scaling[2] += 1;
            else if (ID < 550) scaling[4] += 1;
            else if (ID < 600) scaling[5] += 0.25f;
            switch (ID)
            {
                case 314:
                    scaling[2] += 1;
                    break;
                case 316:
                    scaling[3] += 1;
                    break;
                case 319:
                    scaling[4] += 0.5f;
                    break;
                case 324:
                    scaling[3] += 0.5f;
                    break;
                case 328:
                    scaling[0] += 1;
                    break;
                case 329:
                    scaling[2] += 0.5f;
                    break;
                case 362:
                    scaling[2] += 0.5f;
                    break;
                case 363:
                    scaling[2] += 2;
                    break;
                case 364:
                    scaling[2] += 1;
                    break;
                case 367:
                    scaling[2] += 0.5f;
                    scaling[5] += 0.5f;
                    break;
                case 369:
                    scaling[4] += 0.5f;
                    break;
                case 372:
                    scaling[0] += 1f / 3f;
                    break;
                case 373:
                    scaling[0] += 0.5f;
                    break;
                case 377:
                    scaling[2] += 0.5f;
                    break;
                case 378:
                    scaling[2] += 0.5f;
                    break;
                case 412:
                    scaling[3] += 0.5f;
                    break;
                case 417:
                    scaling[3] += 1;
                    break;
                case 418:
                    scaling[4] += 0.5f;
                    break;
                case 420:
                    scaling[4] += 0.5f;
                    break;
                case 421:
                    scaling[3] += 1;
                    break;
                case 423:
                    scaling[0] += 0.5f;
                    break;
                case 425:
                    scaling[2] += 1;
                    break;
                case 426:
                    scaling[3] += 0.5f;
                    break;
                case 427:
                    scaling[5] += 0.5f;
                    break;
                case 429:
                    scaling[4] += 0.5f;
                    break;
                case 462:
                    scaling[0] += 1;
                    break;
                case 463:
                    scaling[0] += 0.5f;
                    break;
                case 464:
                    scaling[3] += 1;
                    break;
                case 465:
                    scaling[4] += 1;
                    break;
                case 466:
                    scaling[4] += 1;
                    break;
                case 467:
                    scaling[4] += 0.5f;
                    break;
                case 468:
                    scaling[0] += 1;
                    scaling[3] += 1;
                    break;
                case 469:
                    scaling[1] += 0.5f;
                    break;
                case 470:
                    scaling[1] += 1;
                    break;
                case 471:
                    scaling[3] += 0.5f;
                    scaling[0] += 0.5f;
                    break;
                case 472:
                    scaling[0] += 1;
                    scaling[4] += 1;
                    break;
                case 474:
                    scaling[3] += 0.5f;
                    break;
                case 475:
                    scaling[5] += 0.5f;
                    break;
                case 476:
                    scaling[0] += 0.5f;
                    break;
                case 477:
                    scaling[4] += 0.5f;
                    break;
                case 478:
                    scaling[4] += 0.5f;
                    break;
                case 495:
                    scaling[3] += 1;
                    break;
                case 512:
                    scaling[5] += 1;
                    break;
                case 513:
                    scaling[0] += 0.5f;
                    break;
                case 514:
                    scaling[5] += 0.5f;
                    break;
                case 516:
                    scaling[4] += 1;
                    break;
                case 517:
                    scaling[2] += 0.5f;
                    break;
                case 518:
                    scaling[5] += 0.5f;
                    break;
                case 519:
                    scaling[3] += 0.5f;
                    break;
                case 520:
                    scaling[5] += 1;
                    break;
                case 521:
                    scaling[2] += 1;
                    break;
                case 522:
                    scaling[3] += 1;
                    scaling[2] += 1;
                    break;
                case 523:
                    scaling[1] += 1;
                    break;
                case 524:
                    scaling[5] += 0.5f;
                    break;
                case 525:
                    scaling[1] += 0.5f;
                    break;
                case 526:
                    scaling[0] += 1;
                    scaling[3] += 1;
                    break;
                case 527:
                    scaling[2] += 1;
                    break;
                case 528:
                    scaling[2] += 0.5f;
                    break;
                case 529:
                    scaling[4] += 2;
                    break;
                case 549:
                    scaling[4] += 1;
                    break;
                case 562:
                    scaling[0] += 0.5f;
                    break;
                case 563:
                    scaling[4] += 0.5f;
                    break;
                case 565:
                    scaling[3] += 0.5f;
                    scaling[4] += 0.5f;
                    break;
                case 566:
                    scaling[4] += 0.5f;
                    break;
                case 567:
                    scaling[1] += 0.5f;
                    break;
                case 568:
                    scaling[0] += 0.5f;
                    break;
                case 569:
                    scaling[0] += 0.5f;
                    break;
                case 571:
                    scaling[3] += 0.5f;
                    break;
                case 572:
                    scaling[3] += 0.5f;
                    break;
                case 573:
                    scaling[0] += 1;
                    break;
                case 574:
                    scaling[4] += 0.5f;
                    break;
                case 575:
                    scaling[1] += 0.5f;
                    break;
                case 576:
                    scaling[4] += 0.25f;
                    break;
                case 577:
                    scaling[0] += 0.5f;
                    scaling[4] += 0.5f;
                    break;
                case 578:
                    scaling[4] += 0.5f;
                    break;
                case 579:
                    scaling[4] += 0.5f;
                    scaling[0] += 0.5f;
                    break;
            }
            return scaling;
        }

        /// <summary>
        /// Gets the default weapon crit chance bonus of the given ID, assuming it is the ID of a vanilla weapon. The returned value is a percentage, I.E., 5 for a 5% bonus.
        /// </summary>
        public static int GetDefaultCritChanceBonus(int ID)
        {
            switch (ID)
            {
                case 369:
                    return 10;
                case 372:
                    return 20;
                case 423:
                    return 10;
                case 475:
                    return 10;
                case 518:
                    return 25;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Gets the default weapon crit power bonus of the given ID, assuming it is the ID of a vanilla weapon. The returned value is an additive multiplier, I.E., 0.5 for a 50% bonus.
        /// </summary>
        public static float GetDefaultCritPowerBonus(int ID)
        {
            switch (ID)
            {
                case 372:
                    return 0.5f;
                case 417:
                    return 0.5f;
                case 423:
                    return 0.5f;
                case 475:
                    return 0.5f;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Gets the type of the specified ID. Applies to vanilla items as well as Gadget items.
        /// </summary>
        public static ItemType GetTypeByID(int ID)
        {
            ItemInfo itemInfo = GetSingleton().GetEntry(ID);
            return itemInfo != null ? itemInfo.Type : GetDefaultTypeByID(ID);
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
