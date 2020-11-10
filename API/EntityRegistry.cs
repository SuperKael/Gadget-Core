﻿namespace GadgetCore.API
{
    /// <summary>
    /// This registry is filled with EntityInfos, and is used for registering custom entities to the game.
    /// </summary>
    public class EntityRegistry : Registry<EntityRegistry, EntityInfo, EntityType>
    {
        /// <summary>
        /// The name of this registry.
        /// </summary>
        public const string REGISTRY_NAME = "Entity";

        /// <summary>
        /// Gets the name of this registry. Must be constant. Returns <see cref="REGISTRY_NAME"/>.
        /// </summary>
        public override string GetRegistryName()
        {
            return REGISTRY_NAME;
        }

        /// <summary>
        /// Gets the ID that modded IDs should start at for this registry. <see cref="EntityRegistry"/> always returns 10000.
        /// </summary>
        public override int GetIDStart()
        {
            return 10000;
        }
    }

    /// <summary>
    /// Specifies what type of entity this is. Note that despite these options, the current version of Gadget Core will never spawn the entity automatically.
    /// </summary>
    public enum EntityType
    {
        /// <summary>
        /// This is a standard common enemy, or a passive creature.
        /// </summary>
        COMMON,
        /// <summary>
        /// This is a rare enemy, such as a miniboss, that has a fair chance of not spawning at all on a given level.
        /// </summary>
        RARE,
        /// <summary>
        /// This is a boss, that will never spawn naturally, and must be manually spawned.
        /// </summary>
        BOSS,
        /// <summary>
        /// This is not a boss, but will also never spawn naturally, and must be manually spawned.
        /// </summary>
        SPECIAL,
        /// <summary>
        /// This is some sort of entity that does not fall into any of the other catagories. It will never spawn naturally, and must be manually spawned.
        /// </summary>
        OTHER
    }
}
