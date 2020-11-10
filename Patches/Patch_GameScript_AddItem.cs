using HarmonyLib;
using GadgetCore.API;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("AddItem")]
    static class Patch_GameScript_AddItem
    {
        public static readonly MethodInfo RefreshSlot = typeof(GameScript).GetMethod("RefreshSlot", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly MethodInfo GetItemName = typeof(GameScript).GetMethod("GetItemName", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly FieldInfo inventory = typeof(GameScript).GetField("inventory", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, Package package, ref IEnumerator __result)
        {
            if (ItemRegistry.GetSingleton().HasEntry(package.item.id))
            {
                __result = AddItem(__instance, package);
                return false;
            }
            return true;
        }

        private static IEnumerator AddItem(GameScript script, Package package)
        {
            bool flag = false;
            bool flag2 = false;
            bool localItem = package.localItem;
            Item[] inventory = Patch_GameScript_AddItem.inventory.GetValue(script) as Item[];
            if (package.item.q == 0)
            {
                package.item.q = 1;
            }
            ItemType type = ItemRegistry.GetTypeByID(package.item.id);
            if ((type & ItemType.NONSTACKING) == ItemType.STACKING)
            {
                for (int i = 0; i < 36; i++)
                {
                    if (GadgetCoreAPI.CanItemsStack(inventory[i], package.item) && inventory[i].q < 9999)
                    {
                        inventory[i].q += package.item.q;
                        if (inventory[i].q > 9999)
                        {
                            Item item = new Item(package.item.id, inventory[i].q - 9999, 0, 0, 0, new int[3], new int[3]);
                            inventory[i].q = 9999;
                            Package package2 = new Package(item, package.obj, localItem);
                            script.AddItem(package2);
                        }
                        RefreshSlot.Invoke(script, new object[] { i });
                        flag2 = true;
                        flag = true;
                        break;
                    }
                }
            }
            if (!flag)
            {
                for (int i = (type & ItemType.EQUIP_MASK) == ItemType.DROID ? 6 : 0; i < 36; i++)
                {
                    if (inventory[i].id == 0)
                    {
                        inventory[i] = package.item;
                        RefreshSlot.Invoke(script, new object[] { i });
                        flag2 = true;
                        break;
                    }
                }
            }
            if (flag2)
            {
                script.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/pickup"), Menuu.soundLevel / 10f);
                Vector3 position = MenuScript.player.transform.position;
                GameObject gameObject = (GameObject)Object.Instantiate(Resources.Load("txtGet"), new Vector3(position.x, position.y + 1f, -3.2f), Quaternion.identity);
                Package1 value = new Package1(GetItemName.Invoke(script, new object[] { package.item.id }) as string, package.item.q);
                gameObject.SendMessage("Init", value);
            }
            if (localItem)
            {
                package.obj.SendMessage("FinalReply", flag2);
            }
            else if (Network.isServer)
            {
                package.obj.SendMessage("FinalReply", flag2);
            }
            else
            {
                package.obj.GetComponent<NetworkView>().RPC("FinalReply", RPCMode.Server, new object[]
                {
                flag2
                });
            }
            return null;
        }
    }
}