using GadgetCore.API;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GadgetCore.Loader
{
    /// <summary>
    /// Main class for accessing and managing loaded <see cref="GadgetMod"/>s
    /// </summary>
    public static class GadgetMods
    {
        private static Dictionary<string, GadgetMod> ModsByName = new Dictionary<string, GadgetMod>();
        private static Dictionary<string, GadgetMod> ModsByPath = new Dictionary<string, GadgetMod>();
        private static Dictionary<Assembly, GadgetMod> ModsByAssembly = new Dictionary<Assembly, GadgetMod>();

        internal static void RegisterMod(GadgetMod Mod)
        {
            Mod.Enabled = !GadgetCoreConfig.enabledMods.ContainsKey(Mod.Name) || GadgetCoreConfig.enabledMods[Mod.Name];
            ModsByName.Add(Mod.Name, Mod);
            ModsByPath.Add(Mod.ModPath, Mod);
            ModsByAssembly.Add(Mod.Assembly, Mod);
        }

        internal static void UnregisterMod(GadgetMod Mod)
        {
            ModsByName.Remove(Mod.Name);
            ModsByPath.Remove(Mod.ModPath);
            ModsByAssembly.Remove(Mod.Assembly);
            bool wasBatchLoading = GadgetLoader.BatchLoading;
            GadgetLoader.BatchLoading = true;
            foreach (GadgetInfo gadget in Mod.LoadedGadgets.ToList())
            {
                Gadgets.UnregisterGadget(gadget);
                GadgetLoader.QueuedGadgets.Add(gadget);
            }
            GadgetLoader.BatchLoading = wasBatchLoading;
            if (!GadgetLoader.BatchLoading)
            {
                GadgetLoader.DisableQueuedGadgets();
                Gadgets.SortGadgets();
            }
        }

        /// <summary>
        /// Gets the GadgetMod with the given name.
        /// </summary>
        public static GadgetMod GetModByName(string modName)
        {
            return ModsByName.ContainsKey(modName) ? ModsByName[modName] : null;
        }

        /// <summary>
        /// Gets the GadgetMod with the given path.
        /// </summary>
        public static GadgetMod GetModByPath(string modPath)
        {
            return ModsByPath.ContainsKey(modPath) ? ModsByPath[modPath] : null;
        }

        /// <summary>
        /// Gets the GadgetMod with the given assembly.
        /// </summary>
        public static GadgetMod GetModByAssembly(Assembly modAssembly)
        {
            return ModsByAssembly.ContainsKey(modAssembly) ? ModsByAssembly[modAssembly] : null;
        }

        /// <summary>
        /// Lists all GadgetMods.
        /// </summary>
        public static GadgetMod[] ListAllMods(bool IncludeUnloaded = false)
        {
            return IncludeUnloaded ? ModsByName.Values.ToArray() : ModsByName.Values.Where(x => x.IsLoaded).ToArray();
        }

        /// <summary>
        /// Lists the names of all GadgetMods.
        /// </summary>
        public static string[] ListAllModNames(bool IncludeUnloaded = false)
        {
            return IncludeUnloaded ? ModsByName.Keys.ToArray() : ModsByName.Where(x => x.Value.IsLoaded).Select(x => x.Key).ToArray();
        }

        /// <summary>
        /// Sets the Enabled status of the gadget with the given name using <see cref="GetModByName(string)"/>. Will throw a NullReferenceException if there is no mod with the given name.
        /// </summary>
        public static void SetEnabled(string name, bool enabled)
        {
            SetEnabled(GetModByName(name), enabled);
        }

        /// <summary>
        /// Sets the Enabled status of the given mod. Note that this can be queried using GadgetMod.Enabled
        /// </summary>
        public static void SetEnabled(GadgetMod mod, bool enabled)
        {
            if (mod.Enabled == enabled) return;
            mod.Enabled = enabled;
            GadgetCoreConfig.enabledMods[mod.Name] = enabled;
            bool wasBatchLoading = GadgetLoader.BatchLoading;
            GadgetLoader.BatchLoading = true;
            if (!enabled) foreach (GadgetInfo gadget in mod.LoadedGadgets)
            {
                Gadgets.SetEnabled(gadget, false);
            }
            GadgetLoader.BatchLoading = wasBatchLoading;
            GadgetCoreConfig.Update();
        }
    }
}
