using UnityEngine;
using UnityEngine.SceneManagement;
using System.Reflection;
using System.Collections.Generic;
using System;
using System.Linq;
using GadgetCore.API;
using System.IO;
using JetBrains.Annotations;
using GadgetCore.Loader;
using System.Threading;
using HarmonyLib;
using IniParser.Model;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GadgetCore
{
    internal class GadgetCore : MonoBehaviour
    {
        private static bool Initialized;
        internal static volatile bool Quitting = false;
        public static IGadgetCoreLib CoreLib { get; private set; }
        public static IUMFAPI UMFAPI { get; private set; }

        internal static Dictionary<string, Assembly> LoadedAssemblies = new Dictionary<string, Assembly>();
        internal static GadgetLogger CoreLogger;
        private static Harmony HarmonyInstance;

        static GadgetCore()
        {
            Initialize();
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
        }

        internal static void Initialize()
        {
            if (Initialized) return;
            Initialized = true;
            try
            {
                CoreLogger = new GadgetLogger("GadgetCore", "Core");
                CoreLogger.Log("GadgetCore v" + GadgetCoreAPI.FULL_VERSION + " Initializing!");
            }
            catch (Exception e)
            {
                Debug.Log(e);
                GadgetCoreAPI.Quit();
                return;
            }
            try
            {
                if (VerifySaveFile())
                {
                    if (GadgetCoreConfig.MaxBackups > 0)
                    {
                        File.Copy(Application.persistentDataPath + "/PlayerPrefs.txt", Path.Combine(GadgetPaths.SaveBackupsPath, "Save Backup - " + DateTime.Now.ToString("yyyy-dd-M_HH-mm-ss") + ".txt"));
                        FileInfo[] backups = new DirectoryInfo(GadgetPaths.SaveBackupsPath).GetFiles().OrderByDescending(x => x.LastWriteTime.Year <= 1601 ? x.CreationTime : x.LastWriteTime).ToArray();
                        if (backups.Length > GadgetCoreConfig.MaxBackups)
                        {
                            for (int i = GadgetCoreConfig.MaxBackups;i < backups.Length;i++)
                            {
                                backups[i].Delete();
                            }
                        }
                    }
                }
                else
                {
                    GadgetCoreAPI.Quit();
                    return;
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
                        HarmonyInstance.CreateClassProcessor(type).Patch();
                    }
                });
                new Thread(new ThreadStart(() => {
                    Thread.CurrentThread.Name = "GadgetCore Unity Engine Log Cloner";
                    Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Lowest;
                    Thread.CurrentThread.IsBackground = true;
                    string logPath = Application.dataPath + "\\output_log.txt";
                    if (!File.Exists(logPath)) logPath = Application.persistentDataPath + "\\output_log.txt";
                    if (!File.Exists(logPath)) logPath = "~/Library/Logs/Unity/Player.log";
                    if (!File.Exists(logPath)) logPath = "~/.config/unity3d/DefaultCompany/Roguelands/Player.log";
                    if (!File.Exists(logPath))
                    {
                        CoreLogger.LogWarning("Unable to find Unity log file!");
                        return;
                    }
                    string targetPath = Path.Combine(GadgetPaths.LogsPath, "Unity Output.log");
                    DateTime t = default;
                    while (!Quitting)
                    {
                        if (File.Exists(logPath) && File.GetLastWriteTime(logPath) > t)
                        {
                            File.Copy(logPath, targetPath, true);
                            t = DateTime.Now;
                        }
                        Thread.Sleep(100);
                    }
                })).Start();
                AppDomain.CurrentDomain.AssemblyResolve += (object sender, ResolveEventArgs args) =>
                {
                    string name = new AssemblyName(args.Name).Name;
                    if (LoadedAssemblies.ContainsKey(name)) return LoadedAssemblies[args.Name];
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
                    if (LoadedAssemblies.ContainsKey(args.Name)) return LoadedAssemblies[args.Name];
                    string name = new AssemblyName(args.Name).Name;
                    foreach (string file in Directory.GetFiles(GadgetPaths.LibsPath))
                    {
                        if (AssemblyName.GetAssemblyName(file).Name == name)
                        {
                            Assembly assembly = Assembly.ReflectionOnlyLoadFrom(file);
                            LoadedAssemblies.Add(args.Name, assembly);
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
                GadgetCoreConfig.Load();
                CoreLogger.Log("Finished loading config.");
                RegisterKeys();
                IniData coreManifest = new IniData();
                coreManifest["Metadata"]["Name"] = "GadgetCore";
                coreManifest["Metadata"]["Assembly"] = Path.Combine(GadgetPaths.ManagedPath, "GadgetCore.dll");
                GadgetMod coreMod = new GadgetMod(GadgetPaths.GadgetCorePath, coreManifest, Assembly.GetExecutingAssembly());
                GadgetMods.RegisterMod(coreMod);
                VanillaRegistration();
                GadgetLoader.LoadAllMods();
                SceneInjector.InjectMainMenu();
                SceneManager.sceneLoaded += OnSceneLoaded;
                DontDestroyOnLoad(new GameObject("GadgetCore", typeof(GadgetCore)));
                CoreLogger.LogConsole("GadgetCore v" + GadgetCoreAPI.FULL_VERSION + " Initialized!");
                if (GadgetCoreAPI.IS_BETA) CoreLogger.LogWarning("You are currently running a beta version of GadgetCore! Be prepared for bugs!");
            }
            catch (Exception e)
            {
                CoreLogger.LogError("There was a fatal error loading GadgetCore: " + e);
            }
        }

        internal static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
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
                        CoreLogger.LogWarning("There was an error loading the save file, but a save backup was succesfully loaded!\n" +
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
                    string[] array = content.Split(new string[]
                    {
                        " ; "
                    }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string text in array)
                    {
                        string[] array3 = text.Split(new string[]
                        {
                            " : "
                        }, StringSplitOptions.None);
                        if (array3.Length < 3) throw new Exception();
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
            Registry.modRegistering = -1;
            Registry.registeringVanilla = true;
            GameRegistry.RegisterRegistry(ItemRegistry.GetSingleton());
            GameRegistry.RegisterRegistry(ChipRegistry.GetSingleton());
            GameRegistry.RegisterRegistry(TileRegistry.GetSingleton());
            GameRegistry.RegisterRegistry(EntityRegistry.GetSingleton());
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