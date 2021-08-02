using UnityEngine;

namespace GadgetCore.API
{
    /// <summary>
    /// Tracks what gear mods each player has, since for some reason the base game does not do this.
    /// </summary>
    public static class PlayerGearModsTracker
    {
        /// <summary>
        /// In theory, returns the gear mods that the given player has, in the same format as GameScript.MODS. In practice, it currently just returns GameScript.MODS.
        /// This exists so that Gadget Core can later add this functionality without it being a breaking change.
        /// As such, whenever you need to query what gear mods the player has, you should use this.
        /// </summary>
        public static int[] GetGearMods(this PlayerScript player)
        {
            return GameScript.MODS;
        }

        /// <summary>
        /// In theory, returns the gear mods that the given player has, in the same format as GameScript.MODS. In practice, it currently just returns GameScript.MODS.
        /// This exists so that Gadget Core can later add this functionality without it being a breaking change.
        /// As such, whenever you need to query what gear mods the player has, you should use this.
        /// </summary>
        public static int[] GetGearMods(this NetworkPlayer player)
        {
            return GetGearMods(GadgetCoreAPI.GetPlayerByName(GadgetNetwork.GetNameByNetworkPlayer(player)));
        }
    }
}
