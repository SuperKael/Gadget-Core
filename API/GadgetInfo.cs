using GadgetCore.Loader;
using GadgetCore.Util;
using System;

namespace GadgetCore.API
{
    /// <summary>
    /// This is a container for a Gadget used for tracking various pieces of information about it.
    /// </summary>
    public sealed class GadgetInfo
    {
        /// <summary>
        /// The Gadget that this GadgetInfo describes.
        /// </summary>
        public readonly Gadget Gadget;
        /// <summary>
        /// The <see cref="GadgetAttribute"/> for this Gadget.
        /// </summary>
        public readonly GadgetAttribute Attribute;
        /// <summary>
        /// The GadgetMod that contains this Gadget. Will be null if this Gadget is in a UMF mod.
        /// </summary>
        public readonly GadgetMod Mod;
        /// <summary>
        /// The name of the mod that contains this Gadget.
        /// </summary>
        public readonly string ModName;
        /// <summary>
        /// References to all of this Gadget's dependencies.
        /// </summary>
        public GadgetInfo[] Dependencies { get; internal set; }
        /// <summary>
        /// References to all Gadgets that are dependent on this Gadget.
        /// </summary>
        public GadgetInfo[] Dependents { get; internal set; }

        internal GadgetInfo(Gadget Gadget, GadgetAttribute Attribute, GadgetMod Mod)
        {
            this.Gadget = Gadget;
            this.Attribute = Attribute;
            this.Mod = Mod;
            ModName = Mod.Name;
            Gadget.Info = this;
        }

        internal GadgetInfo(Gadget Gadget, GadgetAttribute Attribute, string Mod)
        {
            this.Gadget = Gadget;
            this.Attribute = Attribute;
            ModName = Mod;
            Gadget.Info = this;
        }

        /// <summary>
        /// Calculates the load-order tree for Gadgets related to this one by their LoadBefore and LoadAfter attributes.
        /// </summary>
        public MultiTreeList<GadgetInfo> CalculateLoadTree(MultiTreeList<GadgetInfo> parent = null)
        {
            MultiTreeList<GadgetInfo> tree = new MultiTreeList<GadgetInfo>(this);
            return tree;
        }

        /// <summary>
        /// Converts this <see cref="GadgetInfo"/> into a human-readable string representation
        /// </summary>
        public override string ToString()
        {
            return Attribute.Name + " v" + Gadget.GetModVersionString();
        }
    }
}
