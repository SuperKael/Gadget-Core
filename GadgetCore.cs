﻿using UnityEngine;
using UnityEngine.SceneManagement;
using System.Reflection;
using System.Collections.Generic;
using System;
using System.Linq;
using GadgetCore.API;
using System.IO;
using GadgetCore.Loader;
using HarmonyLib;
using IniParser.Model;
using System.Diagnostics.CodeAnalysis;
using System.IO.IsolatedStorage;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Ionic.Zip;
using Debug = UnityEngine.Debug;

namespace GadgetCore
{
    /// <summary>
    /// The 'Core' of GadgetCore - where the magic happens, or more accurately where it begins. Among other things, contains the <see cref="Initialize"/> method,
    /// which is the entrypoint for all of GadgetCore's functionality. However, this class is not intended for mods to access at all, and is only public
    /// because it needs to be in order to make that entrypoint accessible.
    /// </summary>
    public class GadgetCore : MonoBehaviour
    {
        private static bool Initialized;
        internal static volatile bool Quitting = false;
        public static IGadgetCoreLib CoreLib { get; private set; }
        public static IUMFAPI UMFAPI { get; private set; }

        internal static Dictionary<string, Assembly> LoadedAssemblies = new Dictionary<string, Assembly>();
        internal static GadgetLogger CoreLogger;
        internal static GadgetLogger UnityLogger;
        internal static Harmony HarmonyInstance;

        static GadgetCore()
        {
            Initialize();
        }

        [SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Unity Event")]
        private void Awake()
        {
            string updateTempFilePath = Path.Combine(GadgetPaths.GadgetCorePath, "Update.tmp");
            if (File.Exists(updateTempFilePath))
            {
                try
                {
                    string oldVersion = File.ReadAllText(updateTempFilePath);
                    GadgetCoreAPI.DisplayInfoDialog($"GadgetCore has been updated!\n{oldVersion} -> {GadgetCoreAPI.GetFullVersion()}");
                }
                catch (Exception) { }
                try
                {
                    File.Delete(updateTempFilePath);
                }
                catch (Exception) { }
            }
        }

        [SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Unity Event")]
        private void Update()
        {
            if (EventSystem.current?.currentSelectedGameObject == null || EventSystem.current.currentSelectedGameObject.GetComponent<InputField>()?.IsActive() != true)
            {
                foreach (KeyCode key in GadgetCoreAPI.keyDownListeners.Keys)
                {
                    if (Input.GetKeyDown(key))
                    {
                        foreach (Action action in GadgetCoreAPI.keyDownListeners[key])
                        {
                            action();
                        }
                    }
                }
                foreach (KeyCode key in GadgetCoreAPI.keyUpListeners.Keys)
                {
                    if (Input.GetKeyUp(key))
                    {
                        foreach (Action action in GadgetCoreAPI.keyUpListeners[key])
                        {
                            action();
                        }
                    }
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Return) && GadgetCoreAPI.keyDownListeners.ContainsKey(KeyCode.Return))
                {
                    foreach (Action action in GadgetCoreAPI.keyDownListeners[KeyCode.Return])
                    {
                        action();
                    }
                }
                if (Input.GetKeyDown(KeyCode.UpArrow) && GadgetCoreAPI.keyDownListeners.ContainsKey(KeyCode.UpArrow))
                {
                    foreach (Action action in GadgetCoreAPI.keyDownListeners[KeyCode.UpArrow])
                    {
                        action();
                    }
                }
                if (Input.GetKeyDown(KeyCode.DownArrow) && GadgetCoreAPI.keyDownListeners.ContainsKey(KeyCode.DownArrow))
                {
                    foreach (Action action in GadgetCoreAPI.keyDownListeners[KeyCode.DownArrow])
                    {
                        action();
                    }
                }
                if (Input.GetKeyDown(KeyCode.Escape) && GadgetCoreAPI.keyDownListeners.ContainsKey(KeyCode.Escape))
                {
                    foreach (Action action in GadgetCoreAPI.keyDownListeners[KeyCode.Escape])
                    {
                        action();
                    }
                }
            }
            GadgetConsole.hidThisFrame = false;
        }

        /// <summary>
        /// The entrypoint for all of GadgetCore. Will do nothing if called again by a mod.
        /// </summary>
        public static void Initialize()
        {
            if (Initialized) return;
            Initialized = true;
            Debug.Log("GadgetCore v" + GadgetCoreAPI.FULL_VERSION);
            bool earlyConfigLoaded = false;
            try
            {
                GadgetCoreConfig.EarlyLoad();
                earlyConfigLoaded = true;
            }
            catch (Exception e)
            {
                Debug.Log("Failed to load GadgetCore config file early: " + e);
            }
            try
            {
                if (GadgetCoreConfig.MaxLogArchives > 0) BackupLogFiles();
            }
            catch (Exception) { }
            try
            {
                CoreLogger = new GadgetLogger("GadgetCore", "Core");
                CoreLogger.Log("GadgetCore v" + GadgetCoreAPI.FULL_VERSION + " Initializing!");
                
                UnityLogger = new GadgetLogger("Unity Output", "Unity");
                Application.SetStackTraceLogType(LogType.Exception, StackTraceLogType.Full);
                Application.logMessageReceivedThreaded += (text, stackTrace, type) =>
                {
                    switch (type)
                    {
                        case LogType.Log:
                            UnityLogger.Log(text);
                            break;
                        case LogType.Warning:
                            UnityLogger.LogWarning(text, false);
                            break;
                        case LogType.Error:
                        case LogType.Assert:
                            UnityLogger.LogError(text, false);
                            break;
                        case LogType.Exception:
                            if (!string.IsNullOrEmpty(stackTrace))
                            {
                                GadgetMod blameMod = null;
                                foreach (string frame in stackTrace.Split('\n'))
                                {
                                    try
                                    {
                                        if (GadgetLoader.BlameMap.TryGetValue(
                                                frame.Substring(0, frame.LastIndexOf('.', stackTrace.IndexOf(' '))),
                                                out blameMod)) break;
                                    } catch (Exception) {}
                                }
                                if (blameMod != null)
                                {
                                    UnityLogger.LogError($"<Exception from {blameMod.Name}> " + text);
                                }
                                else
                                {
                                    UnityLogger.LogError("<Exception> " + text);
                                }
                                UnityLogger.LogRaw(string.Join("\n", stackTrace.Split('\n').Where(frame => !string.IsNullOrEmpty(frame)).Select(frame => "[StackTrace]  " + frame).ToArray()));
                            }
                            else
                            {
                                UnityLogger.LogError("<Exception> " + text);
                            }
                            break;
                    }
                };

                string logPath = Application.dataPath + "\\output_log.txt";
                if (!File.Exists(logPath)) logPath = Application.persistentDataPath + "\\output_log.txt";
                if (!File.Exists(logPath))
                {
                    string homeVar = Environment.GetEnvironmentVariable("HOME");
                    if (homeVar != null)
                    {
                        logPath = Path.Combine(homeVar, "Library/Logs/Unity/Player.log");
                        if (!File.Exists(logPath)) logPath = Path.Combine(homeVar, ".config/unity3d/DefaultCompany/Roguelands/Player.log");
                    }
                }
                if (File.Exists(logPath))
                {
                    try
                    {
                        using FileStream fileStream = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        using StreamReader streamReader = new StreamReader(fileStream);
                        string logData = string.Join("\n",
                            streamReader.ReadToEnd().Replace('\r', '\n')
                                .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(frame => string.IsNullOrEmpty(frame) ? frame : "  " + frame).ToArray());
                        UnityLogger.LogRaw("=====Begin Unity log prior to GadgetCore initialization=====");
                        UnityLogger.LogRaw(logData);
                        UnityLogger.LogRaw("=====End Unity log prior to GadgetCore initialization=====");
                    }
                    catch (Exception)
                    {
                        UnityLogger.LogWarning("Error reading Unity output log file!");
                    }
                }
                else
                {
                    UnityLogger.LogWarning("Unable to find Unity output log file!");
                }
            }
            catch (Exception e)
            {
                Debug.Log("GadgetCore Logger Initialization Failed: " + e);
            }
            try
            {
                try
                {
                    GadgetLoader.LoadSymbolsInternal("GadgetCore",
                            File.ReadAllBytes(Path.Combine(GadgetPaths.ManagedPath, "GadgetCore.dll")),
                            File.ReadAllBytes(Path.Combine(GadgetPaths.ManagedPath, "GadgetCore.pdb")))
                        .Wait(10000);
                }
                catch (Exception e)
                {
                    CoreLogger.LogWarning("Failed to log GadgetCore symbols due to an exception: " + e.Message, false);
                }

                if (File.Exists(Application.persistentDataPath + "/PlayerPrefs.txt"))
                {
                    if (VerifySaveFile())
                    {
                        if (GadgetCoreConfig.MaxBackups > 0)
                        {
                            File.Copy(Application.persistentDataPath + "/PlayerPrefs.txt", Path.Combine(GadgetPaths.SaveBackupsPath, "Save Backup - " + DateTime.Now.ToString("yyyy-dd-M_HH-mm-ss") + ".txt"));
                            FileInfo[] backups = new DirectoryInfo(GadgetPaths.SaveBackupsPath).GetFiles().OrderByDescending(x => x.LastWriteTime.Year <= 1601 ? x.CreationTime : x.LastWriteTime).ToArray();
                            if (backups.Length > GadgetCoreConfig.MaxBackups)
                            {
                                for (int i = GadgetCoreConfig.MaxBackups; i < backups.Length; i++)
                                {
                                    backups[i].Delete();
                                }
                            }
                        }
                    }
                    else
                    {
                        CoreLogger.LogError("Quitting game due to corrupt save file!");
                        GadgetCoreAPI.Quit();
                        return;
                    }
                }
                HarmonyInstance = new Harmony("GadgetCore.core");
                Type[] types;
                try
                {
                    types = Assembly.GetExecutingAssembly().GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types.Where(t => t != null).ToArray();
                }
                types.Do(delegate (Type type)
                {
                    object[] attributes = type.GetCustomAttributes(true);
                    if (!attributes.Any(x => x.GetType() == typeof(HarmonyGadgetAttribute)))
                    {
                        try
                        {
                            HarmonyInstance.CreateClassProcessor(type).Patch();
                        }
                        catch (HarmonyException e)
                        {
                            if (e.InnerException == null || !e.InnerException.Message.EndsWith("returned an unexpected result: null")) throw e;
                        }
                    }
                });
                AppDomain.CurrentDomain.AssemblyResolve += (object sender, ResolveEventArgs args) =>
                {
                    string name = new AssemblyName(args.Name).Name;
                    if (LoadedAssemblies.ContainsKey(name)) return LoadedAssemblies[name];
                    foreach (string file in Directory.GetFiles(GadgetPaths.LibsPath))
                    {
                        if (AssemblyName.GetAssemblyName(file).Name == name)
                        {
                            Assembly assembly = Assembly.LoadFrom(file);
                            LoadedAssemblies[name] = assembly;
                            return assembly;
                        }
                    }
                    return null;
                };
                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += (object sender, ResolveEventArgs args) =>
                {
                    string name = new AssemblyName(args.Name).Name;
                    if (LoadedAssemblies.ContainsKey("ReflectionOnly: " + name)) return LoadedAssemblies["ReflectionOnly: " + name];
                    foreach (string file in Directory.GetFiles(GadgetPaths.LibsPath))
                    {
                        if (AssemblyName.GetAssemblyName(file).Name == name)
                        {
                            Assembly assembly = Assembly.ReflectionOnlyLoadFrom(file);
                            LoadedAssemblies["ReflectionOnly: " + name] = assembly;
                            return assembly;
                        }
                    }
                    return null;
                };
                LoadMainMenu();
                try
                {
                    UMFAPI = new UMFAPI();
                    UMFAPI.GetModNames();
                    CoreLogger.Log("Enabling UMF API as UMF is installed.");
                }
                catch (Exception)
                {
                    UMFAPI = null;
                    CoreLogger.Log("Disabling UMF API as UMF is not installed.");
                }
                CoreLib = Activator.CreateInstance(Assembly.LoadFile(Path.Combine(Path.Combine(GadgetPaths.GadgetCorePath, "DependentLibs"), "GadgetCoreLib.dll")).GetTypes().First(x => typeof(IGadgetCoreLib).IsAssignableFrom(x))) as IGadgetCoreLib;
                CoreLib.ProvideLogger(CoreLogger);
                if (!earlyConfigLoaded) GadgetCoreConfig.EarlyLoad();
                GadgetCoreConfig.Load();
                CoreLogger.Log("Finished loading config.");
                RegisterKeys();
                IniData coreManifest = new IniData();
                coreManifest["Metadata"]["Name"] = "GadgetCore";
                coreManifest["Metadata"]["Assembly"] = Path.Combine(GadgetPaths.ManagedPath, "GadgetCore.dll");
                GadgetMod coreMod = new GadgetMod(GadgetPaths.GadgetCorePath, coreManifest, Assembly.GetExecutingAssembly());
                GadgetMods.RegisterMod(coreMod);
                VanillaRegistration();
                SceneManager.sceneLoaded += OnSceneLoaded;
                SceneInjector.InjectMainMenu();
                GadgetLoader.LoadAllMods();
                DontDestroyOnLoad(new GameObject("GadgetCore", typeof(GadgetCore)));
                CoreLogger.LogConsole("GadgetCore v" + GadgetCoreAPI.FULL_VERSION + " Initialized!");
#if DEBUG
                CoreLogger.LogWarning("You are currently running a beta version of GadgetCore! Be prepared for bugs!");
#endif
            }
            catch (Exception e)
            {
                CoreLogger.LogError("There was a fatal error loading GadgetCore: " + e);
            }
        }

        private static void BackupLogFiles()
        {
            string[] logFiles = Directory.GetFiles(GadgetPaths.LogsPath, "*.log");
            if (logFiles == null || logFiles.Length < 1) return;
            DateTime logZipTime = File.GetCreationTime(File.Exists(Path.Combine(GadgetPaths.LogsPath, "GadgetCore.log")) ? Path.Combine(GadgetPaths.LogsPath, "GadgetCore.log") : logFiles[0]);
            int warningCount = 0, errorCount = 0, exceptionCount = 0;
            bool warningErr = false, errorErr = false, exceptionErr = false;
            foreach (string logFile in logFiles)
            {
                if (Path.GetFileName(logFile) == "Unity Output.log")
                {
                    try
                    {
                        exceptionCount += File.ReadAllLines(logFile).Count(x => x.Contains("Exception"));
                    }
                    catch (Exception)
                    {
                        exceptionErr = true;
                    }
                }
                else
                {
                    string[] lines;
                    try
                    {
                        lines = File.ReadAllLines(logFile);
                    }
                    catch (Exception)
                    {
                        warningErr = true;
                        errorErr = true;
                        continue;
                    }
                    try
                    {
                        warningCount += lines.Count(x => x.Contains("[Warning]"));
                    }
                    catch (Exception)
                    {
                        warningErr = true;
                    }
                    try
                    {
                        errorCount += lines.Count(x => x.Contains("[Error]"));
                    }
                    catch (Exception)
                    {
                        errorErr = true;
                    }
                }
            }
            string logZipName = $"Archived Logs ({logZipTime:yyyy-MM-dd hh.mm.ss tt}) - " +
                (warningCount < 0 || errorCount < 0 || exceptionCount < 0 ? "[Possibly Corrupt] " : "") + 
                $"[{(warningErr ? $"{warningCount}?" : warningCount.ToString())} Warnings] " +
                $"[{(errorErr ? $"{errorCount}?" : errorCount.ToString())} Errors] " +
                $"[{(exceptionErr ? $"{exceptionCount}?" : exceptionCount.ToString())} Exceptions]" +
                ".zip";
            using (ZipFile logZip = new ZipFile())
            {
                logZip.AddFiles(logFiles, string.Empty);
                logZip.Save(Path.Combine(GadgetPaths.LogArchivesPath, logZipName));
            }
            foreach (string file in logFiles)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception) { }
            }

            FileInfo[] archives = new DirectoryInfo(GadgetPaths.LogArchivesPath).GetFiles().OrderByDescending(x => x.LastWriteTime.Year <= 1601 ? x.CreationTime : x.LastWriteTime).ToArray();
            if (archives.Length > GadgetCoreConfig.MaxLogArchives)
            {
                for (int i = GadgetCoreConfig.MaxLogArchives; i < archives.Length; i++)
                {
                    archives[i].Delete();
                }
            }
        }

        internal static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            GadgetCoreAPI.SceneReset();
            if (scene.buildIndex == 0)
            {
                GadgetNetwork.ResetIDMatrix();
                LoadMainMenu();
                SceneInjector.InjectMainMenu();
            }
            else
            {
                LoadIngame();
                SceneInjector.InjectIngame();
            }
        }

        internal static void LoadMainMenu()
        {
            InstanceTracker.MainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
            InstanceTracker.Menuu = InstanceTracker.MainCamera.GetComponent<Menuu>();
        }

        internal static void LoadIngame()
        {
            InstanceTracker.MainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        }

        private static bool VerifySaveFile()
        {
            bool loadedBackup = false;
            DateTime time = DateTime.Now;
            while (true)
            {
                if (ValidateSaveFileContent(File.ReadAllText(Application.persistentDataPath + "/PlayerPrefs.txt")))
                {
                    if (loadedBackup)
                    {
                        CoreLogger.LogWarning("There was an error loading the save file, but a save backup was successfully loaded!\n" +
                                              "Be prepared for lost progress due to loading the backup. The backup that was loaded was created at: " +
                                              time);
                    }
                    return true;
                }
                else
                {
                    FileInfo[] backups = new DirectoryInfo(GadgetPaths.SaveBackupsPath).GetFiles().OrderByDescending(x => x.LastWriteTime.Year <= 1601 ? x.CreationTime : x.LastWriteTime).ToArray();
                    int fileIndex = 0;
                    string fileContent = null;
                    while (string.IsNullOrEmpty(fileContent) && fileIndex < backups.Length)
                    {
                        time = backups[fileIndex].LastWriteTime.Year <= 1601 ? backups[fileIndex].CreationTime : backups[fileIndex].LastWriteTime;
                        fileContent = File.ReadAllText(backups[fileIndex].FullName);
                        fileIndex++;
                    }
                    if (string.IsNullOrEmpty(fileContent))
                    {
                        CoreLogger.LogError("There was an error loading the save file, and no viable backups were found!");
                        return false;
                    }
                    File.Delete(Application.persistentDataPath + "/PlayerPrefs.txt");
                    File.Move(backups[fileIndex - 1].FullName, Application.persistentDataPath + "/PlayerPrefs.txt");
                    loadedBackup = true;
                }
            }
        }

        private static bool ValidateSaveFileContent(string content)
        {
            try
            {
                if (!string.IsNullOrEmpty(content))
                {
                    if (content.Length > 0 && content[content.Length - 1] == '\n')
                    {
                        content = content.Substring(0, content.Length - 1);
                        if (content.Length > 0 && content[content.Length - 1] == '\r')
                        {
                            content = content.Substring(0, content.Length - 1);
                        }
                    }
                    string[] array = content.Split(new[]
                    {
                        " ; "
                    }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string text in array)
                    {
                        string[] array3 = text.Split(new[]
                        {
                            " : "
                        }, StringSplitOptions.None);
                        if (array3.Length < 3) return false;
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static void RegisterKeys()
        {
            GadgetCoreAPI.RegisterKeyDownListener(KeyCode.Escape, GadgetConsole.HideConsole);
        }

        private static void VanillaRegistration()
        {
            Registry.gadgetRegistering = -1;
            Registry.registeringVanilla = true;

            GameRegistry.RegisterRegistry(ItemRegistry.Singleton);
            GameRegistry.RegisterRegistry(ChipRegistry.Singleton);
            GameRegistry.RegisterRegistry(TileRegistry.Singleton);
            GameRegistry.RegisterRegistry(EntityRegistry.Singleton);
            GameRegistry.RegisterRegistry(MenuRegistry.Singleton);
            GameRegistry.RegisterRegistry(ObjectRegistry.Singleton);
            GameRegistry.RegisterRegistry(PlanetRegistry.Singleton);
            GameRegistry.RegisterRegistry(AllegianceRegistry.Singleton);
            GameRegistry.RegisterRegistry(CharacterRaceRegistry.Singleton);
            GameRegistry.RegisterRegistry(CharacterAugmentRegistry.Singleton);
            GameRegistry.RegisterRegistry(CharacterUniformRegistry.Singleton);

            GadgetCoreAPI.MissingItemMaterial = new Material(Shader.Find("Unlit/Transparent"))
            {
                mainTexture = GadgetCoreAPI.LoadTexture2D("missing_item")
            };
            GadgetCoreAPI.MissingTileSprite = GadgetCoreAPI.AddTextureToSheet(GadgetCoreAPI.LoadTexture2D("missing_tile"));

            GameObject expCustom = Instantiate(Resources.Load<GameObject>("exp/exp7"));
            GadgetCoreAPI.AddCustomResource("exp/expCustom", expCustom);

            GadgetCoreAPI.AddCustomResource("mat/mRaceBack", new Material(Shader.Find("Unlit/Transparent"))
            {
                mainTexture = GadgetCoreAPI.LoadTexture2D("blank_race_select.png")
            });
            GadgetCoreAPI.AddCustomResource("mat/mRaceSlot", new Material(Shader.Find("Unlit/Transparent"))
            {
                mainTexture = GadgetCoreAPI.LoadTexture2D("blank_race_slot.png")
            });
            GadgetCoreAPI.AddCustomResource("mat/mUniformBack", new Material(Shader.Find("Unlit/Transparent"))
            {
                mainTexture = GadgetCoreAPI.LoadTexture2D("blank_uniform_select.png")
            });
            GadgetCoreAPI.AddCustomResource("mat/mUniformSlot", new Material(Shader.Find("Unlit/Transparent"))
            {
                mainTexture = GadgetCoreAPI.LoadTexture2D("blank_uniform_slot.png")
            });
            GadgetCoreAPI.AddCustomResource("mat/mAugmentBack", new Material(Shader.Find("Unlit/Transparent"))
            {
                mainTexture = GadgetCoreAPI.LoadTexture2D("blank_augment_select.png")
            });
            GadgetCoreAPI.AddCustomResource("mat/mAugmentSlot", new Material(Shader.Find("Unlit/Transparent"))
            {
                mainTexture = GadgetCoreAPI.LoadTexture2D("blank_augment_slot.png")
            });

            Registry.registeringVanilla = false;
        }

        internal static void GenerateSpriteSheet()
        {
            GadgetCoreAPI.spriteSheetSize = MathUtils.SmallestPerfectSquare(GadgetCoreAPI.spriteSheetSprites.Count + 16);
            int spritesOnAxis = (int)Mathf.Sqrt(GadgetCoreAPI.spriteSheetSize);
            int spritesOnFirstFourRows = spritesOnAxis - 4;
            int spriteSheetDimensions = spritesOnAxis * 32;
            GadgetCoreAPI.spriteSheet = new Texture2D(spriteSheetDimensions, spriteSheetDimensions, InstanceTracker.GameScript.TileManager.GetComponent<ChunkWorld>().Texture.format, false, false)
            {
                filterMode = FilterMode.Point
            };
            for (int i = 0;i < GadgetCoreAPI.spriteSheetSprites.Count;i++)
            {
                Vector2 coords;
                if (i < spritesOnFirstFourRows * 4)
                {
                    coords = new Vector2(4 + (i % spritesOnFirstFourRows), i / spritesOnFirstFourRows);
                }
                else
                {
                    coords = new Vector2((i - (spritesOnFirstFourRows * 4)) % spritesOnAxis, 4 + ((i - (spritesOnFirstFourRows * 4)) / spritesOnAxis));
                }
                GadgetCoreAPI.spriteSheetSprites[i].coords = coords;
                GadgetUtils.SafeCopyTexture(GadgetCoreAPI.spriteSheetSprites[i].tex, 0, 0, 0, 0, 32, 32, GadgetCoreAPI.spriteSheet, 0, 0, (int)coords.x * 32, (int)coords.y * 32);
            }
        }
	}
}