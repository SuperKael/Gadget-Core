using GadgetCore.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GadgetCore.API
{
    /// <summary>
    /// This class is responsible for managing Gadget Mods.
    /// </summary>
    public static class GadgetMods
    {
        private static Dictionary<string, GadgetModInfo> mods = new Dictionary<string, GadgetModInfo>();
        private static Dictionary<string, GadgetModInfo> previousNames = new Dictionary<string, GadgetModInfo>();
        private static List<GadgetModInfo> sortedModList;

        internal static void RegisterMod(GadgetModInfo mod)
        {
            mods.Add(mod.Attribute.Name, mod);
            foreach (string name in mod.Mod.GetPreviousModNames()) previousNames[name] = mod;
            if (sortedModList != null) SortMods();
        }

        internal static void SortMods()
        {
            sortedModList = mods.Values.OrderByDescending(x => x.Attribute.LoadPriority).ToList();
            for (int i = 0; i < sortedModList.Count; i++) sortedModList[i].Mod.ModID = i;
        }

        /// <summary>
        /// Returns the count of Gadget Mods. Note that this includes Gadget Mods that are not enabled.
        /// </summary>
        public static int CountMods()
        {
            return mods.Count;
        }
        /// <summary>
        /// Returns the count of Gadget Mods that are enabled.
        /// </summary>
        public static int CountEnabledMods()
        {
            return mods.Count(x => x.Value.Mod.Enabled);
        }

        /// <summary>
        /// Returns an array of all Gadget Mods. Note that this includes Gadget Mods that are not enabled.
        /// </summary>
        public static GadgetMod[] ListAllMods()
        {
            return sortedModList.Select(x => x.Mod).ToArray();
        }

        /// <summary>
        /// Returns an array of all enabled Gadget Mods.
        /// </summary>
        public static GadgetMod[] ListAllEnabledMods()
        {
            return sortedModList.Select(x => x.Mod).Where(x => x.Enabled).ToArray();
        }

        /// <summary>
        /// Returns an array of all Gadget Mod Infos. Note that this includes Gadget Mods that are not enabled.
        /// </summary>
        public static GadgetModInfo[] ListAllModInfos()
        {
            return sortedModList.ToArray();
        }

        /// <summary>
        /// Returns an array of all enabled Gadget Mod Infos.
        /// </summary>
        public static GadgetModInfo[] ListAllEnabledModInfos()
        {
            return sortedModList.Where(x => x.Mod.Enabled).ToArray();
        }

        /// <summary>
        /// Gets the Gadget Mod with the specified name. If it is not found, attempts to find a mod that has the given name listed as a previous name. If still no mod is found, returns null.
        /// </summary>
        public static GadgetMod GetMod(string name)
        {
            return mods.ContainsKey(name) ? mods[name].Mod : (previousNames.ContainsKey(name) ? previousNames[name].Mod : null);
        }

        /// <summary>
        /// Gets the Gadget Mod with the specified index. The Gadget Mods are sorted by load order.
        /// </summary>
        public static GadgetMod GetMod(int index)
        {
            return sortedModList[index].Mod;
        }

        /// <summary>
        /// Gets the Gadget Mod Info with the specified name. If it is not found, attempts to find a mod that has the given name listed as a previous name. If still no mod is found, returns null.
        /// </summary>
        public static GadgetModInfo GetModInfo(string name)
        {
            return mods.ContainsKey(name) ? mods[name] : (previousNames.ContainsKey(name) ? previousNames[name] : null);
        }

        /// <summary>
        /// Gets the Gadget Mod with the specified index. The Gadget Mods are sorted by load order.
        /// </summary>
        public static GadgetModInfo GetModInfo(int modID)
        {
            return sortedModList[modID];
        }

        /// <summary>
        /// Sets the Enabled status of the given mod. Note that this can be queried using GadgetMod.Enabled, although it will not apply until the game is restarted.
        /// </summary>
        public static void SetEnabled(GadgetModInfo mod, bool enabled)
        {
            mod.Mod.Enabled = enabled;
            GadgetCoreConfig.enabledMods[mod.Attribute.Name] = enabled;
            GadgetCoreConfig.Update();
        }

        /// <summary>
        /// Sets the Enabled status of the mod with the given name using <see cref="GetModInfo(string)"/>. Will throw a NullReferenceException if there is no mod with the given name.
        /// </summary>
        public static void SetEnabled(string name, bool enabled)
        {
            GetModInfo(name).Mod.Enabled = enabled;
            GadgetCoreConfig.enabledMods[name] = enabled;
            GadgetCoreConfig.Update();
        }

        /// <summary>
        /// Sets the Enabled status of the mod with the given index.
        /// </summary>
        public static void SetEnabled(int index, bool enabled)
        {
            GadgetModInfo modInfo = GetModInfo(index);
            modInfo.Mod.Enabled = enabled;
            GadgetCoreConfig.enabledMods[modInfo.Attribute.Name] = enabled;
            GadgetCoreConfig.Update();
        }
    }
}
