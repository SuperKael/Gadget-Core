using GadgetCore.Util;
using System;
using System.Linq;
using UnityEngine;

namespace GadgetCore.API.Dialog
{
    /// <summary>
    /// Static class containing commonly-used <see cref="DialogMessage"/> trigger actions.
    /// </summary>
    public static class DialogActions
    {
        /// <summary>
        /// Combines multiple trigger actions together.
        /// </summary>
        public static Action ComboAction(params Action[] actions)
        {
            return () =>
            {
                actions.ForEach(x => x());
            };
        }

        /// <summary>
        /// Triggers a random trigger action.
        /// </summary>
        public static Action RandomAction(params Action[] actions)
        {
            return () =>
            {
                actions[UnityEngine.Random.Range(0, actions.Length)]();
            };
        }

        /// <summary>
        /// Triggers a weighted random trigger action.
        /// </summary>
        public static Action WeightedRandomAction(params Tuple<Action, int>[] actions)
        {
            int[] weightedActionIndices = actions.SelectMany((x, i) => Enumerable.Repeat(i, x.Item2)).ToArray();
            return () =>
            {
                actions[weightedActionIndices[UnityEngine.Random.Range(0, weightedActionIndices.Length)]].Item1();
            };
        }

        /// <summary>
        /// Plays a given audio clip.
        /// </summary>
        public static Action PlayAudioClip(AudioClip clip)
        {
            return () =>
            {
                InstanceTracker.GameScript.GetComponent<AudioSource>().PlayOneShot(clip, Menuu.soundLevel / 10f);
            };
        }

        /// <summary>
        /// Increments the <see cref="GameScript.allegianceQuest"/> value.
        /// </summary>
        public static Action IncrementAllegianceLevel()
        {
            return () =>
            {
                GameScript.allegianceQuest++;
                InstanceTracker.GameScript.LevelUpAllegiance();
            };
        }

        /// <summary>
        /// Sets the <see cref="GameScript.allegianceQuest"/> value.
        /// </summary>
        public static Action SetAllegianceLevel(int level)
        {
            return () =>
            {
                int oldLevel = GameScript.allegianceQuest;
                GameScript.allegianceQuest = level;
                if (level > oldLevel) InstanceTracker.GameScript.LevelUpAllegiance();
            };
        }

        /// <summary>
        /// Gives the player an item.
        /// </summary>
        public static Action GrantItem(Item item, bool randomizeTier = false, int randomQuantityVariation = 0)
        {
            return () =>
            {
                InstanceTracker.GameScript.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/glitter"), Menuu.soundLevel / 10f);
                int[] itemArray = GadgetCoreAPI.ConstructIntArrayFromItem(item);
                if (randomQuantityVariation > 0) itemArray[1] += UnityEngine.Random.Range(0, randomQuantityVariation + 1);
                if (randomQuantityVariation < 0) itemArray[1] -= UnityEngine.Random.Range(0, -randomQuantityVariation + 1);
                if (randomizeTier) itemArray[3] = InstanceTracker.GameScript.GetRandomTier();
                GameObject itemObject = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("i3"), new Vector3(InstanceTracker.GameScript.transform.position.x, InstanceTracker.GameScript.transform.position.y, 0f), Quaternion.identity);
                itemObject.SendMessage("InitL", itemArray);
            };
        }

        /// <summary>
        /// Takes an item from the player.
        /// </summary>
        public static Action TakeItem(int itemID, int quantity = 1)
        {
            return () =>
            {
                int slot = InstanceTracker.GameScript.ItemExistsSlot(itemID);
                if (slot != -1 && GadgetCoreAPI.GetInventory()[slot].q >= quantity)
                {
                    GadgetCoreAPI.GetInventory()[slot].q -= quantity;
                    InstanceTracker.GameScript.RefreshSlot(slot);
                }
            };
        }

        /// <summary>
        /// Activates the story choice menu.
        /// </summary>
        public static Action StoryChoiceMenu()
        {
            return () =>
            {
                InstanceTracker.GameScript.MenuStory();
            };
        }

        /// <summary>
        /// Immediately exits dialog.
        /// </summary>
        public static Action ExitDialog()
        {
            return () =>
            {
                InstanceTracker.GameScript.ExitTalking();
            };
        }

        /// <summary>
        /// Branches to another dialog chain as long as the chain's trigger condition is met.
        /// This chain does not need to have been registered with <see cref="DialogChains.RegisterDialogChain(DialogChain)"/>
        /// </summary>
        public static Action BranchDialog(DialogChain chain)
        {
            return () =>
            {
                if (chain.TriggerCondition == null || chain.TriggerCondition(InstanceTracker.GameScript.GetFieldValue<int>("curB"))) DialogChains.InitiateDialog(chain);
            };
        }

        /// <summary>
        /// Branches to another dialog chain based on the story choice.
        /// If the matching dialog chain parameter is null, then does nothing.
        /// </summary>
        public static Action BranchStoryChoice(DialogChain choice1, DialogChain choice2, DialogChain choice3, bool force = false)
        {
            return () =>
            {
                switch (GameScript.choice)
                {
                    case 1:
                        if (choice1 != null && (force || choice1.TriggerCondition == null ||
                            choice1.TriggerCondition(InstanceTracker.GameScript.GetFieldValue<int>("curB")))) DialogChains.InitiateDialog(choice1);
                        break;
                    case 2:
                        if (choice2 != null && (force || choice2.TriggerCondition == null ||
                            choice2.TriggerCondition(InstanceTracker.GameScript.GetFieldValue<int>("curB")))) DialogChains.InitiateDialog(choice2);
                        break;
                    case 3:
                        if (choice3 != null && (force || choice3.TriggerCondition == null ||
                            choice3.TriggerCondition(InstanceTracker.GameScript.GetFieldValue<int>("curB")))) DialogChains.InitiateDialog(choice3);
                        break;
                }
            };
        }
    }
}
