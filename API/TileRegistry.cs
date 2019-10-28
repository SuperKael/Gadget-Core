using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GadgetCore.API
{
    public class TileRegistry : Registry<TileRegistry, TileInfo, TileType>
    {
        private static Dictionary<string, int> IDsByPropName = new Dictionary<string, int>();
        public override string GetRegistryName()
        {
            return "Tile";
        }

        public override int Register(TileInfo entry, int preferredID = -1, bool registerAnyway = true)
        {
            int id = base.Register(entry, preferredID < 0 && entry.Item != null ? entry.Item.ID : preferredID, registerAnyway);
            if (entry.Type == TileType.INTERACTIVE && entry.Prop != null) IDsByPropName.Add(entry.Prop.name, id);
            return id;
        }

        public static int GetIDByName(string name)
        {
            if (IDsByPropName.ContainsKey(name))
            {
                return IDsByPropName[name];
            }
            else
            {
                return -1;
            }
        }
    }

    public enum TileType
    {
        SOLID,
        NONSOLID,
        INTERACTIVE,
        WALL
    }
}
