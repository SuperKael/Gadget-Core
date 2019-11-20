using GadgetCore.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace GadgetCore
{

    /// <summary>
    /// Provides utility methods used by GadgetCore's patches. You probably shouldn't use these yourself.
    /// </summary>
    public static class PatchMethods
    {
        private static MethodInfo GetItemLevel2 = typeof(GameScript).GetMethod("GetItemLevel2", BindingFlags.NonPublic | BindingFlags.Instance);
        private static MethodInfo GetGearAspect = typeof(GameScript).GetMethod("GetGearAspect", BindingFlags.NonPublic | BindingFlags.Instance);

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
                    if ((type & ItemType.MODABLE) == ItemType.MODABLE)
                    {
                        InstanceTracker.GameScript.itemexpbar.SetActive(true);
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
                        float[] itemLevel2 = GetItemLevel2.Invoke(InstanceTracker.GameScript, new object[] { item.exp }) as float[];
                        int num2 = (int)itemLevel2[0];
                        InstanceTracker.GameScript.itemexpbar.transform.localScale = new Vector3(itemLevel2[1], 0.015f, 1f);
                        if (num2 < 10)
                        {
                            InstanceTracker.GameScript.itemLevel.text = "Lv." + num2;
                        }
                        else
                        {
                            InstanceTracker.GameScript.itemLevel.text = "MAX";
                        }
                        for (int i = 0; i < 3; i++)
                        {
                            if (item.aspectLvl[i] > 0)
                            {
                                InstanceTracker.GameScript.itemAspect[i].text = (GetGearAspect.Invoke(InstanceTracker.GameScript, new object[] { item.aspect[i] - 200 }) as string) + " " + item.aspectLvl[i];
                            }
                            else
                            {
                                InstanceTracker.GameScript.itemAspect[i].text = "(EMPTY)";
                            }
                            InstanceTracker.GameScript.aspectObj[i].SetActive(true);
                        }
                        int[] gearStats = GadgetCoreAPI.GetGearBaseStats(id).GetStatArray();
                        for (int i = 0; i < 6; i++)
                        {
                            int num3 = 0;
                            for (int j = 0; j < 3; j++)
                            {
                                if (item.aspect[j] - 200 == i + 1)
                                {
                                    num3 += item.aspectLvl[j];
                                }
                            }
                            int num4;
                            if (gearStats[i] > 0)
                            {
                                num4 = gearStats[i] * num2 + item.tier * 3 + num3;
                            }
                            else
                            {
                                num4 = num3;
                            }
                            if (num4 > 0)
                            {
                                InstanceTracker.GameScript.itemStat[i].text = "+" + num4;
                            }
                            else
                            {
                                InstanceTracker.GameScript.itemStat[i].text = string.Empty;
                            }
                        }
                        InstanceTracker.GameScript.txtStats.SetActive(true);
                    }
                    else
                    {
                        InstanceTracker.GameScript.itemexpbar.SetActive(true);
                        InstanceTracker.GameScript.itemName.color = Color.white;
                        InstanceTracker.GameScript.hoverItem.GetComponent<Renderer>().material = InstanceTracker.GameScript.hoverItemMat1;
                        float[] itemLevel = GetItemLevel2.Invoke(InstanceTracker.GameScript, new object[] { item.exp }) as float[];
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
    }
}
