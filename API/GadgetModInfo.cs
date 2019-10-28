using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GadgetCore.API
{
    public sealed class GadgetModInfo
    {
        public readonly GadgetMod Mod;

        public readonly Assembly Assembly;
        public readonly GadgetModAttribute Attribute;
        public readonly string UMFName;
        internal bool isMultiple = false;

        internal GadgetModInfo(GadgetMod mod, Assembly assembly, GadgetModAttribute attribute, string name)
        {
            Mod = mod;
            Assembly = assembly;
            Attribute = attribute;
            UMFName = name;
            Mod.Enabled = GadgetCoreConfig.enabledMods.ContainsKey(attribute.Name) ? GadgetCoreConfig.enabledMods[attribute.Name] : GadgetCoreConfig.enabledMods[attribute.Name] = attribute.EnableByDefault;
        }
    }
}
