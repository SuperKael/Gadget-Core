using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GadgetCore.API
{
    public static class GameRegistry
    {
        private static Dictionary<Type, Registry> registries = new Dictionary<Type, Registry>();

        internal static void RegisterRegistry(Registry registry)
        {
            GadgetCore.Log("Initializing " + registry.GetRegistryName() + " Registry");
            registries.Add(registry.GetEntryType(), registry);
        }

        public static Registry GetRegistry(Type entryType)
        {
            return registries[entryType];
        }
    }
}
