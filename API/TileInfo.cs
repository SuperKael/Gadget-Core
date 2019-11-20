using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GadgetCore.API
{
    /// <summary>
    /// Defines a custom Tile. Make sure to call Register on it to register your Tile.
    /// </summary>
    public class TileInfo : RegistryEntry<TileInfo, TileType>
    {
        /// <summary>
        /// The TileType of this Tile
        /// </summary>
        public readonly TileType Type;
        /// <summary>
        /// The ItemInfo of the item that places this tile. May be null.
        /// </summary>
        public readonly ItemInfo Item;
        /// <summary>
        /// The Prop or NPC associated with this tile. Will by an NPC if this tile's Type is Interactive, otherwise will be a Prop. May be null.
        /// </summary>
        public readonly GameObject Prop;

        /// <summary>
        /// The Texture associated with this tile. May be null.
        /// </summary>
        public virtual Texture Tex { get; protected set; }
        /// <summary>
        /// The Material associated with this tile. May be null.
        /// </summary>
        public virtual Material Mat { get; protected set; }
        /// <summary>
        /// The <see cref="GadgetCoreAPI.SpriteSheetEntry"/> associated with this tile. Will be null unless the tile's Type is SOLID or WALL.
        /// </summary>
        public virtual GadgetCoreAPI.SpriteSheetEntry Sprite { get; protected set; }

        /// <summary>
        /// Use to create a new TileInfo. This constructor should be used if the Type is SOLID or WALL. Make sure to call Register on it to register your Tile.
        /// </summary>
        /// <param name="Type">The <see cref="TileType"/> of this Tile</param>
        /// <param name="Tex">The Texture of this Tile</param>
        /// <param name="Item">The Item that places this tile. Automatically calls <see cref="ItemInfo.SetTile(TileInfo)"/>, so there is no need to do that yourself.</param>
        public TileInfo(TileType Type, Texture Tex, ItemInfo Item = null)
        {
            this.Type = Type;
            this.Tex = Tex;
            this.Item = Item;
        }

        /// <summary>
        /// Use to create a new TileInfo. This constructor should be used if the Type is SOLID or WALL. Make sure to call Register on it to register your Tile.
        /// </summary>
        /// <param name="Type">The <see cref="TileType"/> of this Tile</param>
        /// <param name="Mat">The Material of this Tile</param>
        /// <param name="Item">The Item that places this tile. Automatically calls <see cref="ItemInfo.SetTile(TileInfo)"/>, so there is no need to do that yourself.</param>
        public TileInfo(TileType Type, Material Mat, ItemInfo Item = null)
        {
            this.Type = Type;
            this.Mat = Mat;
            this.Item = Item;
        }

        /// <summary>
        /// Use to create a new TileInfo. This constructor should be used if the Type is NONSOLID or INTERACTIVE. Make sure to call Register on it to register your Tile.
        /// </summary>
        /// <param name="Type">The <see cref="TileType"/> of this Tile</param>
        /// <param name="Tex">The Texture of this Tile</param>
        /// <param name="Prop">The Prop of this Tile. Will be registered as an NPC of the Type is INTERACTIVE</param>
        /// <param name="Item">The Item that places this tile. Automatically calls <see cref="ItemInfo.SetTile(TileInfo)"/>, so there is no need to do that yourself.</param>
        public TileInfo(TileType Type, Texture Tex, GameObject Prop, ItemInfo Item = null)
        {
            this.Type = Type;
            this.Tex = Tex;
            this.Prop = Prop;
            this.Item = Item;
        }

        /// <summary>
        /// Use to create a new TileInfo. This constructor should be used if the Type is NONSOLID or INTERACTIVE. Make sure to call Register on it to register your Tile.
        /// </summary>
        /// <param name="Type">The <see cref="TileType"/> of this Tile</param>
        /// <param name="Mat">The Material of this Tile</param>
        /// <param name="Prop">The Prop of this Tile. Will be registered as an NPC of the Type is INTERACTIVE</param>
        /// <param name="Item">The Item that places this tile. Automatically calls <see cref="ItemInfo.SetTile(TileInfo)"/>, so there is no need to do that yourself.</param>
        public TileInfo(TileType Type, Material Mat, GameObject Prop, ItemInfo Item = null)
        {
            this.Type = Type;
            this.Mat = Mat;
            this.Prop = Prop;
            this.Item = Item;
        }

        /// <summary>
        /// Registers this TileInfo to the TileRegistry.
        /// </summary>
        /// <param name="name">The registry name to use.</param>
        /// <param name="preferredID">If specified, will use this registry ID.</param>
        /// <param name="overrideExisting">If false, will not register if the preferred ID is already used. Ignored if no preferred ID is specified.</param>
        public virtual TileInfo Register(string name, int preferredID = -1, bool overrideExisting = true)
        {
            return RegisterInternal(name, preferredID, overrideExisting) as TileInfo;
        }

        /// <summary>
        /// Called after this Registry Entry has been registered to its Registry. You should never call this yourself.
        /// </summary>
        public override void PostRegister()
        {
            if (Mat == null && Tex != null)
            {
                Mat = new Material(Shader.Find("Unlit/Transparent Cutout"))
                {
                    mainTexture = Tex
                };
            }
            else if (Mat != null)
            {
                Tex = Mat.mainTexture;
            }
            if (Type == TileType.SOLID || Type == TileType.WALL)
            {
                if (Tex != null) Sprite = GadgetCoreAPI.AddTextureToSheet(Tex);
            }
            if (Mat != null) GadgetCoreAPI.AddCustomResource("construct/c" + ID, Mat);
            if (Prop != null)
            {
                if (Type == TileType.INTERACTIVE)
                {
                    Prop.layer = 12;
                    GadgetCoreAPI.AddCustomResource("npc/npc" + ID, Prop);
                }
                else
                {
                    GadgetCoreAPI.AddCustomResource("prop/" + ID, Prop);
                }
            }
            if (Item != null && Item.ID > 0)
            {
                Item.SetTile(this);
            }
        }

        /// <summary>
        /// Returns the Registry Entry's Type enum. Used in the registration process, although it is safe to check this yourself by directly accessing the <see cref="Type"/> property.
        /// </summary>
        public override TileType GetEntryType()
        {
            return Type;
        }

        /// <summary>
        /// Returns the singleton of the registry used for storing this type of Registry Entry.
        /// </summary>
        public override Registry<TileInfo, TileType> GetRegistry()
        {
            return Registry<TileRegistry, TileInfo, TileType>.GetSingleton();
        }

        /// <summary>
        /// Returns whether the specified ID is valid for this Registry Entry's Type.
        /// </summary>
        public override bool IsValidIDForType(int id)
        {
            return id > 0;
        }

        /// <summary>
        /// Returns the next valid ID for this Registry Entry's Type, after the provided lastValidID. Should skip the vanilla ID range.
        /// </summary>
        public override int GetNextIDForType(int lastValidID)
        {
            if (lastValidID < GetRegistry().GetIDStart() - 1) lastValidID = GetRegistry().GetIDStart() - 1;
            return ++lastValidID;
        }

        /// <summary>
        /// Called when this tile's item is about to be placed using Build Mode. Return false to prevent it from being placed.
        /// </summary>
        public virtual bool TryPlaceTile() { return true; }

        /// <summary>
        /// Called when this tile is interacted with. Will only ever be called if the Prop's layer is 12, which it will automatically be set to if the Tyspe is specified as INTERACTIVE
        /// </summary>
        public event Func<IEnumerator> OnInteract;

        internal IEnumerator Interact() { return OnInteract?.Invoke(); }
    }
}
