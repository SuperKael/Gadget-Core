using GadgetCore.API;
using PreviewLabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public static readonly string configVersion = "1.2";

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

                    string enabledModsString = PlayerPrefs.GetString("EnabledMods", "");
                    GadgetCore.Log(enabledModsString);
                    try
                    {
                        enabledMods = enabledModsString.Split(',').Select(x => x.Split(':')).ToDictionary(x => x[0], x => bool.Parse(x[1]));
                    }
                    catch (IndexOutOfRangeException)
                    {
                        enabledMods = new Dictionary<string, bool>();
                    }
                    foreach (GadgetModInfo mod in GadgetMods.ListAllModInfos())
                    {
                        GadgetCore.Log(mod.Attribute.Name + ": " + enabledMods.ContainsKey(mod.Attribute.Name));
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
                foreach (Registry reg in GameRegistry.ListAllRegistries())
                {
                    try
                    {
                        reg.reservedIDs = PlayerPrefs.GetString("Reserved" + reg.GetRegistryName() + "IDs", "").Split(',').Select(x => x.Split('=')).ToDictionary(x => x[0], x => int.Parse(x[1]));
                    }
                    catch (IndexOutOfRangeException)
                    {
                        reg.reservedIDs = new Dictionary<string, int>();
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
                PlayerPrefs.SetString("EnabledMods", enabledMods.Select(x => x.Key + ":" + x.Value).Aggregate(new StringBuilder(), (a, b) => { if (a.Length > 0) a.Append(","); a.Append(b); return a; }).ToString());
                foreach (Registry reg in GameRegistry.ListAllRegistries())
                {
                    PlayerPrefs.SetString("Reserved" + reg.GetRegistryName() + "IDs", reg.reservedIDs.Select(x => x.Key + "=" + x.Value).Aggregate(new StringBuilder(), (a, b) => { if (a.Length > 0) a.Append(","); a.Append(b); return a; }).ToString());
                }
                PlayerPrefs.Flush();
            }
            catch (Exception e)
            {
                GadgetCore.Log("Error updating mod settings: " + e.Message + "(" + e.InnerException?.Message + ")");
            }
        }
    }
}