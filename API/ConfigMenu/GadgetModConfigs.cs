using GadgetCore.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UModFramework.API;
using UnityEngine;

namespace GadgetCore.API.ConfigMenu
{
    /// <summary>
    /// Tracker for <see cref="IGadgetConfigMenu"/>s.
    /// </summary>
    public static class GadgetModConfigs
    {
        private static List<Tuple<IGadgetConfigMenu, RectTransform>> ConfigMenus;
        private static Tuple<IGadgetConfigMenu, RectTransform> UMFConfigMenu;

        internal static void BuildConfigMenus(RectTransform parent)
        {
            UMFConfigMenu umfConfigMenu = new UMFConfigMenu();
            if (umfConfigMenu != null)
            {
                RectTransform menuParent = new GameObject("uModFramework", typeof(RectTransform)).GetComponent<RectTransform>();
                menuParent.gameObject.SetActive(false);
                menuParent.SetParent(parent);
                menuParent.anchorMin = new Vector2(0f, 0f);
                menuParent.anchorMax = new Vector2(1f, 1f);
                menuParent.offsetMin = Vector2.zero;
                menuParent.offsetMax = Vector2.zero;
                umfConfigMenu.Build(menuParent);
                UMFConfigMenu = Tuple.Create<IGadgetConfigMenu, RectTransform>(umfConfigMenu, menuParent);
            }

            ConfigMenus = new List<Tuple<IGadgetConfigMenu, RectTransform>>();
            GadgetModInfo[] gadgetMods = GadgetMods.ListAllModInfos();
            string[] allMods = gadgetMods.Select(x => x.Attribute.Name).Concat(GadgetCore.nonGadgetMods).Concat(GadgetCore.disabledMods).Concat(GadgetCore.incompatibleMods).Concat(GadgetCore.packedMods).ToArray();
            for (int i = 0;i < allMods.Length;i++)
            {
                try
                {
                    if (i < gadgetMods.Length)
                    {
                        IGadgetConfigMenu configMenu = gadgetMods[i].Mod.GetConfigMenu();
                        if (configMenu != null)
                        {
                            RectTransform menuParent = new GameObject(allMods[i], typeof(RectTransform)).GetComponent<RectTransform>();
                            menuParent.gameObject.SetActive(false);
                            menuParent.SetParent(parent);
                            menuParent.anchorMin = new Vector2(0f, 0f);
                            menuParent.anchorMax = new Vector2(1f, 1f);
                            menuParent.offsetMin = Vector2.zero;
                            menuParent.offsetMax = Vector2.zero;
                            configMenu.Build(menuParent);
                            ConfigMenus.Add(Tuple.Create(configMenu, menuParent));
                        }
                        else
                        {
                            ConfigMenus.Add(null);
                        }
                    }
                    else
                    {
                        try
                        {
                            IGadgetConfigMenu configMenu = new UMFGadgetConfigMenu(allMods[i], false, Path.Combine(UMFData.ConfigsPath, allMods[i]) + ".ini");
                            if (configMenu != null)
                            {
                                RectTransform menuParent = new GameObject(allMods[i], typeof(RectTransform)).GetComponent<RectTransform>();
                                menuParent.gameObject.SetActive(false);
                                menuParent.SetParent(parent);
                                menuParent.anchorMin = new Vector2(0f, 0f);
                                menuParent.anchorMax = new Vector2(1f, 1f);
                                menuParent.offsetMin = Vector2.zero;
                                menuParent.offsetMax = Vector2.zero;
                                configMenu.Build(menuParent);
                                ConfigMenus.Add(Tuple.Create(configMenu, menuParent));
                            }
                            else
                            {
                                ConfigMenus.Add(null);
                            }
                        }
                        catch (InvalidOperationException e)
                        {
                            if (e.Message == UMFGadgetConfigMenu.NO_CONFIGURABLE_DATA)
                            {
                                ConfigMenus.Add(null);
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                }
                catch
                {
                    ConfigMenus.Add(null);
                    throw;
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="IGadgetConfigMenu"/> for the given ID. Returns null if the mod with the given ID does not have a config menu. Throws an <see cref="IndexOutOfRangeException"/> if the ID is invalid.
        /// </summary>
        public static IGadgetConfigMenu GetConfigMenu(int ID)
        {
            return ID == -1 ? UMFConfigMenu?.Item1 : ConfigMenus[ID]?.Item1;
        }

        /// <summary>
        /// Gets the <see cref="RectTransform"/> for config menu the given ID. Returns null if the mod with the given ID does not have a config menu. Throws an <see cref="IndexOutOfRangeException"/> if the ID is invalid.
        /// </summary>
        public static RectTransform GetConfigMenuObject(int ID)
        {
            return ID == -1 ? UMFConfigMenu?.Item2 : ConfigMenus[ID]?.Item2;
        }

        /// <summary>
        /// Checks if the specified config menu is currently open.
        /// </summary>
        public static bool IsConfigMenuOpen(int ID)
        {
            return GetConfigMenuObject(ID)?.gameObject?.activeSelf ?? false;
        }

        /// <summary>
        /// Opens the specified config menu.
        /// </summary>
        public static void OpenConfigMenu(int ID)
        {
            CloseAllConfigMenus();
            if (GetConfigMenu(ID).ShouldHideModMenu()) SceneInjector.ModMenuPanel.gameObject.SetActive(false);
            GetConfigMenu(ID).Render();
            GetConfigMenuObject(ID).gameObject.SetActive(true);
        }

        /// <summary>
        /// Closes the specified config menu. Also, if <paramref name="showModMenu"/> is true, then displays the Mod Menu (if it isn't already being displayed).
        /// </summary>
        public static void CloseConfigMenu(int ID, bool showModMenu = true)
        {
            GetConfigMenuObject(ID).gameObject.SetActive(false);
            GetConfigMenu(ID).Derender();
            if (showModMenu) SceneInjector.ModMenuPanel.gameObject.SetActive(true);
        }

        /// <summary>
        /// Closes any and all config menus. Also, if <paramref name="showModMenu"/> is true, then displays the Mod Menu (if it isn't already being displayed).
        /// </summary>
        public static void CloseAllConfigMenus(bool showModMenu = true)
        {
            if (UMFConfigMenu != null && UMFConfigMenu.Item2.gameObject.activeSelf)
            {
                UMFConfigMenu.Item2.gameObject.SetActive(false);
                UMFConfigMenu.Item1.Derender();
            }
            foreach (Tuple<IGadgetConfigMenu, RectTransform> configMenu in ConfigMenus)
            {
                if (configMenu != null && configMenu.Item2.gameObject.activeSelf)
                {
                    configMenu.Item2.gameObject.SetActive(false);
                    configMenu.Item1.Derender();
                }
            }
            if (showModMenu) SceneInjector.ModMenuPanel.gameObject.SetActive(true);
        }
    }
}
