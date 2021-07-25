using System;

namespace GadgetCore.API
{
    /// <summary>
    /// Required by classes that extend Gadget. Provides information about your Gadget.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class GadgetAttribute : Attribute
    {
        /// <summary>
        /// The friendly name of this Gadget. If you change this after your initial release, make sure to override GetPreviousModNames to prevent registry data corruption!
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// Whether multiplayer clients attempting to connect without this Gadget should be rejected.
        /// </summary>
        public readonly bool RequiredOnClients;
        /// <summary>
        /// Whether the Gadget should default to being enabled or not when first installed.
        /// </summary>
        public readonly bool EnableByDefault;
        /// <summary>
        /// The list of any and all Gadgets that this Gadget should load after, regardless of <see cref="LoadPriority"/>
        /// </summary>
        public readonly string[] LoadAfter;
        /// <summary>
        /// The list of any and all Gadgets that this Gadget should load before, regardless of <see cref="LoadPriority"/>
        /// </summary>
        public readonly string[] LoadBefore;
        /// <summary>
        /// The priority for loading your Gadget. Higher number means the Gadget will be loaded earlier in the loading process.
        /// </summary>
        public readonly int LoadPriority;
        /// <summary>
        /// The names of any and all other Gadgets that your Gadget is dependent upon. If they are not present, your Gadget will not be loaded, and if your Gadget has the same load priority as them, your Gadget will be guaranteed to load after them. May or may not include version numbers.
        /// </summary>
        public readonly string[] Dependencies;
        /// <summary>
        /// Indicates whether this Gadget can be reloaded at runtime. Gadget reloads will also cause the config to be reloaded. Defaults to true.
        /// </summary>
        public readonly bool AllowRuntimeReloading;
        /// <summary>
        /// Indicates whether this Gadget can have its config reloaded at runtime, without reloading the entire Gadget. Defaults to true.
        /// </summary>
        public readonly bool AllowConfigReloading;
        /// <summary>
        /// The point at which a difference between the Gadget versions of the host and client should flag an incompatibility.
        /// </summary>
        public readonly VersionSpecificity GadgetVersionSpecificity;
        /// <summary>
        /// The point at which a change to GadgetCore should flag this Gadget as incompatible.
        /// </summary>
        public readonly VersionSpecificity GadgetCoreVersionSpecificity;
        /// <summary>
        /// The version of GadgetCore your Gadget is made for.
        /// </summary>
        public readonly string TargetGCVersion;

        /// <summary>
        /// Required by classes that extend Gadget. Provides information about your Gadget.
        /// </summary>
        /// <param name="Name">The friendly name of this Gadget. Also used for registry names. Should not contain special characters.</param>
        /// <param name="RequiredOnClients">Whether multiplayer clients attempting to connect without this Gadget should be rejected.</param>
        /// <param name="EnableByDefault">Whether the Gadget should default to being enabled or not when first installed.</param>
        /// <param name="LoadAfter">The list of any and all Gadgets that this Gadget should load after, regardless of <paramref name="LoadPriority"></paramref></param>
        /// <param name="LoadBefore">The list of any and all Gadgets that this Gadget should load before, regardless of <paramref name="LoadPriority"></paramref></param>
        /// <param name="LoadPriority">The priority for loading your Gadget. Higher number means higher priority. Leave at 0 if you don't care.</param>
        /// <param name="Dependencies">The names of any and all other Gadgets that your Gadget is dependent upon. If they are not present, your Gadget will not be loaded, and if your Gadget has the same load priority as them, your Gadget will be guaranteed to load after them. May include version numbers with the syntax of \"ModName v1.0\". The level of precision used in the specified version number indicates the version specificity of the dependency.</param>
        /// <param name="AllowRuntimeReloading">Whether this Gadget can be reloaded at runtime. Gadget reloads will also cause the config to be reloaded. Defaults to true.</param>
        /// <param name="AllowConfigReloading">Whether this Gadget can have its config reloaded at runtime, without reloading the entire Gadget. Defaults to true.</param>
        /// <param name="GadgetVersionSpecificity">The point at which a difference between the mod versions of the host and client should flag an incompatibility.</param>
        /// <param name="GadgetCoreVersionSpecificity">The point at which a change to GadgetCore should flag this Gadget as incompatible.</param>
        /// <param name="TargetGCVersion">The version of GadgetCore your Gadget is made for. This is automatically assigned to the version of GadgetCore you are building with, so you should always leave this at default.</param>
        public GadgetAttribute(string Name, bool RequiredOnClients, bool EnableByDefault = true, string[] LoadAfter = null, string[] LoadBefore = null, int LoadPriority = 0, string[] Dependencies = null, bool AllowRuntimeReloading = true, bool AllowConfigReloading = true, VersionSpecificity GadgetVersionSpecificity = VersionSpecificity.MINOR, VersionSpecificity GadgetCoreVersionSpecificity = VersionSpecificity.MINOR, string TargetGCVersion = GadgetCoreAPI.RAW_VERSION)
        {
            this.Name = Name;
            this.RequiredOnClients = RequiredOnClients;
            this.EnableByDefault = EnableByDefault;
            this.LoadAfter = LoadAfter ?? new string[0];
            this.LoadBefore = LoadBefore ?? new string[0];
            this.LoadPriority = LoadPriority;
            this.Dependencies = Dependencies ?? new string[0];
            this.AllowRuntimeReloading = AllowRuntimeReloading;
            this.AllowConfigReloading = AllowConfigReloading;
            this.GadgetVersionSpecificity = GadgetVersionSpecificity;
            this.GadgetCoreVersionSpecificity = GadgetCoreVersionSpecificity;
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
