using System;
using System.Linq;
using UnityEngine;

namespace GadgetCore.API
{
    /// <summary>
    /// Defines a custom Allegiance. Make sure to call Register on it to register your Object.
    /// </summary>
    public class AllegianceInfo : UnaryTypedRegistryEntry<AllegianceInfo>
    {
        /// <summary>
        /// The name of this Allegiance
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The Texture associated with this Object. May be null.
        /// </summary>
        public virtual Texture FlagTex { get; protected set; }

        /// <summary>
        /// The Material associated with this Object. May be null.
        /// </summary>
        public virtual Material FlagMat { get; protected set; }

        /// <summary>
        /// Use to create a new AllegianceInfo. Make sure to call Register on it to register your Allegiance.
        /// </summary>
        public AllegianceInfo(string Name, Texture FlagTex)
        {
            this.Name = Name;
            this.FlagTex = FlagTex;
        }

        /// <summary>
        /// Use to create a new AllegianceInfo. Make sure to call Register on it to register your Allegiance.
        /// </summary>
        public AllegianceInfo(string Name, Material FlagMat)
        {
            this.Name = Name;
            this.FlagMat = FlagMat;
        }

        /// <summary>
        /// Registers this AllegianceInfo to the AllegianceRegistry.
        /// </summary>
        /// <param name="name">The registry name to use.</param>
        /// <param name="preferredID">If specified, will use this registry ID.</param>
        /// <param name="overrideExisting">If false, will not register if the preferred ID is already used. Ignored if no preferred ID is specified.</param>
        public virtual AllegianceInfo Register(string name, int preferredID = -1, bool overrideExisting = true)
        {
            return RegisterInternal(name, preferredID, overrideExisting) as AllegianceInfo;
        }

        /// <summary>
        /// Called after this Registry Entry has been registered to its Registry. You should never call this yourself.
        /// </summary>
        protected internal override void PostRegister()
        {
            if (FlagMat == null)
            {
                FlagMat = new Material(Shader.Find("Unlit/Transparent Cutout"))
                {
                    mainTexture = FlagTex
                };
                FlagMat.SetFloat("_Cutoff", 0.5f);
            }
            else
            {
                FlagTex = FlagMat.mainTexture;
            }
            GadgetCoreAPI.AddCustomResource("flag/flag" + GetID(), FlagMat);
        }

        /// <summary>
        /// Returns the singleton of the registry used for storing this type of Registry Entry.
        /// </summary>
        public override Registry<AllegianceInfo, UnaryRegistryType> GetRegistry()
        {
            return AllegianceRegistry.Singleton;
        }

        /// <summary>
        /// Returns whether the specified ID is valid for this Registry.
        /// </summary>
        public override bool IsValidID(int id)
        {
            return id >= 0;
        }

        /// <summary>
        /// Returns the next valid ID for this Registry, after the provided lastValidID. Should skip the vanilla ID range.
        /// </summary>
        public override int GetNextID(int lastValidID)
        {
            if (lastValidID < GetRegistry().GetIDStart() - 1) lastValidID = GetRegistry().GetIDStart() - 1;
            return ++lastValidID;
        }
    }
}
