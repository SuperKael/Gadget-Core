using GadgetCore.API;
using System.Collections.Generic;
using UnityEngine;

namespace GadgetCore
{
    /// <summary>
    /// Represents an entry in the mod menu. Can be relevant when working with config menus.
    /// </summary>
    public sealed class ModMenuEntry
    {
        /// <summary>
        /// The name of this entry.
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// The type of this entry.
        /// </summary>
        public readonly ModMenuEntryType Type;
        /// <summary>
        /// The description of this entry.
        /// </summary>
        public readonly string Description;
        /// <summary>
        /// A dictionary filled with extra information about this entry.
        /// </summary>
        public readonly Dictionary<string, string> Info;
        /// <summary>
        /// The <see cref="GadgetInfo"/>s from the mod represented by this entry.
        /// </summary>
        public readonly GadgetInfo[] Gadgets;
        /// <summary>
        /// This ModMenuEntry's transform, if applicable.
        /// </summary>
        public RectTransform Transform { get; internal set; }

        internal ModMenuEntry(string Name, ModMenuEntryType Type, string Description, Dictionary<string, string> Info = null, params GadgetInfo[] Gadgets)
        {
            this.Name = Name;
            this.Type = Type;
            this.Description = Description;
            this.Info = Info ?? new Dictionary<string, string>();
            this.Gadgets = Gadgets;
        }
    }

    /// <summary>
    /// Represents the type of entry that a <see cref="ModMenuEntry"/> represents.
    /// </summary>
    public enum ModMenuEntryType
    {
        /// <summary>
        /// A Gadget mod that loaded succesfully and contains at least one Gadget.
        /// </summary>
        GADGET,
        /// <summary>
        /// A Gadget mod that didn't load because it doesn't contain any Gadgets.
        /// </summary>
        EMPTY_GADGET,
        /// <summary>
        /// A Gadget mod that didn't load because it is missing required dependencies.
        /// </summary>
        INCOMPATIBLE_GADGET,
        /// <summary>
        /// A Gadget mod that didn't load due to an error in its load process.
        /// </summary>
        ERRORED_GADGET,
        /// <summary>
        /// A UMF mod that may or may not contain Gadgets.
        /// </summary>
        UMF,
        /// <summary>
        /// A UMF mod that is disabled. May contain Gadgets, although they will not be known.
        /// </summary>
        DISABLED_UMF,
        /// <summary>
        /// A UMF mod that didn't load because of an assembly reference that failed to resolve.
        /// </summary>
        INCOMPATIBLE_UMF
    }
}
