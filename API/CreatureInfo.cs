using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GadgetCore.API
{
    public class CreatureInfo : RegistryEntry<CreatureInfo, CreatureType>
    {
        public readonly CreatureType type;

        public CreatureInfo(CreatureType type)
        {
            this.type = type;
        }

        public override CreatureType GetEntryTypeEnum()
        {
            return type;
        }

        public override Registry GetRegistry()
        {
            return Registry<CreatureRegistry, CreatureInfo, CreatureType>.GetSingleton();
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
