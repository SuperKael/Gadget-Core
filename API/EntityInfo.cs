using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GadgetCore.API
{
    /// <summary>
    /// Defines a custom Entity. Make sure to call Register on it to register your Entity.
    /// </summary>
    public class EntityInfo : RegistryEntry<EntityInfo, EntityType>
    {
        /// <summary>
        /// The EntityType of this Entity
        /// </summary>
        public readonly EntityType Type;

        /// <summary>
        /// The GameObject representing this Entity. This will be registered as a prefab, and as such should be a freshly constructed GameObject.
        /// </summary>
        public readonly GameObject Entity;

        /// <summary>
        /// The World IDs that this entity will spawn in. Note that the current version of Gadget Core does not actually use these values, although in the future these may be used for natural creature spawning depending on the Entity's Type.
        /// </summary>
        public readonly int[] WorldIDs;

        /// <summary>
        /// Use to create a new EntityInfo. Make sure to call Register on it to register your Entity. Note that the Entity object's name must not contain spaces.
        /// </summary>
        public EntityInfo(EntityType Type, GameObject Entity, params int[] WorldIDs)
        {
            if (Entity != null && Entity.name.Any(x => x == ' ')) throw new InvalidOperationException("Entity object name must not have any spaces!");
            this.Type = Type;
            this.Entity = Entity;
            this.WorldIDs = WorldIDs;
        }

        /// <summary>
        /// Registers this EntityInfo to the EntityRegistry.
        /// </summary>
        /// <param name="name">The registry name to use.</param>
        /// <param name="preferredID">If specified, will use this registry ID.</param>
        /// <param name="overrideExisting">If false, will not register if the preferred ID is already used. Ignored if no preferred ID is specified.</param>
        public virtual EntityInfo Register(string name, int preferredID = -1, bool overrideExisting = true)
        {
            return RegisterInternal(name, preferredID, overrideExisting) as EntityInfo;
        }

        /// <summary>
        /// Spawns an instance of this Entity at the specified position. Override to add custom spawning behavior. Default implementation fails silently if the Entity has not been registered, or if the Entity GameObject is null.
        /// </summary>
        public virtual GameObject Spawn(Vector3 pos)
        {
            if (ID < 0 || Entity == null) return null;
            return Network.Instantiate(Resources.Load("e/" + Entity.name), pos, Quaternion.identity, 0) as GameObject;
        }

        /// <summary>
        /// Called after this Registry Entry has been registered to its Registry. You should never call this yourself.
        /// </summary>
        public override void PostRegister()
        {
            if (Entity != null)
            {
                GadgetCoreAPI.AddCustomResource("e/" + Entity.name, Entity);
            }
        }

        /// <summary>
        /// Returns the Registry Entry's Type enum. Used in the registration process, although it is safe to check this yourself by directly accessing the <see cref="Type"/> property.
        /// </summary>
        public override EntityType GetEntryType()
        {
            return Type;
        }

        /// <summary>
        /// Returns the singleton of the registry used for storing this type of Registry Entry.
        /// </summary>
        public override Registry<EntityInfo, EntityType> GetRegistry()
        {
            return Registry<EntityRegistry, EntityInfo, EntityType>.GetSingleton();
        }

        /// <summary>
        /// Returns whether the specified ID is valid for this Registry Entry's Type.
        /// </summary>
        public override bool IsValidIDForType(int id)
        {
            return id > 0;
        }

        /// <summary>
        /// Returns the next valid ID for this Registry Entry's Type, after the provided lastValidID. Should skip the vanilla ID range.
        /// </summary>
        public override int GetNextIDForType(int lastValidID)
        {
            if (lastValidID < GetRegistry().GetIDStart() - 1) lastValidID = GetRegistry().GetIDStart() - 1;
            return ++lastValidID;
        }
    }
}
