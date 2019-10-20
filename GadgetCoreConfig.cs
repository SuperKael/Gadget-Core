using System;
using System.Collections.Generic;
using System.Linq;
using UModFramework.API;

namespace GadgetCore
{
    public class GadgetCoreConfig
    {
        private static readonly string configVersion = "1.0";

        internal static Dictionary<string, bool> enabledMods = new Dictionary<string, bool>();

        //Add your config vars here.

        internal static void Load()
        {
            GadgetCore.Log("Loading settings.");
            try
            {
                using (UMFConfig cfg = new UMFConfig())
                {
                    string cfgVer = cfg.Read("ConfigVersion", new UMFConfigString());
                    if (cfgVer != string.Empty && cfgVer != configVersion)
                    {
                        cfg.DeleteConfig(false);
                        GadgetCore.Log("The config file was outdated and has been deleted. A new config will be generated.");
                    }

                    //cfg.Write("SupportsHotLoading", new UMFConfigBool(false)); //Uncomment if your mod can't be loaded once the game has started.
                    //cfg.Write("ModDependencies", new UMFConfigStringArray(new string[] { "" })); //A comma separated list of mod names that this mod requires to function.
                    //cfg.Write("ModDependenciesVersions", new UMFConfigStringArray(new string[] { "" })); //A comma separated list of mod versions matching the ModDependencies setting.
                    cfg.Read("LoadPriority", new UMFConfigString("Normal"));
                    cfg.Write("MinVersion", new UMFConfigString("0.52.1"));
                    //cfg.Write("MaxVersion", new UMFConfigString("0.54.99999.99999")); //Uncomment if you think your mod may break with the next major UMF release.
                    cfg.Write("UpdateURL", new UMFConfigString(""));
                    cfg.Write("ConfigVersion", new UMFConfigString(configVersion));

                    GadgetCore.Log("Finished UMF Settings.");

                    try
                    {
                        enabledMods = cfg.Read("EnabledMods", new UMFConfigStringArray(new string[0])).Select(x => x.Split(':')).ToDictionary(x => x[0], x => bool.Parse(x[1]));
                    }
                    catch (Exception)
                    {
                        GadgetCore.Log("EnabledMods is improperly formatted!");
                    }

                    GadgetCore.Log("Finished loading settings.");
                }
            }
            catch (Exception e)
            {
                GadgetCore.Log("Error loading mod settings: " + e.Message + "(" + e.InnerException?.Message + ")");
            }
        }

        internal static void Update()
        {
            try
            {
                using (UMFConfig cfg = new UMFConfig())
                {
                    cfg.Write("EnabledMods", new UMFConfigStringArray(enabledMods.Select(x => x.Key + ":" + x.Value).ToArray()));
                }
            }
            catch (Exception e)
            {
                GadgetCore.Log("Error updating mod settings: " + e.Message + "(" + e.InnerException?.Message + ")");
            }
        }
    }
}