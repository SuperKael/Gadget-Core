using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GadgetCore.API
{
    public class EntityInfo : RegistryEntry<EntityInfo, EntityType>
    {
        public readonly EntityType Type;

        public readonly GameObject Entity;

        public EntityInfo(EntityType Type, GameObject Entity)
        {
            if (Entity != null && Entity.name.Any(x => x == ' ')) throw new InvalidOperationException("Entity object name must not have any spaces!");
            this.Type = Type;
            this.Entity = Entity;
        }

        public virtual EntityInfo Register(int preferredID = -1, bool overrideExisting = true)
        {
            return RegisterInternal(preferredID, overrideExisting) as EntityInfo;
        }

        public virtual GameObject Spawn(Vector3 pos)
        {
            if (ID < 0 || Entity == null) return null;
            return Network.Instantiate(Resources.Load("e/" + Entity.name), pos, Quaternion.identity, 0) as GameObject;
        }

        public override void PostRegister()
        {
            if (Entity != null)
            {
                GadgetCoreAPI.AddCustomResource("e/" + Entity.name, Entity);
            }
        }

        public override EntityType GetEntryTypeEnum()
        {
            return Type;
        }

        public override Registry<EntityInfo, EntityType> GetRegistry()
        {
            return Registry<EntityRegistry, EntityInfo, EntityType>.GetSingleton();
        }

        public override bool IsValidIDForType(int id)
        {
            return id > 0;
        }

        public override int GetNextIDForType(int lastValidID)
        {
            if (lastValidID < 9999) lastValidID = 9999;
            return ++lastValidID;
        }
    }
}
