using GadgetCore.Loader;
using GadgetCore.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GadgetCore.API
{
    /// <summary>
    /// This class is responsible for managing Gadgets.
    /// </summary>
    public static class Gadgets
    {
        private static Dictionary<string, GadgetInfo> gadgets = new Dictionary<string, GadgetInfo>();
        private static Dictionary<string, GadgetInfo> previousNames = new Dictionary<string, GadgetInfo>();
        private static List<GadgetInfo> sortedGadgetList;
        /// <summary>
        /// A <see cref="MultiTreeList{GadgetInfo}"/> representing the relationships between the installed Gadgets. It is read-only, so do not attempt to edit it. Note that this is just the parent of the structure, and its <see cref="MultiTreeList{T}.Value"/> is null.
        /// </summary>
        public static MultiTreeList<GadgetInfo> LoadOrderTree { get; private set; }

        internal static void RegisterGadget(GadgetInfo mod)
        {
            if (gadgets.ContainsKey(mod.Attribute.Name)) throw new InvalidOperationException("Failed to register Gadget: Another Gadget by the same name has already been registered!");
            mod.Gadget.Enabled = GadgetCoreConfig.enabledGadgets.ContainsKey(mod.Attribute.Name) ? GadgetCoreConfig.enabledGadgets[mod.Attribute.Name] : (GadgetCoreConfig.enabledGadgets[mod.Attribute.Name] = mod.Attribute.EnableByDefault);
            gadgets.Add(mod.Attribute.Name, mod);
            foreach (string name in mod.Gadget.GetPreviousModNames()) previousNames[name] = mod;
            if (!GadgetLoader.BatchLoading) SortGadgets();
        }

        internal static void UnregisterGadget(GadgetInfo mod)
        {
            gadgets.Remove(mod.Attribute.Name);
            foreach (Registry reg in GameRegistry.ListAllRegistries())
            {
                reg.UnregisterGadget(mod);
            }
            mod.Mod.m_LoadedGadgets.Remove(mod);
            mod.Mod.m_UnloadedGadgets.Add(mod);
            if (!GadgetLoader.BatchLoading) SortGadgets();
        }

        internal static void SortGadgets()
        {
            Dictionary<GadgetInfo, IEnumerable<GadgetInfo>> orderDependencies = new Dictionary<GadgetInfo, IEnumerable<GadgetInfo>>();
            foreach (GadgetInfo info in gadgets.Values)
            {
                List<GadgetInfo> list = new List<GadgetInfo>();
                list.AddRange(info.Attribute.LoadBefore.Select(GetGadgetInfo).Where(x => x != null));
                list.AddRange(gadgets.Values.Where(x => x.Attribute.LoadAfter.Contains(info.Attribute.Name)));
                orderDependencies[info] = list;
            }
            LoadOrderTree = new MultiTreeList<GadgetInfo>(orderDependencies);
            foreach (MultiTreeList<GadgetInfo> node in LoadOrderTree.SelectMany(x => x.ListAllNodes()))
            {
                node.Sort((x, y) => x.Value.Attribute.LoadPriority - y.Value.Attribute.LoadPriority);
            }
            sortedGadgetList = LoadOrderTree.FlattenUniqueByBreadth().Where(x => x != null).ToList();
            LoadOrderTree.MakeReadOnly();
            for (int i = 0; i < sortedGadgetList.Count; i++) sortedGadgetList[i].Gadget.ModID = i;
            foreach (GadgetInfo gadget in sortedGadgetList)
            {
                gadget.Dependencies = gadget.Attribute.Dependencies.Select(GetGadgetInfo).ToArray();
                gadget.Dependents = sortedGadgetList.Where(x => x.Attribute.Dependencies.Contains(gadget.Attribute.Name)).ToArray();
            }
        }

        /// <summary>
        /// Returns the count of Gadgets. Note that this includes Gadgets that are not enabled.
        /// </summary>
        public static int CountGadgets()
        {
            return gadgets.Count;
        }
        /// <summary>
        /// Returns the count of Gadgets that are enabled.
        /// </summary>
        public static int CountEnabledGadgets()
        {
            return gadgets.Count(x => x.Value.Gadget.Enabled);
        }

        /// <summary>
        /// Returns an array of all Gadgets. Note that this includes Gadgets that are not enabled.
        /// </summary>
        public static Gadget[] ListAllGadgets()
        {
            return sortedGadgetList?.Select(x => x.Gadget).ToArray() ?? new Gadget[0];
        }

        /// <summary>
        /// Returns an array of all enabled Gadgets.
        /// </summary>
        public static Gadget[] ListAllEnabledGadgets()
        {
            return sortedGadgetList?.Select(x => x.Gadget).Where(x => x.Enabled).ToArray() ?? new Gadget[0];
        }

        /// <summary>
        /// Returns an array of all GadgetInfos. Note that this includes Gadgets that are not enabled.
        /// </summary>
        public static GadgetInfo[] ListAllGadgetInfos()
        {
            return sortedGadgetList?.ToArray() ?? new GadgetInfo[0];
        }

        /// <summary>
        /// Returns an array of all enabled GadgetInfos.
        /// </summary>
        public static GadgetInfo[] ListAllEnabledGadgetInfos()
        {
            return sortedGadgetList?.Where(x => x.Gadget.Enabled).ToArray() ?? new GadgetInfo[0];
        }

        /// <summary>
        /// Gets the Gadget with the specified name. If it is not found, attempts to find a Gadget that has the given name listed as a previous name. If still no Gadget is found, returns null.
        /// </summary>
        public static Gadget GetGadget(string name)
        {
            return gadgets.ContainsKey(name) ? gadgets[name].Gadget : (previousNames.ContainsKey(name) ? previousNames[name].Gadget : null);
        }

        /// <summary>
        /// Gets the Gadget with the specified index. The Gadgets are sorted by load order.
        /// </summary>
        public static Gadget GetGadget(int index)
        {
            return sortedGadgetList[index].Gadget;
        }

        /// <summary>
        /// Gets the GadgetInfo with the specified name. If it is not found, attempts to find a Gadget that has the given name listed as a previous name. If still no Gadget is found, returns null.
        /// </summary>
        public static GadgetInfo GetGadgetInfo(string name)
        {
            return gadgets.ContainsKey(name) ? gadgets[name] : (previousNames.ContainsKey(name) ? previousNames[name] : null);
        }

        /// <summary>
        /// Gets the Gadget with the specified index. The Gadgets are sorted by load order.
        /// </summary>
        public static GadgetInfo GetGadgetInfo(int modID)
        {
            return sortedGadgetList[modID];
        }

        /// <summary>
        /// Reloads the given Gadget.
        /// </summary>
        public static void ReloadGadget(GadgetInfo gadget)
        {
            SetEnabled(gadget, false);
            SetEnabled(gadget, true);
        }

        /// <summary>
        /// Sets the Enabled status of the given gadget. Note that this can be queried using Gadget.Enabled.
        /// </summary>
        public static void SetEnabled(GadgetInfo gadget, bool enabled)
        {
            if (enabled && !gadget.Mod.Enabled) GadgetMods.SetEnabled(gadget.Mod, true);
            if (!SetEnabledInternal(gadget, enabled)) return;
        }

        /// <summary>
        /// Sets the Enabled status of the gadget with the given name using <see cref="GetGadgetInfo(string)"/>. Will throw a NullReferenceException if there is no mod with the given name.
        /// </summary>
        public static void SetEnabled(string name, bool enabled)
        {
            if (!SetEnabledInternal(GetGadgetInfo(name), enabled)) return;
        }

        /// <summary>
        /// Sets the Enabled status of the gadget with the given index.
        /// </summary>
        public static void SetEnabled(int index, bool enabled)
        {
            if (!SetEnabledInternal(GetGadgetInfo(index), enabled)) return;
        }

        private static bool SetEnabledInternal(GadgetInfo gadget, bool enabled)
        {
            try
            {
                if (gadget.Gadget.Enabled == enabled) return false;
                bool wasBatchLoading = GadgetLoader.BatchLoading;
                GadgetLoader.BatchLoading = true;
                if (enabled)
                {
                    if (gadget.Attribute.AllowRuntimeReloading)
                    {
                        EnableDependencies(gadget);
                        RefreshAfters(gadget);
                    }
                }
                else
                {
                    DisableDependents(gadget);
                }
                GadgetCoreConfig.enabledGadgets[gadget.Attribute.Name] = enabled;
                if (gadget.Attribute.AllowRuntimeReloading || !enabled) gadget.Gadget.Enabled = enabled;
                GadgetLoader.BatchLoading = wasBatchLoading;
                GadgetLoader.QueuedGadgets.Add(gadget);
                if (!GadgetLoader.BatchLoading)
                {
                    if (enabled)
                    {
                        if (gadget.Attribute.AllowRuntimeReloading) GadgetLoader.EnableQueuedGadgets();
                    }
                    else
                    {
                        GadgetLoader.DisableQueuedGadgets();
                    }
                    GadgetCoreConfig.Update();
                }
                return true;
            }
            catch (Exception e)
            {
                GadgetLoader.Logger.LogError("Error " + (enabled ? "enabling" : "disabling") + " Gadget `" + gadget.Attribute.Name + "`: " + e);
                return false;
            }
        }

        private static void RefreshAfters(GadgetInfo gadget)
        {
            foreach (GadgetInfo info in LoadOrderTree.Find(gadget).FlattenUniqueByBreadth().Where(x => x != gadget && x.Gadget.Enabled).Distinct())
            {
                SetEnabled(info, false);
                SetEnabled(info, true);
            }
        }

        private static void EnableDependencies(GadgetInfo gadget)
        {
            foreach (GadgetInfo dependency in gadget.Dependencies)
            {
                if (!dependency.Gadget.Enabled)
                {
                    GadgetLoader.Logger.Log("Enabling Gadget Dependency of " + gadget + ": " + dependency);
                    SetEnabled(dependency, true);
                }
            }
        }

        private static void DisableDependents(GadgetInfo gadget)
        {
            foreach (GadgetInfo dependent in gadget.Dependents)
            {
                if (dependent.Gadget.Enabled)
                {
                    GadgetLoader.Logger.Log("Disabling Gadget Dependent of " + gadget + ": " + dependent);
                    SetEnabled(dependent, false);
                }
            }
        }
    }
}
