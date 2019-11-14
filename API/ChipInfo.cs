using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GadgetCore.API
{
    /// <summary>
    /// Defines a custom Chip. Make sure to call Register on it to register your Chip.
    /// </summary>
    public class ChipInfo : RegistryEntry<ChipInfo, ChipType>
    {
        /// <summary>
        /// The ChipType of this Chip
        /// </summary>
        public readonly ChipType Type;
        /// <summary>
        /// The name of this Chip
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// The description of this Chip
        /// </summary>
        public readonly string Desc;
        /// <summary>
        /// The cost to use this active Chip
        /// </summary>
        public readonly int Cost;
        /// <summary>
        /// The stats of this passive Chip
        /// </summary>
        public readonly EquipStats Stats;
        /// <summary>
        /// The type of cost of this active Chip, either MANA, ENERGY, HEALTH, or HEALTH_LETHAL
        /// </summary>
        public readonly ChipCostType CostType;

        /// <summary>
        /// The Texture associated with this Chip. May be null.
        /// </summary>
        public Texture Tex { get; protected set; }
        /// <summary>
        /// The Material associated with this Chip. May be null.
        /// </summary>
        public Material Mat { get; protected set; }

        /// <summary>
        /// Use to create a new ChipInfo. Make sure to call Register on it to register your Chip.
        /// </summary>
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

        /// <summary>
        /// Use to create a new ChipInfo. Make sure to call Register on it to register your Chip.
        /// </summary>
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

        /// <summary>
        /// Registers this ChipInfo to the ChipRegistry.
        /// </summary>
        /// <param name="name">The registry name to use.</param>
        /// <param name="preferredID">If specified, will use this registry ID.</param>
        /// <param name="overrideExisting">If false, will not register if the preferred ID is already used. Ignored if no preferred ID is specified.</param>
        public virtual ChipInfo Register(string name, int preferredID = -1, bool overrideExisting = true)
        {
            return RegisterInternal(name, preferredID, overrideExisting) as ChipInfo;
        }

        /// <summary>
        /// Called after this Registry Entry has been registered to its Registry. You should never call this yourself.
        /// </summary>
        public override void PostRegister()
        {
            if (Mat == null)
            {
                Mat = new Material(Shader.Find("Unlit/Transparent"))
                {
                    mainTexture = Tex
                };
            }
            else
            {
                Tex = Mat.mainTexture;
            }
            GadgetCoreAPI.AddCustomResource("cc/cc" + ID, Mat);
        }

        /// <summary>
        /// Returns the Registry Entry's Type enum. Used in the registration process, although it is safe to check this yourself by directly accessing the <see cref="Type"/> property.
        /// </summary>
        public override ChipType GetEntryType()
        {
            return Type;
        }

        /// <summary>
        /// Returns the singleton of the registry used for storing this type of Registry Entry.
        /// </summary>
        public override Registry<ChipInfo, ChipType> GetRegistry()
        {
            return Registry<ChipRegistry, ChipInfo, ChipType>.GetSingleton();
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
        /// Checks if the chip should be considered to currently be in use, and should not be allowed to be activated again. Override to add custom behavior - by default, always returns false.
        /// </summary>
        public virtual bool IsChipActive(int slot) { return false; }

        /// <summary>
        /// Invoked whenever this chip is activated. Will never be invoked if the ChipType is not ACTIVE. The int parameter is the slot the chip is in.
        /// </summary>
        public event Action<int> OnUse;
        /// <summary>
        /// Invoked whenever this chip is equipped. The int parameter is the slot the chip is being equipped to. This is invoked immediately after the chip is placed into the slot.
        /// </summary>
        public event Action<int> OnEquip;
        /// <summary>
        /// Invoked whenever this chip is dequipped. The int parameter is the slot the chip is being dequipped from. This is invoked immediately before the chip is removed from the slot.
        /// </summary>
        public event Action<int> OnDequip;

        internal void InvokeOnUse(int slot) { OnUse?.Invoke(slot); }
        internal void InvokeOnEquip(int slot) { OnEquip?.Invoke(slot); }
        internal void InvokeOnDequip(int slot) { OnDequip?.Invoke(slot); }

        /// <summary>
        /// This indicates what should the active chip's cost represent.
        /// </summary>
        public enum ChipCostType
        {
            /// <summary>
            /// The cost represents a number of points of mana, as normal chips do.
            /// </summary>
            MANA,
            /// <summary>
            /// The cost represents a number of points of stamina.
            /// </summary>
            ENERGY,
            /// <summary>
            /// The cost represents a number of points of health, although activating the chip will be unable to kill the player, and if it would the activation fails.
            /// </summary>
            HEALTH_SAFE,
            /// <summary>
            /// The cost represents a number of points of health, and activating the chip is able to kill the player if they have less than or equal to the number of points of health that the cost requires. If the player dies from attempting to activate this chip, then the effect does not get activated.
            /// </summary>
            HEALTH_LETHAL,
            /// <summary>
            /// The cost represents a number of points of health, and activating the chip is able to kill the player if they have less than or equal to the number of points of health that the cost requires. If the player dies from attempting to activate this chip, then the effect still activates. Note that at the moment of invocation, the player well have 0 or less health, and will be flagged as dead (GameScript.dead == true) but the death screen will not have opened yet, and the death will be canceled if the chip restores the player's health.
            /// </summary>
            HEALTH_LETHAL_POSTMORTEM
        }
    }
}
