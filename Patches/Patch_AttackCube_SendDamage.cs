using HarmonyLib;
using GadgetCore.API;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(AttackCube))]
    [HarmonyPatch("SendDamage")]
    static class Patch_AttackCube_SendDamage
    {
        [HarmonyPrefix]
        public static bool Prefix(AttackCube __instance, Collider c)
        {
            if (ItemRegistry.Singleton.HasEntry(GameScript.equippedIDs[0]))
            {
                ItemInfo weapon = ItemRegistry.Singleton[GameScript.equippedIDs[0]];
                PlayerScript player = __instance.gameObject.GetComponentInParent<PlayerScript>();
                int num = 0;
                if (PlayerScript.berserkBool)
                {
                    num = InstanceTracker.GameScript.GetFinalStat(1);
                }
                float[] array = new float[]
                {
                    weapon.GetDamage(player) + num,
                    MenuScript.player.transform.position.x
                };
                if (weapon.TryCrit(player))
                {
                    if (GameScript.equippedIDs[0] == 373)
                    {
                        InstanceTracker.GameScript.RecoverHP(5);
                    }
                    else if (GameScript.equippedIDs[0] == 378)
                    {
                        GameScript.mana = GameScript.maxmana;
                        InstanceTracker.GameScript.UpdateMana();
                    }
                    __instance.GetComponent<AudioSource>().PlayOneShot(__instance.critSound, Menuu.soundLevel / 10f);
                    Object.Instantiate<GameObject>(__instance.crit, __instance.transform.position, Quaternion.identity);
                    array[0] = weapon.MultiplyCrit(player, (int)array[0]);
                }
                if (GameScript.debugMode)
                {
                    array[0] = 99999f;
                }
                if (Network.isServer)
                {
                    c.gameObject.SendMessage("TD", array);
                }
                else
                {
                    c.gameObject.GetComponent<NetworkView>().RPC("TD", RPCMode.Server, new object[]
                    {
                        array
                    });
                }
                return false;
            }
            return true;
        }
    }
}