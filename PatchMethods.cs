using GadgetCore.API;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GadgetCore
{

    /// <summary>
    /// Provides utility methods used by GadgetCore's patches. You probably shouldn't use these yourself.
    /// </summary>
    public static class PatchMethods
    {
        internal static Dictionary<int, ICharacterFeatureRegistry> characterFeatureRegistries = new Dictionary<int, ICharacterFeatureRegistry>();
        internal static Dictionary<int, ICharacterFeatureRegistryEntry> characterFeatureRegistryEntries = new Dictionary<int, ICharacterFeatureRegistryEntry>();

        /// <summary>
        /// This event is invoked whenever the player levels up. The passed int is the level that the player just reached.
        /// </summary>
        public static event Action<int> OnLevelUp;

        internal static void InvokeOnLevelUp(int level) { OnLevelUp?.Invoke(level); }

        /// <summary>
        /// Displays the item-hover window on the cursor. For some reason, the base game has six different methods that all pretty much just do this.
        /// </summary>
        public static void HoverItem(Item item)
        {
            InstanceTracker.GameScript.hoverDroid.SetActive(false);
            int id = item.id;
            InstanceTracker.GameScript.txtHoverStat[0].text = string.Empty;
            InstanceTracker.GameScript.txtHoverStat[1].text = InstanceTracker.GameScript.txtHoverStat[0].text;
            InstanceTracker.GameScript.txtHoverStatInfo.text = string.Empty;
            if (Camera.main.ScreenToWorldPoint(Input.mousePosition).y < MenuScript.player.transform.position.y - 4.5f)
            {
                InstanceTracker.GameScript.hoverItem.transform.localPosition = new Vector3(5f, 0f, -4.55f);
            }
            else
            {
                InstanceTracker.GameScript.hoverItem.transform.localPosition = new Vector3(5f, -4f, -4.55f);
            }
            if (id != 0)
            {
                InstanceTracker.GameScript.itemName.text = GadgetCoreAPI.GetItemName(id);
                ItemType type = ItemRegistry.GetTypeByID(id);
                if ((type & ItemType.EQUIPABLE) == ItemType.EQUIPABLE)
                {
                    if ((type & ItemType.LEVELING) == ItemType.LEVELING)
                    {
                        InstanceTracker.GameScript.itemexpbar.SetActive(true);
                        float[] itemLevel = InstanceTracker.GameScript.GetItemLevel2(item.exp);
                        int num = (int)itemLevel[0];
                        InstanceTracker.GameScript.itemexpbar.transform.localScale = new Vector3(itemLevel[1], 0.015f, 1f);
                        if (num < 10)
                        {
                            InstanceTracker.GameScript.itemLevel.text = "Lv." + num;
                        }
                        else
                        {
                            InstanceTracker.GameScript.itemLevel.text = "MAX";
                        }
                    }
                    else
                    {
                        InstanceTracker.GameScript.itemexpbar.SetActive(false);
                    }
                    if ((type & ItemType.MODABLE) == ItemType.MODABLE)
                    {
                        if (item.tier == 0)
                        {
                            InstanceTracker.GameScript.itemName.color = Color.white;
                        }
                        else if (item.tier == 1)
                        {
                            InstanceTracker.GameScript.itemName.color = Color.cyan;
                        }
                        else if (item.tier == 2)
                        {
                            InstanceTracker.GameScript.itemName.color = Color.magenta;
                        }
                        else if (item.tier == 3)
                        {
                            InstanceTracker.GameScript.itemName.color = Color.yellow;
                        }
                        InstanceTracker.GameScript.hoverItem.GetComponent<Renderer>().material = InstanceTracker.GameScript.hoverItemMat2;
                        InstanceTracker.GameScript.itemDesc.text = string.Empty;
                        for (int i = 0; i < 3; i++)
                        {
                            if (item.aspectLvl[i] > 0)
                            {
                                InstanceTracker.GameScript.itemAspect[i].text = InstanceTracker.GameScript.GetGearAspect(item.aspect[i] - 200) + " " + item.aspectLvl[i];
                            }
                            else
                            {
                                InstanceTracker.GameScript.itemAspect[i].text = "(EMPTY)";
                            }
                            InstanceTracker.GameScript.aspectObj[i].SetActive(true);
                        }
                        int[] gearStats = GadgetCoreAPI.GetGearStats(item).GetStatArray();
                        for (int i = 0; i < 6; i++)
                        {
                            if (gearStats[i] > 0)
                            {
                                InstanceTracker.GameScript.itemStat[i].text = "+" + gearStats[i];
                                if (InstanceTracker.GameScript.itemStat[i].text.Length > 4)
                                {
                                    InstanceTracker.GameScript.itemStat[i].characterSize = 4f / InstanceTracker.GameScript.itemStat[i].text.Length;
                                }
                                else
                                {
                                    InstanceTracker.GameScript.itemStat[i].characterSize = 1;
                                }
                            }
                            else
                            {
                                InstanceTracker.GameScript.itemStat[i].text = string.Empty;
                                InstanceTracker.GameScript.itemStat[i].characterSize = 1;
                            }
                        }
                        InstanceTracker.GameScript.txtStats.SetActive(true);
                    }
                    else
                    {
                        if (GadgetCoreConfig.BetterDroidHover)
                        {
                            if (item.tier == 0)
                            {
                                InstanceTracker.GameScript.itemName.color = Color.white;
                            }
                            else if (item.tier == 1)
                            {
                                InstanceTracker.GameScript.itemName.color = Color.cyan;
                            }
                            else if (item.tier == 2)
                            {
                                InstanceTracker.GameScript.itemName.color = Color.magenta;
                            }
                            else if (item.tier == 3)
                            {
                                InstanceTracker.GameScript.itemName.color = Color.yellow;
                            }
                            InstanceTracker.GameScript.hoverItem.GetComponent<Renderer>().material = InstanceTracker.GameScript.hoverItemMat2;
                            InstanceTracker.GameScript.itemDesc.text = string.Empty;
                            for (int i = 0; i < 3; i++)
                            {
                                InstanceTracker.GameScript.aspectObj[i].SetActive(false);
                            }
                            InstanceTracker.GameScript.itemAspect[0].text = "";
                            InstanceTracker.GameScript.itemAspect[1].text = "NOT MODABLE";
                            InstanceTracker.GameScript.itemAspect[2].text = "";
                            int[] gearStats = GadgetCoreAPI.GetGearStats(item).GetStatArray();
                            for (int i = 0; i < 6; i++)
                            {
                                if (gearStats[i] > 0)
                                {
                                    InstanceTracker.GameScript.itemStat[i].text = "+" + gearStats[i];
                                    if (InstanceTracker.GameScript.itemStat[i].text.Length > 4)
                                    {
                                        InstanceTracker.GameScript.itemStat[i].characterSize = 4f / InstanceTracker.GameScript.itemStat[i].text.Length;
                                    }
                                    else
                                    {
                                        InstanceTracker.GameScript.itemStat[i].characterSize = 1;
                                    }
                                }
                                else
                                {
                                    InstanceTracker.GameScript.itemStat[i].text = string.Empty;
                                    InstanceTracker.GameScript.itemStat[i].characterSize = 1;
                                }
                            }
                            InstanceTracker.GameScript.txtStats.SetActive(true);
                        }
                        else
                        {
                            InstanceTracker.GameScript.itemName.color = Color.white;
                            InstanceTracker.GameScript.hoverItem.GetComponent<Renderer>().material = InstanceTracker.GameScript.hoverItemMat1;
                            InstanceTracker.GameScript.itemAspect[0].text = string.Empty;
                            InstanceTracker.GameScript.itemAspect[1].text = string.Empty;
                            InstanceTracker.GameScript.itemAspect[2].text = string.Empty;
                            InstanceTracker.GameScript.txtStats.SetActive(false);
                            InstanceTracker.GameScript.aspectObj[0].SetActive(false);
                            InstanceTracker.GameScript.aspectObj[1].SetActive(false);
                            InstanceTracker.GameScript.aspectObj[2].SetActive(false);
                            InstanceTracker.GameScript.itemDesc.text = GadgetCoreAPI.GetItemDesc(id);
                        }
                    }
                }
                else
                {
                    InstanceTracker.GameScript.itemexpbar.SetActive(false);
                    InstanceTracker.GameScript.itemName.color = Color.white;
                    InstanceTracker.GameScript.hoverItem.GetComponent<Renderer>().material = InstanceTracker.GameScript.hoverItemMat1;
                    InstanceTracker.GameScript.itemLevel.text = string.Empty;
                    InstanceTracker.GameScript.itemAspect[0].text = string.Empty;
                    InstanceTracker.GameScript.itemAspect[1].text = string.Empty;
                    InstanceTracker.GameScript.itemAspect[2].text = string.Empty;
                    InstanceTracker.GameScript.txtStats.SetActive(false);
                    InstanceTracker.GameScript.aspectObj[0].SetActive(false);
                    InstanceTracker.GameScript.aspectObj[1].SetActive(false);
                    InstanceTracker.GameScript.aspectObj[2].SetActive(false);
                    InstanceTracker.GameScript.itemDesc.text = GadgetCoreAPI.GetItemDesc(id);
                }
                InstanceTracker.GameScript.hoverItem.SetActive(true);
            }
            else
            {
                InstanceTracker.GameScript.hoverItem.SetActive(false);
            }
        }

        /// <summary>
        /// Completely recalculates GameScript.GEARSTAT from your equipped gear.
        /// </summary>
        public static void RecalculateGearStats(Item[] inventory)
        {
            GameScript.GEARSTAT = new int[6];
            for (int i = 36; i < 45; i++)
            {
                int[] gearStats = GadgetCoreAPI.GetGearStats(inventory[i]).GetStatArray();
                for (int s = 0; s < 6; s++)
                {
                    if (gearStats[s] > 0)
                    {
                        GameScript.GEARSTAT[s] += gearStats[s];
                    }
                }
                GadgetCoreAPI.equippedGearStats[i - 36] = gearStats;
            }
        }

        /// <summary>
        /// Returns whether the given planet consists entirely of town zone(s)
        /// </summary>
        public static bool PlanetIsTownOnly(int planetID)
        {
            return PlanetRegistry.Singleton[planetID] is PlanetInfo planet ? planet.Type == PlanetType.TOWNS || planet.Type == PlanetType.SINGLE : (planetID == 8 || planetID == 11);
        }

        /// <summary>
        /// Gets the <see cref="ICharacterFeatureRegistry"/> with the given SelectorID
        /// </summary>
        public static ICharacterFeatureRegistry GetCharacterFeatureRegistry(int ID)
        {
            return characterFeatureRegistries.TryGetValue(ID, out ICharacterFeatureRegistry reg) ? reg : null;
        }

        /// <summary>
        /// Gets the <see cref="ICharacterFeatureRegistryEntry"/> with the given ChestID
        /// </summary>
        public static ICharacterFeatureRegistryEntry GetCharacterFeatureRegistryEntry(int ID)
        {
            return characterFeatureRegistryEntries.TryGetValue(ID, out ICharacterFeatureRegistryEntry entry) ? entry : null;
        }
    }
}
