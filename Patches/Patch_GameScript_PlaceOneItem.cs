using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("PlaceOneItem")]
    static class Patch_GameScript_PlaceOneItem
    {
        public static readonly MethodInfo SwapItem = typeof(GameScript).GetMethod("SwapItem", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly MethodInfo RefreshSlot = typeof(GameScript).GetMethod("RefreshSlot", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly MethodInfo RefreshHoldingSlot = typeof(GameScript).GetMethod("RefreshHoldingSlot", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly MethodInfo UpdateDroids = typeof(GameScript).GetMethod("UpdateDroids", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly MethodInfo GetItemLevel = typeof(GameScript).GetMethod("GetItemLevel", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly MethodInfo RefreshStats = typeof(GameScript).GetMethod("RefreshStats", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly MethodInfo RefreshMODS = typeof(GameScript).GetMethod("RefreshMODS", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly MethodInfo EmblemForging = typeof(GameScript).GetMethod("EmblemForging", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int slot, ref Item ___holdingItem, ref Item[] ___inventory, bool ___emblemAgain)
        {
            ItemInfo itemInfo = ItemRegistry.Singleton.GetEntry(___holdingItem.id);
            ItemType holdingItemType = itemInfo != null ? (itemInfo.Type & (ItemType.EQUIP_MASK | ItemType.TYPE_MASK)) : ItemRegistry.GetDefaultTypeByID(___holdingItem.id);
            if ((holdingItemType & ItemType.NONSTACKING) == ItemType.NONSTACKING)
            {
                SwapItem.Invoke(__instance, new object[] { slot });
                return false;
            }
            if ((slot == 36 && (holdingItemType & ItemType.EQUIP_MASK) != ItemType.WEAPON) || (slot == 37 && ((holdingItemType & ItemType.EQUIP_MASK) != ItemType.OFFHAND)) || (slot == 38 && ((holdingItemType & ItemType.EQUIP_MASK) != ItemType.HELMET)) || (slot == 39 && ((holdingItemType & ItemType.EQUIP_MASK) != ItemType.ARMOR)) || ((slot == 40 || slot == 41) && ((holdingItemType & ItemType.EQUIP_MASK) != ItemType.RING)) || (slot > 41 && ((holdingItemType & ItemType.EQUIP_MASK) != ItemType.DROID)))
            {
                MonoBehaviour.print("CANNOT PUT THAT THERE!");
            }
            else
            {
                MonoBehaviour.print("PLACING ONE ITEM");
                if (___emblemAgain)
                {
                    EmblemForging.Invoke(__instance, new object[] { });
                }
                bool newItem = false;
                if (___inventory[slot].id == ___holdingItem.id)
                {
                    ___inventory[slot].q++;
                }
                else
                {
                    newItem = true;
                    ___inventory[slot] = GadgetCoreAPI.CopyItem(___holdingItem);
                    ___inventory[slot].q = 1;
                }
                ___holdingItem.q--;
                MonoBehaviour.print(string.Concat(new object[]
                {
                "holding Item ",
                ___holdingItem.id,
                " x",
                ___holdingItem.q
                }));
                RefreshSlot.Invoke(__instance, new object[] { slot });
                RefreshHoldingSlot.Invoke(__instance, new object[] { });
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
                    if (newItem)
                    {
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
                        for (int i = 0; i < 6; i++)
                        {
                            if (gearStats[i] > 0)
                            {
                                GameScript.GEARSTAT[i] += gearStats[i];
                                __instance.txtPlayerStat[i].GetComponent<Animation>().Play();
                            }
                        }
                        GadgetCoreAPI.equippedGearStats[slot - 36] = gearStats;
                        RefreshStats.Invoke(__instance, new object[] { });
                        Network.RemoveRPCs(MenuScript.playerAppearance.GetComponent<NetworkView>().viewID);
                        MenuScript.playerAppearance.GetComponent<NetworkView>().RPC("UA", RPCMode.AllBuffered, new object[]
                        {
                            GameScript.equippedIDs,
                            0,
                            GameScript.dead
                        });
                        RefreshMODS.Invoke(__instance, new object[] { });
                        __instance.UpdateHP();
                        __instance.UpdateEnergy();
                        __instance.UpdateMana();
                    }
                    if (itemInfo != null) itemInfo.InvokeOnEquip(slot);
                }
                else
                {
                    __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/CLICK2"), Menuu.soundLevel / 10f);
                }
            }
            return false;
        }
    }
}