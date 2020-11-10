using System.IO;

namespace GadgetCore.API.ConfigMenu
{
    internal class UMFConfigMenu : INIGadgetConfigMenu
    {
        public UMFConfigMenu() : base("uModFramework", true, Path.Combine(GadgetCoreAPI.GetUMFAPI().GetConfigsPath(), "uModFramework") + ".ini", null, "ConfigVersion") { }

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
