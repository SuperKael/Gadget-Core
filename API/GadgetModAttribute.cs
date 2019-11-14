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
        /// <summary>
        /// The friendly name of this mod. If you change this after your initial release, make sure to override GetPreviousModNames to prevent registry data corruption!
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// Whether multiplayer clients attempting to connect without this mod should be rejected.
        /// </summary>
        public readonly bool RequiredOnClients;
        /// <summary>
        /// Whether the mod should default to being enabled or not when first installed.
        /// </summary>
        public readonly bool EnableByDefault;
        /// <summary>
        /// The priority for loading your mod. Higher number means higher priority.
        /// </summary>
        public readonly int LoadPriority;
        /// <summary>
        /// The names of any and all other Gadget mods that your mod is dependent upon. If they are not present, your mod will not be loaded, and if your mod has the same load priority as them, your mod will be guaranteed to load after them. May or may not include version numbers.
        /// </summary>
        public readonly string[] Dependencies;
        /// <summary>
        /// The point at which a difference between the mod versions of the host and client should flag an incompatibility.
        /// </summary>
        public readonly VersionSpecificity ModVersionSpecificity;
        /// <summary>
        /// The point at which a change to GadgetCore should flag this mod as incompatible.
        /// </summary>
        public readonly VersionSpecificity GadgetVersionSpecificity;
        /// <summary>
        /// The version of GadgetCore your mod is made for.
        /// </summary>
        public readonly string TargetGCVersion;

        /// <summary>
        /// Required by classes that extend GadgetMod. Provides information about your mod.
        /// </summary>
        /// <param name="Name">The friendly name of this mod. Also used for registry names. Should not contain special characters.</param>
        /// <param name="RequiredOnClients">Whether multiplayer clients attempting to connect without this mod should be rejected.</param>
        /// <param name="EnableByDefault">Whether the mod should default to being enabled or not when first installed.</param>
        /// <param name="LoadPriority">The priority for loading your mod. Higher number means higher priority. Leave at 0 if you don't care.</param>
        /// <param name="Dependencies">The names of any and all other Gadget mods that your mod is dependent upon. If they are not present, your mod will not be loaded, and if your mod has the same load priority as them, your mod will be guaranteed to load after them. May include version numbers with the syntax of \"ModName v1.0\". The level of precision used in the specified version number indicates the version specificity of the dependency.</param>
        /// <param name="ModVersionSpecificity">The point at which a difference between the mod versions of the host and client should flag an incompatibility.</param>
        /// <param name="GadgetVersionSpecificity">The point at which a change to GadgetCore should flag this mod as incompatible.</param>
        /// <param name="TargetGCVersion">The version of GadgetCore your mod is made for. This is automatically assigned to the version of GadgetCore you are building with, so you should always leave this at default.</param>
        public GadgetModAttribute(string Name, bool RequiredOnClients = true, bool EnableByDefault = true, int LoadPriority = 0, string[] Dependencies = null, VersionSpecificity ModVersionSpecificity = VersionSpecificity.MINOR, VersionSpecificity GadgetVersionSpecificity = VersionSpecificity.MINOR, string TargetGCVersion = GadgetCoreAPI.VERSION)
        {
            this.Name = Name;
            this.RequiredOnClients = RequiredOnClients;
            this.EnableByDefault = EnableByDefault;
            this.LoadPriority = LoadPriority;
            this.Dependencies = Dependencies ?? new string[0];
            this.ModVersionSpecificity = ModVersionSpecificity;
            this.GadgetVersionSpecificity = GadgetVersionSpecificity;
            this.TargetGCVersion = TargetGCVersion;
        }
    }

    /// <summary>
    /// Used to specify how specific a version number comparison should be.
    /// </summary>
    public enum VersionSpecificity
    {
        /// <summary>
        /// Only compare against the major version number.
        /// </summary>
        MAJOR = 1,
        /// <summary>
        /// Compare against the major and minor version numbers.
        /// </summary>
        MINOR = 2,
        /// <summary>
        /// Compare against the major, minor, and nonbreaking version numbers.
        /// </summary>
        NONBREAKING = 3,
        /// <summary>
        /// Compare against the major, minor, nonbreaking, and bugfix version numbers.
        /// </summary>
        BUGFIX = 4
    }
}
