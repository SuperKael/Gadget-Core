using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GadgetCore.API
{
    public class ChipInfo : RegistryEntry<ChipInfo, ChipType>
    {
        public readonly ChipType type;

        public ChipInfo(ChipType type)
        {
            this.type = type;
        }

        public override ChipType GetEntryTypeEnum()
        {
            return type;
        }

        public override Registry GetRegistry()
        {
            return Registry<ChipRegistry, ChipInfo, ChipType>.GetSingleton();
        }

        public override bool IsValidIDForType(int id)
        {
            return true;
        }

        public override int GetNextIDForType(int lastValidID)
        {
            return lastValidID++;
        }
    }
}
