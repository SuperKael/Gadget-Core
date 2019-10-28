using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GadgetCore.API
{
    public class ChipInfo : RegistryEntry<ChipInfo, ChipType>
    {
        public readonly ChipType Type;
        public readonly string Name;
        public readonly string Desc;
        public readonly int Cost;
        public readonly EquipStats Stats;
        public readonly ChipCostType CostType;

        public Texture Tex { get; protected set; }
        public Material Mat { get; protected set; }

        public ChipInfo(ChipType Type, string Name, string Desc, int Cost, Texture Tex, EquipStats Stats = default, ChipCostType CostType = ChipCostType.MANA)
        {
            this.Type = Type;
            this.Name = Name;
            this.Desc = Desc;
            this.Tex = Tex;
            this.Cost = Cost;
            this.Stats = Stats;
            this.CostType = CostType;
        }

        public ChipInfo(ChipType Type, string Name, string Desc, int Cost, Material Mat, EquipStats Stats = default, ChipCostType CostType = ChipCostType.MANA)
        {
            this.Type = Type;
            this.Name = Name;
            this.Desc = Desc;
            this.Mat = Mat;
            this.Cost = Cost;
            this.Stats = Stats;
            this.CostType = CostType;
        }

        public virtual ChipInfo Register(int preferredID = -1, bool overrideExisting = true)
        {
            return RegisterInternal(preferredID, overrideExisting) as ChipInfo;
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
            GadgetCoreAPI.AddCustomResource("cc/cc" + ID, Mat);
        }

        public override ChipType GetEntryTypeEnum()
        {
            return Type;
        }

        public override Registry<ChipInfo, ChipType> GetRegistry()
        {
            return Registry<ChipRegistry, ChipInfo, ChipType>.GetSingleton();
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

        public virtual bool IsChipActive() { return false; }

        public event Action<int> OnUse;
        public event Action<int> OnEquip;
        public event Action<int> OnDequip;

        internal void InvokeOnUse(int slot) { OnUse?.Invoke(slot); }
        internal void InvokeOnEquip(int slot) { OnEquip?.Invoke(slot); }
        internal void InvokeOnDequip(int slot) { OnDequip?.Invoke(slot); }

        public enum ChipCostType
        {
            MANA,
            ENERGY,
            HEALTH,
            HEALTH_LETHAL
        }
    }
}
