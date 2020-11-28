﻿using GadgetCore.Loader;
using GadgetCore.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace GadgetCore.API
{
    /// <summary>
    /// General-purpose utility class for accessing various parts of the Gadget Core API
    /// </summary>
    public static class GadgetCoreAPI
    {
        /// <summary>
        /// The version numbers for this version of Gadget Core. You generally shouldn't access this directly, instead use <see cref="GetRawVersion()"/>
        /// </summary>
        public const string RAW_VERSION = "2.0.0.3";
        /// <summary>
        /// A slightly more informative version. You generally shouldn't access this directly, instead use <see cref="GetFullVersion()"/>
        /// </summary>
        public const string FULL_VERSION = "2.0.0.3-2";
        /// <summary>
        /// Indicates whether this version of GadgetCore is a beta version. You generally shouldn't access this directly, instead use <see cref="GetIsBeta()"/>
        /// </summary>
        public const bool IS_BETA = false;

        internal static readonly int[] currentVersionNums = RAW_VERSION.Split('.').Select(x => int.Parse(x)).ToArray();

        /// <summary>
        /// The sprite used for missing tile textures
        /// </summary>
        public static SpriteSheetEntry MissingTexSprite { get; internal set; }

        private static readonly MethodInfo GetItemNameMethod = typeof(GameScript).GetMethod("GetItemName", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo GetItemDescMethod = typeof(GameScript).GetMethod("GetItemDesc", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo GetChipNameMethod = typeof(GameScript).GetMethod("GetChipName", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo GetChipDescMethod = typeof(GameScript).GetMethod("GetChipDesc", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo GetGearBaseStatsMethod = typeof(GameScript).GetMethod("GetGearBaseStats", BindingFlags.NonPublic | BindingFlags.Instance);

        internal static Dictionary<string, UnityEngine.Object> resources = new Dictionary<string, UnityEngine.Object>();
        internal static Dictionary<int, string> resourcePaths = new Dictionary<int, string>();
        internal static Dictionary<int, List<int>> modResources = new Dictionary<int, List<int>>();
        internal static Dictionary<string, Texture2D> cachedTexes = new Dictionary<string, Texture2D>();
        internal static Dictionary<string, AudioClip> cachedAudioClips = new Dictionary<string, AudioClip>();
        internal static Dictionary<string, Mesh> cachedMeshes = new Dictionary<string, Mesh>();
        internal static Dictionary<string, AssetBundle> cachedBundles = new Dictionary<string, AssetBundle>();
        internal static Dictionary<KeyCode, List<Action>> keyDownListeners = new Dictionary<KeyCode, List<Action>>();
        internal static Dictionary<KeyCode, List<Action>> keyUpListeners = new Dictionary<KeyCode, List<Action>>();
        internal static Dictionary<StatModifierType, List<Tuple<StatModifier, int>>> statModifiers = new Dictionary<StatModifierType, List<Tuple<StatModifier, int>>>();
        internal static Dictionary<string, PlayerScript> playersByName = new Dictionary<string, PlayerScript>();
        internal static Dictionary<string, Action<object[]>> customRPCs = new Dictionary<string, Action<object[]>>();
        internal static Dictionary<int, Tuple<int, int>> emblemForgeRecipes = new Dictionary<int, Tuple<int, int>>();
        internal static List<SpriteSheetEntry> spriteSheetSprites = new List<SpriteSheetEntry>();
        internal static int spriteSheetSize = -1;
        internal static Texture2D spriteSheet;
        internal static int[][] equipedGearStats = new int[9][];
        private static List<string> frozenInput = new List<string>();

        static GadgetCoreAPI()
        {
            if (currentVersionNums.Length != 4) Array.Resize(ref currentVersionNums, 4);
        }

        /// <summary>
        /// Returns the raw version of the running version of GadgetCore. Use this instead of directly accessing <see cref="RAW_VERSION"/>
        /// </summary>
        public static string GetRawVersion()
        {
            return RAW_VERSION;
        }
        /// <summary>
        /// Returns the full version of the running version of GadgetCore. Use this instead of directly accessing <see cref="FULL_VERSION"/>
        /// </summary>
        public static string GetFullVersion()
        {
            return FULL_VERSION;
        }
        /// <summary>
        /// Returns whether the running version of GadgetCore is a beta version. Use this instead of directly accessing <see cref="IS_BETA"/>
        /// </summary>
        public static bool GetIsBeta()
        {
            return IS_BETA;
        }

        /// <summary>
        /// Returns the <see cref="PlayerScript"/> for the player with the given name. Returns null if no player with that name exists.
        /// </summary>
        public static PlayerScript GetPlayerByName(string name)
        {
            return playersByName.ContainsKey(name) ? playersByName[name] : name == Menuu.curName ? InstanceTracker.PlayerScript : null;
        }

        /// <summary>
        /// Freezes keyboard input to GameScript.Update and PlayerScript.Update. Useful when presenting menus that have keyboard input.
        /// </summary>
        public static void FreezeInput(string reason)
        {
            if (!frozenInput.Contains(reason)) frozenInput.Add(reason);
        }

        /// <summary>
        /// Unfreezes input on a one-frame delay.
        /// </summary>
        public static void DelayUnfreezeInput(string reason)
        {
            SceneInjector.PersistantCanvas.GetComponent<UnityEngine.UI.CanvasScaler>().StartCoroutine(DelayUnfreezeInputRoutine(reason));
        }

        private static IEnumerator DelayUnfreezeInputRoutine(string reason)
        {
            yield return new WaitForEndOfFrame();
            frozenInput.Remove(reason);
            yield break;
        }

        /// <summary>
        /// Unfreezes input as it was set for the given reason.
        /// </summary>
        public static void UnfreezeInput(string reason)
        {
            frozenInput.Remove(reason);
        }

        /// <summary>
        /// Indicates whether keyboard input was frozen through <see cref="FreezeInput"/>
        /// </summary>
        public static bool IsInputFrozen()
        {
            return frozenInput.Count > 0;
        }

        /// <summary>
        /// Quits the game. You should use this instead of <see cref="Application.Quit"/>
        /// </summary>
        public static void Quit()
        {
            GadgetCore.Quitting = true;
            Application.Quit();
        }

        /// <summary>
        /// Returns an interface that can be used for interfacing with UMF. Will return null if UMF is not installed.
        /// </summary>
        public static IUMFAPI GetUMFAPI()
        {
            return GadgetCore.UMFAPI;
        }

        /// <summary>
        /// Registers the provided action so that it will be triggered when the provided key is pressed.
        /// </summary>
        public static void RegisterKeyDownListener(KeyCode key, Action action)
        {
            if (!keyDownListeners.ContainsKey(key)) keyDownListeners.Add(key, new List<Action>());
            keyDownListeners[key].Add(action);
        }

        /// <summary>
        /// Unregisters the given key down listener for the given key.
        /// </summary>
        public static bool UnregisterKeyDownListener(KeyCode key, Action action)
        {
            return keyDownListeners.ContainsKey(key) && keyDownListeners[key].Remove(action) && (keyDownListeners[key].Count != 0 || keyDownListeners.Remove(key));
        }

        /// <summary>
        /// Registers the provided action so that it will be triggered when the provided key is released.
        /// </summary>
        public static void RegisterKeyUpListener(KeyCode key, Action action)
        {
            if (!keyUpListeners.ContainsKey(key)) keyUpListeners.Add(key, new List<Action>());
            keyUpListeners[key].Add(action);
        }

        /// <summary>
        /// Unregisters the given key up listener for the given key.
        /// </summary>
        public static bool UnregisterKeyUpListener(KeyCode key, Action action)
        {
            return keyUpListeners.ContainsKey(key) && keyUpListeners[key].Remove(action) && (keyUpListeners[key].Count != 0 || keyUpListeners.Remove(key));
        }

        /// <summary>
        /// Determines if two <see cref="Item"/>s are able to stack together. Ignores matters of quantity.
        /// </summary>
        public static bool CanItemsStack(Item item1, Item item2)
        {
            return (ItemRegistry.GetTypeByID(item1.id) & ItemType.NONSTACKING) == ItemType.STACKING &&
                item1.id == item2.id && item1.corrupted == item2.corrupted &&
                ((item1.GetAllExtraData() == null && item2.GetAllExtraData() == null) || (item1.GetAllExtraData() != null && item2.GetAllExtraData() != null &&
                item1.GetAllExtraData().Count == item2.GetAllExtraData().Count &&
                item1.GetAllExtraData().All(keyValuePair => item2.GetAllExtraData().TryGetValue(keyValuePair.Key, out object value) && value.Equals(keyValuePair.Value))));
        }

        /// <summary>
        /// Creates a market stand.
        /// </summary>
        /// <param name="item">The item to sell</param>
        /// <param name="pos">The position to put the stand at</param>
        /// <param name="cost">The cost to buy the item</param>
        /// <param name="isBuild">If true, costs scrap metal. If false, costs world frags.</param>
        /// <param name="isCredits">If isBuild and isCredits are true, costs credits instead of scrap metal.</param>
        /// <param name="isTrophies">If isBuild, isCredits, and isTrophies are true, costs wealth trophies instead of credits.</param>
        public static GameObject CreateMarketStand(ItemInfo item, Vector2 pos, int cost, bool isBuild = true, bool isCredits = false, bool isTrophies = false)
        {
            GameObject shopStand = UnityEngine.Object.Instantiate(SceneInjector.BuildStand, SceneInjector.BuildStand.transform.parent);
            shopStand.transform.localPosition = new Vector3(pos.x, pos.y, SceneInjector.BuildStand.transform.position.z);
            shopStand.name = isBuild ? "buildStand" : "kylockeStand";
            KylockeStand standScript = shopStand.GetComponent<KylockeStand>();
            standScript.isTrophies = isTrophies;
            standScript.isCredits = isCredits;
            standScript.isBuild = isBuild;
            standScript.itemID = item.ID;
            standScript.cost = cost;
            standScript.currency.GetComponent<MeshRenderer>().material = isBuild ? isCredits ? isTrophies ? GetItemMaterial(59) : GetItemMaterial(52) : GetItemMaterial(57) : GetItemMaterial(51);
            standScript.icon.GetComponent<MeshRenderer>().material = item.Mat;
            standScript.txtName[0].text = item.GetName();
            standScript.txtName[1].text = standScript.txtName[0].text;
            standScript.txtCost[0].text = string.Empty + standScript.cost;
            standScript.txtCost[1].text = standScript.txtCost[0].text;
            return shopStand;
        }

        /// <summary>
        /// Displays a dialog box with the button 'Yes' and 'No' on it. One of the provided actions will be executed once a choice is made.
        /// </summary>
        public static void DisplayYesNoDialog(string text, Action onYes, Action onNo = null)
        {
            SceneInjector.ConfirmationText.text = text;
            SceneInjector.ConfirmationYesAction = onYes;
            SceneInjector.ConfirmationNoAction = onNo;
            SceneInjector.ConfirmationDialog.SetActive(true);
        }

        /// <summary>
        /// Displays a dialog box with the button 'OK' and 'Cancel' on it. One of the provided actions will be executed once a choice is made.
        /// </summary>
        public static void DisplayOKCancelDialog(string text, Action onOK, Action onCancel = null)
        {
            SceneInjector.ConfirmationText.text = text;
            SceneInjector.ConfirmationYesAction = onOK;
            SceneInjector.ConfirmationNoAction = onCancel;
            SceneInjector.ConfirmationYesText.text = "OK";
            SceneInjector.ConfirmationNoText.text = "Cancel";
            SceneInjector.ConfirmationDialog.SetActive(true);
        }

        /// <summary>
        /// Displays an information dialog box with just the button 'OK' on it.
        /// </summary>
        public static void DisplayInfoDialog(string text)
        {
            SceneInjector.ConfirmationText.text = text;
            SceneInjector.ConfirmationYesText.transform.parent.gameObject.SetActive(false);
            SceneInjector.ConfirmationNoText.text = "OK";
            SceneInjector.ConfirmationDialog.SetActive(true);
        }

        /// <summary>
        /// Closes the general-purpose dialog box, if it's open.
        /// </summary>
        public static void CloseDialog()
        {
            SceneInjector.ConfirmationDialog.SetActive(false);
            SceneInjector.ConfirmationText.text = "";
            SceneInjector.ConfirmationYesText.text = "Yes";
            SceneInjector.ConfirmationNoText.text = "No";
            SceneInjector.ConfirmationYesText.transform.parent.gameObject.SetActive(true);
            SceneInjector.ConfirmationNoText.transform.parent.gameObject.SetActive(true);
        }

        /// <summary>
        /// Creates an empty Item, used to represent the content of an empty slot.
        /// </summary>
        public static Item EmptyItem()
        {
            return new Item(0, 0, 0, 0, 0, new int[3], new int[3]);
        }

        /// <summary>
        /// Creates a copy of an Item.
        /// </summary>
        public static Item CopyItem(Item item)
        {
            Item copy = new Item(item.id, item.q, item.exp, item.tier, item.corrupted, item.aspect, item.aspectLvl);
            copy.SetAllExtraData(item.GetAllExtraData());
            return copy;
        }

        /// <summary>
        /// The base game transmits Items across the network using int arrays. This method converts an int array into an Item, and restores the Item's extra data in the process. Indexes 0-10 of the array are used for vanilla data, while indexes 11 and up are used for any and all extra data, and are encoded in a format that is not meant to be manually read.
        /// </summary>
        public static Item ConstructItemFromIntArray(int[] st)
        {
            Item item = new Item(st[0], st[1], st[2], st[3], st[4], new int[]
            {
                st[5],
                st[6],
                st[7]
            }, new int[]
            {
                st[8],
                st[9],
                st[10]
            });
            if (st.Length > 11)
            {
                item.DeserializeExtraData(Encoding.Default.GetString(st.Where((x, i) => i > 10).SelectMany(x => BitConverter.GetBytes(x)).ToArray()));
            }
            return item;
        }

        /// <summary>
        /// The base game transmits Items across the network using int arrays. This method converts an Item into an int array, and preserves the Item's extra data in the process. Indexes 0-10 of the array are used for vanilla data, while indexes 11 and up are used for any and all extra data, and are encoded in a format that is not meant to be manually read.
        /// </summary>
        public static int[] ConstructIntArrayFromItem(Item item)
        {
            byte[] bytes = Encoding.Default.GetBytes(item.SerializeExtraData());
            if (bytes.Length % 4 != 0) Array.Resize(ref bytes, bytes.Length + (4 - (bytes.Length % 4)));
            var size = bytes.Count() / sizeof(int);
            var extraData = new int[size];
            for (var index = 0; index < size; index++)
            {
                extraData[index] = BitConverter.ToInt32(bytes, index * sizeof(int));
            }
            int[] st = new int[11 + extraData.Length];
            st[0] = item.id;
            st[1] = item.q;
            st[2] = item.exp;
            st[3] = item.tier;
            st[4] = item.corrupted;
            st[5] = item.aspect[0];
            st[6] = item.aspect[1];
            st[7] = item.aspect[2];
            st[8] = item.aspectLvl[0];
            st[9] = item.aspectLvl[1];
            st[10] = item.aspectLvl[2];
            for (int i = 0; i < extraData.Length; i++)
            {
                st[i + 11] = extraData[i];
            }
            return st;
        }

        /// <summary>
        /// Returns an IEnumerator that does nothing. Use when you need to return an IEnumerator, but you don't want it to do anything and you can't use null.
        /// </summary>
        public static IEnumerator EmptyEnumerator()
        {
            yield break;
        }

        /// <summary>
        /// Use to spawn an item into the game world.
        /// You may notice that the vanilla game's source-code uses Resources.Load to spawn items. You should not use that.
        /// </summary>
        /// <param name="pos">The position to spawn the item at. Note that despite being a 2D game, Roguelands uses 3D space. That being said, the z-coordinate should nearly always be 0.</param>
        /// <param name="item">The item to spawn.</param>
        /// <param name="isChip">True to drop a chip instead of a normal item.</param>
        public static void SpawnItem(Vector3 pos, Item item, bool isChip = false)
        {
            if (!isChip)
            {
                int[] st = ConstructIntArrayFromItem(item);
                if (Network.isServer)
                {
                    ItemScript itemScript = ((GameObject)Network.Instantiate(Resources.Load("i"), pos, Quaternion.identity, 0)).GetComponent<ItemScript>();
                    itemScript.gameObject.GetComponent<NetworkView>().RPC("Init", RPCMode.AllBuffered, st);
                }
                else
                {
                    InstanceTracker.GameScript.GetComponent<NetworkView>().RPC("SpawnItem", RPCMode.Server, new object[]
                    {
                        st,
                        pos
                    });
                }
            }
            else
            {
                ItemScript itemScript = ((GameObject)Network.Instantiate(Resources.Load("i"), pos, Quaternion.identity, 0)).GetComponent<ItemScript>();
                itemScript.gameObject.GetComponent<NetworkView>().RPC("Chip", RPCMode.AllBuffered, item.id);
            }
        }

        /// <summary>
        /// Use to spawn an item into the local player's world.
        /// You may notice that the vanilla game's source-code uses Resources.Load to spawn items. You should not use that.
        /// </summary>
        /// <param name="pos">The position to spawn the item at. Note that despite being a 2D game, Roguelands uses 3D space. That being said, the z-coordinate should nearly always be 0.</param>
        /// <param name="item">The item to spawn.</param>
        /// <param name="isChip">True to drop a chip instead of a normal item.</param>
        public static ItemScript SpawnItemLocal(Vector3 pos, Item item, bool isChip = false)
        {
            if (!isChip)
            {
                int[] st = ConstructIntArrayFromItem(item);
                ItemScript itemScript = ((GameObject)UnityEngine.Object.Instantiate(Resources.Load("i3"), pos, Quaternion.identity)).GetComponent<ItemScript>();
                itemScript.SendMessage("InitL", st);
                if ((ItemRegistry.GetItem(item.id)?.Type & ItemType.LEVELING) == ItemType.LEVELING) itemScript.back.SetActive(true);
                return itemScript;
            }
            else
            {
                ItemScript itemScript = ((GameObject)UnityEngine.Object.Instantiate(Resources.Load("i3"), pos, Quaternion.identity)).GetComponent<ItemScript>();
                itemScript.SendMessage("ChipL", item.id);
                return itemScript;
            }
        }

        /// <summary>
        /// Use to spawn exp into the world.
        /// You may notice that the vanilla game's source-code uses Resources.Load to spawn exp. You should not use that.
        /// </summary>
        /// <param name="pos">The position to spawn the item at. Note that despite being a 2D game, Roguelands uses 3D space. That being said, the z-coordinate should nearly always be 0.</param>
        /// <param name="exp">The amount of exp points to spawn.</param>
        /// <param name="delay">The optional amount of time to wait between spawning each exp orb.</param>
        public static void SpawnExp(Vector3 pos, int exp, float delay = 0)
        {
            if (delay > 0)
            {
                InstanceTracker.GameScript.StartCoroutine(SpawnEXPRoutine(pos, exp, delay));
                return;
            }
            if (exp >= 10000)
            {
                int rem = exp % 20;
                if (rem > 0)
                {
                    exp -= rem;
                    while (rem > 0)
                    {
                        if (rem - 15 > 0)
                        {
                            rem -= 15;
                            UnityEngine.Object.Instantiate(Resources.Load("exp/exp3"), pos, Quaternion.identity);
                        }
                        else if (rem - 10 > 0)
                        {
                            rem -= 10;
                            UnityEngine.Object.Instantiate(Resources.Load("exp/exp2"), pos, Quaternion.identity);
                        }
                        else if (rem - 5 > 0)
                        {
                            rem -= 5;
                            UnityEngine.Object.Instantiate(Resources.Load("exp/exp1"), pos, Quaternion.identity);
                        }
                        else
                        {
                            rem--;
                            UnityEngine.Object.Instantiate(Resources.Load("exp/exp0"), pos, Quaternion.identity);
                        }
                    }
                }
                for (int i = 0;i < 20; i++)
                {
                    ((GameObject)UnityEngine.Object.Instantiate(Resources.Load("exp/expCustom"), pos, Quaternion.identity)).GetComponentInChildren<EXPScript>().exp = exp / 20;
                }
            }
            else
            {
                while (exp > 0)
                {
                    if (exp - 1000 > 0)
                    {
                        exp -= 1000;
                        UnityEngine.Object.Instantiate(Resources.Load("exp/exp7"), pos, Quaternion.identity);
                    }
                    else if (exp - 250 > 0)
                    {
                        exp -= 250;
                        UnityEngine.Object.Instantiate(Resources.Load("exp/exp6"), pos, Quaternion.identity);
                    }
                    else if (exp - 60 > 0)
                    {
                        exp -= 60;
                        UnityEngine.Object.Instantiate(Resources.Load("exp/exp5"), pos, Quaternion.identity);
                    }
                    else if (exp - 20 > 0)
                    {
                        exp -= 20;
                        UnityEngine.Object.Instantiate(Resources.Load("exp/exp4"), pos, Quaternion.identity);
                    }
                    else if (exp - 15 > 0)
                    {
                        exp -= 15;
                        UnityEngine.Object.Instantiate(Resources.Load("exp/exp3"), pos, Quaternion.identity);
                    }
                    else if (exp - 10 > 0)
                    {
                        exp -= 10;
                        UnityEngine.Object.Instantiate(Resources.Load("exp/exp2"), pos, Quaternion.identity);
                    }
                    else if (exp - 5 > 0)
                    {
                        exp -= 5;
                        UnityEngine.Object.Instantiate(Resources.Load("exp/exp1"), pos, Quaternion.identity);
                    }
                    else
                    {
                        exp--;
                        UnityEngine.Object.Instantiate(Resources.Load("exp/exp0"), pos, Quaternion.identity);
                    }
                }
            }
        }
        
        private static IEnumerator SpawnEXPRoutine(Vector3 pos, int exp, float delay)
        {
            if (exp >= 10000)
            {
                int rem = exp % 20;
                if (rem > 0)
                {
                    exp -= rem;
                    while (rem > 0)
                    {
                        if (rem - 15 > 0)
                        {
                            rem -= 15;
                            GadgetCore.CoreLogger.LogConsole("15");
                            UnityEngine.Object.Instantiate(Resources.Load("exp/exp3"), pos, Quaternion.identity);
                        }
                        else if (rem - 10 > 0)
                        {
                            rem -= 10;
                            GadgetCore.CoreLogger.LogConsole("10");
                            UnityEngine.Object.Instantiate(Resources.Load("exp/exp2"), pos, Quaternion.identity);
                        }
                        else if (rem - 5 > 0)
                        {
                            rem -= 5;
                            GadgetCore.CoreLogger.LogConsole("5");
                            UnityEngine.Object.Instantiate(Resources.Load("exp/exp1"), pos, Quaternion.identity);
                        }
                        else
                        {
                            rem--;
                            GadgetCore.CoreLogger.LogConsole("1");
                            UnityEngine.Object.Instantiate(Resources.Load("exp/exp0"), pos, Quaternion.identity);
                        }
                        yield return new WaitForSeconds(delay);
                    }
                }
                for (int i = 0; i < 20; i++)
                {
                    ((GameObject)UnityEngine.Object.Instantiate(Resources.Load("exp/expCustom"), pos, Quaternion.identity)).GetComponentInChildren<EXPScript>().exp = exp / 20;
                    GadgetCore.CoreLogger.LogConsole(exp / 20);
                    yield return new WaitForSeconds(delay);
                }
            }
            else
            {
                while (exp > 0)
                {
                    if (exp - 1000 > 0)
                    {
                        exp -= 1000;
                        UnityEngine.Object.Instantiate(Resources.Load("exp/exp7"), pos, Quaternion.identity);
                    }
                    else if (exp - 250 > 0)
                    {
                        exp -= 250;
                        UnityEngine.Object.Instantiate(Resources.Load("exp/exp6"), pos, Quaternion.identity);
                    }
                    else if (exp - 60 > 0)
                    {
                        exp -= 60;
                        UnityEngine.Object.Instantiate(Resources.Load("exp/exp5"), pos, Quaternion.identity);
                    }
                    else if (exp - 20 > 0)
                    {
                        exp -= 20;
                        UnityEngine.Object.Instantiate(Resources.Load("exp/exp4"), pos, Quaternion.identity);
                    }
                    else if (exp - 15 > 0)
                    {
                        exp -= 15;
                        UnityEngine.Object.Instantiate(Resources.Load("exp/exp3"), pos, Quaternion.identity);
                    }
                    else if (exp - 10 > 0)
                    {
                        exp -= 10;
                        UnityEngine.Object.Instantiate(Resources.Load("exp/exp2"), pos, Quaternion.identity);
                    }
                    else if (exp - 5 > 0)
                    {
                        exp -= 5;
                        UnityEngine.Object.Instantiate(Resources.Load("exp/exp1"), pos, Quaternion.identity);
                    }
                    else
                    {
                        exp--;
                        UnityEngine.Object.Instantiate(Resources.Load("exp/exp0"), pos, Quaternion.identity);
                    }
                }
                yield return new WaitForSeconds(delay);
            }
        }

        /// <summary>
        /// Use to spawn an item into the game world as if dropped by this player.
        /// You may notice that the vanilla game's source-code uses Resources.Load to spawn items. You should not use that.
        /// </summary>
        /// <param name="pos">The position to spawn the item at. Note that despite being a 2D game, Roguelands uses 3D space. That being said, the z-coordinate should nearly always be 0.</param>
        /// <param name="item">The item to spawn.</param>
        /// <param name="isChip">True to drop a chip instead of a normal item.</param>
        public static ItemScript DropItem(Vector3 pos, Item item, bool isChip = false)
        {
            if (!isChip)
            {
                int[] st = ConstructIntArrayFromItem(item);
                ItemScript itemScript = ((GameObject)Network.Instantiate(Resources.Load("i2"), pos, Quaternion.identity, 0)).GetComponent<ItemScript>();
                itemScript.gameObject.GetComponent<NetworkView>().RPC("Init", RPCMode.AllBuffered, st);
                if ((ItemRegistry.GetItem(item.id)?.Type & ItemType.LEVELING) == ItemType.LEVELING) itemScript.back.SetActive(true);
                return itemScript;
            }
            else
            {
                ItemScript itemScript = ((GameObject)Network.Instantiate(Resources.Load("i2"), pos, Quaternion.identity, 0)).GetComponent<ItemScript>();
                itemScript.gameObject.GetComponent<NetworkView>().RPC("Chip", RPCMode.AllBuffered, item.id);
                return itemScript;
            }
        }

        /// <summary>
        /// Use to spawn an item into the game world as if dropped by this player, but only spawns it locally.
        /// You may notice that the vanilla game's source-code uses Resources.Load to spawn items. You should not use that.
        /// </summary>
        /// <param name="pos">The position to spawn the item at. Note that despite being a 2D game, Roguelands uses 3D space. That being said, the z-coordinate should nearly always be 0.</param>
        /// <param name="item">The item to spawn.</param>
        /// <param name="isChip">True to drop a chip instead of a normal item.</param>
        public static ItemScript DropItemLocal(Vector3 pos, Item item, bool isChip = false)
        {
            if (!isChip)
            {
                int[] st = ConstructIntArrayFromItem(item);
                ItemScript itemScript = ((GameObject)UnityEngine.Object.Instantiate(Resources.Load("i2"), pos, Quaternion.identity)).GetComponent<ItemScript>();
                itemScript.SendMessage("InitL", st);
                if ((ItemRegistry.GetItem(item.id)?.Type & ItemType.LEVELING) == ItemType.LEVELING) itemScript.back.SetActive(true);
                return itemScript;
            }
            else
            {
                ItemScript itemScript = ((GameObject)UnityEngine.Object.Instantiate(Resources.Load("i2"), pos, Quaternion.identity)).GetComponent<ItemScript>();
                itemScript.SendMessage("ChipL", item.id);
                return itemScript;
            }
        }

        /// <summary>
        /// Returns the position of the mouse cursor.
        /// </summary>
        public static Vector3 GetCursorPos()
        {
            if (Menuu.usingController)
            {
                return new Vector3(ControllerCursor.staticX, ControllerCursor.staticY, 0f);
            }
            else
            {
                Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                return new Vector3(pos.x, pos.y, 0);
            }
        }

        /// <summary>
        /// Registers a custom RPC that can be later called using <see cref="CallCustomRPC(string, RPCMode, object[])"/>
        /// </summary>
        public static void RegisterCustomRPC(string name, Action<object[]> rpc)
        {
            if (!Registry.registeringVanilla && Registry.modRegistering < 0) throw new InvalidOperationException("Data registration may only be performed by the Initialize method of a Gadget!");
            if (name == null) throw new ArgumentNullException("name");
            if (rpc == null) throw new ArgumentNullException("rpc");
            customRPCs.Add(name, rpc);
        }

        /// <summary>
        /// Calls a custom RPC that was previously registered with <see cref="RegisterCustomRPC(string, Action{object[]})"/>
        /// </summary>
        public static void CallCustomRPC(string name, RPCMode mode, params object[] args)
        {
            RPCHooks.Singleton.CallGeneral(name, mode, args);
        }

        /// <summary>
        /// Calls a custom RPC that was previously registered with <see cref="RegisterCustomRPC(string, Action{object[]})"/>
        /// </summary>
        public static void CallCustomRPC(string name, NetworkPlayer target, params object[] args)
        {
            RPCHooks.Singleton.CallGeneral(name, target, args);
        }

        /// <summary>
        /// Convenience method to override a Material resource's main texture. May only be called from the Initialize method of a Gadget.
        /// </summary>
        /// <param name="materialPath">The pseudo-file-path to the material to override.</param>
        /// <param name="tex">The new texture to use for the Material override.</param>
        public static void ReplaceMatResourceTex(string materialPath, Texture2D tex)
        {
            Material mat = GetResource(materialPath) as Material;
            mat.mainTexture = tex;
            AddCustomResource(materialPath, mat);
        }

        /// <summary>
        /// Use to manually add new resources to the game, or overwrite existing ones. May only be called from the Initialize method of a Gadget.
        /// </summary>
        /// <param name="path">The pseudo-file-path to place the resource on.</param>
        /// <param name="resource">The resource to register.</param>
        public static void AddCustomResource(string path, UnityEngine.Object resource)
        {
            if (!Registry.registeringVanilla && Registry.modRegistering < 0) throw new InvalidOperationException("Data registration may only be performed by the Initialize method of a Gadget!");
            UnityEngine.Object.DontDestroyOnLoad(resource);
            resource.hideFlags |= HideFlags.HideAndDontSave;
            if (resource is GameObject)
            {
                if (!(resource as GameObject).activeSelf)
                {
                    resource.hideFlags |= HideFlags.HideInInspector;
                }
                else
                {
                    (resource as GameObject).SetActive(false);
                }
            }
            resources[path] = resource;
            resourcePaths[resource.GetInstanceID()] = path;
            if (!modResources.ContainsKey(Registry.modRegistering)) modResources.Add(Registry.modRegistering, new List<int>());
            if (!modResources[Registry.modRegistering].Contains(resource.GetInstanceID())) modResources[Registry.modRegistering].Add(resource.GetInstanceID());
        }

        internal static void RemoveModResources(int modID)
        {
            if (!modResources.ContainsKey(modID)) return;
            foreach (int resourceID in modResources[modID])
            {
                resources.Remove(resourcePaths[resourceID]);
                resourcePaths.Remove(resourceID);
            }
            modResources.Remove(modID);
        }

        /// <summary>
        /// Use to register a texture for the tile spritesheet. The texture must be 32x32 in size. You probably shouldn't use this yourself - it is automatically called by <see cref="TileInfo"/> after registration. May only be called from <see cref="Gadget.Initialize"/>.
        /// </summary>
        /// <param name="sprite">The Texture2D to register to the spritesheet</param>
        public static SpriteSheetEntry AddTextureToSheet(Texture2D sprite)
        {
            if (!Registry.registeringVanilla && Registry.modRegistering < 0) throw new InvalidOperationException("Data registration may only be performed by the Initialize method of a Gadget!");
            SpriteSheetEntry entry = new SpriteSheetEntry(sprite, spriteSheetSprites.Count);
            spriteSheetSprites.Add(entry);
            return entry;
        }

        /// <summary>
        /// Gets the name of the item with the given ID. Easier than using reflection to call GetItemName on GameScript.
        /// </summary>
        public static string GetItemName(int ID)
        {
            return GetItemNameMethod.Invoke(InstanceTracker.GameScript, new object[] { ID }) as string;
        }

        /// <summary>
        /// Gets the description of the item with the given ID. Easier than using reflection to call GetItemDesc on GameScript.
        /// </summary>
        public static string GetItemDesc(int ID)
        {
            return GetItemDescMethod.Invoke(InstanceTracker.GameScript, new object[] { ID }) as string;
        }

        /// <summary>
        /// Gets the name of the chip with the given ID. Easier than using reflection to call GetChipName on GameScript.
        /// </summary>
        public static string GetChipName(int ID)
        {
            return GetChipNameMethod.Invoke(InstanceTracker.GameScript, new object[] { ID }) as string;
        }

        /// <summary>
        /// Gets the description of the chip with the given ID. Easier than using reflection to call GetChipDesc on GameScript.
        /// </summary>
        public static string GetChipDesc(int ID)
        {
            return GetChipDescMethod.Invoke(InstanceTracker.GameScript, new object[] { ID }) as string;
        }


        /// <summary>
        /// Adds a recipe to the Emblem Forge.
        /// </summary>
        public static void AddEmblemRecipe(int lootID, int emblemID, int lootCount = 10)
        {
            emblemForgeRecipes[lootID] = Tuple.Create(emblemID, lootCount);
        }

        /// <summary>
        /// Gets the level of the given piece of gear.
        /// </summary>
        public static int GetGearLevel(Item item)
        {
            int num = 1;
            int num2 = 100;
            int i = item.exp;
            int num3 = 0;
            while (i > num2)
            {
                i -= num2;
                num3++;
                num2 += 75 + num3 * 100;
                num++;
                if (num == 10)
                {
                    break;
                }
            }
            if (num > 10)
            {
                num = 10;
            }
            return num;
        }

        /// <summary>
        /// Gets the true EquipStats of the item with the given ID, before any stat modifiers at all are applied.
        /// </summary>
        public static EquipStats GetTrueGearBaseStats(int ID)
        {
            if (ItemRegistry.Singleton.HasEntry(ID))
            {
                return ItemRegistry.Singleton.GetEntry(ID).Stats;
            }
            return new EquipStats(GetGearBaseStatsMethod.Invoke(InstanceTracker.GameScript, new object[] { ID }) as int[]);
        }

        /// <summary>
        /// Gets the EquipStats of the given item. Applies 'Base' stat modifiers to the stats.
        /// </summary>
        public static EquipStats GetGearBaseStats(Item item)
        {
            EquipStats baseStats = GetTrueGearBaseStats(item.id);
            EquipStats stats = baseStats;
            if (statModifiers.ContainsKey(StatModifierType.BaseFlat))
            {
                foreach (Tuple<StatModifier, int> modifier in statModifiers[StatModifierType.BaseFlat])
                {
                    stats += modifier.Item1(item);
                }
            }
            if (statModifiers.ContainsKey(StatModifierType.BaseAddMult))
            {
                foreach (Tuple<StatModifier, int> modifier in statModifiers[StatModifierType.BaseAddMult])
                {
                    stats += baseStats * modifier.Item1(item);
                }
            }
            if (statModifiers.ContainsKey(StatModifierType.BaseExpMult))
            {
                foreach (Tuple<StatModifier, int> modifier in statModifiers[StatModifierType.BaseExpMult])
                {
                    stats *= modifier.Item1(item);
                }
            }
            return stats;
        }

        /// <summary>
        /// Gets the EquipStats of the given item. This accounts for all factors that effect the item's stats.
        /// </summary>
        public static EquipStats GetGearStats(Item item)
        {
            ItemInfo itemInfo = ItemRegistry.GetItem(item.id);
            bool itemLevels = (itemInfo?.Type & ItemType.LEVELING) == ItemType.LEVELING && (itemInfo?.Type & ItemType.EQUIP_MASK) != (ItemType.DROID & ItemType.EQUIP_MASK);
            int level = itemLevels ? GetGearLevel(item) : 1;
            EquipStats baseStats = GetGearBaseStats(item);
            EquipStats stats = baseStats * level;
            if ((itemInfo?.Type & ItemType.EQUIP_MASK) != (ItemType.DROID & ItemType.EQUIP_MASK)) for (int j = 0; j < 6; j++)
            {
                if (stats.GetByIndex(j) > 0)
                {
                    stats.AddByIndex(j, item.tier * 3);
                }
            }
            for (int i = 0; i < item.aspect.Length; i++)
            {
                ItemInfo gearMod = ItemRegistry.Singleton.GetEntry(item.aspect[i]);
                if (gearMod == null && (item.aspect[i] < 201 || item.aspect[i] > 206)) continue;
                for (int j = 0; j < 6; j++)
                {
                    if (gearMod != null && gearMod.Stats != EquipStats.NONE)
                    {
                        stats.AddByIndex(j, gearMod.Stats.GetByIndex(j));
                    }
                    else if (item.aspect[i] - 200 == j + 1)
                    {
                        stats.AddByIndex(j, item.aspectLvl[i]);
                    }
                }
            }
            if (statModifiers.ContainsKey(StatModifierType.Flat))
            {
                foreach (Tuple<StatModifier, int> modifier in statModifiers[StatModifierType.Flat])
                {
                    stats += modifier.Item1(item);
                }
            }
            if (statModifiers.ContainsKey(StatModifierType.AddMult))
            {
                foreach (Tuple<StatModifier, int> modifier in statModifiers[StatModifierType.AddMult])
                {
                    stats += baseStats * modifier.Item1(item);
                }
            }
            if (statModifiers.ContainsKey(StatModifierType.ExpMult))
            {
                foreach (Tuple<StatModifier, int> modifier in statModifiers[StatModifierType.ExpMult])
                {
                    stats *= modifier.Item1(item);
                }
            }
            if (itemLevels)
            {
                if (statModifiers.ContainsKey(StatModifierType.LevelFlat))
                {
                    foreach (Tuple<StatModifier, int> modifier in statModifiers[StatModifierType.LevelFlat])
                    {
                        stats += modifier.Item1(item) * level;
                    }
                }
                if (statModifiers.ContainsKey(StatModifierType.LevelAddMult))
                {
                    foreach (Tuple<StatModifier, int> modifier in statModifiers[StatModifierType.LevelAddMult])
                    {
                        stats += baseStats * modifier.Item1(item) * level;
                    }
                }
                if (statModifiers.ContainsKey(StatModifierType.LevelExpMult))
                {
                    foreach (Tuple<StatModifier, int> modifier in statModifiers[StatModifierType.LevelExpMult])
                    {
                        EquipStatsDouble multiplier = modifier.Item1(item);
                        for (int i = 0;i < level;i++) stats *= multiplier;
                    }
                }
            }
            baseStats = stats;
            if (statModifiers.ContainsKey(StatModifierType.FinalFlat))
            {
                foreach (Tuple<StatModifier, int> modifier in statModifiers[StatModifierType.FinalFlat])
                {
                    stats += modifier.Item1(item);
                }
            }
            if (statModifiers.ContainsKey(StatModifierType.FinalAddMult))
            {
                foreach (Tuple<StatModifier, int> modifier in statModifiers[StatModifierType.FinalAddMult])
                {
                    stats += baseStats * modifier.Item1(item);
                }
            }
            if (statModifiers.ContainsKey(StatModifierType.FinalExpMult))
            {
                foreach (Tuple<StatModifier, int> modifier in statModifiers[StatModifierType.FinalExpMult])
                {
                    stats *= modifier.Item1(item);
                }
            }
            return stats;
        }

        /// <summary>
        /// Registers an gear stats modifier. Can be used to change how one or more pieces of gear's stats are calculated.
        /// </summary>
        /// <param name="modifier"></param>
        /// <param name="type"></param>
        public static void RegisterStatModifier(StatModifier modifier, StatModifierType type)
        {
            if (!Registry.registeringVanilla && Registry.modRegistering < 0) throw new InvalidOperationException("Stat modifiers may only be registered by the Initialize method of a Gadget!");
            if (!statModifiers.ContainsKey(type)) statModifiers.Add(type, new List<Tuple<StatModifier, int>>());
            statModifiers[type].Add(Tuple.Create(modifier, Registry.modRegistering));
        }

        internal static void UnregisterStatModifiers(int modID)
        {
            foreach (StatModifierType type in statModifiers.Keys.ToList())
            {
                foreach (Tuple<StatModifier, int> modifier in statModifiers[type].Where(x => x.Item2 == modID).ToList()) statModifiers[type].Remove(modifier);
                if (statModifiers[type].Count == 0) statModifiers.Remove(type);
            }
        }

        /// <summary>
        /// Delegate used for stat modifiers.
        /// </summary>
        /// <param name="item">The item who's stats are being modified.</param>
        public delegate EquipStatsDouble StatModifier(Item item);

        /// <summary>
        /// Use to check if there is a resource registered at the specified path. This includes resources registered by the base game.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsResourceRegistered(string path)
        {
            return Resources.Load(path) != null;
        }

        /// <summary>
        /// Use to check if there is a custom resource registered at the specified path. This does not include resources registered by the base game.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsCustomResourceRegistered(string path)
        {
            return resources.ContainsKey(path);
        }

        /// <summary>
        /// Gets the custom resource at the specified path.
        /// </summary>
        public static UnityEngine.Object GetCustomResource(string path)
        {
            return resources[path];
        }

        /// <summary>
        /// Gets the resource at the specified path. This can retrieve vanilla or custom resources.
        /// </summary>
        public static UnityEngine.Object GetResource(string path)
        {
            return (Material)Resources.Load(path);
        }

        /// <summary>
        /// Gets the item material with the specified ID.
        /// </summary>
        public static Material GetItemMaterial(int ID)
        {
            return (Material)Resources.Load("i/i" + ID);
        }

        /// <summary>
        /// Gets the tile material with the specified ID.
        /// </summary>
        public static Material GetTileMaterial(int ID)
        {
            return (Material)Resources.Load("construct/c" + ID);
        }

        /// <summary>
        /// Gets the chip material with the specified ID.
        /// </summary>
        public static Material GetChipMaterial(int ID)
        {
            return (Material)Resources.Load("cc/cc" + ID);
        }

        /// <summary>
        /// Gets the weapon material with the specified ID. Used for when the weapon is equipped.
        /// </summary>
        public static Material GetWeaponMaterial(int ID)
        {
            return (Material)Resources.Load("ie/ie" + ID);
        }

        /// <summary>
        /// Gets the offhand material with the specified ID. Used for when the offhand item is equipped.
        /// </summary>
        public static Material GetOffhandMaterial(int ID)
        {
            return (Material)Resources.Load("o/o" + ID);
        }

        /// <summary>
        /// Gets the head material with the specified ID. Used for when a helmet is equipped.
        /// </summary>
        public static Material GetHeadMaterial(int ID)
        {
            return (Material)Resources.Load("h/h" + ID);
        }

        /// <summary>
        /// Gets the body material with the specified ID. Used for when an armor is equipped, or for the worn outfit if no armor is equipped.
        /// </summary>
        public static Material GetBodyMaterial(int ID)
        {
            return (Material)Resources.Load("b/b" + ID);
        }

        /// <summary>
        /// Gets the arm material with the specified ID. Used for when an armor is equipped, or for the worn outfit if no armor is equipped.
        /// </summary>
        public static Material GetArmMaterial(int ID)
        {
            return (Material)Resources.Load("a/a" + ID);
        }

        /// <summary>
        /// Gets the droid head material with the specified ID. Used for the head of a droid, which is animated seperately from the body.
        /// </summary>
        public static Material GetDroidHeadMaterial(int ID)
        {
            return (Material)Resources.Load("droid/d" + ID + "h");
        }

        /// <summary>
        /// Gets the droid body material with the specified ID. Used for the body of a droid, which is animated seperately from the head.
        /// </summary>
        public static Material GetDroidBodyMaterial(int ID)
        {
            return (Material)Resources.Load("droid/d" + ID + "b");
        }

        /// <summary>
        /// Gets the race head material with the specified ID and variant. Used for when no helmet is worn.
        /// </summary>
        public static Material GetRaceMaterial(int ID, int variant)
        {
            return (Material)Resources.Load("r/r" + ID + "v" + (variant - 1));
        }

        /// <summary>
        /// Gets the world sign material with the specified world ID. This is the colored bar that is shown inside of portals.
        /// </summary>
        public static Material GetSignMaterial(int ID)
        {
            return (Material)Resources.Load("sign/sign" + ID);
        }

        /// <summary>
        /// Gets the terrain side material with the specified world ID. This is used to fill in each side of a chunk that doesn't lead anywhere.
        /// </summary>
        public static Material GetTerrainSideMaterial(int ID, bool vertical)
        {
            return (Material)Resources.Load("side/side" + (vertical ? "V" : "H") + ID);
        }

        /// <summary>
        /// Gets the terrain zone material with the specified world ID. This represents the four L-shaped corners of a terrain chunk. Does not include the sides, or the mid-chunk.
        /// </summary>
        public static Material GetTerrainZoneMaterial(int ID)
        {
            return (Material)Resources.Load("z/z" + ID);
        }

        /// <summary>
        /// Gets the terrain entrance material with the specified world ID. This is the entrance room of a world. It is flipped and re-used for the exit as well.
        /// </summary>
        public static Material GetTerrainEntranceMaterial(int ID)
        {
            return (Material)Resources.Load("z/entrance" + ID);
        }

        /// <summary>
        /// Gets the terrain mid-chunk material with the specified world ID. This is the chunk of terrain in the middle of a room. Has a variant with an opening in the middle, and one without.
        /// </summary>
        public static Material GetTerrainMidChunkMaterial(int ID, bool opening)
        {
            return (Material)Resources.Load("z/midchunk" + (opening ? 1 : 0) + "b" + ID);
        }

        /// <summary>
        /// Gets the world background material with the specified world ID and depth index. There are four of these for each world, and they are used to draw the world's background. The depth index goes from 0 to 3 - a higher index indicates farther back into the background, and generally means a darker-colored material.
        /// </summary>
        public static Material GetWorldBackgroundMaterial(int ID, int depthIndex)
        {
            return (Material)Resources.Load("bg/b" + ID + "bg" + depthIndex);
        }

        /// <summary>
        /// Gets the world parallax material with the specified world ID. This is a vertical color gradient used to create the background parallax effect.
        /// </summary>
        public static Material GetWorldParallaxMaterial(int ID)
        {
            return (Material)Resources.Load("par/parallax" + ID);
        }

        /// <summary>
        /// Gets the faction flag material with the specified ID. This refers to the allegiance icon shown in the character creation screen. In vanilla, it is either The Galactic Fleet, the Starlight Rebellion, the Church of Faust, or the Gray Enigma. There are two additional unused flags with the IDs of 4 and 5: 4 is for the Junkbelt Mercenaries, and 5 is for the Droidtech Enterprise.
        /// </summary>
        public static Material GetFactionFlagMaterial(int ID)
        {
            return (Material)Resources.Load("flag/flag" + ID);
        }

        /// <summary>
        /// Gets the difficulty flag material with the specified ID. This refers to the difficulty icon shown in the character creation screen. In vanilla, it is either Standard or Ironman. For some reason, IDs 2 through 5 are also used by the vanilla game, and are copies of the 'Standard' flag. (ID 0)
        /// </summary>
        public static Material GetDifficultyFlagMaterial(int ID)
        {
            return (Material)Resources.Load("flag/prof" + ID);
        }

        /// <summary>
        /// Gets the chat portrait material with the specified ID. This is the character portrait shown when in dialog with an NPC, and the ID is not necessarily related to the NPC being spoken to in any way, although NPCs added with Gadget Core will have the same portrait ID.
        /// </summary>
        public static Material GetChatPortraitMaterial(int ID)
        {
            return (Material)Resources.Load("mat/por/portrait" + ID);
        }

        /// <summary>
        /// Gets the miscellaneous material with the specified name. This is used for any material that does not fit into another catagory. In vanilla, this includes menus and UI elements, opened chests, and the planet icons as shown in the planet selector, among other things.
        /// </summary>
        public static Material GetMiscellaneousMaterial(string name)
        {
            return (Material)Resources.Load("mat/" + name);
        }

        /// <summary>
        /// Gets the prop with the specified ID. A Prop is the in-world object representing any ship tile that has custom behavior. In the vanilla game, this is the engine blocks, the lamp blocks, and the spawner block, as well as all of the cards and badges. Keep in mind that interactable objects have their props saved as NPCs, for whatever reason.
        /// </summary>
        public static GameObject GetPropResource(int ID)
        {
            return (GameObject)Resources.Load("prop/" + ID);
        }

        /// <summary>
        /// Gets the entity with the specified name.
        /// </summary>
        public static GameObject GetEntityResource(string name)
        {
            return (GameObject)Resources.Load("e/" + name);
        }

        /// <summary>
        /// Gets the projectile with the specified name. Note that this does not include weapon projectiles.
        /// </summary>
        public static GameObject GetProjectileResource(string name)
        {
            return (GameObject)Resources.Load("proj/" + name);
        }

        /// <summary>
        /// Gets the special projectile with the specified name.
        /// </summary>
        public static GameObject GetSpecialProjectileResource(string name)
        {
            return (GameObject)Resources.Load("spec/" + name);
        }

        /// <summary>
        /// Gets the weapon projectile with the specified ID. These refer specifically to projectiles fired by the player's weapon, and the ID is that weapon's ID.
        /// </summary>
        public static GameObject GetWeaponProjectileResource(int ID)
        {
            return (GameObject)Resources.Load("proj/shot" + ID);
        }

        /// <summary>
        /// Gets the hazard with the specified name. Note that some enemy projectiles are actually hazards, such as the blade-like projectiles summoned by a few different enemies.
        /// </summary>
        public static GameObject GetHazardResource(string name)
        {
            return (GameObject)Resources.Load("haz/" + name);
        }

        /// <summary>
        /// Gets the object with the specified name. These are the objects found within worlds and towns, meaning anything other than entities and objectives. Objectives are unique, and can be loaded with Resources.Load("objective/objective1")
        /// </summary>
        public static GameObject GetObjectResource(string name)
        {
            return (GameObject)Resources.Load("obj/" + name);
        }

        /// <summary>
        /// Gets the npc with the specified name. Note that this also includes interactable structures, but not the ones that can be picked up and placed in your ship.
        /// </summary>
        public static GameObject GetNPCResource(string name)
        {
            return (GameObject)Resources.Load("npc/" + name);
        }

        /// <summary>
        /// Gets the placeable npc with the specified ID. Note that this refers to interactable structures that can be picked up and placed in your ship.
        /// </summary>
        public static GameObject GetPlaceableNPCResource(int ID)
        {
            return (GameObject)Resources.Load("npc/npc" + ID);
        }

        /// <summary>
        /// Gets the audio clip with the specified name. Note that the sounds made by weapons should be accessed using GetItemAudioClip
        /// </summary>
        public static AudioClip GetSound(string name)
        {
            return (AudioClip)Resources.Load("au/" + name);
        }

        /// <summary>
        /// Gets the attack audio clip with the specified ID. This is the sound played when a weapon is used.
        /// </summary>
        public static AudioClip GetAttackSound(int ID)
        {
            return (AudioClip)Resources.Load("au/i/i" + ID);
        }

        /// <summary>
        /// Loads an image file from your Assets folder as a Texture2D. Assumes a file extension of .png if one is not specified. Returns null if no file with the given name was found.
        /// </summary>
        public static Texture2D LoadTexture2D(string file, bool shared = false)
        {
            if (file.IndexOf('.') == -1) file += ".png";
            GadgetMod mod = GadgetMods.GetModByAssembly(Assembly.GetCallingAssembly());
            string modName = mod.Name ?? "GadgetCore";
            if (cachedTexes.ContainsKey(modName + ":" + file))
            {
                return cachedTexes[modName + ":" + file];
            }
            Stream stream;
            if (File.Exists(Path.Combine(Path.Combine(GadgetPaths.AssetsPath, modName), file)))
            {
                stream = File.OpenRead(Path.Combine(Path.Combine(GadgetPaths.AssetsPath, modName), file));
            }
            else if (mod?.HasModFile(Path.Combine("Assets", file)) ?? false)
            {
                stream = mod.ReadModFile(Path.Combine("Assets", file));
            }
            else
            {
                return null;
            }
            byte[] fileData = new byte[stream.Length];
            stream.Read(fileData, 0, fileData.Length);
            stream.Dispose();
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);
            tex.filterMode = FilterMode.Point;
            cachedTexes.Add(modName + ":" + file, tex);
            return tex;
        }

        /// <summary>
        /// Loads an audio file from your Assets folder as an AudioClip. Assumes a file extension of .wav if one is not specified. Returns null if no file with the given name was found.
        /// </summary>
        public static AudioClip LoadAudioClip(string file, bool shared = false)
        {
            if (file.IndexOf('.') == -1) file += ".wav";
            GadgetMod mod = GadgetMods.GetModByAssembly(Assembly.GetCallingAssembly());
            if (cachedAudioClips.ContainsKey(mod.Name + ":" + file))
            {
                return cachedAudioClips[mod.Name + ":" + file];
            }
            string filePath = Path.Combine("Assets", file);
            if (mod.HasModFile(filePath))
            {
                using (GadgetModFile modFile = mod.GetModFile(filePath))
                {
                    using (WWW www = new WWW("file:///" + modFile.FilePath))
                    {
                        AudioClip clip = www.GetAudioClip(true, false);
                        cachedAudioClips.Add(mod.Name + ":" + file, clip);
                        return clip;
                    }
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Loads an obj-format mesh from your Assets folder as a Mesh. Assumes a file extension of .obj if one is not specified. Returns null if no file with the given name was found.
        /// </summary>
        public static Mesh LoadObjMesh(string file, bool shared = false)
        {
            if (file.IndexOf('.') == -1) file += ".wav";
            GadgetMod mod = GadgetMods.GetModByAssembly(Assembly.GetCallingAssembly());
            if (cachedMeshes.ContainsKey(mod.Name + ":" + file))
            {
                return cachedMeshes[mod.Name + ":" + file];
            }
            string filePath = Path.Combine("Assets", file);
            if (mod.HasModFile(filePath))
            {
                using (Stream modFile = mod.ReadModFile(filePath))
                using (StreamReader reader = new StreamReader(mod.ReadModFile(filePath)))
                {
                    Mesh mesh = ObjImporter.ImportMesh(reader.ReadToEnd());
                    cachedMeshes.Add(mod.Name + ":" + file, mesh);
                    return mesh;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Loads an AssetBundle from your Assets folder. Returns null if no file with the given name was found.
        /// </summary>
        public static AssetBundle LoadAssetBundle(string file, bool shared = false)
        {
            GadgetMod mod = GadgetMods.GetModByAssembly(Assembly.GetCallingAssembly());
            if (cachedBundles.ContainsKey(mod.Name + ":" + file))
            {
                return cachedBundles[mod.Name + ":" + file];
            }
            string filePath = Path.Combine("Assets", file);
            if (mod.HasModFile(filePath))
            {
                Stream stream = mod.ReadModFile(filePath);
                byte[] fileData = new byte[stream.Length];
                stream.Read(fileData, 0, fileData.Length);
                stream.Dispose();
                AssetBundle bundle = AssetBundle.LoadFromMemory(fileData);
                cachedBundles.Add(mod.Name + ":" + file, bundle);
                return bundle;  
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// This represents an entry in the tile spritesheet. Returned by <see cref="AddTextureToSheet"/>
        /// </summary>
        public sealed class SpriteSheetEntry
        {
            internal readonly Texture2D tex;
            internal readonly int index;

            internal Vector2 coords;

            internal SpriteSheetEntry(Texture2D tex, int index)
            {
                if (tex.width != 32 || tex.height != 32) throw new InvalidOperationException("SpriteSheet textures must be 32x32!");
                this.tex = tex;
                this.index = index;
            }

            /// <summary>
            /// Returns the Texture that this SpriteSheetEntry was built from.
            /// </summary>
            public Texture GetTex()
            {
                return tex;
            }

            /// <summary>
            /// Returns the coordinates on the spritesheet that this texture resides at. This will throw an exception if the spritesheet has not been generated yet.
            /// </summary>
            public Vector2 GetCoords()
            {
                if (spriteSheet != null) return coords;
                throw new InvalidOperationException("The SpriteSheet has not been generated yet!");
            }
        }
    }
}