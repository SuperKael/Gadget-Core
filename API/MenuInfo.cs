using GadgetCore.Util;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace GadgetCore.API
{
    /// <summary>
    /// Represents a craft menu, such as the emblem forge or the alchemy station. Also includes custom craft menus.
    /// </summary>
    public class MenuInfo : RegistryEntry<MenuInfo, MenuType>
    {
        private static readonly FieldInfo holdingItem = typeof(GameScript).GetField("holdingItem", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo curRecipePage = typeof(GameScript).GetField("curRecipePage", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo crafting = typeof(GameScript).GetField("crafting", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo canCraft = typeof(GameScript).GetField("canCraft", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo interacting = typeof(PlayerScript).GetField("interacting", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo r = typeof(PlayerScript).GetField("r", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// The MenuType of this Menu
        /// </summary>
        public readonly MenuType Type;

        /// <summary>
        /// The Prefab that is injected into the game scene.
        /// </summary>
        public virtual GameObject MenuPrefab { get; protected set; }

        /// <summary>
        /// The Material associated with this item. May be null.
        /// </summary>
        public virtual TileInfo Tile { get; protected set; }

        /// <summary>
        /// Indicates whether this menu is currently open.
        /// </summary>
        public virtual bool IsOpen
        {
            get
            {
                return MenuObj != null && MenuObj.activeSelf;
            }
        }

        /// <summary>
        /// The GameObject that represents this menu in the scene. Not to be confused with MenuPrefab.
        /// </summary>
        protected internal GameObject MenuObj { get; internal set; }

        /// <summary>
        /// Use to create a new MenuInfo. Make sure to call Register on it to register your Menu.\
        /// </summary>
        /// <param name="Type">The MenuType of this Menu</param>
        /// <param name="MenuPrefab">The Prefab that is injected into the game scene.</param>
        /// <param name="Tile">An optional parameter that specifies what Interactive tile should open the menu when interacted with.</param>
        public MenuInfo(MenuType Type, GameObject MenuPrefab, TileInfo Tile = null)
        {
            this.Type = Type;
            this.MenuPrefab = MenuPrefab;
            this.Tile = Tile;
        }

        /// <summary>
        /// Registers this MenuInfo to the MenuRegistry.
        /// </summary>
        /// <param name="name">The registry name to use.</param>
        /// <param name="preferredID">If specified, will use this registry ID.</param>
        /// <param name="overrideExisting">If false, will not register if the preferred ID is already used. Ignored if no preferred ID is specified.</param>
        public virtual MenuInfo Register(string name, int preferredID = -1, bool overrideExisting = true)
        {
            return RegisterInternal(name, preferredID, overrideExisting) as MenuInfo;
        }

        /// <summary>
        /// Called after this Registry Entry has been registered to its Registry. You should never call this yourself.
        /// </summary>
        protected internal override void PostRegister()
        {
            if (Tile != null) Tile.OnInteract += OpenMenuRoutine;
        }

        /// <summary>
        /// Override to add special conditions under which the menu may not be opened.
        /// </summary>
        protected virtual bool CanOpenMenu()
        {
            return true;
        }

        /// <summary>
        /// Opens this Menu
        /// </summary>
        public void OpenMenu()
        {
            InstanceTracker.GameScript.StartCoroutine(OpenMenuRoutine());
        }

        /// <summary>
        /// Closes this Menu
        /// </summary>
        public void CloseMenu()
        {
            InstanceTracker.GameScript.StartCoroutine(CloseMenuRoutine());
        }

        /// <summary>
        /// Opens this Menu
        /// </summary>
        public virtual IEnumerator OpenMenuRoutine()
        {
            r.GetValue<Rigidbody>(InstanceTracker.PlayerScript).velocity = new Vector3(0f, 0f, 0f);
            if (interacting.GetValue<bool>(InstanceTracker.PlayerScript)) InstanceTracker.PlayerScript.w.SetActive(false);
            if (MenuObj != null && CanOpenMenu())
            {
                if (GameScript.buildMode)
                {
                    InstanceTracker.GameScript.ExitBuildMode();
                }
                if (GameScript.combatMode)
                {
                    if (Type == MenuType.CHIP)
                    {
                        InstanceTracker.GameScript.ExitCM3();
                    }
                    else
                    {
                        InstanceTracker.GameScript.ExitCM();
                    }
                }
                InvokeOnMenuOpened();
                GameScript.inventoryOpen = true;
                InstanceTracker.GameScript.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/invOpen"), Menuu.soundLevel / 10f);
                switch (Type)
                {
                    case MenuType.SIMPLE:
                    case MenuType.CRAFTING:
                        InstanceTracker.GameScript.inventoryMain.SetActive(true);
                        MenuObj.SetActive(true);
                        break;
                    case MenuType.CHIP:
                        if (holdingItem.GetValue<Item>(InstanceTracker.GameScript).id != 0)
                        {
                            InstanceTracker.GameScript.InvokeMethod("DropItem");
                        }
                        InstanceTracker.GameScript.skillBar.SetActive(true);
                        InstanceTracker.GameScript.inventoryMain.SetActive(false);
                        break;
                    case MenuType.EXCLUSIVE:
                        InstanceTracker.GameScript.inventoryMain.SetActive(false);
                        MenuObj.SetActive(true);
                        break;
                }
            }
            yield return new WaitForSeconds(0.5f);
            interacting.SetValue(InstanceTracker.PlayerScript, false);
            yield break;
        }

        /// <summary>
        /// Closes this Menu
        /// </summary>
        public virtual IEnumerator CloseMenuRoutine()
        {
            if (IsOpen)
            {
                InstanceTracker.GameScript.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/invClose"), Menuu.soundLevel / 10f);
                switch (Type)
                {
                    case MenuType.SIMPLE:
                        InstanceTracker.GameScript.hoverItem.SetActive(false);
                        if (holdingItem.GetValue<Item>(InstanceTracker.GameScript).id != 0)
                        {
                            InstanceTracker.GameScript.InvokeMethod("DropItem");
                        }
                        Cursor.visible = true;
                        break;
                    case MenuType.CRAFTING:
                        InstanceTracker.GameScript.InvokeMethod("DropCraftItems");
                        curRecipePage.SetValue(InstanceTracker.GameScript, 0);
                        crafting.SetValue(InstanceTracker.GameScript, false);
                        canCraft.SetValue(InstanceTracker.GameScript, false);
                        goto case MenuType.SIMPLE;
                    case MenuType.CHIP:
                        InstanceTracker.GameScript.skillBar.SetActive(false);
                        break;
                    case MenuType.EXCLUSIVE:
                        break;
                }
                InstanceTracker.GameScript.inventoryMain.SetActive(false);
                MenuObj.SetActive(false);
                GameScript.inventoryOpen = false;
                InvokeOnMenuClosed();
            }
            yield break;
        }

        /// <summary>
        /// Called immediately <em>before</em> the menu is opened. (Before it is enabled)
        /// </summary>
        public event Action OnMenuOpened;

        /// <summary>
        /// Called immediately <em>after</em> the menu is closed. (After it is disabled)
        /// </summary>
        public event Action OnMenuClosed;

        /// <summary>
        /// Triggers the OnMenuOpened event. Should only be used by overrides of <see cref="OpenMenuRoutine"/>
        /// </summary>
        protected void InvokeOnMenuOpened()
        {
            OnMenuOpened?.Invoke();
        }

        /// <summary>
        /// Triggers the OnMenuClosed event. Should only be used by overrides of <see cref="CloseMenuRoutine"/>
        /// </summary>
        protected void InvokeOnMenuClosed()
        {
            OnMenuClosed?.Invoke();
        }

        /// <summary>
        /// Returns the Registry Entry's Type enum. Used in the registration process, although it is safe to check this yourself by directly accessing the <see cref="Type"/> property.
        /// </summary>
        public override MenuType GetEntryType()
        {
            return Type;
        }

        /// <summary>
        /// Returns the singleton of the registry used for storing this type of Registry Entry.
        /// </summary>
        public override Registry<MenuInfo, MenuType> GetRegistry()
        {
            return MenuRegistry.Singleton;
        }

        /// <summary>
        /// Returns whether the specified ID is valid for this Registry Entry's Type.
        /// </summary>
        public override bool IsValidIDForType(int id)
        {
            return id > 0;
        }

        /// <summary>
        /// Returns the next valid ID for this Registry Entry's Type, after the provided lastValidID. Should skip the vanilla ID range.
        /// </summary>
        public override int GetNextIDForType(int lastValidID)
        {
            if (lastValidID < GetRegistry().GetIDStart() - 1) lastValidID = GetRegistry().GetIDStart() - 1;
            return ++lastValidID;
        }
    }
}
