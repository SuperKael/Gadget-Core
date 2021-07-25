using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GadgetCore.API
{
    /// <summary>
    /// Represents a registry filled with <see cref="UnaryTypedRegistryEntry{E}"/>s. You can extend this to create your own typeless registries, and then you should return their singletons in <see cref="Gadget.CreateRegistries"/>
    /// </summary>
    /// <typeparam name="R">Registry Type</typeparam>
    /// <typeparam name="E">Entry Type</typeparam>
    public abstract class UnaryTypedRegistry<R, E> : Registry<R, E, UnaryRegistryType>, IEnumerable<E> where R : UnaryTypedRegistry<R, E>, new() where E : UnaryTypedRegistryEntry<E>
    {

    }

    /// <summary>
    /// Abstract classed used for typeless Registry Entries. Extend to create Registry Entries for your own custom Registry.
    /// </summary>
    /// <typeparam name="E">Entry Type</typeparam>
    public abstract class UnaryTypedRegistryEntry<E> : RegistryEntry<E, UnaryRegistryType> where E : UnaryTypedRegistryEntry<E>
    {
        /// <summary>
        /// Returns whether the specified ID is valid for this Registry Entry's Type.
        /// </summary>
        public sealed override bool IsValidIDForType(int id)
        {
            return IsValidID(id);
        }

        /// <summary>
        /// Returns the next valid ID for this Registry Entry's Type, after the provided lastValidID.
        /// </summary>
        public sealed override int GetNextIDForType(int lastValidID)
        {
            return GetNextID(lastValidID);
        }

        /// <summary>
        /// Returns the Registry Entry's Type. Used in the registration process.
        /// </summary>
        public sealed override UnaryRegistryType GetEntryType()
        {
            return UnaryRegistryType.Type;
        }

        /// <summary>
        /// Returns whether the specified ID is valid for this Registry.
        /// </summary>
        public abstract bool IsValidID(int id);
        /// <summary>
        /// Returns the next valid ID for this Registry, after the provided lastValidID.
        /// </summary>
        public abstract int GetNextID(int lastValidID);
    }

    /// <summary>
    /// Enum containing only one value intended for use in typeless registries. A placeholder of sorts for backwards-compatibility reasons.
    /// </summary>
    public enum UnaryRegistryType
    {
        /// <summary>
        /// The one and only value for this enum.
        /// </summary>
        Type
    }
}
