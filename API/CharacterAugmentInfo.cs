using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GadgetCore.API
{
    /// <summary>
    /// Defines a custom character Augment. Make sure to call Register on it to register your Augment.
    /// </summary>
    public class CharacterAugmentInfo : CharacterFeatureRegistryEntry<CharacterAugmentInfo>
    {
        /// <summary>
        /// Use to create a new AugmentInfo. Make sure to call Register on it to register your Augment. Note that there must be at least one variant provided, as this is the default form of the augment.
        /// </summary>
        public CharacterAugmentInfo(string Name, string Desc, string UnlockCondition, Texture SelectorIconTex, Texture Tex) : base(Name, Desc, UnlockCondition, SelectorIconTex, Tex) { }
        /// <summary>
        /// Use to create a new AugmentInfo. Make sure to call Register on it to register your Augment. Note that there must be at least one variant provided, as this is the default form of the augment.
        /// </summary>
        public CharacterAugmentInfo(string Name, string Desc, string UnlockCondition, Material SelectorIconMat, Material Mat) : base(Name, Desc, UnlockCondition, SelectorIconMat, Mat) { }

        /// <summary>
        /// Gets the name of this character feature as it appears when it has not yet been unlocked.
        /// </summary>
        public override string GetLockedName()
        {
            return string.Empty;
        }

        /// <summary>
        /// Gets the description of this character feature as it appears when it has not yet been unlocked.
        /// </summary>
        public override string GetLockedDesc()
        {
            return string.Empty;
        }

        /// <summary>
        /// Registers this CharacterAugmentInfo to the CharacterAugmentRegistry.
        /// </summary>
        /// <param name="preferredID">If specified, will use this registry ID.</param>
        /// <param name="overrideExisting">If false, will not register if the preferred ID is already used. Ignored if no preferred ID is specified.</param>
        public virtual CharacterAugmentInfo Register(int preferredID = -1, bool overrideExisting = true)
        {
            return RegisterInternal(Name, preferredID, overrideExisting) as CharacterAugmentInfo;
        }

        /// <summary>
        /// Registers this CharacterAugmentInfo to the CharacterAugmentRegistry.
        /// </summary>
        /// <param name="name">The registry name to use.</param>
        /// <param name="preferredID">If specified, will use this registry ID.</param>
        /// <param name="overrideExisting">If false, will not register if the preferred ID is already used. Ignored if no preferred ID is specified.</param>
        public virtual CharacterAugmentInfo Register(string name, int preferredID = -1, bool overrideExisting = true)
        {
            return RegisterInternal(name, preferredID, overrideExisting) as CharacterAugmentInfo;
        }

        /// <summary>
        /// Called after this Registry Entry has been registered to its Registry. You should never call this yourself.
        /// </summary>
        protected internal override void PostRegister()
        {
            base.PostRegister();
            GadgetCoreAPI.AddCustomResource("aug/aug" + ID, Mat);
        }

        /// <summary>
        /// Returns the singleton of the registry used for storing this type of Registry Entry.
        /// </summary>
        public override Registry<CharacterAugmentInfo, CharacterFeatureType> GetRegistry()
        {
            return CharacterAugmentRegistry.Singleton;
        }
    }
}
