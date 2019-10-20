using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GadgetCore.API
{
    public class TileRegistry : Registry<TileRegistry, TileInfo, TileType>
    {
        public override string GetRegistryName()
        {
            return "Tile";
        }
    }

    public enum TileType
    {
        SOLID,
        NONSOLID,
        INTERACTIVE
    }
}
