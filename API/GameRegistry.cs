using PreviewLabs;
using System;
using System.Collections.Generic;
using System.Linq;

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
            if (registries.Any(x => x.Value.GetRegistryName() == registry.GetRegistryName()))
            {
                GadgetCore.Log("Skipping " + registry.GetRegistryName() + " Registry as a registry with that name is already registered.");
                return;
            }
            GadgetCore.Log("Initializing " + registry.GetRegistryName() + " Registry");
            registries.Add(registry.GetEntryType(), registry);
            registriesByName.Add(registry.GetRegistryName(), registry);
            if (registry.reservedIDs == null)
            {
                try
                {
                    registry.reservedIDs = PlayerPrefs.GetString("Reserved" + registry.GetRegistryName() + "IDs", "").Split(',').Select(x => x.Split('=')).ToDictionary(x => x[0], x => int.Parse(x[1]));
                }
                catch (IndexOutOfRangeException)
                {
                    registry.reservedIDs = new Dictionary<string, int>();
                }
            }
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
