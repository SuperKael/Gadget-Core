using System;
using HarmonyLib;

namespace GadgetCore.API
{
    /// <summary>
    /// Indicates that this Prefix is intentionally overriding patches from one or more other mods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class HarmonyOverridesAttribute : Attribute
    {
        /// <summary>
        /// The patcher IDs to flag this method as intentionally overriding.
        /// </summary>
        public readonly string[] Overrides;

        /// <summary>
        /// Indicates that this Prefix is intentionally overriding patches from any and all other mods.
        /// Carefully consider whether it is truly appropriate to use this before doing so.
        /// Alternatively, use the <see cref="HarmonyOverridesAttribute(string[])"/> version.
        /// </summary>
        public HarmonyOverridesAttribute()
        {
            Overrides = new string[0];
        }

        /// <summary>
        /// Indicates that this Prefix is intentionally overriding patches from one or more other mods.
        /// Uses the same syntax as <see cref="HarmonyBefore"/> and <see cref="HarmonyAfter"/>, I.E. "ModName.GadgetName.gadget"
        /// </summary>
        /// <param name="Overrides">The patcher IDs to flag this method as intentionally overriding.</param>
        public HarmonyOverridesAttribute(params string[] Overrides)
        {
            this.Overrides = Overrides ?? new string[0];
        }
    }
}
