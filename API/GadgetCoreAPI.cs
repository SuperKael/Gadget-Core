using Ionic.Zip;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UModFramework.API;
using UnityEngine;

namespace GadgetCore.API
{
    /// <summary>
    /// General-purpose utility class for accessing various parts of the Gadget Core API
    /// </summary>
    public static class GadgetCoreAPI
    {
        /// <summary>
        /// The version of Gadget Core.
        /// </summary>
        public const string VERSION = "1.1.4.2";

        /// <summary>
        /// List of UMF mod names, not including mod libraries.
        /// </summary>
        public static ReadOnlyCollection<string> ModNames { get; internal set; }
        /// <summary>
        /// List of UMF mod libraries.
        /// </summary>
        public static ReadOnlyCollection<string> LibNames { get; internal set; }

        private static readonly MethodInfo RefreshExpBar = typeof(GameScript).GetMethod("RefreshExpBar", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo Crafting = typeof(GameScript).GetMethod("Crafting", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo CraftCheck = typeof(GameScript).GetMethod("CraftCheck", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo GetItemNameMethod = typeof(GameScript).GetMethod("GetItemName", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo GetItemDescMethod = typeof(GameScript).GetMethod("GetItemDesc", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo GetGearBaseStatsMethod = typeof(GameScript).GetMethod("GetGearBaseStats", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo craftType = typeof(GameScript).GetField("craftType", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo craftValue = typeof(GameScript).GetField("craftValue", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo crafting = typeof(GameScript).GetField("crafting", BindingFlags.NonPublic | BindingFlags.Instance);

        internal static Dictionary<string, UnityEngine.Object> resources = new Dictionary<string, UnityEngine.Object>();
        internal static Dictionary<int, string> resourcePaths = new Dictionary<int, string>();
        internal static Dictionary<string, Texture2D> cachedTexes = new Dictionary<string, Texture2D>();
        internal static Dictionary<string, AssetBundle> cachedBundles = new Dictionary<string, AssetBundle>();
        internal static List<SpriteSheetEntry> spriteSheetSprites = new List<SpriteSheetEntry>();
        internal static int spriteSheetSize = -1;
        internal static Texture2D spriteSheet;
        internal static List<CustomCraftMenu> customCraftMenus = new List<CustomCraftMenu>();
        internal static List<string> menuPaths = new List<string>();
        internal static List<GameObject> menus;
        private static int lastCrafterID = 3;

        /// <summary>
        /// Use to register a custom crafting menu. Using RegisterMenu is unnecessary for crafting menus. Note that all crafting menus must have, at most, 3 ingredient slots. Use OpenCraftMenu to open the menu.
        /// </summary>
        /// <param name="Title">The text to display as the title of the crafting window.</param>
        /// <param name="Desc">The text to display as the description of the crafting window.</param>
        /// <param name="MenuMat">The material to use for the crafting window.</param>
        /// <param name="ProgressBarMat">The material to use for the crafting progress bar.</param>
        /// <param name="SlotValidator">A Func that is used to check if an item is valid for a slot.</param>
        /// <param name="CraftValidator">A Func that is used to check if the items currently in the slots are valid for a recipe. The Item array is of length 4, where the first three Items are the inputs, and the fourth Item is the output.</param>
        /// <param name="CraftPerformer">An Action that is used to perform a crafting operation by modifying the contents of the Item array. The Item array is of length 4, where the first three Items are the inputs, and the fourth Item is the output. Should decrement the inputs, and set the output, possibly respecting whatever is already in the output.</param>
        /// <param name="Tile">An optional parameter that specifies what Interactive tile should open the menu when interacted with. Shortcut for adding OpenCraftMenuRoutine to the tile's OnInteract event.</param>
        /// <returns></returns>
        public static int RegisterCustomCraftMenu(string Title, string Desc, Material MenuMat, Material ProgressBarMat, Func<Item, int, bool> SlotValidator, Func<Item[], bool> CraftValidator, Action<Item[]> CraftPerformer, TileInfo Tile = null)
        {
            customCraftMenus.Add(new CustomCraftMenu(Title, Desc, MenuMat, ProgressBarMat, SlotValidator, CraftValidator, CraftPerformer));
            if (Tile != null) Tile.OnInteract += () => OpenCraftMenuRoutine(lastCrafterID + 1);
            return ++lastCrafterID;
        }

        /// <summary>
        /// Registers a non-crafting menu to the game. Use OpenMenu to open the menu. Components on the Menu may have methods named OnMenuOpened and/or OnMenuClosed that will get automatically invoked when the menu is opened or closed, respectively.
        /// </summary>
        /// <param name="Menu">The menu prefab.</param>
        /// <param name="Tile">An optional parameter that specifies what Interactive tile should open the menu when interacted with. Shortcut for adding OpenMenuRoutine to the tile's OnInteract event.</param>
        public static int RegisterMenu(GameObject Menu, TileInfo Tile = null)
        {
            Menu.SetActive(false);
            AddCustomResource("menu/" + Menu.name, Menu);
            menuPaths.Add("menu/" + Menu.name);
            if (Tile != null) Tile.OnInteract += () => OpenMenuRoutine(menuPaths.Count - 1);
            return menuPaths.Count - 1;
        }

        /// <summary>
        /// Opens the custom crafting menu with the specified ID. Functions as a Coroutine, to facilitate use in TileInfo's OnInteract.
        /// </summary>
        public static IEnumerator OpenCraftMenuRoutine(int ID)
        {
            OpenCraftMenu(ID);
            yield break;
        }

        /// <summary>
        /// Opens the custom menu with the specified ID. Not for use with crafting menus. Functions as a Coroutine, to facilitate use in TileInfo's OnInteract.
        /// </summary>
        public static IEnumerator OpenMenuRoutine(int ID)
        {
            OpenMenu(ID);
            yield break;
        }

        /// <summary>
        /// Opens the custom crafting menu with the specified ID.
        /// </summary>
        public static void OpenCraftMenu(int ID)
        {
            CustomCraftMenu craftMenu = customCraftMenus[ID];
            InstanceTracker.GameScript.StartCoroutine(RefreshExpBar.Invoke(InstanceTracker.GameScript, new object[] { }) as IEnumerator);
            InstanceTracker.GameScript.txtCraftName[0].text = craftMenu.Title;
            InstanceTracker.GameScript.txtCraftName[1].text = InstanceTracker.GameScript.txtCraftName[0].text;
            InstanceTracker.GameScript.txtCraftTip[0].text = craftMenu.Desc;
            InstanceTracker.GameScript.txtCraftTip[1].text = InstanceTracker.GameScript.txtCraftTip[0].text;
            InstanceTracker.GameScript.barCraft.GetComponent<Renderer>().material = craftMenu.ProgressBarMat;
            craftType.SetValue(InstanceTracker.GameScript, ID);
            craftValue.SetValue(InstanceTracker.GameScript, 0);
            InstanceTracker.GameScript.barCraft.transform.localScale = new Vector3(0f, 0.6f, 1f);
            crafting.SetValue(InstanceTracker.GameScript, true);
            InstanceTracker.GameScript.bRecipe.SetActive(true);
            InstanceTracker.GameScript.StartCoroutine(Crafting.Invoke(InstanceTracker.GameScript, new object[] { }) as IEnumerator);
            InstanceTracker.GameScript.menuCraftObj.GetComponent<Renderer>().material = craftMenu.MenuMat;
            InstanceTracker.GameScript.menuCraft.SetActive(true);
            CraftCheck.Invoke(InstanceTracker.GameScript, new object[] { });
            InstanceTracker.GameScript.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/invOpen"), Menuu.soundLevel / 10f);
            InstanceTracker.GameScript.inventoryMain.SetActive(true);
            GameScript.inventoryOpen = true;
        }

        /// <summary>
        /// Opens the custom menu with the specified ID. Not for use with crafting menus.
        /// </summary>
        public static void OpenMenu(int ID)
        {
            if (menus != null)
            {
                menus[ID].SetActive(true);
                menus[ID].SendMessage("OnMenuOpened", options: SendMessageOptions.DontRequireReceiver);
            }
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
                    itemScript.SendMessage("Init", st);
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
                itemScript.SendMessage("Chip", item.id);
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
                if (ItemRegistry.GetSingleton().HasEntry(item.id) && (ItemRegistry.GetSingleton().GetEntry(item.id).Type & ItemType.EQUIPABLE) == ItemType.EQUIPABLE) itemScript.back.SetActive(true);
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
        public static void SpawnExp(Vector3 pos, int exp)
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
                itemScript.SendMessage("Init", st);
                if (ItemRegistry.GetSingleton().HasEntry(item.id) && (ItemRegistry.GetSingleton().GetEntry(item.id).Type & ItemType.EQUIPABLE) == ItemType.EQUIPABLE) itemScript.back.SetActive(true);
                return itemScript;
            }
            else
            {
                ItemScript itemScript = ((GameObject)Network.Instantiate(Resources.Load("i2"), pos, Quaternion.identity, 0)).GetComponent<ItemScript>();
                itemScript.SendMessage("Chip", item.id);
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
                return Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
        }

        /// <summary>
        /// Use to manually add new resources to the game, or overwrite existing ones. May only be called from the Initialize method of a GadgetMod.
        /// </summary>
        /// <param name="path">The pseudo-file-path to place the resource on.</param>
        /// <param name="resource">The resource to register.</param>
        public static void AddCustomResource(string path, UnityEngine.Object resource)
        {
            if (!Registry.registeringVanilla && Registry.modRegistering < 0) throw new InvalidOperationException("Data registration may only be performed by the Initialize method of a GadgetMod!");
            UnityEngine.Object.DontDestroyOnLoad(resource);
            resource.hideFlags |= HideFlags.HideAndDontSave;
            if (resource is GameObject)
            {
                if (!(resource as GameObject).active)
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
        }

        /// <summary>
        /// Use to register a texture for the tile spritesheet. The texture must be 32x32 in size. You probably shouldn't use this yourself - it is automatically called by <see cref="TileInfo"/> after registration. May only be called from <see cref="GadgetMod.Initialize"/>.
        /// </summary>
        /// <param name="sprite">The Texture2D to register to the spritesheet</param>
        public static SpriteSheetEntry AddTextureToSheet(Texture2D sprite)
        {
            if (!Registry.registeringVanilla && Registry.modRegistering < 0) throw new InvalidOperationException("Data registration may only be performed by the Initialize method of a GadgetMod!");
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
        /// Gets the EquipStats of the item with the given ID. Easier than using reflection to call GetGearBaseStats on GameScript.
        /// </summary>
        public static EquipStats GetGearBaseStats(int ID)
        {
            return new EquipStats(GetGearBaseStatsMethod.Invoke(InstanceTracker.GameScript, new object[] { ID }) as int[]);
        }

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
            return (Material)Resources.Load("side/side" + (vertical ? "v" : "h") + ID);
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
        /// Works like UMFAsset.LoadTexture2D, except it isn't dependent on System.Drawing. Returns null if the texture was not found.
        /// </summary>
        public static Texture2D LoadTexture2D(string file, bool shared = false)
        {
            string modName = shared ? "Shared" : Assembly.GetCallingAssembly().GetName().Name;
            string filePath = Path.Combine(Path.Combine(UMFData.AssetsPath, modName), file);
            if (cachedTexes.ContainsKey(filePath))
            {
                return cachedTexes[filePath];
            }
            string assetPath = Path.Combine(Path.Combine("Assets", modName), file);
            if (cachedTexes.ContainsKey(Path.Combine(UMFData.TempPath, assetPath)))
            {
                return cachedTexes[Path.Combine(UMFData.TempPath, assetPath)];
            }
            if (!File.Exists(filePath) && GadgetCore.CoreLib != null)
            {
                filePath = Path.Combine(UMFData.TempPath, assetPath);
                string[] umfmods = Directory.GetFiles(UMFData.ModsPath, (shared ? "" : modName) + "*.umfmod");
                string[] zipmods = Directory.GetFiles(UMFData.ModsPath, (shared ? "" : modName) + "*.zip");
                string[] mods = new string[umfmods.Length + zipmods.Length];
                Array.Copy(umfmods, mods, umfmods.Length);
                Array.Copy(zipmods, 0, mods, umfmods.Length, zipmods.Length);
                foreach (string mod in mods)
                {
                    using (ZipFile modZip = new ZipFile(mod))
                    {
                        if (mod.EndsWith(".umfmod")) GadgetCore.CoreLib.DecryptUMFModFile(modZip);
                        if (modZip.ContainsEntry(assetPath))
                        {
                            modZip[assetPath].Extract(UMFData.TempPath, ExtractExistingFileAction.OverwriteSilently);
                        }
                    }
                }
            }
            if (File.Exists(filePath))
            {
                byte[] fileData;
                fileData = File.ReadAllBytes(filePath);
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(fileData);
                tex.filterMode = FilterMode.Point;
                cachedTexes.Add(filePath, tex);
                if (filePath.StartsWith(UMFData.TempPath))
                {
                    File.Delete(filePath);
                    GadgetUtils.RecursivelyDeleteDirectory(UMFData.TempPath);
                }
                return tex;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Loads an AssetBundle in a similar fashion to LoadTexture2D. Note that the file should not have an extension. AssetBundles would normally have the extension 'assets', but if it did then UMF would try to load it and crash because of a bug in UMF.
        /// </summary>
        public static AssetBundle LoadAssetBundle(string file, bool shared = false)
        {
            string modName = shared ? "Shared" : Assembly.GetCallingAssembly().GetName().Name;
            string filePath = Path.Combine(Path.Combine(Path.Combine(Path.GetDirectoryName(UMFData.AssetsPath), "Assets"), modName), file);
            if (cachedTexes.ContainsKey(filePath))
            {
                return cachedBundles[filePath];
            }
            string bundlePath = Path.Combine(Path.Combine("Assets", modName), file);
            if (cachedTexes.ContainsKey(Path.Combine(UMFData.TempPath, bundlePath)))
            {
                return cachedBundles[Path.Combine(UMFData.TempPath, bundlePath)];
            }
            if (!File.Exists(filePath) && GadgetCore.CoreLib != null)
            {
                filePath = Path.Combine(UMFData.TempPath, bundlePath);
                string[] umfmods = Directory.GetFiles(UMFData.ModsPath, (shared ? "" : modName) + "*.umfmod");
                string[] zipmods = Directory.GetFiles(UMFData.ModsPath, (shared ? "" : modName) + "*.zip");
                string[] mods = new string[umfmods.Length + zipmods.Length];
                Array.Copy(umfmods, mods, umfmods.Length);
                Array.Copy(zipmods, 0, mods, umfmods.Length, zipmods.Length);
                foreach (string mod in mods)
                {
                    using (ZipFile modZip = new ZipFile(mod))
                    {
                        if (mod.EndsWith(".umfmod")) GadgetCore.CoreLib.DecryptUMFModFile(modZip);
                        if (modZip.ContainsEntry(bundlePath))
                        {
                            modZip[bundlePath].Extract(UMFData.TempPath, ExtractExistingFileAction.OverwriteSilently);
                        }
                    }
                }
            }
            if (File.Exists(filePath))
            {
                AssetBundle bundle = AssetBundle.LoadFromFile(filePath);
                if (filePath.StartsWith(UMFData.TempPath))
                {
                    File.Delete(filePath);
                    GadgetUtils.RecursivelyDeleteDirectory(UMFData.TempPath);
                }
                cachedBundles[filePath] = bundle;
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

        internal struct CustomCraftMenu
        {
            internal readonly string Title;
            internal readonly string Desc;
            internal readonly Material MenuMat;
            internal readonly Material ProgressBarMat;
            internal readonly Func<Item, int, bool> SlotValidator;
            internal readonly Func<Item[], bool> CraftValidator;
            internal readonly Action<Item[]> CraftPerformer;

            internal CustomCraftMenu(string Title, string Desc, Material MenuMat, Material ProgressBarMat, Func<Item, int, bool> SlotValidator, Func<Item[], bool> CraftValidator, Action<Item[]> CraftPerformer)
            {
                this.Title = Title;
                this.Desc = Desc;
                this.MenuMat = MenuMat;
                this.ProgressBarMat = ProgressBarMat;
                this.SlotValidator = SlotValidator;
                this.CraftValidator = CraftValidator;
                this.CraftPerformer = CraftPerformer;
            }
        }
    }
}