using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GadgetCore.API
{
    public class ItemInfo : RegistryEntry<ItemInfo, ItemType>
    {
        public readonly ItemType Type;
        public readonly string Name;
        public readonly string Desc;
        public readonly EquipStats Stats;

        public int Value { get; protected set; }
        public TileInfo Tile { get; protected set; }
        public Texture Tex { get; protected set; }
        public Texture HeldTex { get; protected set; }
        public Texture HeadTex { get; protected set; }
        public Texture BodyTex { get; protected set; }
        public Texture ArmTex { get; protected set; }
        public Material Mat { get; protected set; }
        public Material HeldMat { get; protected set; }
        public Material HeadMat { get; protected set; }
        public Material BodyMat { get; protected set; }
        public Material ArmMat { get; protected set; }

        public ItemInfo(ItemType Type, string Name, string Desc, Texture Tex, int Value = -1, EquipStats Stats = default, Texture HeldTex = null, Texture HeadTex = null, Texture BodyTex = null, Texture ArmTex = null)
        {
            Tex.filterMode = FilterMode.Point;
            this.Type = Type;
            this.Name = Name;
            this.Desc = Desc;
            this.Value = Value;
            this.Stats = Stats;
            this.Tex = Tex;
            this.HeldTex = HeldTex;
            this.HeadTex = HeadTex;
            this.BodyTex = BodyTex;
            this.ArmTex = ArmTex;
        }

        public ItemInfo(ItemType Type, string Name, string Desc, Material Mat, int Value = -1, EquipStats Stats = default, Material HeldMat = null, Material HeadMat = null, Material BodyMat = null, Material ArmMat = null)
        {
            this.Type = Type;
            this.Name = Name;
            this.Desc = Desc;
            this.Value = Value;
            this.Stats = Stats;
            this.Mat = Mat;
            this.HeldMat = HeldMat;
            this.HeadMat = HeadMat;
            this.BodyMat = BodyMat;
            this.ArmMat = ArmMat;
        }

        public virtual ItemInfo Register(int preferredID = -1, bool overrideExisting = true)
        {
            return RegisterInternal(preferredID, overrideExisting) as ItemInfo;
        }

        internal ItemInfo SetTile(TileInfo Tile)
        {
            this.Tile = Tile;
            return this;
        }

        public override void PostRegister()
        {
            if (Mat == null)
            {
                Mat = new Material(Shader.Find("Unlit/Transparent"));
                Mat.mainTexture = Tex;
            }
            else
            {
                Tex = Mat.mainTexture;
            }
            GadgetCoreAPI.AddCustomResource("i/i" + ID, Mat);
            if (HeldTex != null || HeldMat != null)
            {
                if (HeldMat == null)
                {
                    HeldMat = new Material(Shader.Find("Unlit/Transparent Cutout"));
                    HeldMat.mainTexture = HeldTex;
                }
                else
                {
                    HeldTex = HeldMat.mainTexture;
                }
                if ((Type & (ItemType.BASIC_MASK | ItemType.TYPE_MASK)) == ItemType.WEAPON) GadgetCoreAPI.AddCustomResource("ie/ie" + ID, HeldMat);
                if ((Type & (ItemType.BASIC_MASK | ItemType.TYPE_MASK)) == ItemType.OFFHAND) GadgetCoreAPI.AddCustomResource("o/o" + ID, HeldMat);
            }
            if (HeadTex != null || HeadMat != null)
            {
                if (HeadMat == null)
                {
                    HeadMat = new Material(Shader.Find("Unlit/Transparent Cutout"));
                    HeadMat.mainTexture = HeadTex;
                }
                else
                {
                    HeadTex = HeadMat.mainTexture;
                }
                GadgetCoreAPI.AddCustomResource((Type & (ItemType.BASIC_MASK | ItemType.TYPE_MASK)) == ItemType.DROID ? ("droid/d" + ID + "h") : "h/h" + ID, HeadMat);
            }
            if (BodyTex != null || BodyMat != null)
            {
                if (BodyMat == null)
                {
                    BodyMat = new Material(Shader.Find("Unlit/Transparent Cutout"));
                    BodyMat.mainTexture = BodyTex;
                }
                else
                {
                    BodyTex = BodyMat.mainTexture;
                }
                GadgetCoreAPI.AddCustomResource((Type & (ItemType.BASIC_MASK | ItemType.TYPE_MASK)) == ItemType.DROID ? ("droid/d" + ID + "b") : "b/b" + ID, BodyMat);
            }
            if (ArmTex != null || ArmMat != null)
            {
                if (ArmMat == null)
                {
                    ArmMat = new Material(Shader.Find("Unlit/Transparent Cutout"));
                    ArmMat.mainTexture = ArmTex;
                }
                else
                {
                    ArmTex = ArmMat.mainTexture;
                }
                GadgetCoreAPI.AddCustomResource("a/a" + ID, ArmMat);
            }

            if (Value < 0)
            {
                if (ID == 52 || ID == 53)
                {
                    Value = 1;
                }
                else if (ID == 59)
                {
                    Value = 9999;
                }
                else if ((Type & (ItemType.BASIC_MASK | ItemType.TYPE_MASK)) == ItemType.LOOT)
                {
                    Value = 2;
                }
                else if ((Type & (ItemType.BASIC_MASK | ItemType.TYPE_MASK)) == ItemType.EMBLEM)
                {
                    Value = 15;
                }
                else if ((Type & ItemType.MOD) == ItemType.MOD)
                {
                    Value = 15;
                }
                else if ((Type & ItemType.EQUIPMENT_F) == ItemType.EQUIPMENT_F)
                {
                    Value = 20;
                }
                else if (ID < 100)
                {
                    Value = 2;
                }
                else if (ID < 300)
                {
                    Value = 15;
                }
                else if (ID < 2000)
                {
                    Value = 20;
                }
                else if (ID > 2500 && ID <= 3000)
                {
                    Value = 700;
                }
                else
                {
                    Value = 0;
                }
            }
        }

        public virtual int GetValue()
        {
            return Value;
        }

        public virtual int GetTier()
        {
            if ((Type & ItemType.LOOT) == ItemType.LOOT && (Type & ItemType.EQUIPMENT_F) == ItemType.GENERIC)
            {
                return (Type & ItemType.TIER_MASK) != ItemType.TIER10 ? ((int)(Type & ItemType.TIER_MASK) >> 8) : 10;
            }
            return -1;
        }

        public virtual int GetTierDigit()
        {
            if ((Type & ItemType.LOOT) == ItemType.LOOT && (Type & ItemType.EQUIPMENT_F) == ItemType.GENERIC)
            {
                return (int)(Type & ItemType.TIER_MASK) >> 8;
            }
            return -1;
        }

        public override ItemType GetEntryTypeEnum()
        {
            return Type;
        }

        public override Registry<ItemInfo, ItemType> GetRegistry()
        {
            return Registry<ItemRegistry, ItemInfo, ItemType>.GetSingleton();
        }

        public override bool IsValidIDForType(int id)
        {
            if (id <= 0) return false;
            if ((Type & ItemType.LOOT) == ItemType.LOOT && (Type & ItemType.EQUIPMENT_F) == ItemType.GENERIC)
            {
                return id % 10 == ((int)(Type & ItemType.TIER_MASK) >> 8);
            }
            return true;
        }

        public override int GetNextIDForType(int lastValidID)
        {
            if (lastValidID < 9999) lastValidID = 9999;
            if ((Type & ItemType.LOOT) == ItemType.LOOT && (Type & ItemType.EQUIPMENT_F) == ItemType.GENERIC)
            {
                int tier = (int)(Type & ItemType.TIER_MASK) >> 8;
                int lastValidIDTier = lastValidID % 10;
                if (tier == lastValidIDTier)
                {
                    return lastValidID + 10;
                }
                else if (tier > lastValidIDTier)
                {
                    return (lastValidID / 10 * 10) | tier;
                }
                else
                {
                    return ((lastValidID / 10 * 10) | tier) + 10;
                }
            }
            return ++lastValidID;
        }

        public event Func<int, bool> OnUse;
        public event Action<int> OnEquip;
        public event Action<int> OnDequip;

        internal bool InvokeOnUse(int slot) { return OnUse?.Invoke(slot) ?? false; }
        internal void InvokeOnEquip(int slot) { OnEquip?.Invoke(slot); }
        internal void InvokeOnDequip(int slot) { OnDequip?.Invoke(slot); }
    }
}
