using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GadgetCore.API
{
    public class CreatureRegistry : Registry<CreatureRegistry, CreatureInfo, CreatureType>
    {
        public override string GetRegistryName()
        {
            return "Creature";
        }
    }

    public enum CreatureType
    {
        PASSIVE,
        HOSTILE,
        BOSS
    }
}
