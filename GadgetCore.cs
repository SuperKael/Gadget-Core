using UnityEngine;
using UModFramework.API;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using GadgetCore.API;
using System;
using System.Linq;
using System.IO;

namespace GadgetCore
{
    class GadgetCore
    {
        public static List<string> nonGadgetMods;
        public static List<string> disabledMods;

        internal static void Log(string text, bool clean = false)
        {
            using (UMFLog log = new UMFLog()) log.Log(text, clean);
        }

        [UMFConfig]
        public static void LoadConfig()
        {
            GadgetCoreConfig.Load();
            LoadModAssemblies();
            InitializeRegistries();
            LoadMainMenu();
            SceneInjector.InjectMainMenu();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

		[UMFHarmony(7)] //Set this to the number of harmony patches in your mod.
        public static void Start()
		{
			Log("GadgetCore v" + GadgetCoreAPI.VERSION, true);
        }

        internal static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.buildIndex == 0)
            {
                SceneInjector.InjectMainMenu();
                LoadMainMenu();
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

        private static void LoadModAssemblies()
        {
            Log("Identifying Gadget Mods");
            nonGadgetMods = new List<string>();
            disabledMods = new List<string>();
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
            GadgetCoreConfig.Update();
            Log("Finished Identifying Gadget Mods, to load: " + GadgetMods.CountMods());
        }

        private static void InitializeRegistries()
        {
            Log("Initializing Registries");
            GameRegistry.RegisterRegistry(ItemRegistry.GetSingleton());
            GameRegistry.RegisterRegistry(ChipRegistry.GetSingleton());
            GameRegistry.RegisterRegistry(TileRegistry.GetSingleton());
            GameRegistry.RegisterRegistry(CreatureRegistry.GetSingleton());
            foreach (GadgetMod mod in GadgetMods.ListAllMods())
            {
                foreach (Registry registry in mod.CreateRegistries())
                {
                    GameRegistry.RegisterRegistry(registry);
                }
            }
            Log("Finished Initializing Registries");
        }
	}
}