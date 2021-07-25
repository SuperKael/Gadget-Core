using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GadgetCore.API.Dialog;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("Talk")]
    static class Patch_GameScript_Talk
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int a, int b, int c, ref int ___curA, ref int ___curB, ref int ___curC)
        {
            DialogChain chain = c < 0 ? DialogChains.activeChain : c > 0 ?
                DialogChains.activeChain != null && DialogChains.activeChain.NPCID == a ?
                    DialogChains.activeChain : null :
                DialogChains.dialogChains.TryGetValue(a, out List<DialogChain> list) ?
                    DialogChains.activeChain = list.LastOrDefault(x => x.TriggerCondition != null && x.TriggerCondition(b)) ?? list.LastOrDefault(x => x.TriggerCondition == null) : null;
            if (chain != null)
            {
                if (c < 0) c = 0;

                __instance.hoverItem.SetActive(false);
                if (GameScript.inventoryOpen && a != -1)
                {
                    __instance.inventoryMain.SetActive(false);
                    GameScript.inventoryOpen = false;
                }
                ___curA = a;
                ___curB = b;
                ___curC = c;

                if (c == 0)
                {
                    __instance.talkingPortrait.GetComponent<Renderer>().material = chain.NPCPortrait ?? (Material)Resources.Load("mat/por/portrait" + a);
                    __instance.txtTalkingName[0].text = chain.NPCName;
                    __instance.txtTalkingName[1].text = chain.NPCName;
                }

                do
                {
                    if (___curC < 0 || ___curC >= chain.Messages.Length)
                    {
                        __instance.StartCoroutine(__instance.ExitTalking());
                        break;
                    }
                }
                while (!chain.Messages[___curC++].DisplayMessage());
                return false;
            }
            else if (a >= TileRegistry.Singleton.GetIDStart())
            {
                TileInfo tile = TileRegistry.Singleton[a];

                __instance.hoverItem.SetActive(false);
                if (GameScript.inventoryOpen)
                {
                    __instance.inventoryMain.SetActive(false);
                    GameScript.inventoryOpen = false;
                }
                ___curA = a;
                ___curB = b;
                ___curC = c;

                __instance.talkingPortrait.GetComponent<Renderer>().material = (Material)Resources.Load("mat/por/portrait" + a);
                __instance.txtTalkingName[0].text = tile?.Item?.Name ?? tile?.RegistryName.Split(':')[1].Trim() ?? string.Empty;
                __instance.txtTalkingName[1].text = __instance.txtTalkingName[0].text;

                if (b == 0)
                {
                    if (c == 0)
                    {
                        __instance.menuTalking.SendMessage("Set", "...");
                        ___curC++;
                    }
                    else
                    {
                        __instance.StartCoroutine(__instance.ExitTalking());
                    }
                }
                return false;
            }
            return true;
        }
    }
}