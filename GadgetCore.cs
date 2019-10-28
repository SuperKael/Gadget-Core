using UnityEngine;
using UModFramework.API;
using UnityEngine.SceneManagement;
using System.Reflection;
using System.Collections.Generic;
using GadgetCore.API;
using System;
using System.Linq;

namespace GadgetCore
{
    class GadgetCore
    {
        public static List<string> nonGadgetMods;
        public static List<string> disabledMods;
        public static List<string> incompatibleMods;
        public static Dictionary<string, string[]> dependencies;

        private static bool firstLoad = true;

        internal static void Log(string text, bool clean = false)
        {
            using (UMFLog log = new UMFLog()) log.Log(text, clean);
        }

        [UMFConfig]
        public static void LoadConfig()
        {
            GadgetCoreConfig.Load();
            if (firstLoad)
            {
                firstLoad = false;
                LoadModAssemblies();
                InitializeRegistries();
                LoadMainMenu();
                SceneInjector.InjectMainMenu();
                SceneManager.sceneLoaded += OnSceneLoaded;
                VanillaRegistration();
                InitializeMods();
                GenerateSpriteSheet();
            }
        }

		[UMFHarmony(62)]
        public static void Start()
		{
			Log("GadgetCore v" + GadgetCoreAPI.VERSION, true);
        }

        internal static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.buildIndex == 0)
            {
                LoadMainMenu();
                SceneInjector.InjectMainMenu();
            }
            else
            {
                SceneInjector.InjectIngame();
            }
        }

        internal static void LoadMainMenu()
        {
            InstanceTracker.mainCamera = GameObject.Find("Main Camera");
            InstanceTracker.menuu = InstanceTracker.mainCamera.GetComponent<Menuu>();
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
            Log("Gadget Mod Initialization Complete");
        }

        private static void LoadModAssemblies()
        {
            Log("Identifying Gadget Mods");
            nonGadgetMods = new List<string>();
            disabledMods = new List<string>();
            incompatibleMods = new List<string>();
            List<string> modNames = UMFData.ModNames;
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
                            int rD = (int)attribute.VersionSpecificity;
                            string targetVersion = new string(attribute.TargetGCVersion.TakeWhile(x => (x == '.' ? --rD : rD) > 0).ToArray());
                            rD = (int)attribute.VersionSpecificity;
                            if (targetVersion.Equals(new string(GadgetCoreAPI.VERSION.TakeWhile(x => (x == '.' ? --rD : rD) > 0).ToArray())))
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
                                Log("Found Gadget Mod with an incompatible version: " + attribute.Name + ", in assembly: {" + assembly.FullName + "}. Requires version: " + targetVersion);
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

            dependencies = new Dictionary<string, string[]>();
            Assembly[] allMods = GadgetMods.ListAllModInfos().Select(x => x.Assembly).Concat(nonGadgetMods.Select(x => UMFMod.GetMod(x))).Concat(incompatibleMods.Select(x => UMFMod.GetMod(x))).ToArray();
            for (int i = 0;i < allMods.Length;i++)
            {
                Assembly mod = allMods[i];
                List<string> dependencies = new List<string>();
                if (i < GadgetMods.CountMods())
                {
                    GadgetModInfo gadgetMod = GadgetMods.GetModInfo(i);
                    int rD = (int)gadgetMod.Attribute.VersionSpecificity;
                    dependencies.Add("GadgetCore v" + new string(gadgetMod.Attribute.TargetGCVersion.TakeWhile(x => (x == '.' ? --rD : rD) > 0).ToArray()));
                }
                dependencies.AddRange(mod.GetReferencedAssemblies().Where(x => !x.Name.Equals("GadgetCore") && allMods.Select(a => a.GetName().Name).Contains(x.Name)).Select(x => x.Name + " v" + x.Version));
                if (dependencies.Count > 0)
                {
                    GadgetCore.dependencies.Add(mod.GetName().Name, dependencies.ToArray());
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
            GadgetCoreAPI.spriteSheet = new Texture2D(spriteSheetDimensions, spriteSheetDimensions, TextureFormat.ARGB32, false, false);
            GadgetCoreAPI.spriteSheet.filterMode = FilterMode.Point;
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