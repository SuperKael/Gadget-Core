using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GadgetCore.API
{
    /// <summary>
    /// Indicates what Gadget owns this patch. Required for use of GadgetCore's automatic Harmony patching.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class HarmonyGadgetAttribute : Attribute
    {
        /// <summary>
        /// The name of the <see cref="Gadget"/> that owns this patch.
        /// </summary>
        public readonly string Gadget;

        /// <summary>
        /// Indicates what Gadget owns this patch. Required for use of GadgetCore's automatic Harmony patching.
        /// </summary>
        /// <param name="Gadget">The name of the <see cref="Gadget"/> that owns this patch.</param>
        public HarmonyGadgetAttribute(string Gadget)
        {
            this.Gadget = Gadget;
        }
    }
}
