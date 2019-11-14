using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GadgetCore.API
{
    /// <summary>
    /// This is a container for a Gadget Mod used for tracking various pieces of information about it.
    /// </summary>
    public sealed class GadgetModInfo
    {
        /// <summary>
        /// The <see cref="GadgetMod"/> that this GadgetModInfo describes.
        /// </summary>
        public readonly GadgetMod Mod;

        /// <summary>
        /// The <see cref="System.Reflection.Assembly"/> that contains this Gadget mod.
        /// </summary>
        public readonly Assembly Assembly;
        /// <summary>
        /// The <see cref="GadgetModAttribute"/> for this Gadget mod.
        /// </summary>
        public readonly GadgetModAttribute Attribute;
        /// <summary>
        /// The name of the UMF mod that contains this Gadget mod.
        /// </summary>
        public readonly string UMFName;

        internal GadgetModInfo(GadgetMod Mod, Assembly Assembly, GadgetModAttribute Attribute, string Name)
        {
            this.Mod = Mod;
            this.Assembly = Assembly;
            this.Attribute = Attribute;
            UMFName = Name;
        }
    }
}
