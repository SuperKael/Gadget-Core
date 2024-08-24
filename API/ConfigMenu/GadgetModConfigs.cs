using GadgetCore.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GadgetCore.API.ConfigMenu
{
    /// <summary>
    /// Tracker for <see cref="IGadgetConfigMenu"/>s.
    /// </summary>
    public static class GadgetModConfigs
    {
        internal static Dictionary<int, Tuple<IGadgetConfigMenu, RectTransform>> ConfigMenus = new Dictionary<int, Tuple<IGadgetConfigMenu, RectTransform>>();
        private static Tuple<IGadgetConfigMenu, RectTransform> UMFConfigMenu;

        internal static void BuildConfigMenus(RectTransform parent)
        {
            ConfigMenus.Clear();

            if (GadgetCoreAPI.GetUMFAPI() != null)
            {
                if (UMFConfigMenu == null || UMFConfigMenu.Item2 == null)
                {
                    UMFConfigMenu umfConfigMenu = new UMFConfigMenu();
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
                else
                {
                    UMFConfigMenu.Item1.Reset();
                }
            }

            for (int i = 0;i < ModMenuController.modEntries.Count;i++)
            {
                ModMenuEntry entry = ModMenuController.modEntries[i];
                if (entry.Type == ModMenuEntryType.GADGET || entry.Type == ModMenuEntryType.UMF || entry.Type == ModMenuEntryType.DISABLED_UMF)
                {
                    try
                    {
                        GadgetInfo gadgetForConfig = entry.Gadgets.FirstOrDefault(x => x.Attribute.Name == entry.Name);
                        IGadgetConfigMenu configMenu = gadgetForConfig?.Gadget.GetConfigMenu();
                        if (configMenu != null)
                        {
                            RectTransform menuParent = new GameObject(entry.Name, typeof(RectTransform)).GetComponent<RectTransform>();
                            menuParent.gameObject.SetActive(false);
                            menuParent.SetParent(parent);
                            menuParent.anchorMin = new Vector2(0f, 0f);
                            menuParent.anchorMax = new Vector2(1f, 1f);
                            menuParent.offsetMin = Vector2.zero;
                            menuParent.offsetMax = Vector2.zero;
                            configMenu.Build(menuParent);
                            ConfigMenus.Add((i << 16) + 0xFFFF, Tuple.Create(configMenu, menuParent));
                        }
                        else
                        {
                            ConfigMenus.Add((i << 16) + 0xFFFF, null);
                        }
                    }
                    catch (InvalidOperationException e)
                    {
                        if (e.Message == INIGadgetConfigMenu.NO_CONFIGURABLE_DATA)
                        {
                            ConfigMenus.Add((i << 16) + 0xFFFF, null);
                        }
                        else
                        {
                            throw;
                        }
                    }

                    for (int g = 0;g < entry.Gadgets.Length;g++)
                    {
                        GadgetInfo gadget = entry.Gadgets[g];
                        try
                        {
                            IGadgetConfigMenu configMenu = gadget?.Gadget.GetConfigMenu();
                            if (configMenu != null)
                            {
                                RectTransform menuParent = new GameObject(gadget.Attribute.Name, typeof(RectTransform)).GetComponent<RectTransform>();
                                menuParent.gameObject.SetActive(false);
                                menuParent.SetParent(parent);
                                menuParent.anchorMin = new Vector2(0f, 0f);
                                menuParent.anchorMax = new Vector2(1f, 1f);
                                menuParent.offsetMin = Vector2.zero;
                                menuParent.offsetMax = Vector2.zero;
                                configMenu.Build(menuParent);
                                ConfigMenus.Add((i << 16) + g, Tuple.Create(configMenu, menuParent));
                            }
                            else
                            {
                                ConfigMenus.Add((i << 16) + g, null);
                            }
                        }
                        catch (InvalidOperationException e)
                        {
                            if (e.Message == INIGadgetConfigMenu.NO_CONFIGURABLE_DATA)
                            {
                                ConfigMenus.Add((i << 16) + g, null);
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="IGadgetConfigMenu"/> for the given ID. Returns null if the mod with the given ID does not have a config menu. Throws an <see cref="IndexOutOfRangeException"/> if the ID is invalid.
        /// </summary>
        public static IGadgetConfigMenu GetConfigMenu(int ID)
        {
            return ID == -1 ? UMFConfigMenu?.Item1 : ConfigMenus.ContainsKey(ID) ? ConfigMenus[ID]?.Item1 : null;
        }

        /// <summary>
        /// Gets the <see cref="RectTransform"/> for config menu the given ID. Returns null if the mod with the given ID does not have a config menu. Throws an <see cref="IndexOutOfRangeException"/> if the ID is invalid.
        /// </summary>
        public static RectTransform GetConfigMenuObject(int ID)
        {
            return ID == -1 ? UMFConfigMenu?.Item2 : ConfigMenus.ContainsKey(ID) ? ConfigMenus[ID]?.Item2 : null;
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
            foreach (Tuple<IGadgetConfigMenu, RectTransform> configMenu in ConfigMenus.Values)
            {
                if (configMenu != null && configMenu.Item2.gameObject.activeSelf)
                {
                    configMenu.Item2.gameObject.SetActive(false);
                    configMenu.Item1.Derender();
                }
            }
            if (showModMenu) SceneInjector.ModMenuPanel.gameObject.SetActive(true);
        }

        /// <summary>
        /// Rebuilds all config menus.
        /// </summary>
        public static void RebuildAllConfigMenus()
        {
            if (UMFConfigMenu?.Item1 != null)
            {
                UMFConfigMenu.Item1.Rebuild();
            }
            if (ConfigMenus != null) foreach (Tuple<IGadgetConfigMenu, RectTransform> configMenu in ConfigMenus.Values)
            {
                if (configMenu?.Item1 != null)
                {
                    configMenu.Item1.Rebuild();
                }
            }
        }

        /// <summary>
        /// Resets all config menus.
        /// </summary>
        public static void ResetAllConfigMenus()
        {
            if (UMFConfigMenu != null)
            {
                UMFConfigMenu.Item1.Reset();
            }
            foreach (Tuple<IGadgetConfigMenu, RectTransform> configMenu in ConfigMenus.Values)
            {
                if (configMenu != null)
                {
                    configMenu.Item1.Reset();
                }
            }
        }
    }
}
