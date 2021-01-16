namespace GadgetCore.API
{
    /// <summary>
    /// This registry is filled with ObjectInfos, and is used for registering custom harvestable objects to the game.
    /// </summary>
    public class ObjectRegistry : Registry<ObjectRegistry, ObjectInfo, ObjectType>
    {
        /// <summary>
        /// The name of this registry.
        /// </summary>
        public const string REGISTRY_NAME = "Object";

        /// <summary>
        /// Gets the name of this registry. Must be constant. Returns <see cref="REGISTRY_NAME"/>.
        /// </summary>
        public override string GetRegistryName()
        {
            return REGISTRY_NAME;
        }

        /// <summary>
        /// Gets the ID that modded IDs should start at for this registry. <see cref="ObjectRegistry"/> always returns 200.
        /// </summary>
        public override int GetIDStart()
        {
            return 200;
        }
    }

    /// <summary>
    /// Specifies what type of object this is.
    /// </summary>
    public enum ObjectType
    {
        /// <summary>
        /// This is functionally an ore node.
        /// </summary>
        ORE,
        /// <summary>
        /// This is functionally a tree.
        /// </summary>
        TREE,
        /// <summary>
        /// This is functionally a plant.
        /// </summary>
        PLANT,
        /// <summary>
        /// This is functionally a bugspot.
        /// </summary>
        BUGSPOT,
        /// <summary>
        /// This is some sort of object that does not fall into any of the standard types. By default, will be ignored by droids despite being clickable.
        /// </summary>
        OTHER
    }
}
