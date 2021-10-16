using GadgetCore.API;
using PreviewLabs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
        public static readonly string configVersion = "2.4";

        internal static Dictionary<string, bool> enabledMods = new Dictionary<string, bool>();
        internal static Dictionary<string, bool> enabledGadgets = new Dictionary<string, bool>();

        /// <summary>
        /// Whether to use the standard hover box for droids, rather than the text-based description.
        /// </summary>
        public static bool BetterDroidHover { get; private set; } = true;
        /// <summary>
        /// The maximum connections allowed when hosting a game. This value is irrelevant if we are the client, not the host.
        /// </summary>
        public static int MaxConnections { get; private set; } = 4;
        /// <summary>
        /// Whether to use UPnP to bypass the need for port-forwarding to play over the internet. Not all routers support this. This value is irrelevant if we are the client, not the host.
        /// </summary>
        public static bool UseUPnP { get; private set; } = false;
        /// <summary>
        /// The key used to open the console.
        /// </summary>
        public static UnityEngine.KeyCode ConsoleKey { get; private set; } = UnityEngine.KeyCode.BackQuote;
        /// <summary>
        /// The maximum number of save backups to keep at any one time.
        /// </summary>
        public static int MaxBackups { get; private set; } = 30;
        /// <summary>
        /// The maximum number of log archives to keep at any one time.
        /// </summary>
        public static int MaxLogArchives { get; private set; } = 30;
        /// <summary>
        /// Whether to use Graphics.CopyTexture to speed up texture handling. Disabling may fix graphical issues, at the cost of performance.
        /// </summary>
        public static bool UseCopyTexture { get; private set; } = true;

        private static GadgetConfig cfg = new GadgetConfig(Path.Combine(GadgetPaths.ConfigsPath, "GadgetCore.ini"), "GadgetCore");

        internal static void EarlyLoad()
        {
            cfg.Load();

            string fileVersion = cfg.ReadString("ConfigVersion", configVersion, comments: "The Config Version (not to be confused with mod version)");

            if (fileVersion != configVersion)
            {
                cfg.Reset();
                cfg.WriteString("ConfigVersion", configVersion, comments: "The Config Version (not to be confused with mod version)");
            }

            MaxBackups = cfg.ReadInt("MaxBackups", 30, 0, false, 0, int.MaxValue, "The maximum number of save backups to keep at any one time. Disables backups if set to 0");
            MaxLogArchives = cfg.ReadInt("MaxLogArchives", 30, 0, false, 0, int.MaxValue, "The maximum number of log archives to keep at any one time. Disables log archiving if set to 0");
        }

        internal static void Load()
        {
            try
            {
                GadgetCoreAPI.UnregisterKeyDownListener(ConsoleKey, GadgetConsole.ShowConsole);

                BetterDroidHover = cfg.ReadBool("BetterDroidHover", true, false, false, "Whether to use the standard hover box for droids, rather than the text-based description.");
                MaxConnections = cfg.ReadInt("MaxConnections", 4, 4, false, 1, int.MaxValue, "The maximum number of connections allowed when using host-and-play. This setting only matters on the host.");
                UseUPnP = cfg.ReadBool("UseUPnP", false, false, false, "If True, will attempt to use UPnP to bypass the need to port-forward to host-and-play over the internet. Not all routers support this. Disabled by default due to severe unresolved bugs that prevent the game from working at all sometimes.");
                GadgetNetwork.MatrixTimeout = cfg.ReadFloat("NetworkTimeout", 2.5f, comments: "How long to wait for the host's game to respond to Gadget Core's ID synchronization. If the host's game does not respond in time, it will be assumed that the host does not have Gadget Core installed.");
                ConsoleKey = cfg.ReadKeyCode("ConsoleKey", UnityEngine.KeyCode.BackQuote, comments: "The key to open the console.");
                GadgetConsole.Debug = cfg.ReadBool("ConsoleDebug", false, comments: "If true, shows debug messages in the console.");
                UseCopyTexture = cfg.ReadBool("UseCopyTexture", true, comments: "If True, uses Graphics.CopyTexture to speed up texture handling. Disabling may fix graphical issues, at the cost of performance.");

                cfg.Save();

                GadgetCoreAPI.RegisterKeyDownListener(ConsoleKey, GadgetConsole.ShowConsole);

                string enabledModsString = PlayerPrefs.GetString("EnabledMods", "");
                try
                {
                    enabledMods = enabledModsString.Split(',').Select(x => x.Split(':')).ToDictionary(x => x[0], x => bool.Parse(x[1]));
                }
                catch (IndexOutOfRangeException)
                {
                    enabledMods = new Dictionary<string, bool>();
                }
                string enabledGadgetsString = PlayerPrefs.GetString("EnabledGadgets", "");
                try
                {
                    enabledGadgets = enabledGadgetsString.Split(',').Select(x => x.Split(':')).ToDictionary(x => x[0], x => bool.Parse(x[1]));
                }
                catch (IndexOutOfRangeException)
                {
                    enabledGadgets = new Dictionary<string, bool>();
                }
            }
            catch (Exception e)
            {
                GadgetCore.CoreLogger.LogError("Error loading GadgetCore config: " + e.ToString());
            }
        }

        internal static void Update()
        {
            try
            {
                PlayerPrefs.SetString("EnabledGadgets", enabledGadgets.Select(x => x.Key + ":" + x.Value).Aggregate(new StringBuilder(), (a, b) => { if (a.Length > 0) a.Append(","); a.Append(b); return a; }).ToString());
                foreach (Registry reg in GameRegistry.ListAllRegistries())
                {
                    PlayerPrefs.SetString("Reserved" + reg.GetRegistryName() + "IDs", reg.reservedIDs.Select(x => x.Key + "=" + x.Value).Aggregate(new StringBuilder(), (a, b) => { if (a.Length > 0) a.Append(","); a.Append(b); return a; }).ToString());
                }
                PlayerPrefs.Flush();
            }
            catch (Exception e)
            {
                GadgetCore.CoreLogger.LogError("Error updating config: " + e);
            }
        }
    }
}