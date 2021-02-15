using System;
using System.Collections.Generic;

namespace GadgetCore.API
{
    /// <summary>
    /// This registry is filled with ChipInfos, and is used for registering custom chips to the game.
    /// </summary>
    public class ChipRegistry : Registry<ChipRegistry, ChipInfo, ChipType>
    {
        private static Dictionary<string, int> chipIDsByName;
        private static Dictionary<string, int> chipIDsByRegistryName;

        /// <summary>
        /// The name of this registry.
        /// </summary>
        public const string REGISTRY_NAME = "Chip";

        static ChipRegistry()
        {
            InitializeVanillaChipIDNames();
        }

        /// <summary>
        /// Gets the name of this registry. Must be constant. Returns <see cref="REGISTRY_NAME"/>.
        /// </summary>
        public override string GetRegistryName()
        {
            return REGISTRY_NAME;
        }

        /// <summary>
        /// Gets the item ID for the given name. Case-insensitive. Returns -1 if there is no item with the given name.
        /// </summary>
        public static int GetChipIDByName(string name)
        {
            name = name.ToLowerInvariant();
            if (chipIDsByName.ContainsKey(name))
            {
                return chipIDsByName[name];
            }
            return -1;
        }

        /// <summary>
        /// Gets the item ID for the given registry name. Case-insensitive. Returns -1 if there is no item with the given name.
        /// </summary>
        public static int GetChipIDByRegistryName(string name)
        {
            name = name.ToLowerInvariant();
            if (chipIDsByRegistryName.ContainsKey(name))
            {
                return chipIDsByRegistryName[name];
            }
            return -1;
        }

        /// <summary>
        /// Called after the specified Registry Entry has been registered. You should never call this yourself. Note that this is called before <see cref="RegistryEntry{E, T}.PostRegister"/>
        /// </summary>
        protected override void PostRegistration(ChipInfo entry)
        {
            chipIDsByName[entry.Name] = entry.ID;
            chipIDsByRegistryName[entry.RegistryName] = entry.ID;
        }

        /// <summary>
        /// Called just before an entry is removed from the registry by <see cref="Registry.UnregisterGadget(GadgetInfo)"/>
        /// </summary>
        protected override void OnUnregister(ChipInfo entry)
        {
            chipIDsByName.Remove(entry.Name);
            chipIDsByRegistryName.Remove(entry.RegistryName);
        }

        /// <summary>
        /// Gets the ID that modded IDs should start at for this registry. <see cref="ChipRegistry"/> always returns 10000.
        /// </summary>
        public override int GetIDStart()
        {
            return 10000;
        }

        private static void InitializeVanillaChipIDNames()
        {
            chipIDsByName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["Swiftness"] = 1,
                ["Vitality I"] = 2,
                ["Strength I"] = 3,
                ["Dexterity I"] = 4,
                ["Tech I"] = 5,
                ["Intelligence I"] = 6,
                ["Faith I"] = 7,
                ["Photon Blade"] = 8,
                ["Dancing Slash"] = 9,
                ["Triple Shot"] = 10,
                ["Atalanta's Eye"] = 11,
                ["Plasma Grenade"] = 12,
                ["Gadget Turret"] = 13,
                ["Blaze"] = 14,
                ["Shock"] = 15,
                ["Healing Ward"] = 16,
                ["Bubble"] = 17,
                ["Berserk"] = 18,
                ["Megaslash"] = 19,
                ["Hyperbeam"] = 20,
                ["Trickster"] = 21,
                ["Quadracopter"] = 22,
                ["Cluster Bomber"] = 23,
                ["Inferno"] = 24,
                ["Enhanced Mind"] = 25,
                ["Angelic Augur"] = 26,
                ["Prism"] = 27,
                ["Darkfire"] = 38,
                ["Vitality II"] = 52,
                ["Strength II"] = 53,
                ["Dexterity II"] = 54,
                ["Tech II"] = 55,
                ["Intelligence II"] = 56,
                ["Faith II"] = 57,
                ["Vitality X"] = 102,
                ["Strength X"] = 103,
                ["Dexterity X"] = 104,
                ["Tech X"] = 105,
                ["Intelligence X"] = 106,
                ["Faith X"] = 107
            };
            chipIDsByRegistryName = new Dictionary<string, int>(chipIDsByName.Comparer);
            foreach (KeyValuePair<string, int> item in chipIDsByName)
            {
                chipIDsByRegistryName["Roguelands:" + item.Key] = item.Value;
            }
        }
    }

    /// <summary>
    /// Specifies what type of chip this is. If you wish for your chip to grant both passive and active effects, use ACTIVE.
    /// </summary>
    public enum ChipType
    {
        /// <summary>
        /// This chip is passive, and cannot be activated.
        /// </summary>
        PASSIVE,

        /// <summary>
        /// This chip is active, and can be activated. Note that active chips can still grant passive effects, if you desire.
        /// </summary>
        ACTIVE
    }
}