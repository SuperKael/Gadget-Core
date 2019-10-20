using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GadgetCore.API
{
    public abstract class RegistryEntry<E, T> where E : RegistryEntry<E, T> where T : Enum
    {
        internal int id = -1;

        public int GetID()
        {
            return id;
        }

        public abstract T GetEntryTypeEnum();
        public abstract Registry GetRegistry();

        public abstract bool IsValidIDForType(int id);
        public abstract int GetNextIDForType(int lastValidID);
    }
}
