using GadgetCore.API;
using System;
using System.Collections.Generic;
using System.Linq;
using UModFramework.API;

namespace GadgetCore
{
    /// <summary>
    /// Class used for management of Gadget Core's config.
    /// </summary>
    public static class GadgetCoreConfig
    {
        /// <summary>
        /// The current version of Gadget Core's config.
        /// </summary>
        public static readonly string configVersion = "1.1";

        internal static Dictionary<string, bool> enabledMods = new Dictionary<string, bool>();

        /// <summary>
        /// The maximum connections allowed when hosting a game. This value is irrelevant if we are the client, not the host.
        /// </summary>
        public static int MaxConnections { get; private set; } = 4;
        /// <summary>
        /// Whether to use UPnP to bypass the need for port-forwarding to play over the internet. Not all routers support this. This value is irrelevant if we are the client, not the host.
        /// </summary>
        public static bool UseUPnP { get; private set; } = false;

        //Add your config vars here.

        internal static void Load()
        {
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
                    cfg.Read("LoadPriority", new UMFConfigString("AboveNormal"));
                    cfg.Write("MinVersion", new UMFConfigString("0.52.1"));
                    //cfg.Write("MaxVersion", new UMFConfigString("0.54.99999.99999")); //Uncomment if you think your mod may break with the next major UMF release.
                    cfg.Write("UpdateURL", new UMFConfigString(""));
                    cfg.Write("ConfigVersion", new UMFConfigString(configVersion));

                    GadgetCore.Log("Finished UMF Settings.");

                    MaxConnections = cfg.Read("MaxConnections", new UMFConfigInt(4), "The maximum number of connections allowed when using host-and-play. This setting only matters on the host.");
                    UseUPnP = cfg.Read("UseUPnP", new UMFConfigBool(false), "If True, will attempt to use UPnP to bypass the need to port-forward to host-and-play over the internet. Not all routers support this.");
                    GadgetNetwork.MatrixTimeout = cfg.Read("NetworkTimeout", new UMFConfigFloat(2.5f), "How long to wait for the host's game to respond to Gadget Core's ID synchronization. If the host's game does not respond in time, it will be assumed that the host does not have Gadget Core installed.");

                    try
                    {
                        enabledMods = cfg.Read("EnabledMods", new UMFConfigStringArray(new string[0])).Select(x => x.Split(':')).ToDictionary(x => x[0], x => bool.Parse(x[1]));
                    }
                    catch (Exception)
                    {
                        GadgetCore.Log("EnabledMods is empty or improperly formatted!");
                    }
                    foreach (GadgetModInfo mod in GadgetMods.ListAllModInfos())
                    {
                        mod.Mod.Enabled = enabledMods.ContainsKey(mod.Attribute.Name) ? enabledMods[mod.Attribute.Name] : (enabledMods[mod.Attribute.Name] = mod.Attribute.EnableByDefault);
                    }
                    GadgetCore.Log("Finished loading settings.");
                }
            }
            catch (Exception e)
            {
                GadgetCore.Log("Error loading mod settings: " + e.ToString());
            }
        }

        internal static void LoadRegistries()
        {
            try
            {
                using (UMFConfig cfg = new UMFConfig())
                {
                    foreach (Registry reg in GameRegistry.ListAllRegistries())
                    {
                        try
                        {
                            reg.reservedIDs = cfg.Read("Reserved" + reg.GetRegistryName() + "IDs", new UMFConfigStringArray(new string[0])).Select(x => x.Split('=')).ToDictionary(x => x[0], x => int.Parse(x[1]));
                        }
                        catch (Exception)
                        {
                            GadgetCore.Log("Reserved" + reg.GetRegistryName() + "IDs is empty or improperly formatted!");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                GadgetCore.Log("Error loading registry settings: " + e.ToString());
            }
        }

        internal static void Update()
        {
            try
            {
                using (UMFConfig cfg = new UMFConfig())
                {
                    cfg.Write("EnabledMods", new UMFConfigStringArray(enabledMods.Select(x => x.Key + ":" + x.Value).ToArray()));
                    foreach (Registry reg in GameRegistry.ListAllRegistries())
                    {
                        cfg.Write("Reserved" + reg.GetRegistryName() + "IDs", new UMFConfigStringArray(reg.reservedIDs.Select(x => x.Key + "=" + x.Value).ToArray()));
                    }
                }
            }
            catch (Exception e)
            {
                GadgetCore.Log("Error updating mod settings: " + e.Message + "(" + e.InnerException?.Message + ")");
            }
        }
    }
}