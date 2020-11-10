﻿using System;

namespace GadgetCore.API
{
    /// <summary>
    /// Abstract classed used for Registry Entries. Extend to create Registry Entries for your own custom Registry.
    /// </summary>
    /// <typeparam name="E">Entry Type</typeparam>
    /// <typeparam name="T">Entry Type Enum</typeparam>
    public abstract class RegistryEntry<E, T> where E : RegistryEntry<E, T> where T : Enum
    {
        internal int ModID = -1;
        internal int ID = -1;
        internal string RegistryName;

        /// <summary>
        /// Gets the mod that registered this Registry Entry. This will return null until the Registry Entry has been registered.
        /// </summary>
        public Gadget GetMod()
        {
            return ModID >= 0 ? Gadgets.GetGadget(ModID) : null;
        }
        
        /// <summary>
        /// Gets the Mod ID that registered this Registry Entry. This ID is the index of the mod as used in <see cref="Gadgets.GetGadget(int)"/>. This will be -1 until the Registry Entry has been registered.
        /// </summary>
        public int GetModID()
        {
            return ModID;
        }

        /// <summary>
        /// Gets the ID of this registry entry. This will be -1 until the Registry Entry has been registered.
        /// </summary>
        public int GetID()
        {
            return ID;
        }

        /// <summary>
        /// Gets the Registry Name of this item, in the format of ModName:ItemName
        /// </summary>
        public string GetRegistryName()
        {
            return RegistryName;
        }

        /// <summary>
        /// Use to register this RegistryEntry to its registry singleton.
        /// </summary>
        protected RegistryEntry<E, T> RegisterInternal(string name, int preferredID = -1, bool overrideExisting = true)
        {
            GetRegistry().Register(this as E, name, preferredID, overrideExisting);
            return this;
        }

        /// <summary>
        /// Called after this Registry Entry has been registered to its Registry. You should never call this yourself.
        /// </summary>
        protected internal virtual void PostRegister() { }
        /// <summary>
        /// Returns true if this Registry Entry is ready to be registered. Returns false if the registry entry is not yet in the correct state to be registered, or has already been registered.
        /// </summary>
        public virtual bool ReadyToRegister() { return RegistryName == null; }
        /// <summary>
        /// Returns the Registry Entry's Type. Used in the registration process.
        /// </summary>
        public abstract T GetEntryType();
        /// <summary>
        /// Returns the singleton of the registry used for storing this type of Registry Entry.
        /// </summary>
        public abstract Registry<E, T> GetRegistry();
        /// <summary>
        /// Returns whether the specified ID is valid for this Registry Entry's Type.
        /// </summary>
        public abstract bool IsValidIDForType(int id);
        /// <summary>
        /// Returns the next valid ID for this Registry Entry's Type, after the provided lastValidID.
        /// </summary>
        public abstract int GetNextIDForType(int lastValidID);
    }
}
