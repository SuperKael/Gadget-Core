using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GadgetCore.API
{
    /// <summary>
    /// Defines a custom character Race. Make sure to call Register on it to register your Race.
    /// </summary>
    public class CharacterRaceInfo : CharacterFeatureRegistryEntry<CharacterRaceInfo>
    {
        /// <summary>
        /// The Textures associated with this race's variants. May be null, but never empty.
        /// </summary>
        public virtual Texture[] VariantTexes { get; protected set; }

        /// <summary>
        /// The Materials associated with this race's variants. May be null, but never empty.
        /// </summary>
        public virtual Material[] VariantMats { get; protected set; }

        /// <summary>
        /// The bonus base stats granted by this race. These stats are added to the player's stats every three levels.
        /// </summary>
        public virtual EquipStats RaceStats { get; protected set; }

        /// <summary>
        /// Use to create a new RaceInfo. Make sure to call Register on it to register your Race. Note that there must be at least one variant provided, as this is the default form of the race.
        /// </summary>
        public CharacterRaceInfo(string Name, string Desc, string UnlockCondition, EquipStats RaceStats, Texture SelectorIconTex, params Texture[] VariantTexes) : base(Name, Desc, UnlockCondition, SelectorIconTex, VariantTexes[0])
        {
            this.RaceStats = RaceStats;
            this.VariantTexes = VariantTexes;
        }
        /// <summary>
        /// Use to create a new RaceInfo. Make sure to call Register on it to register your Race. Note that there must be at least one variant provided, as this is the default form of the race.
        /// </summary>
        public CharacterRaceInfo(string Name, string Desc, string UnlockCondition, EquipStats RaceStats, Material SelectorIconMat, params Material[] VariantMats) : base(Name, Desc, UnlockCondition, SelectorIconMat, VariantMats[0])
        {
            this.RaceStats = RaceStats;
            this.VariantMats = VariantMats;
        }

        /// <summary>
        /// Returns the number of variants this race has. The default form is considered to be a variant, so by this definition all vanilla races have 3 variants.
        /// </summary>
        public virtual int GetVariantCount()
        {
            return VariantMats != null ? VariantMats.Length : VariantTexes.Length;
        }

        /// <summary>
        /// Registers this CharacterRaceInfo to the CharacterRaceRegistry.
        /// </summary>
        /// <param name="preferredID">If specified, will use this registry ID.</param>
        /// <param name="overrideExisting">If false, will not register if the preferred ID is already used. Ignored if no preferred ID is specified.</param>
        public virtual CharacterRaceInfo Register(int preferredID = -1, bool overrideExisting = true)
        {
            return RegisterInternal(Name, preferredID, overrideExisting) as CharacterRaceInfo;
        }

        /// <summary>
        /// Registers this CharacterRaceInfo to the CharacterRaceRegistry.
        /// </summary>
        /// <param name="name">The registry name to use.</param>
        /// <param name="preferredID">If specified, will use this registry ID.</param>
        /// <param name="overrideExisting">If false, will not register if the preferred ID is already used. Ignored if no preferred ID is specified.</param>
        public virtual CharacterRaceInfo Register(string name, int preferredID = -1, bool overrideExisting = true)
        {
            return RegisterInternal(name, preferredID, overrideExisting) as CharacterRaceInfo;
        }

        /// <summary>
        /// Called when the player opens an unlock chest for this feature on the death screen.
        /// </summary>
        public override void OpenChest()
        {
            int unlockLevel = GetFeatureUnlockLevel() + 1;
            if (unlockLevel > 1)
            {
                SetFeatureUnlocked(unlockLevel);
                InstanceTracker.GameScript.txtUnlockedType[0].text = "NEW VARIANT UNLOCKED";
                InstanceTracker.GameScript.txtUnlockedName[0].text = Name + " #" + unlockLevel;
                InstanceTracker.GameScript.unlockedObj.GetComponent<Renderer>().material = VariantMats[unlockLevel - 1];
                InstanceTracker.GameScript.unlockedObj.transform.localPosition = new Vector3(0f, 0.1267f, 0.013f);
            }
            else
            {
                base.OpenChest();
            }
        }

        /// <summary>
        /// Checks whether this feature can be unlocked - by default, returns false if it is already unlocked.
        /// </summary>
        public override bool CanUnlock()
        {
            return GetFeatureUnlockLevel() < GetVariantCount();
        }

        /// <summary>
        /// Called after this Registry Entry has been registered to its Registry. You should never call this yourself.
        /// </summary>
        protected internal override void PostRegister()
        {
            base.PostRegister();
            if (VariantMats == null)
            {
                VariantMats = new Material[VariantTexes.Length];
                for (int i = 0; i < VariantMats.Length; i++)
                {
                    VariantMats[i] = new Material(Shader.Find("Unlit/Transparent"))
                    {
                        mainTexture = VariantTexes[i]
                    };
                    GadgetCoreAPI.AddCustomResource("r/r" + ID + "v" + i, VariantMats[i]);
                }
            }
            else
            {
                VariantTexes = new Texture[VariantMats.Length];
                for (int i = 0; i < VariantTexes.Length; i++)
                {
                    VariantTexes[i] = VariantMats[i].mainTexture;
                    GadgetCoreAPI.AddCustomResource("r/r" + ID + "v" + i, VariantMats[i]);
                }
            }
        }

        /// <summary>
        /// Returns the singleton of the registry used for storing this type of Registry Entry.
        /// </summary>
        public override Registry<CharacterRaceInfo, CharacterFeatureType> GetRegistry()
        {
            return CharacterRaceRegistry.Singleton;
        }
    }
}
