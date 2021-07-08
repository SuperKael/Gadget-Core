using GadgetCore.Loader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GadgetCore.API
{
    /// <summary>
    /// Represents a registry filled with <see cref="RegistryEntry{E, T}"/>s. You can extend this to create your own registries, and then you should return their singletons in <see cref="Gadget.CreateRegistries"/>
    /// </summary>
    /// <typeparam name="R">Registry Type</typeparam>
    /// <typeparam name="E">Entry Type</typeparam>
    /// <typeparam name="T">Entry Type Enum</typeparam>
    public abstract class Registry<R, E, T> : Registry<E, T>, IEnumerable<E> where R : Registry<R, E, T>, new() where E : RegistryEntry<E, T> where T : Enum
    {
        /// <summary>
        /// Represents this Registry's singleton.
        /// </summary>
        public static Registry<R, E, T> Singleton { get; } = new R();

        private static readonly Dictionary<T, int> lastUsedIDs = new Dictionary<T, int>();
        private readonly Dictionary<int, E> IDRegistry = new Dictionary<int, E>();
        private readonly Dictionary<string, E> NameRegistry = new Dictionary<string, E>();

        internal static int RegisterStatic(E entry, string name, int preferredID = -1, bool overrideExisting = true)
        {
            if (!registeringVanilla && gadgetRegistering < 0)
            {
                throw new InvalidOperationException("Data registration may only be performed by the Initialize method of a Gadget!");
            }
            if (!entry.ReadyToRegister())
            {
                throw new InvalidOperationException("This registry entry is not yet ready to be registered, or has already been registered!");
            }
            string modNamePrefix = Gadgets.GetGadgetInfo(gadgetRegistering).Attribute.Name + ":";
            bool hasPrefix = false;
            if (name != null)
            {
                hasPrefix = name.StartsWith(modNamePrefix);
                if (!(hasPrefix ? name.Substring(modNamePrefix.Length) : name).All(x => char.IsLetterOrDigit(x) || x == ' ')) throw new InvalidOperationException("Registry name must be alphanumeric!");
            }
            if (name == null) name = overrideExisting && Singleton.HasEntry(preferredID) ? Singleton[preferredID].RegistryName.Substring(Singleton[preferredID].RegistryName.IndexOf(':') + 1) : Singleton.GenerateDefaultRegistryName(preferredID);
            if (name == null) throw new InvalidOperationException("Must provide a registry name!");
            string registryName = hasPrefix ? name : modNamePrefix + name;
            int reservedID = Singleton.GetReservedID(registryName);
            if (reservedID >= 0 && preferredID < 0)
            {
                if (!Singleton.HasEntry(reservedID))
                {
                    preferredID = reservedID;
                }
                else
                {
                    GadgetCore.CoreLogger.LogWarning("Duplicate reserved " + Singleton.GetRegistryName() + " ID used by both `" + Singleton[reservedID].RegistryName + "` and `" +
                        (registeringVanilla ? "Roguelands" : Gadgets.GetGadgetInfo(gadgetRegistering).Attribute.Name) + ":" + name + "`. The later has been allocated a new ID - " +
                        "any instances of the later " + Singleton.GetRegistryName() + " in your save file will now load as the former.");
                    Singleton.reservedIDs.Remove(registryName);
                }
            }
            T typeEnum = entry.GetEntryType();
            int id = lastUsedIDs.ContainsKey(typeEnum) ? lastUsedIDs[typeEnum] : -1;
            if (preferredID < 0 || (!overrideExisting && Singleton.HasEntry(preferredID)) || !entry.IsValidIDForType(preferredID))
            {
                if (!overrideExisting && preferredID >= 0) return -1;
                do id = entry.GetNextIDForType(id); while (id >= 0 && Singleton.HasEntry(id) || Singleton.IsIDReserved(id) != null);
                if (id < 0) return id;
            }
            else
            {
                id = preferredID;
            }
            entry.ModID = gadgetRegistering;
            entry.ID = id;
            if (name == preferredID.ToString()) name = id.ToString();
            entry.RegistryName = (registeringVanilla ? "Roguelands" : Gadgets.GetGadgetInfo(gadgetRegistering).Attribute.Name) + ":" + name;
            Singleton.IDRegistry[entry.ID] = entry;
            Singleton.NameRegistry[entry.RegistryName] = entry;
            if (!registeringVanilla) Singleton.reservedIDs[entry.RegistryName] = id;
            lastUsedIDs[typeEnum] = id;
            Singleton.PostRegistration(entry);
            entry.PostRegister();
            return id;
        }

        /// <summary>
        /// Checks if a given Registry ID matches the given Entry type. Optionally applies a flags mask to both the entry's type and the given type.
        /// </summary>
        public static bool IDMatchesType(int id, T type, T mask = default)
        {
            T entryType = Singleton.GetEntry(id).GetEntryType();
            if (!EqualityComparer<T>.Default.Equals(mask, default))
            {
                MethodInfo maskAndOp = typeof(T).GetMethod("op_BitwiseAnd", new Type[] { typeof(T), typeof(T) });
                type = (T)maskAndOp.Invoke(null, new object[] { type, mask });
                entryType = (T)maskAndOp.Invoke(null, new object[] { entryType, mask });
            }
            return EqualityComparer<T>.Default.Equals(type, entryType);
        }

        /// <summary>
        /// Used to register a registry entry to this registry. You should probably use the Register method on that registry entry instead.
        /// </summary>
        /// <param name="entry">The RegistryEntry to register.</param>
        /// <param name="name">The registry name to use.</param>
        /// <param name="preferredID">If specified, will use this registry ID.</param>
        /// <param name="overrideExisting">If false, will not register if the preferred ID is already used. Ignored if no preferred ID is specified.</param>
        public override int Register(E entry, string name, int preferredID = -1, bool overrideExisting = true)
        {
            return RegisterStatic(entry, name, preferredID, overrideExisting);
        }

        /// <summary>
        /// Used to provide a default name if a registry entry is registered without a registry name.
        /// </summary>
        protected virtual string GenerateDefaultRegistryName(int ID)
        {
            return ID >= 0 ? ID.ToString() : null;
        }

        /// <summary>
        /// Called after the specified Registry Entry has been registered. You should never call this yourself. Note that this is called before <see cref="RegistryEntry{E, T}.PostRegister"/>
        /// </summary>
        protected virtual void PostRegistration(E entry) { }

        internal sealed override void UnregisterGadget(GadgetInfo gadget)
        {
            foreach (KeyValuePair<int, E> entry in Singleton.IDRegistry.Where(x => x.Value.RegistryName.Split(':')[0] == gadget.Attribute.Name).ToList())
            {
                OnUnregister(entry.Value);
                Singleton.IDRegistry.Remove(entry.Key);
            }
            foreach (KeyValuePair<string, E> entry in Singleton.NameRegistry.Where(x => x.Value.RegistryName.Split(':')[0] == gadget.Attribute.Name).ToList())
            {
                Singleton.NameRegistry.Remove(entry.Key);
            }
        }

        /// <summary>
        /// Called just before an entry is removed from the registry by <see cref="Registry.UnregisterGadget(GadgetInfo)"/>
        /// </summary>
        protected virtual void OnUnregister(E entry) { }

        /// <summary>
        /// Gets the singleton for this registry.
        /// Deprecated: Use the Singleton property instead.
        /// </summary>
        [Obsolete("GetSingleton has been deprecated, use the Singleton property instead.")]
        public static Registry<R, E, T> GetSingleton()
        {
            return Singleton;
        }

        /// <summary>
        /// Checks if the given ID is used in this registry.
        /// </summary>
        public bool HasEntry(int id)
        {
            return IDRegistry.ContainsKey(id);
        }

        /// <summary>
        /// Checks if the given RegistryName is used in this registry.
        /// </summary>
        public bool HasEntry(string name)
        {
            return NameRegistry.ContainsKey(name);
        }

        /// <summary>
        /// Gets the registry entry with the given ID
        /// </summary>
        public E GetEntry(int id)
        {
            return HasEntry(id) ? IDRegistry[id] : null;
        }

        /// <summary>
        /// Gets the registry entry with the given RegistryName
        /// </summary>
        public E GetEntry(string name)
        {
            return HasEntry(name) ? NameRegistry[name] : null;
        }

        /// <summary>
        /// Tries to get the registry entry with the given ID
        /// </summary>
        public bool TryGetEntry(int id, out E entry)
        {
            if (HasEntry(id))
            {
                entry = IDRegistry[id];
                return true;
            }
            else
            {
                entry = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to get the registry entry with the given RegistryName
        /// </summary>
        public bool TryGetEntry(string name, out E entry)
        {
            if (HasEntry(name))
            {
                entry = NameRegistry[name];
                return true;
            }
            else
            {
                entry = null;
                return false;
            }
        }

        /// <summary>
        /// Equivalent to calling <see cref="GetEntry(int)"/>
        /// </summary>
        public E this[int id]
        {
            get
            {
                return GetEntry(id);
            }

            set
            {
                Register(value, null, id);
            }
        }

        /// <summary>
        /// Equivalent to calling <see cref="GetEntry(string)"/>
        /// </summary>
        public E this[string name]
        {
            get
            {
                return GetEntry(name);
            }

            set
            {
                Register(value, name);
            }
        }

        /// <summary>
        /// Returns the Registry Entry's Type. Not to be confused with <see cref="RegistryEntry{E, T}.GetEntryType"/>, which returns an enum value.
        /// </summary>
        public sealed override Type GetEntryType()
        {
            return typeof(E);
        }

        /// <summary>
        /// Returns an array of every entry in this Registry
        /// </summary>
        public E[] GetAllEntries()
        {
            return IDRegistry.Values.ToArray();
        }

        /// <summary>
        /// Returns an enumerator for the entries in this Registry
        /// </summary>
        public IEnumerator<E> GetEnumerator()
        {
            return IDRegistry.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    /// Registry superclass without the self-referencing supertype. Do not extend this!
    /// </summary>
    /// <typeparam name="E">Entry Type</typeparam>
    /// <typeparam name="T">Entry Type Enum</typeparam>
    public abstract class Registry<E, T> : Registry where E : RegistryEntry<E, T> where T : Enum
    {
        /// <summary>
        /// Used to register a registry entry to this registry. You should probably use the Register method on that registry entry instead.
        /// </summary>
        /// <param name="entry">The RegistryEntry to register.</param>
        /// <param name="name">The registry name to use.</param>
        /// <param name="preferredID">If specified, will use this registry ID.</param>
        /// <param name="overrideExisting">If false, will not register if the preferred ID is already used. Ignored if no preferred ID is specified.</param>
        public abstract int Register(E entry, string name, int preferredID = -1, bool overrideExisting = true);
    }

    /// <summary>
    /// Registry superclass without any generics. Do not extend this!
    /// </summary>
    public abstract class Registry
    {
        internal Dictionary<string, int> reservedIDs;

        internal static bool registeringVanilla = false;
        internal static int gadgetRegistering = -1;

        /// <summary>
        /// Gets the reserved ID for the registry entry with the specified registry name. Returns -1 if there is no ID reserved for that registry name.
        /// </summary>
        /// <param name="name">The registry name, in the format ModName:RegistryEntry</param>
        public int GetReservedID(string name)
        {
            return reservedIDs.ContainsKey(name) ? reservedIDs[name] : -1;
        }

        /// <summary>
        /// Returns the name of the mod that reserved the given ID, or returns null if the ID is not reserved
        /// </summary>
        public string IsIDReserved(int id)
        {
            return reservedIDs.FirstOrDefault(x => x.Value == id).Key?.Split(':')?[0];
        }

        /// <summary>
        /// Returns the Registry Entry's Type. Not to be confused with <see cref="RegistryEntry{E, T}.GetEntryType"/>, which returns an enum value.
        /// </summary>
        public abstract Type GetEntryType();
        /// <summary>
        /// Gets the name of this registry. Must be constant.
        /// </summary>
        public abstract string GetRegistryName();
        /// <summary>
        /// Gets the ID that modded IDs should start at for this registry. May be 0 if the base game does not use IDs for this type of data.
        /// </summary>
        public virtual int GetIDStart()
        {
            return 10000;
        }

        internal abstract void UnregisterGadget(GadgetInfo gadget);
    }
}
