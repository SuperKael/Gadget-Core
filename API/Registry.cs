using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GadgetCore.API
{
    public abstract class Registry<R, E, T> : Registry<E, T> where R : Registry<R, E, T>, new() where E : RegistryEntry<E, T> where T : Enum
    {
        protected static Registry<R, E, T> singleton { get; } = new R();
        protected static Dictionary<T, int> lastUsedIDs = new Dictionary<T, int>();
        protected readonly Dictionary<int, E> registry = new Dictionary<int, E>();

        internal static int RegisterStatic(E entry, int preferredID = -1, bool overrideExisting = true)
        {
            if (!registeringVanilla && modRegistering < 0)
            {
                throw new InvalidOperationException("Data registration may only be performed by the Initialize method of a GadgetMod!");
            }
            T typeEnum = entry.GetEntryTypeEnum();
            int id = lastUsedIDs.ContainsKey(typeEnum) ? lastUsedIDs[typeEnum] : -1;
            if (preferredID < 0 || (!overrideExisting && singleton.HasEntry(preferredID)) || !entry.IsValidIDForType(preferredID))
            {
                if (!overrideExisting && preferredID >= 0) return -1;
                do id = entry.GetNextIDForType(id); while (id >= 0 && singleton.HasEntry(id));
                if (id < 0) return id;
            }
            else
            {
                id = preferredID;
            }
            entry.ModID = modRegistering;
            entry.ID = id;
            singleton.registry.Add(id, entry);
            lastUsedIDs[typeEnum] = id;
            singleton.PostRegistration(entry);
            entry.PostRegister();
            return id;
        }

        public override int Register(E entry, int preferredID = -1, bool overrideExisting = true)
        {
            return RegisterStatic(entry, preferredID, overrideExisting);
        }

        protected virtual void PostRegistration(E entry) { }

        internal static void RegisterVanilla(E entry, int id)
        {
            T typeEnum = entry.GetEntryTypeEnum();
            singleton.registry.Add(id, entry);
            lastUsedIDs[typeEnum] = id;
        }

        public static Registry<R, E, T> GetSingleton()
        {
            return singleton;
        }

        public bool HasEntry(int id)
        {
            return registry.ContainsKey(id);
        }

        public E GetEntry(int id)
        {
            return HasEntry(id) ? registry[id] : null;
        }

        public sealed override Type GetEntryType()
        {
            return typeof(E);
        }
    }

    public abstract class Registry<E, T> : Registry
    {
        public abstract int Register(E entry, int preferredID = -1, bool overrideExisting = true);
    }

    public abstract class Registry
    {
        internal static bool registeringVanilla = false;
        internal static int modRegistering = -1;

        public abstract Type GetEntryType();
        public abstract string GetRegistryName();
    }
}
