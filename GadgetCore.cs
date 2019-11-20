using UnityEngine;
using UModFramework.API;
using UnityEngine.SceneManagement;
using System.Reflection;
using System.Collections.Generic;
using System;
using System.Linq;
using GadgetCore.API;
using System.IO;

namespace GadgetCore
{
    class GadgetCore
    {
        public static bool IsUnpacked { get; private set; }
        public static IGadgetCoreLib CoreLib { get; private set; }

        public static List<string> nonGadgetMods;
        public static List<string> disabledMods;
        public static List<string> incompatibleMods;
        public static List<string> packedMods;
        public static Dictionary<string, List<string>> dependencies;

        private static bool firstLoad = true;
        private static UMFLog Logger = new UMFLog();

        internal static void Log(string text, bool clean = false)
        {
            Logger.Log(text, clean);
        }

        [UMFConfig]
        public static void LoadConfig()
        {
            if (firstLoad)
            {
                firstLoad = false;
                IsUnpacked = File.Exists(UMFData.ModsPath + "\\GadgetCore.dll") && File.Exists(UMFData.LibrariesPath + "\\GadgetCoreLib.dll") && !(Directory.GetFiles(UMFData.ModsPath, "GadgetCore*.zip").Length > 0);
                LoadMainMenu();
                if (IsUnpacked)
                {
                    CoreLib = Activator.CreateInstance(Assembly.LoadFile(UMFData.LibrariesPath + "\\GadgetCoreLib.dll").GetTypes().First(x => typeof(IGadgetCoreLib).IsAssignableFrom(x))) as IGadgetCoreLib;
                    CoreLib.ProvideLogger(Logger);
                    LoadModAssemblies();
                    GadgetCoreConfig.Load();
                    InitializeRegistries();
                    GadgetCoreConfig.LoadRegistries();
                    VanillaRegistration();
                    InitializeMods();
                    GenerateSpriteSheet();
                }
                SceneInjector.InjectMainMenu();
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            else if (IsUnpacked)
            {
                GadgetCoreConfig.Load();
            }
        }

		[UMFHarmony(118)]
        public static void Start()
		{
			Log("GadgetCore v" + GadgetCoreAPI.VERSION, true);
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

        private static void VanillaRegistration()
        {
            Registry.registeringVanilla = true;
            
            Registry.registeringVanilla = false;
        }

        private static void InitializeMods()
        {
            Log("Beginning Gadget Mod Initialization");
            foreach (GadgetModInfo mod in GadgetMods.ListAllModInfos())
            {
                if (!mod.Mod.Enabled) continue;
                if (mod.Attribute.Dependencies.All(x => GadgetMods.ListAllModInfos().Where(y => y.Mod.Enabled).Select(y => y.Attribute.Name).Contains(x) || GadgetMods.ListAllModInfos().Where(y => y.Mod.Enabled).Select(y => y.Mod.GetPreviousModNames()).Any(y => y.Contains(x))))
                {
                    bool compatible = true;
                    string[][] splitDependencies = mod.Attribute.Dependencies.Select(x => x.Split(' ')).Where(x => x.Length == 2).ToArray();
                    GadgetModInfo[] dependencies = splitDependencies.Select(x => GadgetMods.ListAllModInfos().Where(y => y.Mod.Enabled).FirstOrDefault(y => y.Attribute.Name.Equals(x[0]  )) ?? GadgetMods.ListAllModInfos().Where(y => y.Mod.Enabled).First(y => y.Mod.GetPreviousModNames().Contains(x[0]))).ToArray();
                    for (int i = 0; i < dependencies.Length; i++)
                    {
                        int[] currentVersionNums = dependencies[i].Mod.GetModVersionString().Split('.').Select(x => int.Parse(x)).ToArray();
                        int[] targetVersionNums = splitDependencies[i][1].TrimStart('v').Split('.').Select(x => int.Parse(x)).ToArray();
                        VersionSpecificity versionSpecificity = (VersionSpecificity)targetVersionNums.Length;
                        if (!((versionSpecificity == VersionSpecificity.MAJOR && currentVersionNums[0] == targetVersionNums[0] && (currentVersionNums[1] > targetVersionNums[1] || (currentVersionNums[1] == targetVersionNums[1] && (currentVersionNums[2] > targetVersionNums[2] || (currentVersionNums[2] == targetVersionNums[2] && currentVersionNums[3] >= targetVersionNums[3]))))) ||
                            (versionSpecificity == VersionSpecificity.MINOR && currentVersionNums[0] == targetVersionNums[0] && currentVersionNums[1] == targetVersionNums[1] && (currentVersionNums[2] > targetVersionNums[2] || (currentVersionNums[2] == targetVersionNums[2] && currentVersionNums[3] >= targetVersionNums[3]))) ||
                            (versionSpecificity == VersionSpecificity.NONBREAKING && currentVersionNums[0] == targetVersionNums[0] && currentVersionNums[1] == targetVersionNums[1] && currentVersionNums[2] == targetVersionNums[2] && currentVersionNums[3] >= targetVersionNums[3]) ||
                            (versionSpecificity == VersionSpecificity.BUGFIX && currentVersionNums[0] == targetVersionNums[0] && currentVersionNums[1] == targetVersionNums[1] && currentVersionNums[2] == targetVersionNums[2] && currentVersionNums[3] == targetVersionNums[3])))
                        {
                            compatible = false;
                            break;
                        }
                    }
                    if (compatible)
                    {
                        Log("Initializing Gadget Mod '" + mod.Attribute.Name + "'");
                        try
                        {
                            Registry.modRegistering = mod.Mod.ModID;
                            mod.Mod.Initialize();
                        }
                        catch (Exception e)
                        {
                            Log("Exception Loading Gadget Mod '" + mod.Attribute.Name + "':" + Environment.NewLine + e.ToString());
                        }
                        finally
                        {
                            Registry.modRegistering = -1;
                        }
                    }
                    else
                    {
                        GadgetMods.SetEnabled(mod, false);
                        Log("Aborted loading Gadget Mod '" + mod.Attribute.Name + "' because although all required dependencies are available, they are of incompatible versions. " + mod.Attribute.Name + "'s dependencies are as follows: {" + mod.Attribute.Dependencies.Aggregate(string.Empty, (x, y) => x + ", " + y) + "}");
                    }
                }
                else
                {
                    GadgetMods.SetEnabled(mod, false);
                    Log("Aborted loading Gadget Mod '" + mod.Attribute.Name + "' because it is missing required dependencies. " + mod.Attribute.Name + "'s dependencies are as follows: {" + mod.Attribute.Dependencies.Aggregate(string.Empty, (x, y) => x + ", " + y) + "}");
                }
            }
            GadgetCoreConfig.Update();
            Log("Gadget Mod Initialization Complete");
        }

        private static void LoadModAssemblies()
        {
            Log("Identifying Gadget Mods");
            nonGadgetMods = new List<string>();
            disabledMods = new List<string>();
            incompatibleMods = new List<string>();
            List<string> modNames = UMFData.ModNames;
            int[] currentVersionNums = GadgetCoreAPI.VERSION.Split('.').Select(x => int.Parse(x)).ToArray();
            for (int i = 0;i < modNames.Count;i++)
            {
                Assembly assembly = UMFMod.GetMod(modNames[i]);
                if (assembly != null)
                {
                    bool foundGadgetMod = false;
                    foreach (Type type in assembly.GetTypes())
                    {
                        GadgetModAttribute attribute = (GadgetModAttribute)type.GetCustomAttributes(typeof(GadgetModAttribute), true).FirstOrDefault();
                        if (attribute != null && type.IsSubclassOf(typeof(GadgetMod)))
                        {
                            foundGadgetMod = true;
                            int[] targetVersionNums = attribute.TargetGCVersion.Split('.').Select(x => int.Parse(x)).ToArray();
                            if ((attribute.GadgetVersionSpecificity == VersionSpecificity.MAJOR       && currentVersionNums[0] == targetVersionNums[0] && (currentVersionNums[1] > targetVersionNums[1] || (currentVersionNums[1] == targetVersionNums[1] && (currentVersionNums[2] > targetVersionNums[2] || (currentVersionNums[2] == targetVersionNums[2] && currentVersionNums[3] >= targetVersionNums[3]))))) ||
                                (attribute.GadgetVersionSpecificity == VersionSpecificity.MINOR       && currentVersionNums[0] == targetVersionNums[0] && currentVersionNums[1] == targetVersionNums[1] && (currentVersionNums[2] > targetVersionNums[2] || (currentVersionNums[2] == targetVersionNums[2] && currentVersionNums[3] >= targetVersionNums[3]))) ||
                                (attribute.GadgetVersionSpecificity == VersionSpecificity.NONBREAKING && currentVersionNums[0] == targetVersionNums[0] && currentVersionNums[1] == targetVersionNums[1] && currentVersionNums[2] == targetVersionNums[2] && currentVersionNums[3] >= targetVersionNums[3]) ||
                                (attribute.GadgetVersionSpecificity == VersionSpecificity.BUGFIX      && currentVersionNums[0] == targetVersionNums[0] && currentVersionNums[1] == targetVersionNums[1] && currentVersionNums[2] == targetVersionNums[2] && currentVersionNums[3] == targetVersionNums[3]))
                            {
                                GadgetMod mod = null;
                                try
                                {
                                    mod = Activator.CreateInstance(type) as GadgetMod;
                                    if (mod != null)
                                    {
                                        Log("Found Gadget Mod to load: " + attribute.Name + ", in assembly: {" + assembly.FullName + "}");
                                        GadgetMods.RegisterMod(new GadgetModInfo(mod, assembly, attribute, modNames[i]));
                                    }
                                }
                                catch (Exception) { }
                                finally
                                {
                                    if (mod == null)
                                    {
                                        Log("Found Gadget Mod without a parameterless constructor: " + attribute.Name + ", in assembly: {" + assembly.FullName + "}");
                                    }
                                }
                            }
                            else
                            {
                                incompatibleMods.Add(assembly.GetName().Name);
                                int rD = (int)attribute.GadgetVersionSpecificity;
                                Log("Found Gadget Mod with an incompatible version: " + attribute.Name + ", in assembly: {" + assembly.FullName + "}. Requires version: " + new string(attribute.TargetGCVersion.TakeWhile(x => (x == '.' ? --rD : rD) > 0).ToArray()));
                            }
                        }
                    }
                    if (!foundGadgetMod)
                    {
                        nonGadgetMods.Add(modNames[i]);
                    }
                }
                else
                {
                    disabledMods.Add(modNames[i]);
                }
            }
            GadgetMods.SortMods();
            if (!Directory.Exists(Path.Combine(UMFData.ModsPath, "PackedMods"))) Directory.CreateDirectory(Path.Combine(UMFData.ModsPath, "PackedMods"));
            packedMods = Directory.GetFiles(UMFData.ModsPath, "*.zip").Where(x => !UMFData.Mods.Select(y => y.GetName().Name).Any(y => y == Path.GetFileNameWithoutExtension(x).Split('_')[0])).Union(Directory.GetFiles(Path.Combine(UMFData.ModsPath, "PackedMods"), "*.zip").Where(x => !UMFData.Mods.Select(y => y.GetName().Name).Any(y => y == Path.GetFileNameWithoutExtension(x).Split('_')[0]))).ToList();

            dependencies = new Dictionary<string, List<string>>();
            Assembly[] allMods = GadgetMods.ListAllModInfos().Select(x => x.Assembly).Concat(nonGadgetMods.Select(x => UMFMod.GetMod(x))).Concat(incompatibleMods.Select(x => UMFMod.GetMod(x))).ToArray();
            for (int i = 0;i < allMods.Length;i++)
            {
                Assembly mod = allMods[i];
                List<string> dependencies = new List<string>();
                if (i < GadgetMods.CountMods())
                {
                    GadgetModInfo gadgetMod = GadgetMods.GetModInfo(i);
                    int rD = (int)gadgetMod.Attribute.GadgetVersionSpecificity;
                    dependencies.Add("GadgetCore v" + new string(gadgetMod.Attribute.TargetGCVersion.TakeWhile(x => (x == '.' ? --rD : rD) > 0).ToArray()));
                }
                dependencies.AddRange(mod.GetReferencedAssemblies().Where(x => !x.Name.Equals("GadgetCore") && allMods.Select(a => a.GetName().Name).Contains(x.Name)).Select(x => x.Name + " v" + x.Version));
                if (dependencies.Count > 0)
                {
                    GadgetCore.dependencies.Add(mod.GetName().Name, dependencies.ToList());
                }
            }
            foreach (GadgetModInfo mod in GadgetMods.ListAllModInfos())
            {
                foreach (string dependency in mod.Attribute.Dependencies)
                {
                    string[] splitDependency = dependency.Split(' ');
                    if (!dependencies[mod.UMFName].Select(x => { string[] split = x.Split(' '); return string.Join("", split.Take(split.Length - 1).ToArray()); }).Contains(string.Join("", splitDependency.Take(splitDependency.Length - 1).ToArray()))) dependencies[mod.UMFName].Add(dependency);
                }
            }

            GadgetCoreConfig.Update();
            Log("Finished Identifying Gadget Mods, to load: " + GadgetMods.CountMods());
        }

        private static void InitializeRegistries()
        {
            Log("Initializing Registries");
            GameRegistry.RegisterRegistry(ItemRegistry.GetSingleton());
            GameRegistry.RegisterRegistry(ChipRegistry.GetSingleton());
            GameRegistry.RegisterRegistry(TileRegistry.GetSingleton());
            GameRegistry.RegisterRegistry(EntityRegistry.GetSingleton());
            foreach (GadgetModInfo mod in GadgetMods.ListAllModInfos())
            {
                if (mod.Mod.Enabled)
                {
                    foreach (Registry registry in mod.Mod.CreateRegistries())
                    {
                        GameRegistry.RegisterRegistry(registry);
                    }
                }
            }
            Log("Finished Initializing Registries");
        }

        private static void GenerateSpriteSheet()
        {
            GadgetCoreAPI.spriteSheetSize = MathUtils.SmallestPerfectSquare(GadgetCoreAPI.spriteSheetSprites.Count + 16);
            int spritesOnAxis = (int)Mathf.Sqrt(GadgetCoreAPI.spriteSheetSize);
            int spritesOnFirstFourRows = spritesOnAxis - 4;
            int spriteSheetDimensions = spritesOnAxis * 32;
            GadgetCoreAPI.spriteSheet = new Texture2D(spriteSheetDimensions, spriteSheetDimensions, TextureFormat.ARGB32, false, false)
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
                Graphics.CopyTexture(GadgetCoreAPI.spriteSheetSprites[i].tex, 0, 0, 0, 0, 32, 32, GadgetCoreAPI.spriteSheet, 0, 0, (int)coords.x * 32, (int)coords.y * 32);
            }
        }
	}
}