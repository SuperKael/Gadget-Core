using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GadgetCore.API
{
    public class TileInfo : RegistryEntry<TileInfo, TileType>
    {
        public readonly TileType type;

        public TileInfo(TileType type)
        {
            this.type = type;
        }

        public override TileType GetEntryTypeEnum()
        {
            return type;
        }

        public override Registry GetRegistry()
        {
            return Registry<TileRegistry, TileInfo, TileType>.GetSingleton();
        }

        public override bool IsValidIDForType(int id)
        {
            return true;
        }

        public override int GetNextIDForType(int lastValidID)
        {
            return lastValidID++;
        }
    }
}
