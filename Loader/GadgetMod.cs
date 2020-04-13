using GadgetCore.API;
using IniParser.Model;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace GadgetCore.Loader
{
    /// <summary>
    /// The container class used for tracking a Gadget Mod.
    /// </summary>
    public sealed class GadgetMod
    {
        /// <summary>
        /// The name of this Gadget Mod.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The path to this Gadget Mod.
        /// </summary>
        public readonly string ModPath;

        /// <summary>
        /// The version of this Gadget Mod.
        /// </summary>
        public readonly Version Version;

        /// <summary>
        /// The manifest ini of this Gadget Mod.
        /// </summary>
        public readonly IniData Manifest;

        /// <summary>
        /// The <see cref="System.Reflection.Assembly"/> of this Gadget Mod.
        /// </summary>
        public readonly Assembly Assembly;

        /// <summary>
        /// Indicates whether this mod is contained within a .zip archive.
        /// </summary>
        public readonly bool IsArchive;

        /// <summary>
        /// The list of other-mod dependencies needed by this Gadget Mod. This is comes from the "Dependencies" value in the mod's manifest.
        /// </summary>
        public readonly ReadOnlyCollection<string> ModDependencies;
        /// <summary>
        /// The list of assembly dependencies needed by this Gadget Mod. Some may be optional.
        /// </summary>
        public readonly ReadOnlyCollection<AssemblyName> AssemblyDependencies;

        internal List<GadgetInfo> m_LoadedGadgets;
        /// <summary>
        /// The list of all loaded Gadgets contained within this Gadget Mod.
        /// </summary>
        public ReadOnlyCollection<GadgetInfo> LoadedGadgets { get; internal set; }

        internal List<GadgetInfo> m_UnloadedGadgets;
        /// <summary>
        /// The list of all unloaded Gadgets contained within this Gadget Mod.
        /// </summary>
        public ReadOnlyCollection<GadgetInfo> UnloadedGadgets { get; internal set; }

        /// <summary>
        /// Indicates whether this mod is currently loaded. If not, access to any other members of this class is undefined behavior.
        /// </summary>
        public bool IsLoaded { get; internal set; } = false;
        /// <summary>
        /// Indicates whether this mod is enabled. If not, all contained gadgets are disabled and all Harmony patches are not run.
        /// </summary>
        public bool Enabled { get; internal set; } = true;

        private ZipFile Archive;

        internal GadgetMod(string ModPath, IniData Manifest, Assembly Assembly, ZipFile Archive = null)
        {
            this.ModPath = ModPath;
            this.Manifest = Manifest;
            this.Assembly = Assembly;
            this.Archive = Archive;
            IsArchive = Archive != null;
            Name = Manifest["Metadata"]["Name"];
            Version = Assembly.GetName().Version;
            ModDependencies = new ReadOnlyCollection<string>(Manifest["Metadata"].ContainsKey("Dependencies") ? Manifest["Metadata"]["Dependencies"].Split(',') : new string[0]);
            AssemblyDependencies = new ReadOnlyCollection<AssemblyName>(Assembly.GetReferencedAssemblies());
        }

        /// <summary>
        /// Returns a Stream for reading a file contained within a Gadget Mod.
        /// </summary>
        public Stream ReadModFile(string relativeFilePath)
        {
            if (IsArchive)
            {
                MemoryStream stream = new MemoryStream();
                Archive[relativeFilePath].Extract(stream);
                stream.Position = 0;
                return stream;
            }
            else
            {
                return new FileStream(Path.Combine(ModPath, relativeFilePath), FileMode.Open);
            }
        }

        /// <summary>
        /// Gets a temporary reference to a file contained within a Gadget Mod. Make sure to call <see cref="GadgetModFile.Dispose"/> when you are done with it. Do not call this again before disposing of the previous instance of <see cref="GadgetModFile"/>.
        /// </summary>
        public GadgetModFile GetModFile(string relativeFilePath)
        {
            if (IsArchive)
            {
                string tempPath = Path.Combine(GadgetPaths.TempPath, Path.GetFileNameWithoutExtension(ModPath));
                if (File.Exists(Path.Combine(tempPath, relativeFilePath)))
                {
                    throw new InvalidDataException("This file has already been retrieved once, and has not yet been disposed!");
                }
                Directory.CreateDirectory(tempPath);
                Archive[relativeFilePath].Extract(tempPath);
                return new GadgetModFile(Path.Combine(tempPath, relativeFilePath), true, this);
            }
            else
            {
                return new GadgetModFile(Path.Combine(ModPath, relativeFilePath), false, this);
            }
        }

        /// <summary>
        /// Lists all mod files in the given mod directory. The returned values are relative paths that can be used by <see cref="GetModFile(string)"/> or <see cref="ReadModFile(string)"/>
        /// </summary>
        public string[] ListModFiles(string relativeDirectoryPath)
        {
            if (IsArchive)
            {
                return Archive.Where(x => x.FileName.Length > relativeDirectoryPath.Length && x.FileName.StartsWith(relativeDirectoryPath) && !x.FileName.Substring(relativeDirectoryPath.Length + 1).Contains(Path.DirectorySeparatorChar)).Select(x => x.FileName).ToArray();
            }
            else
            {
                return Directory.GetFiles(Path.Combine(ModPath, relativeDirectoryPath)).Select(x => Path.Combine(relativeDirectoryPath, Path.GetFileName(x))).ToArray();
            }
        }

        /// <summary>
        /// Lists all child mod directories of the given mod directory. Note that this will not return empty directories. The returned values are relative paths that can be used by <see cref="ListModFiles(string)"/>
        /// </summary>
        public string[] ListModDirectories(string relativeDirectoryPath)
        {
            if (IsArchive)
            {
                return Archive.Where(x => x.FileName.Length > relativeDirectoryPath.Length && x.FileName.StartsWith(relativeDirectoryPath) && x.FileName.Substring(relativeDirectoryPath.Length + 1).Contains(Path.DirectorySeparatorChar)).Select(x => x.FileName.Substring(0, x.FileName.IndexOf(Path.DirectorySeparatorChar, relativeDirectoryPath.Length + 1))).Distinct().ToArray();
            }
            else
            {
                return Directory.GetDirectories(Path.Combine(ModPath, relativeDirectoryPath)).Select(x => Path.Combine(relativeDirectoryPath, Path.GetFileName(x))).ToArray();
            }
        }

        /// <summary>
        /// Checks if this mod contains the file with the given path.
        /// </summary>
        public bool HasModFile(string relativeFilePath)
        {
            if (IsArchive)
            {
                return Archive.ContainsEntry(relativeFilePath);
            }
            else
            {
                return File.Exists(Path.Combine(ModPath, relativeFilePath));
            }
        }
    }
}
