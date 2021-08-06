using System;
using UnityEngine;

namespace GadgetCore.API.Dialog
{
    /// <summary>
    /// Represents a complete chain of dialog.
    /// </summary>
    public class DialogChain
    {
        /// <summary>
        /// The ID of the Gadget that registered this dialog chain.
        /// </summary>
        public readonly int GadgetID;
        /// <summary>
        /// The NPC ID ('a' parameter to the Talk method) of this dialog chain.
        /// </summary>
        public readonly int NPCID;
        /// <summary>
        /// The name of the NPC to display in the dialog box.
        /// </summary>
        public readonly string NPCName;
        /// <summary>
        /// The portrait of the NPC to display in the dialog box.
        /// </summary>
        public readonly Material NPCPortrait;
        /// <summary>
        /// Returns true if this dialog chain should be triggered under the current circumstances.
        /// The int parameter is is the 'b' parameter to the Talk method.
        /// </summary>
        public readonly Func<int, bool> TriggerCondition;
        /// <summary>
        /// The array of <see cref="DialogMessage"/>s that will be displayed for this dialog chain.
        /// </summary>
        public readonly DialogMessage[] Messages;

        /// <summary>
        /// Constructs a new DialogChain
        /// </summary>
        public DialogChain(int NPCID, string NPCName, Material NPCPortrait, Func<int, bool> TriggerCondition, params DialogMessage[] Messages)
        {
            if (!Registry.registeringVanilla && Registry.gadgetRegistering < 0) throw new InvalidOperationException("Dialog chain registration may only be performed by the Initialize method of a Gadget!");
            GadgetID = Registry.gadgetRegistering;
            this.NPCID = NPCID;
            this.NPCName = NPCName;
            this.TriggerCondition = TriggerCondition;
            this.Messages = Messages;
            if (NPCPortrait != null)
            {
                this.NPCPortrait = NPCPortrait;
                this.NPCPortrait.mainTextureScale = new Vector2(0.5f, 1);
            }
        }

        /// <summary>
        /// Constructs a new DialogChain
        /// </summary>
        public DialogChain(int NPCID, string NPCName, Texture2D NPCPortrait, Func<int, bool> TriggerCondition, params DialogMessage[] Messages)
        {
            if (!Registry.registeringVanilla && Registry.gadgetRegistering < 0) throw new InvalidOperationException("Dialog chain registration may only be performed by the Initialize method of a Gadget!");
            GadgetID = Registry.gadgetRegistering;
            this.NPCID = NPCID;
            this.NPCName = NPCName;
            this.TriggerCondition = TriggerCondition;
            this.Messages = Messages;
            if (NPCPortrait != null)
            {
                this.NPCPortrait = new Material(Shader.Find("Unlit/Transparent")) { mainTexture = NPCPortrait };
                this.NPCPortrait.mainTextureScale = new Vector2(0.5f, 1);
            }
        }
    }
}
