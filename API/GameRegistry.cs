using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GadgetCore.API
{
    /// <summary>
    /// The master registry of all other registries.
    /// </summary>
    public static class GameRegistry
    {
        private static Dictionary<Type, Registry> registries = new Dictionary<Type, Registry>();
        private static Dictionary<string, Registry> registriesByName = new Dictionary<string, Registry>();

        internal static void RegisterRegistry(Registry registry)
        {
            GadgetCore.Log("Initializing " + registry.GetRegistryName() + " Registry");
            registries.Add(registry.GetEntryType(), registry);
            registriesByName.Add(registry.GetRegistryName(), registry);
        }

        /// <summary>
        /// Gets the registry with the specified entry type.
        /// </summary>
        public static Registry GetRegistry(Type entryType)
        {
            return registries[entryType];
        }

        /// <summary>
        /// Gets the registry with the specified name.
        /// </summary>
        public static Registry GetRegistry(string name)
        {
            return registriesByName[name];
        }

        /// <summary>
        /// Checks if a registry with the specified entry type is registered.
        /// </summary>
        public static bool IsRegistryRegistered(Type entryType)
        {
            return registries.ContainsKey(entryType);
        }

        /// <summary>
        /// Checks if a registry with the specified name is registered.
        /// </summary>
        public static bool IsRegistryRegistered(string name)
        {
            return registriesByName.ContainsKey(name);
        }

        /// <summary>
        /// Lists all registries.
        /// </summary>
        public static Registry[] ListAllRegistries()
        {
            return registries.Values.ToArray();
        }
    }
}
