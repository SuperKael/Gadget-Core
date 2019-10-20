using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GadgetCore.API
{
    /// <summary>
    /// Required by classes that extend GadgetMod. Provides information about your mod.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class GadgetModAttribute : Attribute
    {
        public readonly string Name;
        public readonly bool EnableByDefault;
        public readonly int LoadPriority;
        public readonly VersionSpecificity VersionSpecificity;
        public readonly string TargetGCVersion;

        /// <summary>
        /// Required by classes that extend GadgetMod. Provides information about your mod.
        /// </summary>
        /// <param name="Name">The friendly name of your mod</param>
        /// <param name="EnableByDefault">Whether the mod should default to being enabled or not when first installed.</param>
        /// <param name="LoadPriority">The priority for loading your mod. Higher number means higher priority. Leave at 0 if you don't care.</param>
        /// <param name="VersionSpecificity">The point at which a change to GadgetCore so flag this mod as incompatible</param>
        /// <param name="TargetGCVersion">The version of GadgetCore your mod is made for. Always leave this at default.</param>
        public GadgetModAttribute(string Name, bool EnableByDefault = true, int LoadPriority = 0, VersionSpecificity VersionSpecificity = VersionSpecificity.MINOR, string TargetGCVersion = GadgetCoreAPI.VERSION)
        {
            this.Name = Name;
            this.EnableByDefault = EnableByDefault;
            this.LoadPriority = LoadPriority;
            this.VersionSpecificity = VersionSpecificity;
            this.TargetGCVersion = TargetGCVersion;
        }
    }

    public enum VersionSpecificity
    {
        MAJOR = 1,
        MINOR = 2,
        NONBREAKING = 3,
        BUGFIX = 4
    }
}
