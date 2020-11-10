namespace GadgetCore.API
{
    /// <summary>
    /// This registry is filled with MenuInfos, and is used for registering custom menus to the game.
    /// </summary>
    public class MenuRegistry : Registry<MenuRegistry, MenuInfo, MenuType>
    {
        /// <summary>
        /// The name of this registry.
        /// </summary>
        public const string REGISTRY_NAME = "Menu";

        /// <summary>
        /// Gets the name of this registry. Must be constant. Returns <see cref="REGISTRY_NAME"/>.
        /// </summary>
        public override string GetRegistryName()
        {
            return REGISTRY_NAME;
        }

        /// <summary>
        /// Gets the ID that modded IDs should start at for this registry. <see cref="MenuRegistry"/> always returns 10000.
        /// </summary>
        public override int GetIDStart()
        {
            return 100;
        }
    }

    /// <summary>
    /// Specifies what type of menu this is.
    /// </summary>
    public enum MenuType
    {
        /// <summary>
        /// Simple menu. Opens alongside the player's inventory.
        /// </summary>
        SIMPLE,
        /// <summary>
        /// Used for crafting. Opens alongside the player's inventory.
        /// </summary>
        CRAFTING,
        /// <summary>
        /// Contains chip slots. Causes the chip bar to become visible.
        /// </summary>
        CHIP,
        /// <summary>
        /// Does not contain item slots. Cannot be open alongside the player's inventory.
        /// </summary>
        EXCLUSIVE
    }
}
