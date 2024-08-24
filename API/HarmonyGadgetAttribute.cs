using System;

namespace GadgetCore.API
{
    /// <summary>
    /// Indicates what Gadget owns this patch. Required for the use of GadgetCore's automatic Harmony patching.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class HarmonyGadgetAttribute : Attribute
    {
        /// <summary>
        /// The name of the <see cref="API.Gadget"/> that owns this patch.
        /// </summary>
        public readonly string Gadget;
        /// <summary>
        /// Other <see cref="API.Gadget"/>s that must be present for this patch to run.
        /// </summary>
        public readonly string[] RequiredGadgets;

        /// <summary>
        /// Indicates what Gadget owns this patch. Required for use of GadgetCore's automatic Harmony patching.
        /// </summary>
        /// <param name="Gadget">The name of the <see cref="API.Gadget"/> that owns this patch.</param>
        public HarmonyGadgetAttribute(string Gadget)
        {
            this.Gadget = Gadget;
            RequiredGadgets = new string[0];
        }

        /// <summary>
        /// Indicates what Gadget owns this patch. Required for use of GadgetCore's automatic Harmony patching.
        /// </summary>
        /// <param name="Gadget">The name of the <see cref="API.Gadget"/> that owns this patch.</param>
        /// <param name="RequiredGadgets">Other <see cref="API.Gadget"/>s that must be present for this patch to run.</param>
        public HarmonyGadgetAttribute(string Gadget, params string[] RequiredGadgets)
        {
            this.Gadget = Gadget;
            this.RequiredGadgets = RequiredGadgets ?? new string[0];
        }
    }
}
