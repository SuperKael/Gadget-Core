using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace GadgetCore.API.ConfigMenu
{
    /// <summary>
    /// Basic <see cref="IGadgetConfigMenu"/> implementation that displays a list of <see cref="GadgetConfigComponent"/>s.
    /// </summary>
    public class BasicGadgetConfigMenu : IGadgetConfigMenu
    {
        /// <summary>
        /// List of <see cref="GadgetConfigComponent"/>s on this config menu.
        /// </summary>
        protected readonly List<GadgetConfigComponent> ConfigComponents = new List<GadgetConfigComponent>();
        /// <summary>
        /// The parent object of this config menu. Will be null until the menu is built.
        /// </summary>
        public RectTransform MenuParent { get; protected set; }
        /// <summary>
        /// Whether this menu has been built yet.
        /// </summary>
        public bool HasBuilt { get; protected set; }

        /// <summary>
        /// Specifies whether the mod menu should be hidden when this config menu is opened.
        /// </summary>
        public bool HidesModMenu { get; protected set; }

        /// <summary>
        /// Used to cache the scroll position of the body ScrollRect. Allows preservation of scroll level on rebuild.
        /// </summary>
        protected float scrollPositionCache = -1;

        /// <summary>
        /// Constructs a new instance of <see cref="BasicGadgetConfigMenu"/>. Optionally adds <see cref="GadgetConfigComponent"/>s in the process. <see cref="GadgetConfigComponent"/>s can also be added later using <see cref="AddComponent"/>, but only before the menu is built.
        /// </summary>
        public BasicGadgetConfigMenu(bool hidesModMenu = false, params GadgetConfigComponent[] components)
        {
            HidesModMenu = hidesModMenu;
            foreach (GadgetConfigComponent component in components) AddComponent(component, GadgetConfigComponentAlignment.STANDARD);
        }

        /// <summary>
        /// Adds a <see cref="GadgetConfigComponent"/> to this config menu. May only be called before the menu is built.
        /// </summary>
        public virtual void AddComponent(GadgetConfigComponent component, GadgetConfigComponentAlignment alignment = GadgetConfigComponentAlignment.STANDARD)
        {
            if (HasBuilt) throw new InvalidOperationException("GadgetConfigComponents may not be added to a BasicGadgetConfigMenu after the menu has been built!");
            component.SetAlignment(alignment);
            ConfigComponents.Add(component);
        }

        /// <summary>
        /// Adds a <see cref="GadgetConfigComponent"/> to this config menu, at a specified index in the component array. May only be called before the menu is built.
        /// </summary>
        public virtual void InsertComponent(int index, GadgetConfigComponent component, GadgetConfigComponentAlignment alignment = GadgetConfigComponentAlignment.STANDARD)
        {
            if (HasBuilt) throw new InvalidOperationException("GadgetConfigComponents may not be added to a BasicGadgetConfigMenu after the menu has been built!");
            component.SetAlignment(alignment);
            ConfigComponents.Insert(Math.Min(index, ConfigComponents.Count - 1), component);
        }

        /// <summary>
        /// Called for every configurable mod and Gadget when the Mod Menu is injected into the title screen. This is called again if the player returns to the title screen after having selected a character. You should never call this yourself.
        /// </summary>
        /// <param name="parent">A <see cref="RectTransform"/> on the Mod Menu canvas intended to be used as the parent object of your config menu. This object will have a large background <see cref="UnityEngine.UI.Image">Image</see> on it, intended to be the background of your config menu. Feel free to change or remove this background. This object will also be enabled and disabled as needed to open and close the config menu.</param>
        public virtual void Build(RectTransform parent)
        {
            if (HasBuilt) throw new InvalidOperationException("A BasicGadgetConfigMenu cannot be built again after it has already been built!");
            HasBuilt = true;
            MenuParent = parent;
            bool shouldHideModMenu = ShouldHideModMenu();
            Image backgroundImage = new GameObject("Background Image", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
            backgroundImage.GetComponent<RectTransform>().SetParent(parent);
            if (shouldHideModMenu)
            {
                backgroundImage.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);
                backgroundImage.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
                backgroundImage.GetComponent<RectTransform>().offsetMin = new Vector2(0f, 0f);
                backgroundImage.GetComponent<RectTransform>().offsetMax = new Vector2(0f, 0f);
            }
            else
            {
                backgroundImage.GetComponent<RectTransform>().anchorMin = new Vector2(0.4f, 0.25f);
                backgroundImage.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
                backgroundImage.GetComponent<RectTransform>().offsetMin = new Vector2(0f, 5f);
                backgroundImage.GetComponent<RectTransform>().offsetMax = new Vector2(-10f, -10f);
            }
            backgroundImage.sprite = SceneInjector.BoxSprite;
            backgroundImage.type = Image.Type.Sliced;
            backgroundImage.fillCenter = true;
            backgroundImage.transform.localScale = Vector3.one;
            RectTransform menuHeaderContainer = new GameObject("Header Container", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<RectTransform>();
            menuHeaderContainer.SetParent(parent);
            if (shouldHideModMenu)
            {
                menuHeaderContainer.anchorMin = new Vector2(0f, 0.75f);
                menuHeaderContainer.anchorMax = new Vector2(1f, 1f);
                menuHeaderContainer.offsetMin = new Vector2(0f, -20f);
                menuHeaderContainer.offsetMax = new Vector2(0f, 0f);
            }
            else
            {
                menuHeaderContainer.anchorMin = new Vector2(0.4f, 0.75f);
                menuHeaderContainer.anchorMax = new Vector2(1f, 1f);
                menuHeaderContainer.offsetMin = new Vector2(0f, -30f);
                menuHeaderContainer.offsetMax = new Vector2(-10f, -10f);
            }
            menuHeaderContainer.GetComponent<Image>().sprite = SceneInjector.BoxSprite;
            menuHeaderContainer.GetComponent<Image>().type = Image.Type.Sliced;
            menuHeaderContainer.GetComponent<Image>().fillCenter = true;
            RectTransform menuHeader = new GameObject("Header", typeof(RectTransform)).GetComponent<RectTransform>();
            menuHeader.SetParent(parent);
            if (shouldHideModMenu)
            {
                menuHeader.anchorMin = new Vector2(0f, 0f);
                menuHeader.anchorMax = new Vector2(1f, 1f);
                menuHeader.offsetMin = new Vector2(10f, 10f);
                menuHeader.offsetMax = new Vector2(-10f, -10f);
            }
            else
            {
                menuHeader.anchorMin = new Vector2(0.4f, 0.25f);
                menuHeader.anchorMax = new Vector2(1f, 1f);
                menuHeader.offsetMin = new Vector2(10f, 0f);
                menuHeader.offsetMax = new Vector2(-10f, -20f);
            }
            RectTransform menuFooterContainer = new GameObject("Footer Container", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<RectTransform>();
            menuFooterContainer.SetParent(parent);
            if (shouldHideModMenu)
            {
                menuFooterContainer.anchorMin = new Vector2(0f, 0f);
                menuFooterContainer.anchorMax = new Vector2(1f, 0.25f);
                menuFooterContainer.offsetMin = new Vector2(0f, 0f);
                menuFooterContainer.offsetMax = new Vector2(0f, 20f);
            }
            else
            {
                menuFooterContainer.anchorMin = new Vector2(0.4f, 0.25f);
                menuFooterContainer.anchorMax = new Vector2(1f, 0.5f);
                menuFooterContainer.offsetMin = new Vector2(0f, 5f);
                menuFooterContainer.offsetMax = new Vector2(-10f, 25f);
            }
            menuFooterContainer.GetComponent<Image>().sprite = SceneInjector.BoxSprite;
            menuFooterContainer.GetComponent<Image>().type = Image.Type.Sliced;
            menuFooterContainer.GetComponent<Image>().fillCenter = true;
            RectTransform menuFooter = new GameObject("Footer", typeof(RectTransform)).GetComponent<RectTransform>();
            menuFooter.SetParent(parent);
            if (shouldHideModMenu)
            {
                menuFooter.anchorMin = new Vector2(0f, 0f);
                menuFooter.anchorMax = new Vector2(1f, 1f);
                menuFooter.offsetMin = new Vector2(10f, 10f);
                menuFooter.offsetMax = new Vector2(-10f, -10f);
            }
            else
            {
                menuFooter.anchorMin = new Vector2(0.4f, 0.25f);
                menuFooter.anchorMax = new Vector2(1f, 1f);
                menuFooter.offsetMin = new Vector2(10f, 15f);
                menuFooter.offsetMax = new Vector2(-10f, -5f);
            }
            float totalHeaderHeight = 0, totalFooterHeight = 0;
            foreach (GadgetConfigComponent component in ConfigComponents.Where(x => x.Alignment != GadgetConfigComponentAlignment.STANDARD))
            {
                if (component == null) continue;
                RectTransform menuParent = new GameObject(component.Name, typeof(RectTransform)).GetComponent<RectTransform>();
                float oldHeight = component.Height;
                switch (component.Alignment)
                {
                    case GadgetConfigComponentAlignment.HEADER:
                        menuParent.SetParent(menuHeader);
                        menuParent.anchorMax = new Vector2(1f, 1 - totalHeaderHeight);
                        menuParent.anchorMin = new Vector2(0f, 1 - (totalHeaderHeight += component.Height));
                        menuParent.offsetMin = Vector2.zero;
                        menuParent.offsetMax = Vector2.zero;
                        component.Build(menuParent);
                        if (oldHeight != component.Height)
                        {
                            menuParent.anchorMax = new Vector2(1f, 1 - (totalHeaderHeight - oldHeight));
                            menuParent.anchorMin = new Vector2(0f, 1 - (totalHeaderHeight += component.Height - oldHeight));
                            menuParent.offsetMin = Vector2.zero;
                            menuParent.offsetMax = Vector2.zero;
                        }
                        break;
                    case GadgetConfigComponentAlignment.FOOTER:
                        menuParent.SetParent(menuFooter);
                        menuParent.anchorMin = new Vector2(0f, totalFooterHeight);
                        menuParent.anchorMax = new Vector2(1f, totalFooterHeight += component.Height);
                        menuParent.offsetMin = Vector2.zero;
                        menuParent.offsetMax = Vector2.zero;
                        component.Build(menuParent);
                        if (oldHeight != component.Height)
                        {
                            menuParent.anchorMin = new Vector2(0f, totalFooterHeight - oldHeight);
                            menuParent.anchorMax = new Vector2(1f, totalFooterHeight += component.Height - oldHeight);
                            menuParent.offsetMin = Vector2.zero;
                            menuParent.offsetMax = Vector2.zero;
                        }
                        break;
                }
            }
            if (shouldHideModMenu)
            {
                menuHeaderContainer.anchorMin = new Vector2(0f, 1f - Mathf.Min(totalHeaderHeight, 0.25f));
                menuHeaderContainer.anchorMax = new Vector2(1f, 1f);
            }
            else
            {
                menuHeaderContainer.anchorMin = new Vector2(0.4f, 1f - Mathf.Min(totalHeaderHeight, 0.1875f));
                menuHeaderContainer.anchorMax = new Vector2(1f, 1f);
            }
            menuHeaderContainer.localScale = Vector3.one;
            menuHeader.localScale = Vector3.one;
            if (shouldHideModMenu)
            {
                menuFooterContainer.anchorMin = new Vector2(0f, 0f);
                menuFooterContainer.anchorMax = new Vector2(1f, Mathf.Min(totalFooterHeight, 0.25f));
            }
            else
            {
                menuFooterContainer.anchorMin = new Vector2(0.4f, 0.25f);
                menuFooterContainer.anchorMax = new Vector2(1f, 0.25f + Mathf.Min(totalFooterHeight, 0.1875f));
            }
            menuFooterContainer.localScale = Vector3.one;
            menuFooter.localScale = Vector3.one;
            ScrollRect menuBodyScrollPanel = new GameObject("Body Scroll Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(ScrollRect)).GetComponent<ScrollRect>();
            menuBodyScrollPanel.GetComponent<RectTransform>().SetParent(parent);
            if (shouldHideModMenu)
            {
                menuBodyScrollPanel.GetComponent<RectTransform>().anchorMin = new Vector2(0f, Mathf.Min(totalFooterHeight, 0.25f));
                menuBodyScrollPanel.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f - Mathf.Min(totalHeaderHeight, 0.25f));
                menuBodyScrollPanel.GetComponent<RectTransform>().offsetMin = new Vector2(0f, totalFooterHeight > 0 ? 20f : 0f);
                menuBodyScrollPanel.GetComponent<RectTransform>().offsetMax = new Vector2(0f, totalHeaderHeight > 0 ? -20f : 0f);
            }
            else
            {
                menuBodyScrollPanel.GetComponent<RectTransform>().anchorMin = new Vector2(0.4f, 0.25f + Mathf.Min(totalFooterHeight, 0.1875f));
                menuBodyScrollPanel.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f - Mathf.Min(totalHeaderHeight, 0.1875f));
                menuBodyScrollPanel.GetComponent<RectTransform>().offsetMin = new Vector2(0f, totalFooterHeight > 0 ? 25f : 5f);
                menuBodyScrollPanel.GetComponent<RectTransform>().offsetMax = new Vector2(-10f, totalHeaderHeight > 0 ? -30f : -10f);
            }
            menuBodyScrollPanel.GetComponent<Image>().sprite = SceneInjector.BoxSprite;
            menuBodyScrollPanel.GetComponent<Image>().type = Image.Type.Sliced;
            menuBodyScrollPanel.GetComponent<Image>().fillCenter = true;
            Mask menuBodyScrollPanelMask = new GameObject("Body Mask", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask)).GetComponent<Mask>();
            menuBodyScrollPanelMask.GetComponent<RectTransform>().SetParent(menuBodyScrollPanel.transform);
            menuBodyScrollPanelMask.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);
            menuBodyScrollPanelMask.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
            menuBodyScrollPanelMask.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            menuBodyScrollPanelMask.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            menuBodyScrollPanelMask.GetComponent<Image>().sprite = SceneInjector.BoxMask;
            menuBodyScrollPanelMask.GetComponent<Image>().type = Image.Type.Sliced;
            menuBodyScrollPanelMask.GetComponent<Image>().fillCenter = true;
            menuBodyScrollPanelMask.showMaskGraphic = false;
            RectTransform menuBodyViewport = new GameObject("Body Viewport", typeof(RectTransform)).GetComponent<RectTransform>();
            menuBodyViewport.SetParent(menuBodyScrollPanelMask.transform);
            menuBodyViewport.anchorMin = new Vector2(0f, 0f);
            menuBodyViewport.anchorMax = new Vector2(1f, 1f);
            menuBodyViewport.offsetMin = new Vector2(10, 10);
            menuBodyViewport.offsetMax = new Vector2(-10, -10);
            RectTransform menuBody = new GameObject("Body", typeof(RectTransform)).GetComponent<RectTransform>();
            menuBody.SetParent(menuBodyViewport);
            float heightMultiplier = 1f / (1f - Mathf.Min(totalHeaderHeight, shouldHideModMenu ? 0.25f : 0.1875f) - Mathf.Min(totalFooterHeight, shouldHideModMenu ? 0.25f : 0.1875f));
            float totalScrollHeight = Mathf.Max(ConfigComponents.Select(x => x != null ? x.Height : 0).Aggregate(0f, (x, y) => x + y), 1) * heightMultiplier;
            menuBody.anchorMin = new Vector2(0f, 1f - totalScrollHeight);
            menuBody.anchorMax = new Vector2(1f, 1f);
            menuBody.offsetMin = Vector2.zero;
            menuBody.offsetMax = Vector2.zero;
            menuBody.pivot = new Vector2(0.5f, 1f);
            menuBodyScrollPanel.content = menuBody;
            menuBodyScrollPanel.horizontal = false;
            menuBodyScrollPanel.scrollSensitivity = 5;
            menuBodyScrollPanel.viewport = menuBodyViewport;
            float totalBodyHeight = 0;
            foreach (GadgetConfigComponent component in ConfigComponents.Where(x => x.Alignment == GadgetConfigComponentAlignment.STANDARD))
            {
                if (component == null) continue;
                RectTransform menuParent = new GameObject(component.Name, typeof(RectTransform)).GetComponent<RectTransform>();
                float oldHeight = component.Height * heightMultiplier;
                menuParent.SetParent(menuBody);
                menuParent.anchorMax = new Vector2(1f, 1 - (totalBodyHeight / totalScrollHeight));
                menuParent.anchorMin = new Vector2(0f, 1 - ((totalBodyHeight += component.Height * heightMultiplier) / totalScrollHeight));
                menuParent.offsetMin = Vector2.zero;
                menuParent.offsetMax = Vector2.zero;
                component.Build(menuParent);
                if (oldHeight != component.Height * heightMultiplier)
                {
                    menuParent.anchorMax = new Vector2(1f, 1 - ((totalBodyHeight - oldHeight) / totalScrollHeight));
                    menuParent.anchorMin = new Vector2(0f, 1 - ((totalBodyHeight += component.Height * heightMultiplier - oldHeight) / totalScrollHeight));
                    menuParent.offsetMin = Vector2.zero;
                    menuParent.offsetMax = Vector2.zero;
                }
            }
            if (totalScrollHeight != totalBodyHeight)
            {
                menuBody.anchorMin = new Vector2(0f, 1f - totalBodyHeight);
                menuBody.anchorMax = new Vector2(1f, 1f);
                menuBody.offsetMin = Vector2.zero;
                menuBody.offsetMax = Vector2.zero;
                foreach (RectTransform transform in menuBody)
                {
                    transform.anchorMin = new Vector2(0f, ((transform.anchorMin.y - 1) * (totalScrollHeight / totalBodyHeight)) + 1);
                    transform.anchorMax = new Vector2(1f, ((transform.anchorMax.y - 1) * (totalScrollHeight / totalBodyHeight)) + 1);
                    transform.offsetMin = Vector2.zero;
                    transform.offsetMax = Vector2.zero;
                }
            }
            menuBodyScrollPanel.transform.localScale = Vector3.one;
            if (scrollPositionCache >= 0) menuBodyScrollPanel.verticalNormalizedPosition = 1 - (scrollPositionCache / (menuBody.anchorMax.y - menuBody.anchorMin.y));
        }

        /// <summary>
        /// Removes all <see cref="GadgetConfigComponent"/>s from this <see cref="BasicGadgetConfigMenu"/>. You should call <see cref="Rebuild"/> sometime after this to update the displayed menu.
        /// </summary>
        public virtual void Clear()
        {
            ConfigComponents.Clear();
            HasBuilt = false;
        }

        /// <summary>
        /// Called to make the config menu totally reset itself.
        /// </summary>
        public virtual void Reset()
        {
            Rebuild();
        }

        /// <summary>
        /// Called whenever this mod's config menu is opened. The parent <see cref="RectTransform"/> that was passed to <see cref="Build(RectTransform)"/> will be enabled immediately after this method is called. You should never call this yourself.
        /// </summary>
        public virtual void Rebuild()
        {
            RectTransform bodyScrollPanel = MenuParent.Find("Body Scroll Panel") as RectTransform;
            RectTransform body = bodyScrollPanel.Find("Body Mask").Find("Body Viewport").Find("Body") as RectTransform;
            scrollPositionCache = (1 - bodyScrollPanel.GetComponent<ScrollRect>().verticalNormalizedPosition) * (body.anchorMax.y - body.anchorMin.y);
            foreach (Transform t in MenuParent) UnityEngine.Object.Destroy(t.gameObject);
            RectTransform menuParent = MenuParent;
            HasBuilt = false;
            Build(menuParent);
        }

        /// <summary>
        /// Called whenever this mod's config menu is opened. The parent <see cref="RectTransform"/> that was passed to <see cref="Build(RectTransform)"/> will be enabled immediately after this method is called. You should never call this yourself.
        /// </summary>
        public virtual void Render()
        {
            
        }

        /// <summary>
        /// Called whenever this mod's config menu is closed. The parent <see cref="RectTransform"/> that was passed to <see cref="Build(RectTransform)"/> will be disabled immediately before this method is called. You should never call this yourself.
        /// </summary>
        public virtual void Derender()
        {
            
        }

        /// <summary>
        /// Called whenever the config menu's contents are updated in some way. You should probably call this yourself whenever the config menu's contents are updated somehow.
        /// </summary>
        public virtual void Update()
        {
            if (MenuParent == null) return;
        }

        /// <summary>
        /// Called when this config menu is opened, to check whether the mod menu behind your config menu should be hidden when your config menu is opened.
        /// </summary>
        public virtual bool ShouldHideModMenu()
        {
            return HidesModMenu;
        }
    }
}
