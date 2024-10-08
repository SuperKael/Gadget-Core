﻿using GadgetCore.API;
using GadgetCore.API.Dialog;
using GadgetCore.Util;
using HarmonyLib;
using IniParser;
using IniParser.Model;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GadgetCore.MonoSymbol;

namespace GadgetCore.Loader
{
    /// <summary>
    /// Class used for loading <see cref="GadgetMod"/>s. The average Gadget should have no reason to interface with this.
    /// </summary>
    public static class GadgetLoader
    {
        internal static bool BatchLoading;

        internal static GadgetLogger Logger = new GadgetLogger("GadgetCore", "Loader");

        private static readonly FieldInfo LineNumberInfo = AccessTools.Field(typeof(StackFrame), "lineNumber");
        private static readonly FieldInfo FileNameInfo = AccessTools.Field(typeof(StackFrame), "fileName");

        private static FileIniDataParser manifestIniParser = new FileIniDataParser();

        private static List<GadgetMod> m_EmptyMods = new List<GadgetMod>();
        private static List<GadgetMod> m_IncompatibleMods = new List<GadgetMod>();
        private static List<Tuple<string, string>> m_ErroredMods = new List<Tuple<string, string>>();

        private static readonly Dictionary<string, MonoSymbolFile> LoadedSymbolFiles = new Dictionary<string, MonoSymbolFile>();
        private static readonly List<Task<MonoSymbolFile>> SymbolLoadTasks = new List<Task<MonoSymbolFile>>();

        /// <summary>
        /// List of <see cref="GadgetMod"/>s that failed to load because they don't contain any <see cref="Gadget"/>s.
        /// </summary>
        public static ReadOnlyCollection<GadgetMod> EmptyMods = new ReadOnlyCollection<GadgetMod>(m_EmptyMods);
        /// <summary>
        /// List of <see cref="GadgetMod"/>s that failed to load because they are incompatible with this version of GadgetCore, or that are missing a required dependency on another mod.
        /// </summary>
        public static ReadOnlyCollection<GadgetMod> IncompatibleMods = new ReadOnlyCollection<GadgetMod>(m_IncompatibleMods);
        /// <summary>
        /// List of mod names that failed to load because they threw an error during assembly load. (Probably a missing assembly reference that wasn't mentioned in the manifest metadata)
        /// </summary>
        public static ReadOnlyCollection<Tuple<string, string>> ErroredMods = new ReadOnlyCollection<Tuple<string, string>>(m_ErroredMods);

        internal static List<GadgetInfo> QueuedGadgets = new List<GadgetInfo>();

        internal static Dictionary<string, GadgetMod> BlameMap = new Dictionary<string, GadgetMod>();

        internal static void LoadAllMods()
        {
            try
            {
                BatchLoading = true;
                Logger.Log("Loading mod files...");
                foreach (string modDir in Directory.GetDirectories(GadgetPaths.ModsPath))
                {
                    LoadModDir(modDir);
                }
                foreach (string modFile in Directory.GetFiles(GadgetPaths.ModsPath))
                {
                    LoadModFile(modFile);
                }
                Logger.Log("Done loading mod files.");
                if (SymbolLoadTasks.Count > 0)
                {
                    Logger.Log("Loading mod symbols...");
                    Task.WaitAll(SymbolLoadTasks.ToArray());
                    SymbolLoadTasks.Clear();
                    Logger.Log("Done loading mod symbols.");
                }
                Logger.Log("Loading mods...");
                foreach (GadgetMod mod in GadgetMods.ListAllMods(true))
                {
                    LoadGadgetMod(mod);
                }
                Logger.Log("Done loading mods.");
                if (GadgetCoreAPI.GetUMFAPI() != null)
                {
                    Logger.Log("Loading Gadgets inside UMF mods...");
                    foreach (string mod in GadgetCoreAPI.GetUMFAPI().GetModNamesEnabled())
                    {
                        ProcessUMFMod(mod);
                    }
                    Logger.Log("Done loading Gadgets inside UMF mods.");
                }
                Logger.Log("Sorting Gadgets...");
                Gadgets.SortGadgets();
                Logger.Log("Done sorting Gadgets.");
                Logger.Log("Queueing Gadgets for initialization...");
                foreach (GadgetInfo gadget in Gadgets.ListAllEnabledGadgetInfos())
                {
                    QueuedGadgets.Add(gadget);
                }
                Logger.Log("Done queueing Gadgets.");
                EnableQueuedGadgets();
                BatchLoading = false;
                Logger.Log("Initial load complete.");
            }
            catch (Exception e)
            {
                Logger.LogError("Uncaught exception during mod load: " + e);
            }
        }

        /// <summary>
        /// Loads and registers the <see cref="GadgetMod"/> represented by the given directory. Will log a warning and return null if the given directory is not a valid <see cref="GadgetMod"/>. Will also add the name to <see cref="ErroredMods"/> if the directory was valid, but an error occured during the mod-loading process.
        /// </summary>
        public static GadgetMod LoadModDir(string modDir)
        {
            string modName = null;
            try
            {
                string manifestFile = Path.Combine(modDir, "Manifest.ini");
                if (File.Exists(manifestFile))
                {
                    if (Directory.Exists(Path.Combine(modDir, "Libs")))
                    {
                        foreach (string lib in Directory.GetFiles(Path.Combine(modDir, "Libs")))
                        {
                            string existingFilePath = Path.Combine(GadgetPaths.LibsPath, Path.GetFileName(lib));
                            FileInfo libFile = new FileInfo(lib);
                            if (!File.Exists(existingFilePath) || libFile.LastWriteTime > new FileInfo(existingFilePath).LastWriteTime)
                            {
                                libFile.CopyTo(existingFilePath, true);
                            }
                        }
                    }
                    IniData manifest = manifestIniParser.ReadFile(manifestFile);
                    modName = manifest["Metadata"]["Name"];
                    string assemblyFile = Path.Combine(modDir, manifest["Metadata"]["Assembly"]);
                    byte[] assemblyData = File.ReadAllBytes(assemblyFile);
                    string symbolFile = Path.ChangeExtension(assemblyFile, ".pdb");
                    byte[] symbolData = File.Exists(symbolFile) ? File.ReadAllBytes(symbolFile) : null;
                    Assembly modAssembly = Assembly.Load(assemblyData, symbolData);
                    if (symbolData != null)
                    {
                        SymbolLoadTasks.Add(LoadSymbolsInternal(modAssembly.GetName().Name, assemblyData, symbolData));
                    }
                    GadgetCore.LoadedAssemblies[modAssembly.GetName().Name] = modAssembly;
                    GadgetMod mod = new GadgetMod(modDir, manifest, modAssembly);
                    foreach (Type type in modAssembly.GetTypes())
                    {
                        if (type.FullName != null) BlameMap[type.FullName] = mod;
                    }
                    GadgetMods.RegisterMod(mod);
                    if (!BatchLoading) LoadGadgetMod(mod);
                    return mod;
                }
                else
                {
                    Logger.LogWarning("Invalid or non-mod directory '" + Path.GetFileName(modDir) + "' in Mods directory!");
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Exception loading mod directory '" + Path.GetFileName(modDir) + "':");
                Logger.LogError(e.ToString());
                if (modName != null)
                {
                    Tuple<string, string> erroredMod = Tuple.Create(modName, modDir);
                    if (!m_ErroredMods.Contains(erroredMod)) m_ErroredMods.Add(erroredMod);
                }
            }
            return null;
        }

        /// <summary>
        /// Loads and registers the <see cref="GadgetMod"/> represented by the given file. Will log a warning and return null if the given file is not a valid <see cref="GadgetMod"/>. Will also add the name to <see cref="ErroredMods"/> if the file was valid, but an error occured during the mod-loading process.
        /// </summary>
        public static GadgetMod LoadModFile(string modFile)
        {
            string modName = null;
            ZipFile modZip = null;
            try
            {
                modZip = new ZipFile(modFile);
                if (Path.GetExtension(modFile) == ".umfmod")
                {
                    GadgetCore.CoreLib.DecryptUMFModFile(modZip);
                }
                if (modZip.ContainsEntry("Manifest.ini"))
                {
                    if (modZip.ContainsEntry("Libs"))
                    {
                        foreach (ZipEntry lib in modZip.Entries.Where(x => !x.IsDirectory && x.FileName.StartsWith("Libs")))
                        {
                            string existingFilePath = Path.Combine(GadgetPaths.GadgetCorePath, lib.FileName);
                            if (!File.Exists(existingFilePath) || lib.ModifiedTime > new FileInfo(existingFilePath).LastWriteTime)
                            {
                                lib.Extract(GadgetPaths.GadgetCorePath, ExtractExistingFileAction.OverwriteSilently);
                            }
                        }
                    }
                    using (MemoryStream stream = new MemoryStream())
                    {
                        modZip["Manifest.ini"].Extract(stream);
                        stream.Position = 0;
                        IniData manifest = manifestIniParser.ReadData(new StreamReader(stream));
                        modName = manifest["Metadata"]["Name"];
                        stream.SetLength(0);
                        string assemblyEntry = manifest["Metadata"]["Assembly"];
                        if (assemblyEntry != null && modZip.ContainsEntry(assemblyEntry))
                        {
                            modZip[assemblyEntry].Extract(stream);
                        }
                        else
                        {
                            Logger.LogWarning("Failed to load mod `" + modName + "` because " + (manifest["Metadata"]["Assembly"] != null ? "the assembly name in its manifest is not valid!" : "its manifest does not contain the name of its asembly!"));
                            return null;
                        }
                        byte[] assemblyData = stream.ToArray();
                        stream.SetLength(0);
                        string symbolEntry = Path.ChangeExtension(assemblyEntry, ".pdb");
                        byte[] symbolData = null;
                        if (modZip.ContainsEntry(symbolEntry))
                        {
                            modZip[symbolEntry].Extract(stream);
                            symbolData = stream.ToArray();
                        }
                        Assembly modAssembly = Assembly.Load(assemblyData, symbolData);
                        if (symbolData != null)
                        {
                            SymbolLoadTasks.Add(LoadSymbolsInternal(modAssembly.GetName().Name, assemblyData, symbolData));
                        }
                        GadgetCore.LoadedAssemblies[modAssembly.GetName().Name] = modAssembly;
                        GadgetMod mod = new GadgetMod(modFile, manifest, modAssembly, modZip);
                        foreach (Type type in modAssembly.GetTypes())
                        {
                            if (type.FullName != null) BlameMap[type.FullName] = mod;
                        }
                        GadgetMods.RegisterMod(mod);
                        if (!BatchLoading) LoadGadgetMod(mod);
                        modZip = null;
                        return mod;
                    }
                }
                else
                {
                    Logger.LogWarning("Invalid or non-mod file '" + Path.GetFileName(modFile) + "' in Mods directory!");
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Exception loading mod file '" + Path.GetFileName(modFile) + "':");
                Logger.LogError(e.ToString());
                if (modName != null)
                {
                    Tuple<string, string> erroredMod = Tuple.Create(modName, modFile);
                    if (!m_ErroredMods.Contains(erroredMod)) m_ErroredMods.Add(erroredMod);
                }
            }
            finally
            {
                modZip?.Dispose();
            }
            return null;
        }

        private static void LoadGadgetMod(GadgetMod mod)
        {
            Logger.Log("Loading mod '" + mod.Name + "'");
            if (mod.ModDependencies.Any(x => GadgetMods.GetModByName(x) == null))
            {
                m_IncompatibleMods.Add(mod);
                Logger.LogWarning("Aborted loading mod '" + mod.Name + "' because of the following missing dependencies: " + mod.ModDependencies.Where(x => GadgetMods.GetModByName(x) == null).Concat());
                return;
            }
            Type[] gadgetTypes = mod.Assembly.GetExportedTypes().Where(x => x.IsSubclassOf(typeof(Gadget)) && x.GetCustomAttributes(typeof(GadgetAttribute), true).FirstOrDefault() != null).ToArray();
            if (gadgetTypes.Length == 0)
            {
                m_EmptyMods.Add(mod);
                Logger.LogWarning("Aborted loading mod '" + mod.Name + "' because it does not contain any Gadgets");
            }
            mod.m_LoadedGadgets = new List<GadgetInfo>();
            mod.LoadedGadgets = new ReadOnlyCollection<GadgetInfo>(mod.m_LoadedGadgets);
            mod.m_UnloadedGadgets = new List<GadgetInfo>();
            mod.UnloadedGadgets = new ReadOnlyCollection<GadgetInfo>(mod.m_UnloadedGadgets);
            for (int i = 0; i < gadgetTypes.Length; i++)
            {
                Type type = gadgetTypes[i];
                GadgetAttribute attribute = (GadgetAttribute)type.GetCustomAttributes(typeof(GadgetAttribute), true).FirstOrDefault();
                if (mod.m_LoadedGadgets.Any(x => x.Attribute.Name == attribute.Name)) throw new InvalidOperationException("It is illegal for a mod to contain multiple Gadgets with the same name: " + attribute.Name);
                int[] targetVersionNums = attribute.TargetGCVersion.Split('.').Select(x => int.Parse(x)).ToArray();
                if (targetVersionNums.Length != 4) Array.Resize(ref targetVersionNums, 4);
                if ((attribute.GadgetCoreVersionSpecificity == VersionSpecificity.MAJOR && GadgetCoreAPI.CurrentVersionNums[0] == targetVersionNums[0] && (GadgetCoreAPI.CurrentVersionNums[1] > targetVersionNums[1] || (GadgetCoreAPI.CurrentVersionNums[1] == targetVersionNums[1] && (GadgetCoreAPI.CurrentVersionNums[2] > targetVersionNums[2] || (GadgetCoreAPI.CurrentVersionNums[2] == targetVersionNums[2] && GadgetCoreAPI.CurrentVersionNums[3] >= targetVersionNums[3]))))) ||
                    (attribute.GadgetCoreVersionSpecificity == VersionSpecificity.MINOR && GadgetCoreAPI.CurrentVersionNums[0] == targetVersionNums[0] && GadgetCoreAPI.CurrentVersionNums[1] == targetVersionNums[1] && (GadgetCoreAPI.CurrentVersionNums[2] > targetVersionNums[2] || (GadgetCoreAPI.CurrentVersionNums[2] == targetVersionNums[2] && GadgetCoreAPI.CurrentVersionNums[3] >= targetVersionNums[3]))) ||
                    (attribute.GadgetCoreVersionSpecificity == VersionSpecificity.NONBREAKING && GadgetCoreAPI.CurrentVersionNums[0] == targetVersionNums[0] && GadgetCoreAPI.CurrentVersionNums[1] == targetVersionNums[1] && GadgetCoreAPI.CurrentVersionNums[2] == targetVersionNums[2] && GadgetCoreAPI.CurrentVersionNums[3] >= targetVersionNums[3]) ||
                    (attribute.GadgetCoreVersionSpecificity == VersionSpecificity.BUGFIX && GadgetCoreAPI.CurrentVersionNums[0] == targetVersionNums[0] && GadgetCoreAPI.CurrentVersionNums[1] == targetVersionNums[1] && GadgetCoreAPI.CurrentVersionNums[2] == targetVersionNums[2] && GadgetCoreAPI.CurrentVersionNums[3] == targetVersionNums[3]))
                {
                    Gadget gadget = null;
                    try
                    {
                        gadget = Activator.CreateInstance(type) as Gadget;
                    }
                    catch (Exception e)
                    {
                        Logger.LogWarning("Found Gadget that could not be constructed: " + attribute.Name + ", in mod: {" + mod.Name + "}: Error: " + e);
                    }
                    if (gadget != null)
                    {
                        try
                        {
                            Logger.Log("Found Gadget to load: " + attribute.Name + ", in mod: {" + mod.Name + "}");
                            gadget.CreateSingleton(gadget);
                            GadgetInfo info = new GadgetInfo(gadget, attribute, mod);
                            gadget.Logger = new GadgetLogger(mod.Name, attribute.Name);
                            string configFileName = Path.Combine(GadgetPaths.ConfigsPath, mod.Assembly.GetName().Name) + ".ini";
                            string oldConfigFileName = Path.Combine(GadgetPaths.ConfigsPath, mod.Name + ".ini");
                            if (!File.Exists(configFileName) && File.Exists(oldConfigFileName)) File.Move(oldConfigFileName, configFileName);
                            gadget.Config = new GadgetConfig(configFileName, Regex.Replace(attribute.Name, @"\s+", ""));
                            Gadgets.RegisterGadget(info);
                            mod.m_LoadedGadgets.Add(info);
                            if (!BatchLoading)
                            {
                                QueuedGadgets.Add(info);
                                EnableQueuedGadgets();
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.LogWarning("Found Gadget that had an error during registration: " + attribute.Name + ", in mod: {" + mod.Name + "}. Error: " + e);
                            if (Gadgets.GetGadgetInfo(attribute.Name) != null)
                            {
                                Gadgets.UnregisterGadget(Gadgets.GetGadgetInfo(attribute.Name));
                            }
                        }
                    }
                }
                else
                {
                    int rD = (int)attribute.GadgetCoreVersionSpecificity;
                    Logger.LogWarning("Found Gadget with an incompatible version: " + attribute.Name + ", in mod: {" + mod.Name + "}. Requires at least version: " + attribute.TargetGCVersion + ", but no greater than version: " + new string(attribute.TargetGCVersion.TakeWhile(x => (x == '.' ? --rD : rD) > 0).ToArray()) + Enumerable.Repeat(".x", 4 - (int)attribute.GadgetCoreVersionSpecificity).Concat(""));
                }
            }
            mod.IsLoaded = true;
        }

        private static void ProcessUMFMod(string modName)
        {
            Logger.Log("Processing UMF mod '" + modName + "'");
            Type[] gadgetTypes = GadgetCoreAPI.GetUMFAPI().GetModAssembly(modName).GetExportedTypes().Where(x => x.IsSubclassOf(typeof(Gadget)) && x.GetCustomAttributes(typeof(GadgetAttribute), true).FirstOrDefault() != null).ToArray();
            List<GadgetInfo> gadgets = new List<GadgetInfo>();
            for (int i = 0; i < gadgetTypes.Length; i++)
            {
                Type type = gadgetTypes[i];
                GadgetAttribute attribute = (GadgetAttribute)type.GetCustomAttributes(typeof(GadgetAttribute), true).FirstOrDefault();
                if (gadgets.Any(x => x.Attribute.Name == attribute.Name)) throw new InvalidOperationException("It is illegal for a mod to contain multiple Gadgets with the same name: " + attribute.Name);
                int[] targetVersionNums = attribute.TargetGCVersion.Split('.').Select(x => int.Parse(x)).ToArray();
                if (targetVersionNums.Length != 4) Array.Resize(ref targetVersionNums, 4);
                if ((attribute.GadgetCoreVersionSpecificity == VersionSpecificity.MAJOR && GadgetCoreAPI.CurrentVersionNums[0] == targetVersionNums[0] && (GadgetCoreAPI.CurrentVersionNums[1] > targetVersionNums[1] || (GadgetCoreAPI.CurrentVersionNums[1] == targetVersionNums[1] && (GadgetCoreAPI.CurrentVersionNums[2] > targetVersionNums[2] || (GadgetCoreAPI.CurrentVersionNums[2] == targetVersionNums[2] && GadgetCoreAPI.CurrentVersionNums[3] >= targetVersionNums[3]))))) ||
                    (attribute.GadgetCoreVersionSpecificity == VersionSpecificity.MINOR && GadgetCoreAPI.CurrentVersionNums[0] == targetVersionNums[0] && GadgetCoreAPI.CurrentVersionNums[1] == targetVersionNums[1] && (GadgetCoreAPI.CurrentVersionNums[2] > targetVersionNums[2] || (GadgetCoreAPI.CurrentVersionNums[2] == targetVersionNums[2] && GadgetCoreAPI.CurrentVersionNums[3] >= targetVersionNums[3]))) ||
                    (attribute.GadgetCoreVersionSpecificity == VersionSpecificity.NONBREAKING && GadgetCoreAPI.CurrentVersionNums[0] == targetVersionNums[0] && GadgetCoreAPI.CurrentVersionNums[1] == targetVersionNums[1] && GadgetCoreAPI.CurrentVersionNums[2] == targetVersionNums[2] && GadgetCoreAPI.CurrentVersionNums[3] >= targetVersionNums[3]) ||
                    (attribute.GadgetCoreVersionSpecificity == VersionSpecificity.BUGFIX && GadgetCoreAPI.CurrentVersionNums[0] == targetVersionNums[0] && GadgetCoreAPI.CurrentVersionNums[1] == targetVersionNums[1] && GadgetCoreAPI.CurrentVersionNums[2] == targetVersionNums[2] && GadgetCoreAPI.CurrentVersionNums[3] == targetVersionNums[3]))
                {
                    Gadget gadget = null;
                    try
                    {
                        gadget = Activator.CreateInstance(type) as Gadget;
                        if (gadget != null)
                        {
                            Logger.Log("Found Gadget to load: " + attribute.Name + ", in UMF mod: {" + modName + "}");
                            GadgetInfo info = new GadgetInfo(gadget, attribute, modName);
                            gadget.Logger = new GadgetLogger(modName, attribute.Name);
                            gadget.Config = new GadgetConfig(Path.Combine(GadgetPaths.ConfigsPath, modName + ".ini"), attribute.Name);
                            Gadgets.RegisterGadget(info);
                            gadgets.Add(info);
                            if (!BatchLoading)
                            {
                                QueuedGadgets.Add(info);
                                EnableQueuedGadgets();
                            }
                        }
                    }
                    catch (Exception) { }
                    finally
                    {
                        if (gadget == null)
                        {
                            Logger.LogWarning("Found Gadget that could not be constructed: " + attribute.Name + ", in UMF mod: {" + modName + "}");
                        }
                    }
                }
                else
                {
                    int rD = (int)attribute.GadgetCoreVersionSpecificity;
                    Logger.LogWarning("Found Gadget with an incompatible version: " + attribute.Name + ", in UMF mod: {" + modName + "}. Requires version: " + new string(attribute.TargetGCVersion.TakeWhile(x => (x == '.' ? --rD : rD) > 0).ToArray()));
                }
            }
        }

        internal static void EnableQueuedGadgets()
        {
            List<GadgetInfo> preFilteredQueuedGadgets = QueuedGadgets;
            QueuedGadgets = new List<GadgetInfo>();
            foreach (GadgetInfo gadget in preFilteredQueuedGadgets)
            {
                bool valid = true;
                foreach (string dependency in gadget.Attribute.Dependencies)
                {
                    string[] splitDependency = dependency.Split(':');
                    GadgetInfo dependencyGadget = Gadgets.GetGadgetInfo(splitDependency[0]);
                    valid = dependencyGadget != null && dependencyGadget.Gadget.Enabled;
                    if (valid)
                    {
                        if (splitDependency.Length == 2)
                        {
                            string versionString = splitDependency[1].TrimStart('v');
                            int[] targetVersionNums = versionString.Split('.').Select(x => int.Parse(x)).ToArray();
                            if (targetVersionNums.Length > 4) continue;
                            int[] actualVersionNums = dependencyGadget.Mod.Version.ToString().Split('.').Select(x => int.Parse(x)).ToArray();
                            if (targetVersionNums.Length != 4) Array.Resize(ref targetVersionNums, 4);
                            if (actualVersionNums.Length != 4) Array.Resize(ref actualVersionNums, 4);
                            valid = actualVersionNums[0] == targetVersionNums[0] && actualVersionNums[1] == targetVersionNums[1] && (actualVersionNums[2] > targetVersionNums[2] || (actualVersionNums[2] == targetVersionNums[2] && actualVersionNums[3] >= targetVersionNums[3]));
                        }
                        else if (splitDependency.Length == 3)
                        {
                            string versionString = splitDependency[1].TrimStart('v');
                            int[] targetVersionNums = versionString.Split('.').Select(x => int.Parse(x)).ToArray();
                            if (targetVersionNums.Length > 4) continue;
                            int[] actualVersionNums = dependencyGadget.Mod.Version.ToString().Split('.').Select(x => int.Parse(x)).ToArray();
                            VersionSpecificity versionSpecificity;
                            try
                            {
                                versionSpecificity = (VersionSpecificity)Enum.Parse(typeof(VersionSpecificity), splitDependency[2], true);
                            }
                            catch (ArgumentException)
                            {
                                Logger.LogWarning("Gadget " + gadget.Attribute.Name + " has an improperly-formatted dependency version string: " + dependency);
                                versionSpecificity = VersionSpecificity.MINOR;
                            }
                            if (targetVersionNums.Length != 4) Array.Resize(ref targetVersionNums, 4);
                            if (actualVersionNums.Length != 4) Array.Resize(ref actualVersionNums, 4);
                            valid = (versionSpecificity == VersionSpecificity.MAJOR && actualVersionNums[0] == targetVersionNums[0] && (actualVersionNums[1] > targetVersionNums[1] || (actualVersionNums[1] == targetVersionNums[1] && (actualVersionNums[2] > targetVersionNums[2] || (actualVersionNums[2] == targetVersionNums[2] && actualVersionNums[3] >= targetVersionNums[3]))))) ||
                                    (versionSpecificity == VersionSpecificity.MINOR && actualVersionNums[0] == targetVersionNums[0] && actualVersionNums[1] == targetVersionNums[1] && (actualVersionNums[2] > targetVersionNums[2] || (actualVersionNums[2] == targetVersionNums[2] && actualVersionNums[3] >= targetVersionNums[3]))) ||
                                    (versionSpecificity == VersionSpecificity.NONBREAKING && actualVersionNums[0] == targetVersionNums[0] && actualVersionNums[1] == targetVersionNums[1] && actualVersionNums[2] == targetVersionNums[2] && actualVersionNums[3] >= targetVersionNums[3]) ||
                                    (versionSpecificity == VersionSpecificity.BUGFIX && actualVersionNums[0] == targetVersionNums[0] && actualVersionNums[1] == targetVersionNums[1] && actualVersionNums[2] == targetVersionNums[2] && actualVersionNums[3] == targetVersionNums[3]);
                        }
                    }
                }
                if (valid)
                {
                    QueuedGadgets.Add(gadget);
                }
                else
                {
                    gadget.Gadget.Enabled = false;
                    GadgetCoreConfig.enabledGadgets[gadget.Attribute.Name] = false;
                    Gadgets.UnregisterGadget(gadget);
                    Logger.LogWarning("Aborted loading Gadget " + gadget.Attribute.Name + " due to missing dependencies.");
                }
            }
            Logger.Log("Loading Gadget configs...");
            foreach (GadgetInfo gadget in QueuedGadgets.ToList())
            {
                Logger.Log("Loading Config for Gadget '" + gadget.Attribute.Name + "'");
                try
                {
                    gadget.Gadget.LoadConfig();
                }
                catch (Exception e)
                {
                    gadget.Gadget.Enabled = false;
                    GadgetCoreConfig.enabledGadgets[gadget.Attribute.Name] = false;
                    Gadgets.UnregisterGadget(gadget);
                    QueuedGadgets.Remove(gadget);
                    Logger.LogError("Exception Loading Config For Gadget '" + gadget.Attribute.Name + "':" + Environment.NewLine + e.ToString());
                }
            }
            Logger.Log("Done loading Gadget configs.");
            Logger.Log("Preparing Gadgets for patching...");
            foreach (GadgetInfo gadget in QueuedGadgets.ToList())
            {
                Logger.Log("PrePatching Gadget '" + gadget.Attribute.Name + "'");
                try
                {
                    gadget.Gadget.HarmonyInstance = new Harmony(gadget.Mod.Name + "." + gadget.Attribute.Name + ".gadget");
                    gadget.Gadget.PrePatch();
                }
                catch (Exception e)
                {
                    gadget.Gadget.Enabled = false;
                    GadgetCoreConfig.enabledGadgets[gadget.Attribute.Name] = false;
                    Gadgets.UnregisterGadget(gadget);
                    QueuedGadgets.Remove(gadget);
                    Logger.LogError("Exception PrePatching Gadget '" + gadget.Attribute.Name + "':" + Environment.NewLine + e.ToString());
                }
            }
            Logger.Log("Done preparing Gadgets for patching.");
            Logger.Log("Patching Gadgets...");
            foreach (GadgetInfo gadget in QueuedGadgets.ToList())
            {
                int patches = 0, totalMethods = 0, errorPatching = 0, targetMissing = 0;
                try
                {
                    gadget.Mod.Assembly.GetExportedTypes().Do(delegate (Type type)
                    {
                        HarmonyGadgetAttribute attribute = type.GetCustomAttributes(true).FirstOrDefault(x => x.GetType() == typeof(HarmonyGadgetAttribute)) as HarmonyGadgetAttribute;
                        if (attribute?.Gadget == gadget.Attribute.Name && attribute.RequiredGadgets.All(x => Gadgets.GetGadget(x) != null))
                        {
                            try
                            {
                                List<MethodInfo> methods = gadget.Gadget.HarmonyInstance.CreateClassProcessor(type).Patch();
                                totalMethods += methods?.Count ?? 0;
                                if (methods == null || methods.Count == 0)
                                {
                                    Logger.Log("Skipping patch '" + type.Name + "' for Gadget '" + gadget.Attribute.Name + "': Attribute target does not exist.");
                                    targetMissing++;
                                }
                                else
                                {
                                    patches++;
                                }
                            }
                            catch (Exception e)
                            {
                                if (e.InnerException == null || !e.InnerException.Message.EndsWith("returned an unexpected result: null"))
                                {
                                    Logger.LogError("Exception running patch '" + type.Name + "' for Gadget '" + gadget.Attribute.Name + "': " + Environment.NewLine + e.ToString());
                                    errorPatching++;
                                }
                                else
                                {
                                    Logger.Log("Skipping patch '" + type.Name + "' for Gadget '" + gadget.Attribute.Name + "': TargetMethod returned null.");
                                    targetMissing++;
                                }
                            }
                        }
                    });
                    Logger.Log("Performed " + patches + " patches for '" + gadget.Attribute.Name + "'" + (totalMethods > 0 ? $" ({totalMethods} total methods patched)" : string.Empty) + (errorPatching > 0 ? targetMissing > 0 ? $" ({errorPatching} skipped due to errors, {targetMissing} skipped due to missing targets)" : $" ({errorPatching} skipped due to errors)" : targetMissing > 0 ? $" ({targetMissing} skipped due to missing targets)" : string.Empty));
                }
                catch (Exception e)
                {
                    gadget.Gadget.Enabled = false;
                    GadgetCoreConfig.enabledGadgets[gadget.Attribute.Name] = false;
                    Gadgets.UnregisterGadget(gadget);
                    QueuedGadgets.Remove(gadget);
                    Logger.LogError("Exception patching Gadget '" + gadget.Attribute.Name + "':" + Environment.NewLine + e.ToString());
                }
            }
            Logger.Log("Done patching Gadgets.");
            bool logOverrideWarnings = false;
            foreach (MethodBase patchedMethod in Harmony.GetAllPatchedMethods())
            {
                HarmonyLib.Patches patches = Harmony.GetPatchInfo(patchedMethod);
                if (patches == null) continue;
                Dictionary<string, string> owners = new Dictionary<string, string>();
                List<Patch> overridingPrefixes = patches.Prefixes.Where(x => x.PatchMethod.ReturnType == typeof(bool) && (x.PatchMethod.GetCustomAttributes(true).FirstOrDefault(a => a.GetType() == typeof(HarmonyOverridesAttribute)) as HarmonyOverridesAttribute)?.Overrides.Length != 0).ToList();
                if (overridingPrefixes != null && overridingPrefixes.Count > 0)
                {
                    foreach (string ownerID in patches.Owners)
                    {
                        if (ownerID == GadgetCore.HarmonyInstance.Id)
                        {
                            owners[ownerID] = "GadgetCore";
                        }
                        else
                        {
                            string[] splitOwnerID = ownerID.Split('.');
                            if (splitOwnerID.Length == 3 && splitOwnerID[2] == "gadget")
                            {
                                GadgetMod mod = GadgetMods.GetModByName(splitOwnerID[0]);
                                GadgetInfo gadget = mod?.LoadedGadgets.SingleOrDefault(x => x.Attribute.Name == splitOwnerID[1]);
                                if (gadget != null)
                                {
                                    owners[ownerID] = $"'{splitOwnerID[1]}' from the mod {{{splitOwnerID[0]}}}";
                                }
                                else
                                {
                                    owners[ownerID] = $"Unrecognized patcher '{ownerID}'";
                                }
                            }
                            else
                            {
                                owners[ownerID] = $"Unrecognized patcher '{ownerID}'";
                            }
                        }
                    }
                    IEnumerable<Patch> problematicPrefixes = patches.Prefixes.Where(x => !overridingPrefixes.Contains(x) && (x.PatchMethod.GetCustomAttributes(true).FirstOrDefault(a => a.GetType() == typeof(HarmonyOverriddenAttribute)) as HarmonyOverriddenAttribute)?.Overrides.Length != 0 && overridingPrefixes.Any(
                        p => x.owner != p.owner &&
                        (x.PatchMethod.GetCustomAttributes(true).FirstOrDefault(a => a.GetType() == typeof(HarmonyOverriddenAttribute)) as HarmonyOverriddenAttribute)?.Overrides.Contains(p.owner) != true &&
                        (p.PatchMethod.GetCustomAttributes(true).FirstOrDefault(a => a.GetType() == typeof(HarmonyOverridesAttribute)) as HarmonyOverridesAttribute)?.Overrides.Contains(x.owner) != true && 
                        x.priority <= p.priority && 
                        !x.before.Contains(p.owner) && 
                        !p.after.Contains(x.owner)));
                    IEnumerable<Patch> problematicTranspilers = patches.Transpilers.Where(x => (x.PatchMethod.GetCustomAttributes(true).FirstOrDefault(a => a.GetType() == typeof(HarmonyOverriddenAttribute)) as HarmonyOverriddenAttribute)?.Overrides.Length != 0 && overridingPrefixes.Any(
                        p => x.owner != p.owner &&
                        (x.PatchMethod.GetCustomAttributes(true).FirstOrDefault(a => a.GetType() == typeof(HarmonyOverriddenAttribute)) as HarmonyOverriddenAttribute)?.Overrides.Contains(p.owner) != true &&
                        (p.PatchMethod.GetCustomAttributes(true).FirstOrDefault(a => a.GetType() == typeof(HarmonyOverridesAttribute)) as HarmonyOverridesAttribute)?.Overrides.Contains(x.owner) != true));
                    if (problematicPrefixes != null && problematicPrefixes.Any() || problematicTranspilers != null && problematicTranspilers.Any())
                    {
                        if (!logOverrideWarnings)
                        {
                            logOverrideWarnings = true;
                            Logger.LogWarning("Possibly problematic patch overrides detected!", false);
                        }
                        Logger.Log($"Patches to {patchedMethod.DeclaringType.FullName}.{patchedMethod.Name} by {overridingPrefixes.Select(x => owners[x.owner]).Concat()} may override the following patch{(problematicPrefixes.Count() + problematicTranspilers.Count() > 1 ? "es" : "")}:");
                        if (problematicPrefixes != null)
                        {
                            foreach (Patch patch in problematicPrefixes)
                            {
                                Logger.Log($" - Prefix from {owners[patch.owner]}");
                            }
                        }
                        if (problematicTranspilers != null)
                        {
                            foreach (Patch patch in problematicTranspilers)
                            {
                                Logger.Log($" - Transpiler from {owners[patch.owner]}");
                            }
                        }
                    }
                }
            }
            if (logOverrideWarnings)
            {
                Logger.Log("End of possibly problematic patch overrides.");
            }
            Logger.Log("Creating registries...");
            foreach (GadgetInfo gadget in QueuedGadgets.ToList())
            {
                if (gadget.Gadget.Enabled)
                {
                    foreach (Registry registry in gadget.Gadget.CreateRegistries())
                    {
                        GameRegistry.RegisterRegistry(registry);
                    }
                }
            }
            Logger.Log("Done creating registries.");
            Logger.Log("Initializing Gadgets...");
            foreach (GadgetInfo gadget in QueuedGadgets.ToList())
            {
                Logger.Log("Initializing Gadget '" + gadget.Attribute.Name + "'");
                try
                {
                    Registry.gadgetRegistering = gadget.Gadget.ModID;
                    gadget.Gadget.LoadInternal();
                }
                catch (Exception e)
                {
                    gadget.Gadget.Enabled = false;
                    GadgetCoreConfig.enabledGadgets[gadget.Attribute.Name] = false;
                    Gadgets.UnregisterGadget(gadget);
                    QueuedGadgets.Remove(gadget);
                    Logger.LogError("Exception Initializing Gadget '" + gadget.Attribute.Name + "':" + Environment.NewLine + e.ToString());
                }
                finally
                {
                    Registry.gadgetRegistering = -1;
                }
            }
            Logger.Log("Done initializing Gadgets.");
            QueuedGadgets.Clear();
            GadgetCoreConfig.Update();
        }

        internal static void DisableQueuedGadgets()
        {
            Logger.Log("Unloading Gadgets...");
            foreach (GadgetInfo gadget in QueuedGadgets)
            {
                Logger.Log("Unloading Gadget '" + gadget.Attribute.Name + "'");
                gadget.Gadget.UnloadInternal();
                int modID = gadget.Gadget.ModID;
                LootTables.RemoveModEntries(modID);
                GadgetCoreAPI.RemoveModResources(modID);
                GadgetCoreAPI.UnregisterCustomRPCGadget(modID);
                GadgetCoreAPI.UnregisterStatModifiers(modID);
                GadgetConsole.UnregisterGadgetCommands(modID);
                GadgetNetwork.UnregisterSyncVars(modID);
                PlanetRegistry.UnregisterGadget(modID);
                DialogChains.UnregisterGadgetChains(modID);
                CraftMenuInfo.RemoveAllModCraftPerformers(modID);
                foreach (Registry reg in GameRegistry.ListAllRegistries())
                {
                    reg.UnregisterGadget(gadget);
                }
            }
            Logger.Log("Done unloading Gadgets.");
            Logger.Log("Unpatching Gadgets...");
            foreach (GadgetInfo gadget in QueuedGadgets.ToList())
            {
                if (gadget.Gadget.HarmonyInstance == null) continue;
                Logger.Log("Unpatching Gadget '" + gadget.Attribute.Name + "'");
                try
                {
                    gadget.Gadget.HarmonyInstance.UnpatchAll(gadget.Mod.Name + "." + gadget.Attribute.Name + ".gadget");
                }
                catch (Exception e)
                {
                    Logger.LogError("Exception Unpatching Gadget '" + gadget.Attribute.Name + "':" + Environment.NewLine + e.ToString());
                }
            }
            Logger.Log("Done unpatching Gadgets.");
            Logger.Log("Sorting Gadgets...");
            Gadgets.SortGadgets();
            Logger.Log("Done sorting Gadgets.");
            QueuedGadgets.Clear();
        }

        /// <summary>
        /// Unloads the given GadgetMod.
        /// Cannot fully release the mod's RAM usage, as the old instance of the mod assembly that is stored in RAM cannot be fully removed.
        /// WARNING: Will trigger an immediate garbage collection, causing a short freeze!
        /// </summary>
        public static void UnloadMod(GadgetMod mod)
        {
            try
            {
                if (mod.Name == "GadgetCore") throw new InvalidOperationException(GadgetMods.GetModByAssembly(Assembly.GetCallingAssembly()).Name + " seriously just tried to unload GadgetCore... Really?");
                List<GadgetMod> modsToUnload = mod.LoadedGadgets.SelectMany(x => Gadgets.LoadOrderTree.Find(x).FlattenUniqueByBreadth().Where(y => y != x)).Where(x => x != null).Select(x => x.Mod).Distinct().ToList();
                bool wasBatchLoading = BatchLoading;
                BatchLoading = true;
                foreach (GadgetMod modToUnload in modsToUnload)
                {
                    UnloadModInternal(modToUnload);
                }
                mod.Unload();
                GC.Collect();
                BatchLoading = wasBatchLoading;
                DisableQueuedGadgets();
                Gadgets.SortGadgets();
            }
            catch (Exception e)
            {
                Logger.LogError("Error unloading " + mod.Name + ": " + e);
            }
        }

        private static void UnloadModInternal(GadgetMod mod)
        {
            foreach (GadgetMod dependency in GadgetMods.ListAllMods().Where(x => x.ModDependencies.Contains(mod.Name)))
            {
                if (dependency.Name != "GadgetCore") UnloadModInternal(mod);
            }
            GadgetMods.UnregisterMod(mod);
            GadgetCore.LoadedAssemblies.Remove(mod.Assembly.GetName().Name);
            mod.Unload();
        }

        /// <summary>
        /// Soft-reloads the given GadgetMod. (Does not load it freshly from the disk).
        /// Also refreshes all mods that are dependent on this mod.
        /// Be warned that this only reloads the mod's <see cref="Gadget"/> instances, so some mods may not function as expected after a refresh.
        /// </summary>
        public static GadgetMod RefreshMod(ref GadgetMod mod)
        {
            return mod = RefreshMod(mod);
        }

        /// <summary>
        /// Soft-reloads the given GadgetMod. (Does not load it freshly from the disk).
        /// Also refreshes all Gadgets that are dependent on these Gadgets.
        /// Be warned that this only reloads the mod's <see cref="Gadget"/> instances, so some mods may not function as expected after a refresh.
        /// </summary>
        public static GadgetMod RefreshMod(GadgetMod mod)
        {
            foreach (GadgetInfo gadget in mod.LoadedGadgets)
            {
                Gadgets.ReloadGadget(gadget);
            }
            return mod;
        }

        /// <summary>
        /// Hard-reloads the given GadgetMod. (Loads it freshly from the disk).
        /// Also reloads all mods that are dependent on this mod.
        /// Causes a permanent increase in the game's RAM usage, as the old instance of the mod assembly that is stored in RAM cannot be fully removed.
        /// WARNING: Will trigger an immediate GC collection, causing a short freeze!
        /// </summary>
        public static GadgetMod ReloadMod(ref GadgetMod mod)
        {
            return mod = ReloadMod(mod);
        }

        /// <summary>
        /// Hard-reloads the given GadgetMod. (Loads it freshly from the disk).
        /// Also reloads all mods that are dependent on this mod.
        /// Causes a permanent increase in the game's RAM usage, as the old instance of the mod assembly that is stored in RAM cannot be fully removed.
        /// WARNING: Will trigger an immediate GC collection, causing a short freeze!
        /// </summary>
        public static GadgetMod ReloadMod(GadgetMod mod)
        {
            try
            {
                if (mod.Name == "GadgetCore") throw new InvalidOperationException(GadgetMods.GetModByAssembly(Assembly.GetCallingAssembly()).Name + " seriously just tried to reload GadgetCore... Really?");
                string modPath = mod.ModPath;
                List<GadgetMod> modsToReload = mod.LoadedGadgets.SelectMany(x => Gadgets.LoadOrderTree.Find(x).FlattenUniqueByBreadth()).Where(x => x != null).Select(x => x.Mod).Distinct().ToList();
                foreach (GadgetMod modToUnload in modsToReload)
                {
                    if (modToUnload.Name != "GadgetCore")
                    {
                        UnloadModInternal(modToUnload);
                    }
                }
                GC.Collect();
                BatchLoading = true;
                Logger.Log("Loading mod files...");
                for (int i = 0;i < modsToReload.Count;i++)
                {
                    if (modsToReload[i].Name != "GadgetCore")
                    {
                        if (modsToReload[i].IsArchive)
                        {
                            if (modsToReload[i].ModPath == modPath)
                            {
                                modsToReload[i] = mod = LoadModFile(modsToReload[i].ModPath);
                            }
                            else
                            {
                                modsToReload[i] = LoadModFile(modsToReload[i].ModPath);
                            }
                        }
                        else
                        {
                            if (modsToReload[i].ModPath == modPath)
                            {
                                modsToReload[i] = mod = LoadModDir(modsToReload[i].ModPath);
                            }
                            else
                            {
                                modsToReload[i] = LoadModDir(modsToReload[i].ModPath);
                            }
                        }
                    }
                    else
                    {
                        RefreshMod(modsToReload[i]);
                    }
                }
                Logger.Log("Done loading mod files.");
                Logger.Log("Loading mods...");
                foreach (GadgetMod modToReload in modsToReload)
                {
                    LoadGadgetMod(modToReload);
                }
                Logger.Log("Done loading mods.");
                Logger.Log("Sorting Gadgets...");
                Gadgets.SortGadgets();
                Logger.Log("Done sorting Gadgets.");
                Logger.Log("Queueing Gadgets for initialization...");
                foreach (GadgetInfo gadget in modsToReload.SelectMany(x => x.LoadedGadgets))
                {
                    QueuedGadgets.Add(gadget);
                }
                Logger.Log("Done queueing Gadgets.");
                EnableQueuedGadgets();
                BatchLoading = false;
            }
            catch (Exception e)
            {
                Logger.LogError("Error reloading " + mod.Name + ": " + e);
            }
            return mod;
        }

        /// <summary>
        /// Compares two arrays representing sets of version numbers.
        /// Returns a positive value if <paramref name="firstNums"/> is greater/newer,
        /// Returns a negative value if <paramref name="secondNums"/> is greater/newer,
        /// and 0 if the two sets of version numbers are equivalent.
        /// </summary>
        public static int CompareVersionNumbers(int[] firstNums, int[] secondNums, int maxDepth = int.MaxValue)
        {
            int numCount = Math.Max(firstNums.Length, secondNums.Length);
            for (int i = 0; i < numCount; i++)
            {
                int dif;

                if (i >= secondNums.Length)
                    dif = firstNums[i];
                else if (i >= firstNums.Length)
                    dif = -secondNums[i];
                else
                    dif = firstNums[i] - secondNums[i];

                if (dif != 0) return dif;
            }
            return 0;
        }

        /// <summary>
        /// Loads the symbols for the given assembly file from the given PDB file. Once the symbols are loaded, exceptions thrown from the given assembly
        /// will have correct line numbers in their stack trace. Note that it is not necessary to call this for a mod's primary assembly -
        /// the PDB file for that assembly will be loaded automatically if it exists in the mod.
        /// </summary>
        /// <param name="name">The name of the assembly to load symbols for. Must match the AssemblyName.Name value of the assembly.</param>
        /// <param name="dllFile">The assembly file.</param>
        /// <param name="pdbFile">The PDB file.</param>
        /// <returns>An asynchronous Task, once completed, will indicate whether the symbols were successfully loaded with a boolean value.</returns>
        public static Task<bool> LoadSymbols(string name, GadgetModFile dllFile, GadgetModFile pdbFile)
        {
            return LoadSymbolsInternal(name, dllFile.ReadAllBytes(), pdbFile.ReadAllBytes()).ContinueWith(symbolFile => symbolFile != null);
        }

        /// <summary>
        /// Loads the symbols for the given assembly file from the given PDB file. Once the symbols are loaded, exceptions thrown from the given assembly
        /// will have correct line numbers in their stack trace. Note that it is not necessary to call this for a mod's primary assembly -
        /// the PDB file for that assembly will be loaded automatically if it exists in the mod.
        /// </summary>
        /// <param name="name">The name of the assembly to load symbols for. Must match the AssemblyName.Name value of the assembly.</param>
        /// <param name="dllStream">The assembly file, in the form of a Stream.</param>
        /// <param name="pdbStream">The PDB file, in the form of a Stream.</param>
        /// <returns>An asynchronous Task, once completed, will indicate whether the symbols were successfully loaded with a boolean value.</returns>
        public static Task<bool> LoadSymbols(string name, Stream dllStream, Stream pdbStream)
        {
            using MemoryStream memoryStream = new MemoryStream();
            byte[] b = new byte[32768];
            int r;
            while ((r = dllStream.Read(b, 0, b.Length)) > 0)
                memoryStream.Write(b, 0, r);
            byte[] dllBytes = memoryStream.ToArray();
            memoryStream.SetLength(0);
            while ((r = pdbStream.Read(b, 0, b.Length)) > 0)
                memoryStream.Write(b, 0, r);
            byte[] pdbBytes = memoryStream.ToArray();
            return LoadSymbolsInternal(name, dllBytes, pdbBytes).ContinueWith(symbolFile => symbolFile != null);
        }

        /// <summary>
        /// Loads the symbols for the given assembly file from the given PDB file. Once the symbols are loaded, exceptions thrown from the given assembly
        /// will have correct line numbers in their stack trace. Note that it is not necessary to call this for a mod's primary assembly -
        /// the PDB file for that assembly will be loaded automatically if it exists in the mod.
        /// </summary>
        /// <param name="name">The name of the assembly to load symbols for. Must match the AssemblyName.Name value of the assembly.</param>
        /// <param name="dllBytes">The assembly file, in binary form stored in a byte array.</param>
        /// <param name="pdbBytes">The PDB file, in binary form stored in a byte array.</param>
        /// <returns>An asynchronous Task, once completed, will indicate whether the symbols were successfully loaded with a boolean value.</returns>
        public static Task<bool> LoadSymbols(string name, byte[] dllBytes, byte[] pdbBytes)
        {
            return LoadSymbolsInternal(name, dllBytes, pdbBytes).ContinueWith(symbolFile => symbolFile != null);
        }

        internal static Task<MonoSymbolFile> LoadSymbolsInternal(string name, byte[] dllBytes, byte[] pdbBytes)
        {
            Task<MonoSymbolFile> task = new Task<MonoSymbolFile>(() =>
            {
                MonoSymbolFile symbolFile;
                lock (LoadedSymbolFiles) if (LoadedSymbolFiles.TryGetValue(name, out symbolFile)) return symbolFile;
                using SHA1CryptoServiceProvider sha1Provider = new SHA1CryptoServiceProvider();
                string dllSha1 = sha1Provider.ComputeHash(dllBytes).Select(x => x.ToString("X2")).Concat("");
                string nameWithHash = name + "-" + dllSha1;
                string symbolPath = Path.Combine(GadgetPaths.SymbolsPath, Path.ChangeExtension(nameWithHash, ".dll.mdb"));
                if (File.Exists(symbolPath))
                {
                    symbolFile = MonoSymbolFile.ReadSymbolFile(symbolPath);
                    lock (LoadedSymbolFiles) LoadedSymbolFiles[name] = symbolFile;
                    return symbolFile;
                }
                foreach (string oldSymbolFile in Directory.GetFiles(GadgetPaths.SymbolsPath, name + "-*.dll.mdb")) File.Delete(oldSymbolFile);
                string dllPath = Path.Combine(GadgetPaths.TempPath, Path.ChangeExtension(name, ".dll"));
                string pdbPath = Path.ChangeExtension(dllPath, ".pdb");
                string mdbPath = Path.ChangeExtension(dllPath, ".dll.mdb");
                File.WriteAllBytes(dllPath, dllBytes);
                File.WriteAllBytes(pdbPath, pdbBytes);
                ProcessStartInfo psi = new ProcessStartInfo()
                {
                    FileName = Path.Combine(GadgetPaths.ToolsPath, "pdb2mdb.exe"),
                    Arguments = "\"" + dllPath + "\"",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    ErrorDialog = true,
                    UseShellExecute = false
                };
                Process process = Process.Start(psi);
                if (process == null) return null;
                process.WaitForExit();
                File.Delete(dllPath);
                File.Delete(pdbPath);
                if (!File.Exists(mdbPath)) return null;
                File.Move(mdbPath, symbolPath);
                symbolFile = MonoSymbolFile.ReadSymbolFile(symbolPath);
                lock (LoadedSymbolFiles) LoadedSymbolFiles[name] = symbolFile;
                return symbolFile;
            });
            // This is bad. However, doing better would take too much work in this tragic environment where System.Threading is broken
            // and fixing it is impossible without breaking most currently-existing mods.
            new Thread(() => task.RunSynchronously()).Start();
            return task;
        }

        internal static MonoSymbolFile GetSymbolFile(Assembly assembly)
        {
            return LoadedSymbolFiles[assembly.GetName().Name];
        }

        internal static void InjectSymbolsIntoStackFrame(StackFrame frame)
        {
            if (frame == null) return;
            MethodBase method = frame.GetMethod();
            if (method == null) return;
            if (!LoadedSymbolFiles.TryGetValue(method.Module.Assembly.GetName().Name, out MonoSymbolFile symbolFile)) return;
            MethodEntry methodEntry = symbolFile.GetMethodByToken(method.MetadataToken);
            if (methodEntry == null) return;
            int offset = frame.GetILOffset();
            foreach (LineNumberEntry lineNumberEntry in methodEntry.GetLineNumberTable().LineNumbers)
            {
                if (lineNumberEntry.Offset != offset) continue;
                LineNumberInfo.SetValue(frame, lineNumberEntry.Row);
                FileNameInfo.SetValue(frame, methodEntry.CompileUnit.SourceFile.FileName);
                break;
            }
        }
    }
}
