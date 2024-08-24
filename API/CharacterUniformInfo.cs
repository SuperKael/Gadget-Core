using UnityEngine;

namespace GadgetCore.API
{
    /// <summary>
    /// Defines a custom character Uniform. Make sure to call Register on it to register your Uniform.
    /// </summary>
    public class CharacterUniformInfo : CharacterFeatureRegistryEntry<CharacterUniformInfo>
    {
        /// <summary>
        /// Use to create a new UniformInfo. Make sure to call Register on it to register your Uniform. Note that there must be at least one variant provided, as this is the default form of the uniform.
        /// </summary>
        public CharacterUniformInfo(string Name, string Desc, string UnlockCondition, Texture SelectorIconTex, Texture Tex) : base(Name, Desc, UnlockCondition, SelectorIconTex, Tex) { }
        /// <summary>
        /// Use to create a new UniformInfo. Make sure to call Register on it to register your Uniform. Note that there must be at least one variant provided, as this is the default form of the uniform.
        /// </summary>
        public CharacterUniformInfo(string Name, string Desc, string UnlockCondition, Material SelectorIconMat, Material Mat) : base(Name, Desc, UnlockCondition, SelectorIconMat, Mat) { }

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
        /// Registers this CharacterUniformInfo to the CharacterUniformRegistry.
        /// </summary>
        /// <param name="preferredID">If specified, will use this registry ID.</param>
        /// <param name="overrideExisting">If false, will not register if the preferred ID is already used. Ignored if no preferred ID is specified.</param>
        public virtual CharacterUniformInfo Register(int preferredID = -1, bool overrideExisting = true)
        {
            return RegisterInternal(Name, preferredID, overrideExisting) as CharacterUniformInfo;
        }

        /// <summary>
        /// Registers this CharacterUniformInfo to the CharacterUniformRegistry.
        /// </summary>
        /// <param name="name">The registry name to use.</param>
        /// <param name="preferredID">If specified, will use this registry ID.</param>
        /// <param name="overrideExisting">If false, will not register if the preferred ID is already used. Ignored if no preferred ID is specified.</param>
        public virtual CharacterUniformInfo Register(string name, int preferredID = -1, bool overrideExisting = true)
        {
            return RegisterInternal(name, preferredID, overrideExisting) as CharacterUniformInfo;
        }

        /// <summary>
        /// Called after this Registry Entry has been registered to its Registry. You should never call this yourself.
        /// </summary>
        protected internal override void PostRegister()
        {
            base.PostRegister();
            GadgetCoreAPI.AddCustomResource("b/b" + ID, Mat);
        }

        /// <summary>
        /// Returns the singleton of the registry used for storing this type of Registry Entry.
        /// </summary>
        public override Registry<CharacterUniformInfo, CharacterFeatureType> GetRegistry()
        {
            return CharacterUniformRegistry.Singleton;
        }
    }
}
