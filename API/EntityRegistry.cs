using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GadgetCore.API
{
    public class EntityRegistry : Registry<EntityRegistry, EntityInfo, EntityType>
    {
        public override string GetRegistryName()
        {
            return "Entity";
        }
    }

    public enum EntityType
    {
        COMMON,
        RARE,
        BOSS,
        SPECIAL,
        OTHER
    }
}
