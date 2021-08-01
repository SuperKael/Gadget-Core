using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Collections;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("UseItem")]
    static class Patch_GameScript_UseItem
    {
        public static readonly FieldInfo usingItem = typeof(GameScript).GetField("usingItem", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly FieldInfo curBlockSlot = typeof(GameScript).GetField("curBlockSlot", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly MethodInfo RefreshSlot = typeof(GameScript).GetMethod("RefreshSlot", BindingFlags.Public | BindingFlags.Instance);
        public static readonly MethodInfo EnterBuildMode = typeof(GameScript).GetMethod("EnterBuildMode", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int slot, ref Item[] ___inventory, bool ___exitingcombatmode, ref bool ___usingItem)
        {
            if (GadgetCoreAPI.IsInputFrozen()) return false;
            ItemInfo item = ItemRegistry.Singleton.GetEntry(___inventory[slot].id);
            if (item != null && !GameScript.dead && !___exitingcombatmode && !___usingItem)
            {
                ___usingItem = item.InvokeOnUse(slot);
                if ((item.Type & ItemType.USABLE) == ItemType.USABLE || ___usingItem)
                {
                    if (!VanillaItemInfo.Using) __instance.StartCoroutine(UseItemFinal(__instance, slot, item, ___inventory, ___usingItem));
                    VanillaItemInfo.Using = false;
                }
                else if ((item.Type & ItemType.EQUIPABLE) != ItemType.EQUIPABLE)
                {
                    __instance.StartCoroutine(UseItemFinal(__instance, slot, item, ___inventory, ___usingItem));
                }
                return false;
            }
            return true;
        }

        private static IEnumerator UseItemFinal(GameScript instance, int slot, ItemInfo item, Item[] inventory, bool usingItem)
        {
            int tempID = GameScript.equippedIDs[0];
            GameScript.equippedIDs[0] = inventory[slot].id;
            int[] convertedIDs;
            if (usingItem)
            {
                convertedIDs = new int[]
                {
                    GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, GameScript.equippedIDs[0]),
                    GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, GameScript.equippedIDs[1]),
                    GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, GameScript.equippedIDs[2]),
                    GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, GameScript.equippedIDs[3]),
                    GadgetNetwork.ConvertIDToHost(null /* RaceRegistry */, GameScript.equippedIDs[4]),
                    GameScript.equippedIDs[5],
                    GadgetNetwork.ConvertIDToHost(null /* UniformRegistry */, GameScript.equippedIDs[6]),
                    GadgetNetwork.ConvertIDToHost(null /* AugmentRegistry */, GameScript.equippedIDs[7])
                };
                MenuScript.playerAppearance.GetComponent<NetworkView>().RPC("UA", RPCMode.AllBuffered, new object[]
                {
                    convertedIDs,
                    2,
                    GameScript.dead
                });
                MenuScript.player.SendMessage("Use");
                yield return new WaitForSeconds(0.5f);
                instance.StartCoroutine(UsingItem(instance));
                instance.inventoryBarObj[slot].GetComponent<Animation>().Play();
                item.InvokeOnUseFinal(slot);
                if ((item.Type & ItemType.CONSUMABLE) == ItemType.CONSUMABLE)
                {
                    inventory[slot].q--;
                    RefreshSlot.Invoke(instance, new object[] { slot });
                }
            }
            else if (item.Tile != null)
            {
                instance.inventoryBarObj[slot].GetComponent<Animation>().Play();
                GameScript.curBlockID = item.Tile.ID;
                curBlockSlot.SetValue(instance, slot);
                Renderer component = instance.hoverSprite.GetComponent<Renderer>();
                component.material = (Material)Resources.Load("construct/c" + GameScript.curBlockID);
                component.material.mainTextureScale = new Vector2(1f, 1f);
                if (item.Tile.Type == TileType.WALL)
                {
                    instance.hoverSprite.transform.localPosition = new Vector3(0f, 0f, 2f);
                }
                else
                {
                    instance.hoverSprite.transform.localPosition = new Vector3(0f, 0f, 0f);
                }
                if (!GameScript.buildMode)
                {
                    EnterBuildMode.Invoke(instance, new object[] { });
                }
                else
                {
                    instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/buildmode"));
                }
            }
            else
            {
                convertedIDs = new int[]
                {
                    GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, GameScript.equippedIDs[0]),
                    GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, GameScript.equippedIDs[1]),
                    GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, GameScript.equippedIDs[2]),
                    GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, GameScript.equippedIDs[3]),
                    GadgetNetwork.ConvertIDToHost(null /* RaceRegistry */, GameScript.equippedIDs[4]),
                    GameScript.equippedIDs[5],
                    GadgetNetwork.ConvertIDToHost(null /* UniformRegistry */, GameScript.equippedIDs[6]),
                    GadgetNetwork.ConvertIDToHost(null /* AugmentRegistry */, GameScript.equippedIDs[7])
                };
                MenuScript.playerAppearance.GetComponent<NetworkView>().RPC("UA", RPCMode.AllBuffered, new object[]
                {
                    convertedIDs,
                    2,
                    GameScript.dead
                });
                MenuScript.player.SendMessage("Use");
                yield return new WaitForSeconds(0.5f);
                instance.StartCoroutine(UsingItem(instance));
                instance.inventoryBarObj[slot].GetComponent<Animation>().Play();
            }
            yield return new WaitForSeconds(0.1f);
            GameScript.equippedIDs[0] = tempID;
            convertedIDs = new int[]
            {
                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, GameScript.equippedIDs[0]),
                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, GameScript.equippedIDs[1]),
                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, GameScript.equippedIDs[2]),
                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, GameScript.equippedIDs[3]),
                GadgetNetwork.ConvertIDToHost(null /* RaceRegistry */, GameScript.equippedIDs[4]),
                GameScript.equippedIDs[5],
                GadgetNetwork.ConvertIDToHost(null /* UniformRegistry */, GameScript.equippedIDs[6]),
                GadgetNetwork.ConvertIDToHost(null /* AugmentRegistry */, GameScript.equippedIDs[7])
            };
            MenuScript.playerAppearance.GetComponent<NetworkView>().RPC("UA", RPCMode.AllBuffered, new object[]
            {
                convertedIDs,
                0,
                GameScript.dead
            });
            yield break;
        }

        private static IEnumerator UsingItem(GameScript instance)
        {
            yield return new WaitForSeconds(0.2f);
            usingItem.SetValue(instance, false);
            yield break;
        }
    }
}