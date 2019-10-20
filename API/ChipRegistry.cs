using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GadgetCore.API
{
    public class ChipRegistry : Registry<ChipRegistry, ChipInfo, ChipType>
    {
        public override string GetRegistryName()
        {
            return "Chip";
        }
    }

    public enum ChipType
    {
        ACTIVE,
        PASSIVE
    }
}
