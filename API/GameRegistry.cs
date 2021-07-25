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
        private static Dictionary<string, Registry> registries = new Dictionary<string, Registry>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Registers a registry.
        /// </summary>
        public static void RegisterRegistry(Registry registry)
        {
            if (registries.Any(x => x.Value.GetRegistryName() == registry.GetRegistryName()))
            {
                GadgetCore.CoreLogger.Log("Skipping " + registry.GetRegistryName() + " Registry as a registry with that name is already registered.");
                return;
            }
            GadgetCore.CoreLogger.Log("Initializing " + registry.GetRegistryName() + " Registry");
            registries.Add(registry.GetRegistryName(), registry);
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
        /// Gets the registry with the specified name.
        /// </summary>
        public static Registry GetRegistry(string name)
        {
            return registries.TryGetValue(name, out Registry reg) ? reg : null;
        }

        /// <summary>
        /// Checks if a registry with the specified name is registered.
        /// </summary>
        public static bool IsRegistryRegistered(string name)
        {
            return registries.ContainsKey(name);
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
