using System;
using HarmonyLib;

namespace GadgetCore.API
{
    /// <summary>
    /// Indicates that this Prefix or Transpiler is intentionally overridden by patches from one or more other mods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class HarmonyOverriddenAttribute : Attribute
    {
        /// <summary>
        /// The patcher IDs to flag this method as being intentionally overridden by.
        /// </summary>
        public readonly string[] Overrides;

        /// <summary>
        /// Indicates that this Prefix or Transpiler is intentionally overridden by patches from any and all other mods.
        /// Carefully consider whether it is truly appropriate to use this before doing so.
                                                                                                        /// Alternatively, use the <see cref="HarmonyOverriddenAttribute(string[])"/> version.
                                                                                                        /// </summary>
        public HarmonyOverriddenAttribute()
        {
            Overrides = new string[0];
        }

        /// <summary>
        /// Indicates that this Prefix or Transpiler is intentionally overridden by patches from one or more other mods.
        /// Uses the same syntax as <see cref="HarmonyBefore"/> and <see cref="HarmonyAfter"/>, I.E. "ModName.GadgetName.gadget"
        /// </summary>
        /// <param name="Overrides">The patcher IDs to flag this method as being intentionally overridden by.</param>
        public HarmonyOverriddenAttribute(params string[] Overrides)
        {
            this.Overrides = Overrides ?? new string[0];
        }
    }
}
