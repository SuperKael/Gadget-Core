using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GadgetCore.API
{
    /// <summary>
    /// This registry is filled with TileInfos, and is used for registering custom tiles to the game.
    /// </summary>
    public class TileRegistry : Registry<TileRegistry, TileInfo, TileType>
    {
        private static Dictionary<string, int> IDsByPropName = new Dictionary<string, int>();

        /// <summary>
        /// Gets the name of this registry. Must be constant.
        /// </summary>
        public override string GetRegistryName()
        {
            return "Tile";
        }

        /// <summary>
        /// Used to register a registry entry to this registry. You should probably use the Register method on that registry entry instead.
        /// </summary>
        /// <param name="entry">The RegistryEntry to register.</param>
        /// <param name="name">The registry name to use.</param>
        /// <param name="preferredID">If specified, will use this registry ID.</param>
        /// <param name="overrideExisting">If false, will not register if the preferred ID is already used. Ignored if no preferred ID is specified.</param>
        public override int Register(TileInfo entry, string name, int preferredID = -1, bool overrideExisting = true)
        {
            int id = base.Register(entry, name, preferredID < 0 && entry.Item != null ? entry.Item.ID : preferredID, overrideExisting);
            if (entry.Type == TileType.INTERACTIVE && entry.Prop != null) IDsByPropName.Add(entry.Prop.name, id);
            return id;
        }

        /// <summary>
        /// Gets the ID of the TileInfo whose prop has the given name.
        /// </summary>
        public static int GetIDByPropName(string name)
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

        /// <summary>
        /// Gets the ID that modded IDs should start at for this registry. May be 0 if the vanilla game does not use IDs for this type of thing.
        /// </summary>
        public override int GetIDStart()
        {
            return 10000;
        }
    }

    /// <summary>
    /// Specifies what type of tile this is.
    /// </summary>
    public enum TileType
    {
        /// <summary>
        /// This tile is completely solid.
        /// </summary>
        SOLID,
        /// <summary>
        /// This tile is not completely solid, and instead uses a prop, which may or may not have any collision.
        /// </summary>
        NONSOLID,
        /// <summary>
        /// This tile is interactive, such as a crafting station. It uses a prop, although it should not have any collision.
        /// </summary>
        INTERACTIVE,
        /// <summary>
        /// This tile is a background wall.
        /// </summary>
        WALL
    }
}
