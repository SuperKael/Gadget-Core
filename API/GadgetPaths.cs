using System.IO;
using UnityEngine;

namespace GadgetCore.API
{
    /// <summary>
    /// Provides file paths to various commonly used directories.
    /// </summary>
    public static class GadgetPaths
    {
        /// <summary>
        /// The path to Roguelands' root directory, AKA the Roguelands folder.
        /// </summary>
        public static readonly string GamePath;

        /// <summary>
        /// The path to the data directory.
        /// </summary>
        public static readonly string DataPath;
        /// <summary>
        /// The path to the managed assemblies directory.
        /// </summary>
        public static readonly string ManagedPath;

        /// <summary>
        /// The path to the GadgetCore directory.
        /// </summary>
        public static readonly string GadgetCorePath;

        /// <summary>
        /// The path to the Mods directory.
        /// </summary>
        public static readonly string ModsPath;
        /// <summary>
        /// The path to the Configs directory.
        /// </summary>
        public static readonly string ConfigsPath;
        /// <summary>
        /// The path to the Logs directory.
        /// </summary>
        public static readonly string LogsPath;
        /// <summary>
        /// The path to the Log Archives directory.
        /// </summary>
        public static readonly string LogArchivesPath;
        /// <summary>
        /// The path to the Assets directory.
        /// </summary>
        public static readonly string AssetsPath;
        /// <summary>
        /// The path to the Libs directory.
        /// </summary>
        public static readonly string LibsPath;
        /// <summary>
        /// The path to the Tools directory.
        /// </summary>
        public static readonly string ToolsPath;
        /// <summary>
        /// The path to the Temp directory.
        /// </summary>
        public static readonly string TempPath;
        /// <summary>
        /// The path to the Save Backups directory.
        /// </summary>
        public static readonly string SaveBackupsPath;

        static GadgetPaths()
        {
            DataPath = Application.dataPath;

            GamePath = DataPath;
            do
            {
                GamePath = Path.GetDirectoryName(GamePath);
            }
            while (GamePath.Contains(".app"));
            ManagedPath = Path.Combine(DataPath, "Managed");

            GadgetCorePath = Path.Combine(GamePath, "GadgetCore");
            Directory.CreateDirectory(GadgetCorePath);

            ModsPath = Path.Combine(GadgetCorePath, "Mods");
            Directory.CreateDirectory(ModsPath);
            ConfigsPath = Path.Combine(GadgetCorePath, "Configs");
            Directory.CreateDirectory(ConfigsPath);
            LogsPath = Path.Combine(GadgetCorePath, "Logs");
            Directory.CreateDirectory(LogsPath);
            LogArchivesPath = Path.Combine(LogsPath, "Archives");
            Directory.CreateDirectory(LogArchivesPath);
            AssetsPath = Path.Combine(GadgetCorePath, "Assets");
            Directory.CreateDirectory(AssetsPath);
            LibsPath = Path.Combine(GadgetCorePath, "Libs");
            Directory.CreateDirectory(LibsPath);
            ToolsPath = Path.Combine(GadgetCorePath, "Tools");
            Directory.CreateDirectory(ToolsPath);
            TempPath = Path.Combine(GadgetCorePath, "Temp");
            Directory.CreateDirectory(TempPath);
            SaveBackupsPath = Path.Combine(GadgetCorePath, "Save Backups");
            Directory.CreateDirectory(SaveBackupsPath);

            foreach (string directory in Directory.GetDirectories(TempPath))
            {
                Directory.Delete(directory, true);
            }
            foreach (string file in Directory.GetFiles(TempPath))
            {
                File.Delete(file);
            }
        }
    }
}
