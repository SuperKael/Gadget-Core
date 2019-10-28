using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GadgetCore.API
{
    public class ItemRegistry : Registry<ItemRegistry, ItemInfo, ItemType>
    {
        public override string GetRegistryName()
        {
            return "Item";
        }

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
    }

    [Flags]
    public enum ItemType : uint
    {
        GENERIC     = 0b0000000000000000,
        LOOT        = 0b0000000000000010,
        EMBLEM      = 0b0000000000000011,
        USABLE      = 0b0000000000000100,
        CONSUMABLE  = 0b0000000000000101,
        EQUIPABLE   = 0b0000000000111000,
        WEAPON      = 0b0000000001111000,
        OFFHAND     = 0b0000000001111001,
        HELMET      = 0b0000000001111010,
        ARMOR       = 0b0000000001111011,
        RING        = 0b0000000001111100,
        DROID       = 0b0000000000111101,
        MOD         = 0b0000000010000000,

        STACKING    = 0b0000000000000000,
        NONSTACKING = 0b0000000000010000,
        LEVELING    = 0b0000000000111000,
        MODABLE     = 0b0000000001011000,

        TIER1       = 0b0000000100000000,
        TIER2       = 0b0000001000000000,
        TIER3       = 0b0000001100000000,
        TIER4       = 0b0000010000000000,
        TIER5       = 0b0000010100000000,
        TIER6       = 0b0000011000000000,
        TIER7       = 0b0000011100000000,
        TIER8       = 0b0000100000000000,
        TIER9       = 0b0000100100000000,
        TIER10      = 0b0000000000000000,

        ROCK        = 0b0001000000000000,
        PLANT       = 0b0101000000000000,
        MONSTER     = 0b0110000000000000,
        BUG         = 0b0111000000000000,
        OTHER       = 0b1000000000000000,

        ORGANIC     = 0b0100000000000000,

        FLAG1       = 0b00000000000000010000000000000000,
        FLAG2       = 0b00000000000000100000000000000000,
        FLAG3       = 0b00000000000001000000000000000000,
        FLAG4       = 0b00000000000010000000000000000000,
        FLAG5       = 0b00000000000100000000000000000000,
        FLAG6       = 0b00000000001000000000000000000000,
        FLAG7       = 0b00000000010000000000000000000000,
        FLAG8       = 0b00000000100000000000000000000000,
        FLAG9       = 0b00000001000000000000000000000000,
        FLAG10      = 0b00000010000000000000000000000000,
        FLAG11      = 0b00000100000000000000000000000000,
        FLAG12      = 0b00001000000000000000000000000000,
        FLAG13      = 0b00010000000000000000000000000000,
        FLAG14      = 0b00100000000000000000000000000000,
        FLAG15      = 0b01000000000000000000000000000000,
        FLAG16      = 0b10000000000000000000000000000000,

        EQUIPMENT_F = 0b0000000000001000,

        BASIC_MASK  = 0b0000000000001111,
        TYPE_MASK   = 0b0000000011110000,
        TIER_MASK   = 0b0000111100000000,
        LOOT_MASK   = 0b1111000000000000,
        FLAGS_MASK  = 0b11111111111111110000000000000000
    }
}
