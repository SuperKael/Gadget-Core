using System.Collections.Generic;

namespace GadgetCore.API
{
    /// <summary>
    /// This registry is filled with TileInfos, and is used for registering custom tiles to the game.
    /// </summary>
    public class TileRegistry : Registry<TileRegistry, TileInfo, TileType>
    {
        /// <summary>
        /// The name of this registry.
        /// </summary>
        public const string REGISTRY_NAME = "Tile";

        private static readonly Dictionary<string, int> IDsByPropName = new Dictionary<string, int>();

        /// <summary>
        /// Gets the name of this registry. Must be constant. Returns <see cref="REGISTRY_NAME"/>.
        /// </summary>
        public override string GetRegistryName()
        {
            return REGISTRY_NAME;
        }

        /// <summary>
        /// Gets the default type of the given ID, assuming that it is a vanilla ID.
        /// </summary>
        public static TileType GetDefaultTypeByID(int ID)
        {
            if (ID >= 2400) return TileType.NONSOLID;
            else if (ID >= 2200 && ID < 2300) return TileType.WALL;
            else if (ID >= 2100) return TileType.INTERACTIVE;
            else return TileType.SOLID;
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
        /// Called after the specified Registry Entry has been registered. You should never call this yourself. Note that this is called before <see cref="RegistryEntry{E, T}.PostRegister"/>
        /// </summary>
        protected override void PostRegistration(TileInfo entry)
        {
            if (entry.Type == TileType.INTERACTIVE && entry.Prop != null) IDsByPropName[entry.Prop.name] = entry.GetID();
        }

        /// <summary>
        /// Gets the ID that modded IDs should start at for this registry. <see cref="TileRegistry"/> always returns 10000.
        /// </summary>
        public override int GetIDStart()
        {
            return 10000;
        }

        /// <summary>
        /// Gets the <see cref="TileInfo"/> for the given ID. Will return a <see cref="VanillaTileInfo"/> if the given ID is not in the registry, but is within the vanilla ID range. Otherwise, returns null.
        /// </summary>
        public static TileInfo GetTile(int ID)
        {
            return GetSingleton().HasEntry(ID) ? GetSingleton().GetEntry(ID) : ID > 0 && ID < GetSingleton().GetIDStart() ? VanillaTileInfo.Wrap(ID) : null;
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
