using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GadgetCore.API
{
    public abstract class Registry<R, E, T> : Registry where R : Registry<R, E, T>, new() where E : RegistryEntry<E, T> where T : Enum
    {
        protected static Registry<R, E, T> singleton { get; } = new R();
        protected static Dictionary<T, int> lastUsedIDs = new Dictionary<T, int>();
        protected readonly Dictionary<int, E> registry = new Dictionary<int, E>();

        public static int Register(E entry, int preferredID = -1)
        {
            T typeEnum = entry.GetEntryTypeEnum();
            int id = lastUsedIDs.ContainsKey(typeEnum) ? lastUsedIDs[typeEnum] : -1;
            if (preferredID < 0 || singleton.IsIDUsed(preferredID) || !entry.IsValidIDForType(preferredID))
            {
                do id = entry.GetNextIDForType(id); while (singleton.IsIDUsed(id));
            }
            else
            {
                id = preferredID;
            }
            entry.id = id;
            singleton.registry.Add(id, entry);
            lastUsedIDs[typeEnum] = id;
            singleton.PostRegistration(entry);
            return id;
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

        public bool IsIDUsed(int id)
        {
            return registry.ContainsKey(id);
        }

        public E GetEntry(int id)
        {
            return IsIDUsed(id) ? registry[id] : null;
        }

        public sealed override Type GetEntryType()
        {
            return typeof(E);
        }
    }

    public abstract class Registry
    {
        public abstract Type GetEntryType();
        public abstract string GetRegistryName();
    }
}
