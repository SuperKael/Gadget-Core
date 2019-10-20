using GadgetCore.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GadgetCore.API
{
    public static class GadgetMods
    {
        private static Dictionary<string, GadgetModInfo> mods = new Dictionary<string, GadgetModInfo>();
        private static List<GadgetModInfo> sortedModList = new List<GadgetModInfo>();

        internal static void RegisterMod(GadgetModInfo mod)
        {
            mods.Add(mod.Attribute.Name, mod);
        }

        internal static void SortMods()
        {
            sortedModList = mods.Values.OrderByDescending(x => x.Attribute.LoadPriority).ToList();
        }

        public static int CountMods()
        {
            return mods.Count;
        }

        public static GadgetMod[] ListAllMods()
        {
            return sortedModList.Select(x => x.Mod).ToArray();
        }

        public static GadgetModInfo[] ListAllModInfos()
        {
            return sortedModList.ToArray();
        }

        public static GadgetMod GetMod(string name)
        {
            return mods.ContainsKey(name) ? mods[name].Mod : null;
        }

        public static GadgetMod GetMod(int index)
        {
            return sortedModList[index].Mod;
        }

        public static GadgetModInfo GetModInfo(string name)
        {
            return mods.ContainsKey(name) ? mods[name] : null;
        }

        public static GadgetModInfo GetModInfo(int index)
        {
            return sortedModList[index];
        }

        public static void SetEnabled(string name, bool enabled)
        {
            GetModInfo(name).Enabled = enabled;
            GadgetCoreConfig.enabledMods[name] = enabled;
            GadgetCoreConfig.Update();
        }

        public static void SetEnabled(int index, bool enabled)
        {
            GadgetModInfo modInfo = GetModInfo(index);
            modInfo.Enabled = enabled;
            GadgetCoreConfig.enabledMods[modInfo.Attribute.Name] = enabled;
            GadgetCoreConfig.Update();
        }
    }
}
