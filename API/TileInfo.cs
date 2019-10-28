using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GadgetCore.API
{
    public class TileInfo : RegistryEntry<TileInfo, TileType>
    {
        public readonly TileType Type;
        public readonly ItemInfo Item;
        public readonly GameObject Prop;

        public Texture Tex { get; protected set; }
        public Material Mat { get; protected set; }
        public GadgetCoreAPI.SpriteSheetEntry Sprite { get; protected set; }

        public TileInfo(TileType Type, Texture Tex, ItemInfo Item = null)
        {
            this.Type = Type;
            this.Tex = Tex;
            this.Item = Item;
        }

        public TileInfo(TileType Type, Material Mat, ItemInfo Item = null)
        {
            this.Type = Type;
            this.Mat = Mat;
            this.Item = Item;
        }

        public TileInfo(TileType Type, Texture Tex, GameObject Prop, ItemInfo Item = null)
        {
            this.Type = Type;
            this.Tex = Tex;
            this.Prop = Prop;
            this.Item = Item;
        }

        public TileInfo(TileType Type, Material Mat, GameObject Prop, ItemInfo Item = null)
        {
            this.Type = Type;
            this.Mat = Mat;
            this.Prop = Prop;
            this.Item = Item;
        }

        public virtual TileInfo Register(int preferredID = -1, bool overrideExisting = true)
        {
            return RegisterInternal(preferredID, overrideExisting) as TileInfo;
        }

        public override void PostRegister()
        {
            if (Mat == null && Tex != null)
            {
                Mat = new Material(Shader.Find("Unlit/Transparent Cutout"));
                Mat.mainTexture = Tex;
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

        public override TileType GetEntryTypeEnum()
        {
            return Type;
        }

        public override Registry<TileInfo, TileType> GetRegistry()
        {
            return Registry<TileRegistry, TileInfo, TileType>.GetSingleton();
        }

        public override bool IsValidIDForType(int id)
        {
            return id > 0;
        }
        public override int GetNextIDForType(int lastValidID)
        {
            if (lastValidID < 9999) lastValidID = 9999;
            return ++lastValidID;
        }

        public virtual bool TryPlaceTile() { return true; }

        public event Func<IEnumerator> OnInteract;

        internal IEnumerator Interact() { return OnInteract?.Invoke(); }
    }
}
