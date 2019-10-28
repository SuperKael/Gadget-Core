using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GadgetCore.API
{
    public abstract class RegistryEntry<E, T> where E : RegistryEntry<E, T> where T : Enum
    {
        internal int ModID;
        internal int ID = -1;

        public GadgetMod GetMod()
        {
            return GadgetMods.GetMod(ModID);
        }

        public int GetModID()
        {
            return ModID;
        }

        public int GetID()
        {
            return ID;
        }

        protected RegistryEntry<E, T> RegisterInternal(int preferredID = -1, bool overrideExisting = true)
        {
            GetRegistry().Register(this as E, preferredID, overrideExisting);
            return this;
        }

        public virtual void PostRegister() { }
        public abstract T GetEntryTypeEnum();
        public abstract Registry<E, T> GetRegistry();

        public abstract bool IsValidIDForType(int id);
        public abstract int GetNextIDForType(int lastValidID);
    }
}
