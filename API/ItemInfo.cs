using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GadgetCore.API
{
    public class ItemInfo : RegistryEntry<ItemInfo, ItemType>
    {
        public const int ANY_TIER_MASK = 0x0F00;
        public readonly ItemType type;

        public ItemInfo(ItemType type)
        {
            this.type = type;
        }

        public override ItemType GetEntryTypeEnum()
        {
            return type;
        }

        public override Registry GetRegistry()
        {
            return Registry<ItemRegistry, ItemInfo, ItemType>.GetSingleton();
        }

        public override bool IsValidIDForType(int id)
        {
            int tier = ((int)type & ANY_TIER_MASK) >> 8;
            if (tier > 0)
            {
                return (id & 0x7) == (tier & 0x7);
            }
            return true;
        }

        public override int GetNextIDForType(int lastValidID)
        {
            int tier = ((int)type & ANY_TIER_MASK) >> 8;
            if (tier > 0)
            {
                int lastValidIDTier = lastValidID & 0x7;
                int shrunkenTier = tier & 0x7;
                if (shrunkenTier == lastValidIDTier)
                {
                    return lastValidID + 10;
                }
                else if (shrunkenTier > lastValidIDTier)
                {
                    return (lastValidID & ~0x7) | shrunkenTier;
                }
                else
                {
                    return ((lastValidID & ~0x7) | shrunkenTier) + 10;
                }
            }
            return lastValidID++;
        }
    }
}
