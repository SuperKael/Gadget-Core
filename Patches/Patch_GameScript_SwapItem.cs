using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("SwapItem")]
    static class Patch_GameScript_SwapItem
    {
        public static readonly MethodInfo RefreshSlot = typeof(GameScript).GetMethod("RefreshSlot", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly MethodInfo RefreshHoldingSlot = typeof(GameScript).GetMethod("RefreshHoldingSlot", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly MethodInfo UpdateDroids = typeof(GameScript).GetMethod("UpdateDroids", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly MethodInfo GetGearBaseStats = typeof(GameScript).GetMethod("GetGearBaseStats", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly MethodInfo GetItemLevel = typeof(GameScript).GetMethod("GetItemLevel", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly MethodInfo RefreshStats = typeof(GameScript).GetMethod("RefreshStats", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly MethodInfo RefreshMODS = typeof(GameScript).GetMethod("RefreshMODS", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int slot, ref Item ___holdingItem, ref Item[] ___inventory, ref int ___droidCount)
        {
            ItemInfo itemInfo = ItemRegistry.Singleton.GetEntry(___holdingItem.id);
            ItemInfo slotInfo = ItemRegistry.Singleton.GetEntry(___inventory[slot].id);
            ItemType holdingItemType = itemInfo != null ? (itemInfo.Type & (ItemType.EQUIP_MASK | ItemType.TYPE_MASK)) : ItemRegistry.GetDefaultTypeByID(___holdingItem.id);
            ItemType slotItemType = slotInfo != null ? (slotInfo.Type & (ItemType.EQUIP_MASK | ItemType.TYPE_MASK)) : ItemRegistry.GetDefaultTypeByID(___inventory[slot].id);
            if ((slot == 36 && (holdingItemType & ItemType.EQUIP_MASK) != (ItemType.WEAPON & ItemType.EQUIP_MASK)) || (slot == 37 && ((holdingItemType & ItemType.EQUIP_MASK) != (ItemType.OFFHAND & ItemType.EQUIP_MASK))) || (slot == 38 && ((holdingItemType & ItemType.EQUIP_MASK) != (ItemType.HELMET & ItemType.EQUIP_MASK))) || (slot == 39 && ((holdingItemType & ItemType.EQUIP_MASK) != (ItemType.ARMOR & ItemType.EQUIP_MASK))) || ((slot == 40 || slot == 41) && ((holdingItemType & ItemType.EQUIP_MASK) != (ItemType.RING & ItemType.EQUIP_MASK))) || (slot > 41 && ((holdingItemType & ItemType.EQUIP_MASK) != (ItemType.DROID & ItemType.EQUIP_MASK))))
            {
                __instance.Error(2);
            }
            else
            {
                Item item = ___holdingItem;
                ___holdingItem = ___inventory[slot];
                ___inventory[slot] = item;
                ItemType typeHolder = holdingItemType;
                holdingItemType = slotItemType;
                slotItemType = typeHolder;
                RefreshSlot.Invoke(__instance, new object[] { slot });
                RefreshHoldingSlot.Invoke(__instance, new object[] { });
                if (slot > 41 && slot < 45 && slotItemType == ItemType.DROID)
                {
                    if (holdingItemType == ItemType.DROID)
                    {
                        MonoBehaviour.print("swapped droids");
                        UpdateDroids.Invoke(__instance, new object[] { });
                        __instance.droid[slot - 42].SendMessage("SetStats", ___inventory[slot].id);
                    }
                    else
                    {
                        ___droidCount++;
                        UpdateDroids.Invoke(__instance, new object[] { });
                        __instance.droid[slot - 42].SendMessage("SetStats", ___inventory[slot].id);
                    }
                }
                if (slot > 35)
                {
                    if (slot > 41)
                    {
                        __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/placeD"), Menuu.soundLevel / 10f);
                    }
                    else
                    {
                        __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/bling"), Menuu.soundLevel / 10f);
                    }
                    Object.Instantiate(Resources.Load("clickBurst"), new Vector3(__instance.invIcon[slot].transform.position.x, __instance.invIcon[slot].transform.position.y, 0f), Quaternion.identity);
                    if (slot == 36)
                    {
                        GameScript.equippedIDs[0] = ___inventory[slot].id;
                    }
                    else if (slot == 37)
                    {
                        GameScript.equippedIDs[1] = ___inventory[slot].id;
                    }
                    else if (slot == 38)
                    {
                        GameScript.equippedIDs[2] = ___inventory[slot].id;
                    }
                    else if (slot == 39)
                    {
                        GameScript.equippedIDs[3] = ___inventory[slot].id;
                    }
                    int[] gearStats = GadgetCoreAPI.GetGearStats(___inventory[slot]).GetStatArray();
                    int[] oldStats = GadgetCoreAPI.equippedGearStats[slot - 36];
                    for (int s = 0; s < 6; s++)
                    {
                        if (oldStats[s] > 0)
                        {
                            GameScript.GEARSTAT[s] -= oldStats[s];
                            if (gearStats[s] > 0)
                            {
                                GameScript.GEARSTAT[s] += gearStats[s];
                            }
                            __instance.txtPlayerStat[s].GetComponent<Animation>().Play();
                        }
                        else if (gearStats[s] > 0)
                        {
                            GameScript.GEARSTAT[s] += gearStats[s];
                            __instance.txtPlayerStat[s].GetComponent<Animation>().Play();
                        }
                    }
                    GadgetCoreAPI.equippedGearStats[slot - 36] = gearStats;
                    RefreshStats.Invoke(__instance, new object[] { });
                    RefreshMODS.Invoke(__instance, new object[] { });
                    __instance.UpdateHP();
                    __instance.UpdateEnergy();
                    __instance.UpdateMana();
                    Network.RemoveRPCs(MenuScript.playerAppearance.GetComponent<NetworkView>().viewID);
                    MenuScript.playerAppearance.GetComponent<NetworkView>().RPC("UA", RPCMode.AllBuffered, new object[]
                    {
                GameScript.equippedIDs,
                0,
                GameScript.dead
                    });
                    if (slotInfo != null) slotInfo.InvokeOnDequip(slot);
                    if (itemInfo != null) itemInfo.InvokeOnEquip(slot);
                }
                else
                {
                    __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/CLICK1"), Menuu.soundLevel / 10f);
                }
            }
            return false;
        }
    }
}