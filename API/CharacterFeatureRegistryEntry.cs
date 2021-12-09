using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GadgetCore.API
{
    /// <summary>
    /// This registry entry is an abstract base class for registry entries that add custom character features, such as races, uniforms, and augments.
    /// </summary>
    public abstract class CharacterFeatureRegistryEntry<E> : RegistryEntry<E, CharacterFeatureType>, ICharacterFeatureRegistryEntry where E : CharacterFeatureRegistryEntry<E>
    {
        /// <summary>
        /// The name of this character feature.
        /// </summary>
        public virtual string Name { get; protected set; }
        /// <summary>
        /// The description of this character feature.
        /// </summary>
        public virtual string Desc { get; protected set; }
        /// <summary>
        /// The unlock condition for this character feature, as shown on the character creation feature selector.
        /// Keep in mind that this is just text, and does not actually have anything to do with how this character feature is functionally unlocked.
        /// </summary>
        public virtual string UnlockCondition { get; protected set; }
        /// <summary>
        /// Invoked when the player dies to see if they should get an unlock chest for this feature. Only invoked if <see cref="CanUnlock"/> returns true.
        /// </summary>
        public virtual Func<bool> UnlockChecker { get; protected set; }

        /// <summary>
        /// The Texture associated with this character feature. May be null.
        /// </summary>
        public virtual Texture Tex { get; protected set; }
        /// <summary>
        /// The Texture associated with this character feature's icon on the character creation feature selector.
        /// </summary>
        public virtual Texture SelectorIconTex { get; protected set; }

        /// <summary>
        /// The Material associated with this character feature. May be null.
        /// </summary>
        public virtual Material Mat { get; protected set; }
        /// <summary>
        /// The Material associated with this character feature's icon on the character creation feature selector.
        /// </summary>
        public virtual Material SelectorIconMat { get; protected set; }

        private readonly int ChestID;

        /// <summary>
        /// Base constructor for character features.
        /// </summary>
        protected CharacterFeatureRegistryEntry(string Name, string Desc, string UnlockCondition, Texture SelectorIconTex, Texture Tex)
        {
            this.Name = Name;
            this.Desc = Desc;
            this.UnlockCondition = UnlockCondition;
            this.Tex = Tex;
            this.SelectorIconTex = SelectorIconTex;
            ChestID = PatchMethods.characterFeatureRegistryEntries.Count + 10000;
            PatchMethods.characterFeatureRegistryEntries[ChestID] = this;
        }

        /// <summary>
        /// Base constructor for character features.
        /// </summary>
        protected CharacterFeatureRegistryEntry(string Name, string Desc, string UnlockCondition, Material SelectorIconMat, Material Mat)
        {
            this.Name = Name;
            this.Desc = Desc;
            this.UnlockCondition = UnlockCondition;
            this.Mat = Mat;
            this.SelectorIconMat = SelectorIconMat;
            ChestID = PatchMethods.characterFeatureRegistryEntries.Count + 10000;
            PatchMethods.characterFeatureRegistryEntries[ChestID] = this;
        }

        /// <summary>
        /// Returns the ID of this feature for use in GameScript.AddChest
        /// </summary>
        public int GetChestID()
        {
            return ChestID;
        }

        /// <summary>
        /// Gets the name of this character feature.
        /// </summary>
        public string GetName()
        {
            return Name;
        }

        /// <summary>
        /// Gets the description of this character feature.
        /// </summary>
        public string GetDesc()
        {
            return Desc;
        }

        /// <summary>
        /// Gets the unlock condition of this character feature, as shown on the character creation feature selector.
        /// Keep in mind that this is just text, and does not actually have anything to do with how this character feature is functionally unlocked.
        /// </summary>
        public string GetUnlockCondition()
        {
            return UnlockCondition;
        }

        /// <summary>
        /// Gets the name of this character feature as it appears when it has not yet been unlocked.
        /// </summary>
        public virtual string GetLockedName()
        {
            return "???";
        }

        /// <summary>
        /// Gets the description of this character feature as it appears when it has not yet been unlocked.
        /// </summary>
        public virtual string GetLockedDesc()
        {
            return "???";
        }

        /// <summary>
        /// Gets the unlock condition of this character feature, as shown on the character creation feature selector, as it appears when ti has not yet been unlocked.
        /// Keep in mind that this is just text, and does not actually have anything to do with how this character feature is functionally unlocked.
        /// </summary>
        public virtual string GetLockedUnlockCondition()
        {
            return GetUnlockCondition();
        }

        /// <summary>
        /// Called when the player opens an unlock chest for this feature on the death screen.
        /// </summary>
        public virtual void OpenChest()
        {
            SetFeatureUnlocked(GetFeatureUnlockLevel() + 1);
            InstanceTracker.GameScript.txtUnlockedType[0].text = "NEW " + GetRegistry().GetRegistryName().ToUpper() + " UNLOCKED";
            InstanceTracker.GameScript.txtUnlockedName[0].text = Name;
            InstanceTracker.GameScript.unlockedObj.GetComponent<Renderer>().material = SelectorIconMat;
            InstanceTracker.GameScript.unlockedObj.transform.localPosition = new Vector3(0f, 0.001f, 0.013f);
            InstanceTracker.GameScript.unlockedIcon.transform.parent.localScale = new Vector3(0.5f, 0.5f, 1);
        }

        /// <summary>
        /// Called when the player dies to see if they should get an unlock chest for this feature.
        /// </summary>
        public virtual bool ShouldUnlock()
        {
            return CanUnlock() && (UnlockChecker == null || UnlockChecker());
        }

        /// <summary>
        /// Checks whether this feature can be unlocked - by default, returns false if it is already unlocked.
        /// </summary>
        public virtual bool CanUnlock()
        {
            return !IsFeatureUnlocked();
        }

        /// <summary>
        /// Returns the unlock level of the specified feature.
        /// </summary>
        public virtual int GetFeatureUnlockLevel()
        {
            return ((ICharacterFeatureRegistry)GetRegistry()).GetFeatureUnlockLevel(ID);
        }

        /// <summary>
        /// Returns whether the character feature with the specified ID is unlocked.
        /// </summary>
        public virtual bool IsFeatureUnlocked()
        {
            return ((ICharacterFeatureRegistry)GetRegistry()).IsFeatureUnlocked(ID);
        }

        /// <summary>
        /// Sets the unlock level of the specified feature. Setting it to 0 locks the feature.
        /// </summary>
        public virtual void SetFeatureUnlocked(int unlockLevel = 1)
        {
            ((ICharacterFeatureRegistry)GetRegistry()).SetFeatureUnlocked(ID, unlockLevel);
        }

        /// <summary>
        /// This event is invoked whenever the player levels up. The passed int is the level that the player just reached.
        /// </summary>
        public event Action<int> OnLevelUp;

        /// <summary>
        /// Invokes this character feature's OnLevelUp event. Generally, you should not call this yourself.
        /// </summary>
        protected internal void InvokeOnLevelUp(int level) { OnLevelUp?.Invoke(level); }

        /// <summary>
        /// Called after this Registry Entry has been registered to its Registry. Be sure to call this from your subclass if you override it. Otherwise, you should never call this yourself.
        /// </summary>
        protected internal override void PostRegister()
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
            if (SelectorIconMat == null)
            {
                SelectorIconMat = new Material(Shader.Find("Unlit/Transparent"))
                {
                    mainTexture = SelectorIconTex
                };
            }
            else
            {
                SelectorIconTex = SelectorIconMat.mainTexture;
            }
        }

        /// <summary>
        /// Returns the Registry Entry's Type enum. Used in the registration process, although it is safe to check this yourself by directly accessing the <see cref="Type"/> property.
        /// </summary>
        public override CharacterFeatureType GetEntryType()
        {
            return ((ICharacterFeatureRegistry)GetRegistry()).GetFeatureType();
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
        /// Returns whether the specified ID is valid for this Registry Entry's Type.
        /// </summary>
        public override bool IsValidIDForType(int id)
        {
            return id > 0;
        }
    }

    /// <summary>
    /// CharacterFeatureRegistryEntry interface without the self-referencing supertype.
    /// </summary>
    public interface ICharacterFeatureRegistryEntry
    {
        /// <summary>
        /// Gets the name of this character feature.
        /// </summary>
        string GetName();
        /// <summary>
        /// Gets the description of this character feature.
        /// </summary>
        string GetDesc();
        /// <summary>
        /// Gets the unlock condition of this character feature, as shown on the character creation feature selector.
        /// Keep in mind that this is just text, and does not actually have anything to do with how this character feature is functionally unlocked.
        /// </summary>
        string GetUnlockCondition();
        /// <summary>
        /// Gets the name of this character feature as it appears when it has not yet been unlocked.
        /// </summary>
        string GetLockedName();
        /// <summary>
        /// Gets the description of this character feature as it appears when it has not yet been unlocked.
        /// </summary>
        string GetLockedDesc();
        /// <summary>
        /// Gets the unlock condition of this character feature, as shown on the character creation feature selector, as it appears when it has not yet been unlocked.
        /// Keep in mind that this is just text, and does not actually have anything to do with how this character feature is functionally unlocked.
        /// </summary>
        string GetLockedUnlockCondition();
        /// <summary>
        /// Returns the ID of this feature for use in GameScript.AddChest
        /// </summary>
        int GetChestID();
        /// <summary>
        /// Called when the player opens an unlock chest for this feature on the death screen.
        /// </summary>
        void OpenChest();
        /// <summary>
        /// Called when the player dies to see if they should get an unlock chest for this feature.
        /// </summary>
        bool ShouldUnlock();

        /// <summary>
        /// Checks whether this feature can be unlocked - by default, returns false if it is already unlocked.
        /// </summary>
        bool CanUnlock();
    }
}
