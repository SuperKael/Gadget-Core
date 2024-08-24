using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GadgetCore.API.Dialog
{
    /// <summary>
    /// Helper class for adding custom dialog chains.
    /// </summary>
    public static class DialogChains
    {
        internal static Dictionary<int, List<DialogChain>> dialogChains = new Dictionary<int, List<DialogChain>>();
        internal static DialogChain activeChain;

        /// <summary>
        /// Initiates dialog.
        /// </summary>
        public static void InitiateDialog(int NPCID)
        {
            if (!GameScript.talking) InstanceTracker.GameScript.StartCoroutine(InstanceTracker.GameScript.EnterTalking());
            InstanceTracker.GameScript.Talk(NPCID, GameScript.allegianceQuest, 0);
        }

        /// <summary>
        /// Initiates dialog.
        /// </summary>
        public static void InitiateDialog(int a, int b)
        {
            if (!GameScript.talking) InstanceTracker.GameScript.StartCoroutine(InstanceTracker.GameScript.EnterTalking());
            InstanceTracker.GameScript.Talk(a, b, 0);
        }

        /// <summary>
        /// Initiates dialog for the given chain.
        /// </summary>
        public static void InitiateDialog(this DialogChain chain)
        {
            activeChain = chain;
            if (!GameScript.talking) InstanceTracker.GameScript.StartCoroutine(InstanceTracker.GameScript.EnterTalking());
            InstanceTracker.GameScript.Talk(chain.NPCID, 0, -1);
        }

        /// <summary>
        /// Registers a new dialog chain.
        /// </summary>
        public static DialogChain RegisterDialogChain(int NPCID, string NPCName, Material NPCPortrait, Func<int, bool> TriggerCondition, params DialogMessage[] Messages)
        {
            return RegisterDialogChain(new DialogChain(NPCID, NPCName, NPCPortrait, TriggerCondition, Messages));
        }

        /// <summary>
        /// Registers a new dialog chain.
        /// </summary>
        public static DialogChain RegisterDialogChain(this DialogChain chain)
        {
            if (dialogChains.TryGetValue(chain.NPCID, out List<DialogChain> list))
            {
                list.Add(chain);
            }
            else
            {
                dialogChains.Add(chain.NPCID, new List<DialogChain> { chain });
            }
            return chain;
        }

        internal static void UnregisterGadgetChains(int gadgetID)
        {
            foreach (KeyValuePair<int, List<DialogChain>> chainList in dialogChains.ToList())
            {
                chainList.Value.RemoveAll(x => x.GadgetID == gadgetID);
                if (chainList.Value.Count == 0) dialogChains.Remove(chainList.Key);
            }
        }
    }
}
