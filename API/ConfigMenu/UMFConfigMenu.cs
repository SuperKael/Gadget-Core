using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UModFramework.API;

namespace GadgetCore.API.ConfigMenu
{
    internal class UMFConfigMenu : UMFGadgetConfigMenu
    {
        public UMFConfigMenu() : base("uModFramework", true, Path.Combine(UMFData.ConfigsPath, "uModFramework") + ".ini", "ConfigVersion") { }

        protected override GadgetConfigComponentAlignment? AlignConfigEntry(string name)
        {
            switch (name)
            {
                case "ConfigVersion":
                    return null;
                default:
                    return GadgetConfigComponentAlignment.STANDARD;
            }
        }
    }
}
