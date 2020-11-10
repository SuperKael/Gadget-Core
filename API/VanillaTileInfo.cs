using System.Collections.Generic;

namespace GadgetCore.API
{
    /// <summary>
    /// This is a wrapper for vanilla Item IDs to a Gadget Core ItemInfo. Use to get in-depth info about a vanilla item, in Gadget Core's terms. Can also be used to register events to occur upon firing vanilla weapons.
    /// </summary>
    public class VanillaTileInfo : TileInfo
    {
        internal static Dictionary<int, VanillaTileInfo> Wrappers = new Dictionary<int, VanillaTileInfo>();

        /// <summary>
        /// Constructs a new VanillaItemInfo based upon the given ID. Do not try to call this yourself - use <see cref="Wrap"/>
        /// </summary>
        /// <param name="ID">The vanilla ID to be wrapped.</param>
        /// <param name="registerItem">Whether the VanillaItemInfo that matches this Tile ID should be wrapped into the ItemRegistry.</param>
        protected VanillaTileInfo(int ID, bool registerItem = false) : base(TileRegistry.GetDefaultTypeByID(ID), GadgetCoreAPI.GetTileMaterial(ID), TileRegistry.GetDefaultTypeByID(ID) == TileType.INTERACTIVE ? GadgetCoreAPI.GetPlaceableNPCResource(ID) : GadgetCoreAPI.GetPropResource(ID), ItemRegistry.Singleton.HasEntry(ID) ? ItemRegistry.Singleton.GetEntry(ID) : VanillaItemInfo.WrapForTile(ID, registerItem))
        {
            this.ID = ID;
        }

        /// <summary>
        /// Provides a wrapper for the given vanilla ID. If the given ID has already been wrapped before, it will return the same wrapper instance as was returned before. If register is true, then the wrapper will be registered to its ID in the appropriate registry.
        /// </summary>
        public static VanillaTileInfo Wrap(int ID, bool register = false)
        {
            VanillaTileInfo tileInfo = Wrappers.ContainsKey(ID) ? Wrappers[ID] : (Wrappers[ID] = new VanillaTileInfo(ID, register));
            if (register && tileInfo.RegistryName == null) tileInfo.Register(tileInfo.Item.Name, ID, true);
            return tileInfo;
        }
    }
}
