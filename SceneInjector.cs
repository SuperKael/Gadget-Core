using GadgetCore.API;
using GadgetCore.API.ConfigMenu;
using GadgetCore.CoreMod;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GadgetCore
{
    internal static class SceneInjector
    {
        public static Canvas PersistantCanvas { get; internal set; }
        public static GameObject ConfirmationDialog { get; internal set; }
        public static Text ConfirmationText { get; internal set; }
        public static Text ConfirmationYesText { get; internal set; }
        public static Text ConfirmationNoText { get; internal set; }
        public static Action ConfirmationYesAction = null;
        public static Action ConfirmationNoAction = null;

        public static GameObject ModMenuBeam { get; internal set; }
        public static GameObject ModMenuButtonHolder { get; internal set; }
        public static GameObject ModMenu { get; internal set; }
        public static Canvas ModMenuCanvas { get; internal set; }
        public static ModMenuController ModMenuPanel { get; internal set; }
        public static ModBrowser ModBrowserPanel { get; internal set; }
        public static RectTransform ModConfigMenus { get; internal set; }
        public static GameObject ModMenuBackButtonBeam { get; internal set; }
        public static GameObject ModMenuBackButtonHolder { get; internal set; }
        public static GameObject ModConfigMenuText { get; internal set; }
        public static ScrollRect ModMenuDescPanel { get; internal set; }
        public static ScrollRect ModBrowserDescPanel { get; internal set; }
        public static Button ModBrowserButton { get; internal set; }

        public static GameObject BuildStand { get; internal set; }

        public static Material LeftArrow { get; private set; } = new Material(Shader.Find("Unlit/Transparent"));
        public static Material RightArrow { get; private set; } = new Material(Shader.Find("Unlit/Transparent"));
        public static Material LeftArrow2 { get; private set; } = new Material(Shader.Find("Unlit/Transparent"));
        public static Material RightArrow2 { get; private set; } = new Material(Shader.Find("Unlit/Transparent"));

        public static Sprite BoxSprite { get; internal set; }
        public static Sprite BoxMask { get; internal set; }
        public static Sprite BarSprite { get; internal set; }

        internal static void InjectMainMenu()
        {
            GadgetCore.CoreLogger.Log("Injecting objects into Main Menu");

            Texture2D boxTex = GadgetCoreAPI.LoadTexture2D("boxsprite.png");
            boxTex.filterMode = FilterMode.Point;
            Texture2D boxMaskTex = GadgetCoreAPI.LoadTexture2D("boxmask.png");
            boxMaskTex.filterMode = FilterMode.Point;
            Texture2D barTex = GadgetCoreAPI.LoadTexture2D("barsprite.png");
            barTex.filterMode = FilterMode.Point;
            BoxSprite = Sprite.Create(boxTex, new Rect(0, 0, boxTex.width, boxTex.height), new Vector2(0.5f, 0.5f), 100, 1, SpriteMeshType.Tight, new Vector4(15, 15, 15, 15));
            BoxMask = Sprite.Create(boxMaskTex, new Rect(0, 0, boxMaskTex.width, boxMaskTex.height), new Vector2(0.5f, 0.5f), 100, 1, SpriteMeshType.Tight, new Vector4(15, 15, 15, 15));
            BarSprite = Sprite.Create(barTex, new Rect(0, 0, barTex.width, barTex.height), new Vector2(0.5f, 0.5f), 100, 1, SpriteMeshType.Tight, new Vector4(1, 1, 1, 1));

            GameObject mainMenu = InstanceTracker.Menuu.menuMain;
            Array.ForEach(mainMenu.GetComponentsInChildren<Animation>(), x => x.enabled = true);
            ModMenuBeam = UnityEngine.Object.Instantiate(mainMenu.transform.Find("beamm").gameObject, mainMenu.transform);
            ModMenuBeam.name = "beamm";
            ModMenuBeam.transform.localScale = new Vector3(30, 0, 1);
            ModMenuBeam.transform.position = new Vector3(0, -13.5f, 1);
            ModMenuButtonHolder = UnityEngine.Object.Instantiate(mainMenu.transform.Find("BUTTONHOLDER").gameObject, mainMenu.transform);
            ModMenuButtonHolder.name = "BUTTONHOLDER";
            ModMenuButtonHolder.transform.position = new Vector3(-40f, -13.5f, 0);
            ModMenuButtonHolder.GetComponent<Animation>().RemoveClip("enterr1");
            ModMenuButtonHolder.GetComponent<Animation>().AddClip(BuildModMenuButtonAnimClip(false), "enterr1");
            ModMenuButtonHolder.GetComponent<Animation>().clip = ModMenuButtonHolder.GetComponent<Animation>().GetClip("enterr1");
            GameObject bModMenu = ModMenuButtonHolder.transform.GetChild(0).gameObject;
            bModMenu.name = "bModMenu";
            Array.ForEach(bModMenu.GetComponentsInChildren<TextMesh>(), x => x.text = "MOD MENU");
            InstanceTracker.Menuu.StartCoroutine(AnimateModMenuButton(InstanceTracker.Menuu));
            BuildModMenu();
            if (PersistantCanvas == null) BuildPersistantCanvas();
        }

        private static IEnumerator AnimateModMenuButton(Menuu instance)
        {
            ModMenuBeam.GetComponent<Animation>().Play();
            yield return new WaitForSeconds(0.3f);
            ModMenuButtonHolder.GetComponent<Animation>().Play();
            yield break;
        }

        internal static void InjectIngame()
        {
            foreach (MenuInfo menu in MenuRegistry.Singleton)
            {
                if (menu.MenuPrefab != null)
                {
                    menu.MenuObj = UnityEngine.Object.Instantiate(menu.MenuPrefab);
                    menu.MenuObj.name = menu.MenuPrefab.name;
                    menu.MenuObj.transform.SetParent(InstanceTracker.MainCamera.transform);
                    menu.MenuObj.SetActive(false);
                }
            }

            BuildStand = GameObject.Find("Ship").transform.Find("SHIPPLACES").Find("buildStand").gameObject;

            if (MenuRegistry.Singleton["Gadget Core:Crafter Menu"] is CraftMenuInfo craftMenu && craftMenu.CraftPerformers.Count > 0)
            {
                GadgetCoreAPI.CreateMarketStand(ItemRegistry.Singleton["Gadget Core:Crafter Block"], new Vector2(-138f, -7.49f), 10);
            }

            PlanetRegistry.PlanetSelectorPage = 1;
            int totalPages = PlanetRegistry.PlanetSelectorPages;

            if (totalPages > 1)
            {
                LeftArrow.mainTexture = GadgetCoreAPI.LoadTexture2D("left_arrow.png");
                RightArrow.mainTexture = GadgetCoreAPI.LoadTexture2D("right_arrow.png");
                LeftArrow2.mainTexture = GadgetCoreAPI.LoadTexture2D("left_arrow2.png");
                RightArrow2.mainTexture = GadgetCoreAPI.LoadTexture2D("right_arrow2.png");

                Transform bPlanetPageBack = UnityEngine.Object.Instantiate(InstanceTracker.GameScript.menuPlanets.transform.Find("bChallenge")).GetComponent<Transform>();
                List<GameObject> children = new List<GameObject>();
                foreach (Transform child in bPlanetPageBack) children.Add(child.gameObject);
                foreach (GameObject child in children) UnityEngine.Object.DestroyImmediate(child);
                bPlanetPageBack.GetComponent<BoxCollider>().center = new Vector3(0f, 0f, 0f);
                bPlanetPageBack.GetComponent<BoxCollider>().size = new Vector3(0.1625f, 0.1625f, 0f);
                bPlanetPageBack.gameObject.tag = "Untagged";
                bPlanetPageBack.name = "bPlanetPageBack";
                bPlanetPageBack.SetParent(InstanceTracker.GameScript.menuPlanets.transform, false);
                bPlanetPageBack.localPosition = new Vector3(0.015f, -0.07f, 0.25f);
                bPlanetPageBack.GetComponent<MeshRenderer>().material = LeftArrow;
                bPlanetPageBack.GetComponent<ButtonMenu>().button = LeftArrow;
                bPlanetPageBack.GetComponent<ButtonMenu>().buttonSelect = LeftArrow2;
                Transform bPlanetPageForward = UnityEngine.Object.Instantiate(bPlanetPageBack.gameObject).GetComponent<Transform>();
                bPlanetPageForward.name = "bPlanetPageForward";
                bPlanetPageForward.SetParent(InstanceTracker.GameScript.menuPlanets.transform, false);
                bPlanetPageForward.localPosition = new Vector3(0.34375f, -0.07f, 0.25f);
                bPlanetPageForward.GetComponent<MeshRenderer>().material = RightArrow;
                bPlanetPageForward.GetComponent<ButtonMenu>().button = RightArrow;
                bPlanetPageForward.GetComponent<ButtonMenu>().buttonSelect = RightArrow2;

                PlanetRegistry.planetPageText = UnityEngine.Object.Instantiate(InstanceTracker.GameScript.menuPlanets.transform.Find("txtPortals")).GetComponent<TextMesh>();
                PlanetRegistry.planetPageText.transform.SetParent(InstanceTracker.GameScript.menuPlanets.transform, false);
                PlanetRegistry.planetPageText.transform.localPosition = new Vector3(0.179375f, 0.039075f, 0.5f);
                foreach (TextMesh text in PlanetRegistry.planetPageText.GetComponentsInChildren<TextMesh>())
                {
                    text.text = "Page 1/" + totalPages;
                }

                Texture2D emptyTex = new Texture2D(12, 12, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Point
                };
                emptyTex.SetPixels32(Enumerable.Repeat(new Color32(30, 30, 30, 255), emptyTex.width * emptyTex.height).ToArray());
                emptyTex.Apply();
                Material emptyIcon = new Material(Shader.Find("Unlit/Transparent"))
                {
                    mainTexture = emptyTex
                };

                PlanetRegistry.selectorPlanets = PlanetRegistry.Singleton.ToArray();
                PlanetRegistry.planetButtonIcons = new GameObject[totalPages - 1][];
                for (int i = 0; i < 14; i++)
                {
                    MeshFilter planetButton = InstanceTracker.GameScript.menuPlanets.transform.Find(i.ToString()).GetComponent<MeshFilter>();
                    for (int p = 2; p <= totalPages; p++)
                    {
                        if (i == 0) PlanetRegistry.planetButtonIcons[p - 2] = new GameObject[14];
                        int planetIndex = (p - 2) * 14 + i;
                        PlanetRegistry.planetButtonIcons[p - 2][i] = new GameObject("Page " + p + " Icon", typeof(MeshFilter), typeof(MeshRenderer));
                        PlanetRegistry.planetButtonIcons[p - 2][i].SetActive(false);
                        PlanetRegistry.planetButtonIcons[p - 2][i].transform.SetParent(planetButton.transform, false);
                        PlanetRegistry.planetButtonIcons[p - 2][i].GetComponent<MeshFilter>().mesh = planetButton.mesh;
                        Material planetMat;
                        if (planetIndex < PlanetRegistry.selectorPlanets.Length && (planetMat = (Material)Resources.Load("mat/planetIcon" + PlanetRegistry.selectorPlanets[planetIndex].ID)) != null)
                        {
                            PlanetRegistry.planetButtonIcons[p - 2][i].GetComponent<MeshRenderer>().material = planetMat;
                            PlanetRegistry.planetButtonIcons[p - 2][i].transform.localScale *= 0.625f;
                        }
                        else
                        {
                            PlanetRegistry.planetButtonIcons[p - 2][i].GetComponent<MeshRenderer>().material = emptyIcon;
                        }
                    }
                }

                Transform buggedPlanetButton = InstanceTracker.GameScript.menuPlanets.transform.Find("14"); // Fixes problem with The Cathedral button.
                buggedPlanetButton.position = new Vector3(InstanceTracker.GameScript.menuPlanets.transform.Find("0").position.x, buggedPlanetButton.position.y, buggedPlanetButton.position.z);
                buggedPlanetButton.gameObject.SetActive(false);

                foreach (Transform hiddenPlanetButton in InstanceTracker.GameScript.menuPlanets.transform) // Ensure all other hidden planet buttons are inactive as well
                {
                    if (int.TryParse(hiddenPlanetButton.name, out int buttonIndex) && buttonIndex > 14)
                    {
                        hiddenPlanetButton.gameObject.SetActive(false);
                    }
                }
            }
        }

        private static AnimationClip BuildModMenuButtonAnimClip(bool reverse)
        {
            int reverseMult = reverse ? -1 : 1;
            AnimationClip clip = new AnimationClip
            {
                legacy = true
            };
            clip.SetCurve("", typeof(Transform), "m_LocalPosition.x", new AnimationCurve(new Keyframe(0f, reverseMult * 26.09399f, 288.2641f, 288.2641f), new Keyframe(0.1f, reverseMult * -2.732423f, 123.6389f, 123.6389f), new Keyframe(0.1666667f, 0f, -40.98634f, -40.98634f)));
            clip.SetCurve("", typeof(Transform), "m_LocalPosition.y", AnimationCurve.Linear(0, -13.5f, 0.1666667f, -13.5f));
            return clip;
        }

        private static void BuildModMenu()
        {
            GadgetCore.CoreLogger.Log("Injecting Mod Menu into Main Menu");

            ModMenu = new GameObject("MODMENU");
            ModMenu.SetActive(false);
            ModMenuBackButtonBeam = UnityEngine.Object.Instantiate(InstanceTracker.Menuu.menuOptions.transform.Find("beamm").gameObject, ModMenu.transform);
            ModMenuBackButtonBeam.name = "beamm";
            ModMenuBackButtonBeam.transform.localScale = new Vector3(30, 0, 1);
            ModMenuBackButtonBeam.transform.position = new Vector3(0, -13.5f, 1);
            ModMenuBackButtonHolder = UnityEngine.Object.Instantiate(InstanceTracker.Menuu.menuOptions.transform.Find("BUTTONHOLDER").gameObject, ModMenu.transform);
            ModMenuBackButtonHolder.name = "BUTTONHOLDER";
            ModMenuBackButtonHolder.transform.position = new Vector3(0, -13.5f, 0);
            ModMenuBackButtonHolder.GetComponent<Animation>().RemoveClip("bbbac2");
            ModMenuBackButtonHolder.GetComponent<Animation>().AddClip(BuildModMenuButtonAnimClip(true), "bbbac2");
            ModMenuBackButtonHolder.GetComponent<Animation>().clip = ModMenuBackButtonHolder.GetComponent<Animation>().GetClip("bbbac2");
            ModMenuCanvas = new GameObject("Mod Menu Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup)).GetComponent<Canvas>();
            ModMenuCanvas.GetComponent<CanvasGroup>().alpha = 0;
            ModMenuCanvas.GetComponent<CanvasGroup>().interactable = false;
            ModMenuCanvas.GetComponent<CanvasGroup>().blocksRaycasts = false;
            ModMenuCanvas.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
            ModMenuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            ModMenuCanvas.pixelPerfect = true;
            CanvasScaler scaler = ModMenuCanvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referencePixelsPerUnit = 100;
            ModConfigMenuText = UnityEngine.Object.Instantiate(InstanceTracker.Menuu.menuOptions.transform.Find("txt0").gameObject, ModMenu.transform);
            ModConfigMenuText.name = "txt0";
            ModConfigMenuText.transform.localPosition = new Vector3(0, 14, -1);
            Array.ForEach(ModConfigMenuText.GetComponentsInChildren<TextMesh>(), x => { x.text = "MOD CONFIG MENU"; x.anchor = TextAnchor.UpperCenter; });
            GameObject restartRequiredText = UnityEngine.Object.Instantiate(ModConfigMenuText, ModMenu.transform);
            restartRequiredText.SetActive(false);
            restartRequiredText.name = "Restart Required Text";
            restartRequiredText.transform.localPosition = new Vector3(0, -10.5f, -1);
            restartRequiredText.transform.localScale *= 0.75f;
            Array.ForEach(restartRequiredText.GetComponentsInChildren<TextMesh>(), x => { x.text = "Restart Required!"; x.anchor = TextAnchor.UpperCenter; });
            ModMenuPanel = new GameObject("Mod Menu", typeof(RectTransform), typeof(ModMenuController)).GetComponent<ModMenuController>();
            ModMenuPanel.GetComponent<RectTransform>().SetParent(ModMenuCanvas.transform);
            ModMenuPanel.GetComponent<RectTransform>().anchorMin = new Vector2(0.15f, 0.15f);
            ModMenuPanel.GetComponent<RectTransform>().anchorMax = new Vector2(0.85f, 0.85f);
            ModMenuPanel.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            ModMenuPanel.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            ModConfigMenus = new GameObject("Mod Config Menus", typeof(RectTransform)).GetComponent<RectTransform>();
            ModConfigMenus.SetParent(ModMenuCanvas.transform);
            ModConfigMenus.anchorMin = new Vector2(0.15f, 0.15f);
            ModConfigMenus.anchorMax = new Vector2(0.85f, 0.85f);
            ModConfigMenus.offsetMin = Vector2.zero;
            ModConfigMenus.offsetMax = Vector2.zero;
            Image background = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
            background.transform.SetParent(ModMenuPanel.transform);
            background.rectTransform.anchorMin = new Vector2(0f, 0f);
            background.rectTransform.anchorMax = new Vector2(01f, 1f);
            background.rectTransform.offsetMin = Vector2.zero;
            background.rectTransform.offsetMax = Vector2.zero;
            background.type = Image.Type.Sliced;
            background.fillCenter = true;
            background.sprite = BoxSprite;

            ModMenuDescPanel = new GameObject("Mod Desc Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(ScrollRect)).GetComponent<ScrollRect>();
            ModMenuDescPanel.GetComponent<RectTransform>().SetParent(ModMenuPanel.transform);
            ModMenuDescPanel.GetComponent<RectTransform>().anchorMin = new Vector2(0.4f, 0.25f);
            ModMenuDescPanel.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
            ModMenuDescPanel.GetComponent<RectTransform>().offsetMin = new Vector2(0, 5);
            ModMenuDescPanel.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -10);
            ModMenuDescPanel.GetComponent<Image>().sprite = BoxSprite;
            ModMenuDescPanel.GetComponent<Image>().type = Image.Type.Sliced;
            ModMenuDescPanel.GetComponent<Image>().fillCenter = true;
            Mask modMenuDescPanelMask = new GameObject("Mask", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask)).GetComponent<Mask>();
            modMenuDescPanelMask.GetComponent<RectTransform>().SetParent(ModMenuDescPanel.transform);
            modMenuDescPanelMask.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);
            modMenuDescPanelMask.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
            modMenuDescPanelMask.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            modMenuDescPanelMask.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            modMenuDescPanelMask.GetComponent<Image>().sprite = BoxMask;
            modMenuDescPanelMask.GetComponent<Image>().type = Image.Type.Sliced;
            modMenuDescPanelMask.GetComponent<Image>().fillCenter = true;
            modMenuDescPanelMask.showMaskGraphic = false;
            RectTransform modMenuDescViewport = new GameObject("Viewport", typeof(RectTransform)).GetComponent<RectTransform>();
            modMenuDescViewport.SetParent(modMenuDescPanelMask.transform);
            modMenuDescViewport.anchorMin = new Vector2(0f, 0f);
            modMenuDescViewport.anchorMax = new Vector2(1f, 1f);
            modMenuDescViewport.offsetMin = new Vector2(10, 10);
            modMenuDescViewport.offsetMax = new Vector2(-10, -10);
            Text modMenuDescText = new GameObject("Description", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(ContentSizeFitter)).GetComponent<Text>();
            modMenuDescText.rectTransform.SetParent(modMenuDescViewport);
            modMenuDescText.rectTransform.anchorMin = new Vector2(0f, 0f);
            modMenuDescText.rectTransform.anchorMax = new Vector2(1f, 1f);
            modMenuDescText.rectTransform.offsetMin = Vector2.zero;
            modMenuDescText.rectTransform.offsetMax = Vector2.zero;
            modMenuDescText.rectTransform.pivot = new Vector2(0.5f, 1f);
            modMenuDescText.font = ModConfigMenuText.GetComponent<TextMesh>().font;
            modMenuDescText.fontSize = 12;
            modMenuDescText.horizontalOverflow = HorizontalWrapMode.Wrap;
            modMenuDescText.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            ModMenuDescPanel.GetComponent<ScrollRect>().content = modMenuDescText.rectTransform;
            ModMenuDescPanel.GetComponent<ScrollRect>().horizontal = false;
            ModMenuDescPanel.GetComponent<ScrollRect>().scrollSensitivity = 5;
            ModMenuDescPanel.GetComponent<ScrollRect>().viewport = modMenuDescViewport;
            ModMenuPanel.descText = modMenuDescText;
            ModMenuPanel.restartRequiredText = restartRequiredText;

            Image modMenuButtonPanel = new GameObject("Button Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
            modMenuButtonPanel.GetComponent<RectTransform>().SetParent(ModMenuPanel.transform);
            modMenuButtonPanel.GetComponent<RectTransform>().anchorMin = new Vector2(0.4f, 0f);
            modMenuButtonPanel.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 0.25f);
            modMenuButtonPanel.GetComponent<RectTransform>().offsetMin = new Vector2(0, 10);
            modMenuButtonPanel.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -5);
            modMenuButtonPanel.GetComponent<Image>().sprite = BoxSprite;
            modMenuButtonPanel.GetComponent<Image>().type = Image.Type.Sliced;
            modMenuButtonPanel.GetComponent<Image>().fillCenter = true;
            ModMenuPanel.enableButton = new GameObject("Enable Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button)).GetComponent<Button>();
            ModMenuPanel.enableButton.GetComponent<RectTransform>().SetParent(modMenuButtonPanel.transform);
            ModMenuPanel.enableButton.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);
            ModMenuPanel.enableButton.GetComponent<RectTransform>().anchorMax = new Vector2(1f / 3f, 1f);
            ModMenuPanel.enableButton.GetComponent<RectTransform>().offsetMin = new Vector2(10, 10);
            ModMenuPanel.enableButton.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -10);
            ModMenuPanel.enableButton.GetComponent<Image>().sprite = BoxSprite;
            ModMenuPanel.enableButton.GetComponent<Image>().type = Image.Type.Sliced;
            ModMenuPanel.enableButton.GetComponent<Image>().fillCenter = true;
            ModMenuPanel.enableButton.targetGraphic = ModMenuPanel.enableButton.GetComponent<Image>();
            ModMenuPanel.enableButton.onClick.AddListener(ModMenuPanel.EnableButton);
            Text enableButtonText = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
            enableButtonText.rectTransform.SetParent(ModMenuPanel.enableButton.transform);
            enableButtonText.rectTransform.anchorMin = new Vector2(0f, 0f);
            enableButtonText.rectTransform.anchorMax = new Vector2(1f, 1f);
            enableButtonText.rectTransform.offsetMin = Vector2.zero;
            enableButtonText.rectTransform.offsetMax = Vector2.zero;
            enableButtonText.alignment = TextAnchor.MiddleCenter;
            enableButtonText.font = modMenuDescText.font;
            enableButtonText.fontSize = 12;
            enableButtonText.text = "Enable";
            ModMenuPanel.reloadButton = new GameObject("Reload Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button)).GetComponent<Button>();
            ModMenuPanel.reloadButton.GetComponent<RectTransform>().SetParent(modMenuButtonPanel.transform);
            ModMenuPanel.reloadButton.GetComponent<RectTransform>().anchorMin = new Vector2(1f / 3f, 0f);
            ModMenuPanel.reloadButton.GetComponent<RectTransform>().anchorMax = new Vector2(2f / 3f, 1f);
            ModMenuPanel.reloadButton.GetComponent<RectTransform>().offsetMin = new Vector2(10, 10);
            ModMenuPanel.reloadButton.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -10);
            ModMenuPanel.reloadButton.GetComponent<Image>().sprite = BoxSprite;
            ModMenuPanel.reloadButton.GetComponent<Image>().type = Image.Type.Sliced;
            ModMenuPanel.reloadButton.GetComponent<Image>().fillCenter = true;
            ModMenuPanel.reloadButton.targetGraphic = ModMenuPanel.reloadButton.GetComponent<Image>();
            ModMenuPanel.reloadButton.onClick.AddListener(ModMenuPanel.ReloadButton);
            Text reloadButtonText = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
            reloadButtonText.rectTransform.SetParent(ModMenuPanel.reloadButton.transform);
            reloadButtonText.rectTransform.anchorMin = new Vector2(0f, 0f);
            reloadButtonText.rectTransform.anchorMax = new Vector2(1f, 1f);
            reloadButtonText.rectTransform.offsetMin = Vector2.zero;
            reloadButtonText.rectTransform.offsetMax = Vector2.zero;
            reloadButtonText.alignment = TextAnchor.MiddleCenter;
            reloadButtonText.font = modMenuDescText.font;
            reloadButtonText.fontSize = 12;
            reloadButtonText.text = "Reload";
            ModMenuPanel.configButton = new GameObject("Config Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button)).GetComponent<Button>();
            ModMenuPanel.configButton.GetComponent<RectTransform>().SetParent(modMenuButtonPanel.transform);
            ModMenuPanel.configButton.GetComponent<RectTransform>().anchorMin = new Vector2(2f / 3f, 0f);
            ModMenuPanel.configButton.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
            ModMenuPanel.configButton.GetComponent<RectTransform>().offsetMin = new Vector2(10, 10);
            ModMenuPanel.configButton.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -10);
            ModMenuPanel.configButton.GetComponent<Image>().sprite = BoxSprite;
            ModMenuPanel.configButton.GetComponent<Image>().type = Image.Type.Sliced;
            ModMenuPanel.configButton.GetComponent<Image>().fillCenter = true;
            ModMenuPanel.configButton.targetGraphic = ModMenuPanel.configButton.GetComponent<Image>();
            ModMenuPanel.configButton.onClick.AddListener(ModMenuPanel.ConfigButton);
            Text configButtonText = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
            configButtonText.rectTransform.SetParent(ModMenuPanel.configButton.transform);
            configButtonText.rectTransform.anchorMin = new Vector2(0f, 0f);
            configButtonText.rectTransform.anchorMax = new Vector2(1f, 1f);
            configButtonText.rectTransform.offsetMin = Vector2.zero;
            configButtonText.rectTransform.offsetMax = Vector2.zero;
            configButtonText.alignment = TextAnchor.MiddleCenter;
            configButtonText.font = modMenuDescText.font;
            configButtonText.fontSize = 12;
            configButtonText.text = "Configure";
            if (GadgetCoreAPI.GetUMFAPI() != null)
            {
                ModMenuPanel.umfConfigButton = new GameObject("UMF Config Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button)).GetComponent<Button>();
                ModMenuPanel.umfConfigButton.GetComponent<RectTransform>().SetParent(ModMenuCanvas.transform);
                ModMenuPanel.umfConfigButton.GetComponent<RectTransform>().anchorMin = new Vector2(0.025f, 0.4f);
                ModMenuPanel.umfConfigButton.GetComponent<RectTransform>().anchorMax = new Vector2(0.125f, 0.6f);
                ModMenuPanel.umfConfigButton.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
                ModMenuPanel.umfConfigButton.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
                ModMenuPanel.umfConfigButton.GetComponent<Image>().sprite = BoxSprite;
                ModMenuPanel.umfConfigButton.GetComponent<Image>().type = Image.Type.Sliced;
                ModMenuPanel.umfConfigButton.GetComponent<Image>().fillCenter = true;
                ModMenuPanel.umfConfigButton.targetGraphic = ModMenuPanel.umfConfigButton.GetComponent<Image>();
                ModMenuPanel.umfConfigButton.onClick.AddListener(ModMenuPanel.UMFConfigButton);
                Text umfConfigButtonText = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
                umfConfigButtonText.rectTransform.SetParent(ModMenuPanel.umfConfigButton.transform);
                umfConfigButtonText.rectTransform.anchorMin = new Vector2(0f, 0f);
                umfConfigButtonText.rectTransform.anchorMax = new Vector2(1f, 1f);
                umfConfigButtonText.rectTransform.offsetMin = Vector2.zero;
                umfConfigButtonText.rectTransform.offsetMax = Vector2.zero;
                umfConfigButtonText.alignment = TextAnchor.MiddleCenter;
                umfConfigButtonText.font = modMenuDescText.font;
                umfConfigButtonText.fontSize = 12;
                umfConfigButtonText.text = "Configure UMF";
            }
            Button configReloadButton = new GameObject("Config Reload Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button)).GetComponent<Button>();
            configReloadButton.GetComponent<RectTransform>().SetParent(ModMenuCanvas.transform);
            configReloadButton.GetComponent<RectTransform>().anchorMin = new Vector2(0.875f, 0.4f);
            configReloadButton.GetComponent<RectTransform>().anchorMax = new Vector2(0.975f, 0.6f);
            configReloadButton.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
            configReloadButton.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
            configReloadButton.GetComponent<Image>().sprite = BoxSprite;
            configReloadButton.GetComponent<Image>().type = Image.Type.Sliced;
            configReloadButton.GetComponent<Image>().fillCenter = true;
            configReloadButton.targetGraphic = configReloadButton.GetComponent<Image>();
            configReloadButton.onClick.AddListener(() =>
            {
                foreach (GadgetInfo gadget in Gadgets.ListAllEnabledGadgetInfos())
                {
                    if (gadget.Attribute.AllowConfigReloading) gadget.Gadget.ReloadConfig();
                }
                if (GadgetCore.UMFAPI != null) foreach (string mod in GadgetCore.UMFAPI.GetModNames())
                {
                    GadgetCore.UMFAPI.SendCommand("cfgReload " + mod);
                }
                GadgetModConfigs.ResetAllConfigMenus();
            });
            Text configReloadButtonText = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
            configReloadButtonText.rectTransform.SetParent(configReloadButton.transform);
            configReloadButtonText.rectTransform.anchorMin = new Vector2(0f, 0f);
            configReloadButtonText.rectTransform.anchorMax = new Vector2(1f, 1f);
            configReloadButtonText.rectTransform.offsetMin = Vector2.zero;
            configReloadButtonText.rectTransform.offsetMax = Vector2.zero;
            configReloadButtonText.alignment = TextAnchor.MiddleCenter;
            configReloadButtonText.font = modMenuDescText.font;
            configReloadButtonText.fontSize = 12;
            configReloadButtonText.text = "Reload Configs";

            BuildModBrowser();

            GadgetModConfigs.ConfigMenus.Clear();
        }

        private static void BuildModBrowser()
        {
            ModBrowserPanel = new GameObject("Mod Browser", typeof(RectTransform), typeof(ModBrowser), typeof(CanvasRenderer), typeof(Image)).GetComponent<ModBrowser>();
            ModBrowserPanel.GetComponent<RectTransform>().SetParent(ModMenuCanvas.transform);
            ModBrowserPanel.GetComponent<RectTransform>().anchorMin = new Vector2(0.15f, 0.15f);
            ModBrowserPanel.GetComponent<RectTransform>().anchorMax = new Vector2(0.85f, 0.85f);
            ModBrowserPanel.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            ModBrowserPanel.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            ModBrowserPanel.GetComponent<Image>().type = Image.Type.Sliced;
            ModBrowserPanel.GetComponent<Image>().fillCenter = true;
            ModBrowserPanel.GetComponent<Image>().sprite = BoxSprite;

            ModBrowserDescPanel = new GameObject("Mod Desc Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(ScrollRect)).GetComponent<ScrollRect>();
            ModBrowserDescPanel.GetComponent<RectTransform>().SetParent(ModBrowserPanel.transform);
            ModBrowserDescPanel.GetComponent<RectTransform>().anchorMin = new Vector2(0.4f, 0.25f);
            ModBrowserDescPanel.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
            ModBrowserDescPanel.GetComponent<RectTransform>().offsetMin = new Vector2(0, 5);
            ModBrowserDescPanel.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -10);
            ModBrowserDescPanel.GetComponent<Image>().sprite = BoxSprite;
            ModBrowserDescPanel.GetComponent<Image>().type = Image.Type.Sliced;
            ModBrowserDescPanel.GetComponent<Image>().fillCenter = true;
            Mask modBrowserDescPanelMask = new GameObject("Mask", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask)).GetComponent<Mask>();
            modBrowserDescPanelMask.GetComponent<RectTransform>().SetParent(ModBrowserDescPanel.transform);
            modBrowserDescPanelMask.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);
            modBrowserDescPanelMask.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
            modBrowserDescPanelMask.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            modBrowserDescPanelMask.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            modBrowserDescPanelMask.GetComponent<Image>().sprite = BoxMask;
            modBrowserDescPanelMask.GetComponent<Image>().type = Image.Type.Sliced;
            modBrowserDescPanelMask.GetComponent<Image>().fillCenter = true;
            modBrowserDescPanelMask.showMaskGraphic = false;
            RectTransform modBrowserDescViewport = new GameObject("Viewport", typeof(RectTransform)).GetComponent<RectTransform>();
            modBrowserDescViewport.SetParent(modBrowserDescPanelMask.transform);
            modBrowserDescViewport.anchorMin = new Vector2(0f, 0f);
            modBrowserDescViewport.anchorMax = new Vector2(1f, 1f);
            modBrowserDescViewport.offsetMin = new Vector2(10, 10);
            modBrowserDescViewport.offsetMax = new Vector2(-10, -10);
            Text modBrowserDescText = new GameObject("Description", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(ContentSizeFitter)).GetComponent<Text>();
            modBrowserDescText.rectTransform.SetParent(modBrowserDescViewport);
            modBrowserDescText.rectTransform.anchorMin = new Vector2(0f, 0f);
            modBrowserDescText.rectTransform.anchorMax = new Vector2(1f, 1f);
            modBrowserDescText.rectTransform.offsetMin = Vector2.zero;
            modBrowserDescText.rectTransform.offsetMax = Vector2.zero;
            modBrowserDescText.rectTransform.pivot = new Vector2(0.5f, 1f);
            modBrowserDescText.font = ModConfigMenuText.GetComponent<TextMesh>().font;
            modBrowserDescText.fontSize = 12;
            modBrowserDescText.horizontalOverflow = HorizontalWrapMode.Wrap;
            modBrowserDescText.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            ModBrowserDescPanel.GetComponent<ScrollRect>().content = modBrowserDescText.rectTransform;
            ModBrowserDescPanel.GetComponent<ScrollRect>().horizontal = false;
            ModBrowserDescPanel.GetComponent<ScrollRect>().scrollSensitivity = 5;
            ModBrowserDescPanel.GetComponent<ScrollRect>().viewport = modBrowserDescViewport;
            ModBrowserPanel.DescText = modBrowserDescText;

            Image modBrowserButtonPanel = new GameObject("Button Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
            modBrowserButtonPanel.GetComponent<RectTransform>().SetParent(ModBrowserPanel.transform);
            modBrowserButtonPanel.GetComponent<RectTransform>().anchorMin = new Vector2(0.4f, 0f);
            modBrowserButtonPanel.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 0.25f);
            modBrowserButtonPanel.GetComponent<RectTransform>().offsetMin = new Vector2(0, 10);
            modBrowserButtonPanel.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -5);
            modBrowserButtonPanel.GetComponent<Image>().sprite = BoxSprite;
            modBrowserButtonPanel.GetComponent<Image>().type = Image.Type.Sliced;
            modBrowserButtonPanel.GetComponent<Image>().fillCenter = true;
            ModBrowserPanel.InstallButton = new GameObject("Download Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button)).GetComponent<Button>();
            ModBrowserPanel.InstallButton.GetComponent<RectTransform>().SetParent(modBrowserButtonPanel.transform);
            ModBrowserPanel.InstallButton.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);
            ModBrowserPanel.InstallButton.GetComponent<RectTransform>().anchorMax = new Vector2(1f / 3f, 1f);
            ModBrowserPanel.InstallButton.GetComponent<RectTransform>().offsetMin = new Vector2(10, 10);
            ModBrowserPanel.InstallButton.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -10);
            ModBrowserPanel.InstallButton.GetComponent<Image>().sprite = BoxSprite;
            ModBrowserPanel.InstallButton.GetComponent<Image>().type = Image.Type.Sliced;
            ModBrowserPanel.InstallButton.GetComponent<Image>().fillCenter = true;
            ModBrowserPanel.InstallButton.targetGraphic = ModBrowserPanel.InstallButton.GetComponent<Image>();
            ModBrowserPanel.InstallButton.onClick.AddListener(ModBrowserPanel.OnDownloadButton);
            ModBrowserPanel.InstallButton.interactable = false;
            Text installButtonText = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
            installButtonText.rectTransform.SetParent(ModBrowserPanel.InstallButton.transform);
            installButtonText.rectTransform.anchorMin = new Vector2(0f, 0f);
            installButtonText.rectTransform.anchorMax = new Vector2(1f, 1f);
            installButtonText.rectTransform.offsetMin = Vector2.zero;
            installButtonText.rectTransform.offsetMax = Vector2.zero;
            installButtonText.alignment = TextAnchor.MiddleCenter;
            installButtonText.font = modBrowserDescText.font;
            installButtonText.fontSize = 12;
            installButtonText.text = "Install";
            installButtonText.color = new Color(1f, 1f, 1f, 0.25f);
            ModBrowserPanel.ActivateButton = new GameObject("Activate Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button)).GetComponent<Button>();
            ModBrowserPanel.ActivateButton.GetComponent<RectTransform>().SetParent(modBrowserButtonPanel.transform);
            ModBrowserPanel.ActivateButton.GetComponent<RectTransform>().anchorMin = new Vector2(1f / 3f, 0f);
            ModBrowserPanel.ActivateButton.GetComponent<RectTransform>().anchorMax = new Vector2(2f / 3f, 1f);
            ModBrowserPanel.ActivateButton.GetComponent<RectTransform>().offsetMin = new Vector2(10, 10);
            ModBrowserPanel.ActivateButton.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -10);
            ModBrowserPanel.ActivateButton.GetComponent<Image>().sprite = BoxSprite;
            ModBrowserPanel.ActivateButton.GetComponent<Image>().type = Image.Type.Sliced;
            ModBrowserPanel.ActivateButton.GetComponent<Image>().fillCenter = true;
            ModBrowserPanel.ActivateButton.targetGraphic = ModBrowserPanel.ActivateButton.GetComponent<Image>();
            ModBrowserPanel.ActivateButton.onClick.AddListener(ModBrowserPanel.OnActivateButton);
            ModBrowserPanel.ActivateButton.interactable = false;
            Text activateButtonText = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
            activateButtonText.rectTransform.SetParent(ModBrowserPanel.ActivateButton.transform);
            activateButtonText.rectTransform.anchorMin = new Vector2(0f, 0f);
            activateButtonText.rectTransform.anchorMax = new Vector2(1f, 1f);
            activateButtonText.rectTransform.offsetMin = Vector2.zero;
            activateButtonText.rectTransform.offsetMax = Vector2.zero;
            activateButtonText.alignment = TextAnchor.MiddleCenter;
            activateButtonText.font = modBrowserDescText.font;
            activateButtonText.fontSize = 12;
            activateButtonText.text = "Activate";
            activateButtonText.color = new Color(1f, 1f, 1f, 0.25f);
            ModBrowserPanel.VersionsButton = new GameObject("Versions Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button)).GetComponent<Button>();
            ModBrowserPanel.VersionsButton.GetComponent<RectTransform>().SetParent(modBrowserButtonPanel.transform);
            ModBrowserPanel.VersionsButton.GetComponent<RectTransform>().anchorMin = new Vector2(2f / 3f, 0f);
            ModBrowserPanel.VersionsButton.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
            ModBrowserPanel.VersionsButton.GetComponent<RectTransform>().offsetMin = new Vector2(10, 10);
            ModBrowserPanel.VersionsButton.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -10);
            ModBrowserPanel.VersionsButton.GetComponent<Image>().sprite = BoxSprite;
            ModBrowserPanel.VersionsButton.GetComponent<Image>().type = Image.Type.Sliced;
            ModBrowserPanel.VersionsButton.GetComponent<Image>().fillCenter = true;
            ModBrowserPanel.VersionsButton.targetGraphic = ModBrowserPanel.VersionsButton.GetComponent<Image>();
            ModBrowserPanel.VersionsButton.onClick.AddListener(ModBrowserPanel.OnVersionsButton);
            ModBrowserPanel.VersionsButton.interactable = false;
            Text versionButtonText = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
            versionButtonText.rectTransform.SetParent(ModBrowserPanel.VersionsButton.transform);
            versionButtonText.rectTransform.anchorMin = new Vector2(0f, 0f);
            versionButtonText.rectTransform.anchorMax = new Vector2(1f, 1f);
            versionButtonText.rectTransform.offsetMin = Vector2.zero;
            versionButtonText.rectTransform.offsetMax = Vector2.zero;
            versionButtonText.alignment = TextAnchor.MiddleCenter;
            versionButtonText.font = modBrowserDescText.font;
            versionButtonText.fontSize = 12;
            versionButtonText.text = "Only Version";
            versionButtonText.color = new Color(1f, 1f, 1f, 0.25f);

            ModBrowserButton = new GameObject("Mod Browser Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button)).GetComponent<Button>();
            ModBrowserButton.GetComponent<RectTransform>().SetParent(ModMenuCanvas.transform);
            ModBrowserButton.GetComponent<RectTransform>().anchorMin = new Vector2(0.875f, 0.05f);
            ModBrowserButton.GetComponent<RectTransform>().anchorMax = new Vector2(0.975f, 0.20f);
            ModBrowserButton.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
            ModBrowserButton.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
            ModBrowserButton.GetComponent<Image>().sprite = BoxSprite;
            ModBrowserButton.GetComponent<Image>().type = Image.Type.Sliced;
            ModBrowserButton.GetComponent<Image>().fillCenter = true;
            ModBrowserButton.targetGraphic = ModBrowserButton.GetComponent<Image>();
            ModBrowserButton.onClick.AddListener(() => ModBrowser.ToggleModBrowser());
            Text modBrowserButtonText = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
            modBrowserButtonText.rectTransform.SetParent(ModBrowserButton.transform);
            modBrowserButtonText.rectTransform.anchorMin = new Vector2(0f, 0f);
            modBrowserButtonText.rectTransform.anchorMax = new Vector2(1f, 1f);
            modBrowserButtonText.rectTransform.offsetMin = Vector2.zero;
            modBrowserButtonText.rectTransform.offsetMax = Vector2.zero;
            modBrowserButtonText.alignment = TextAnchor.MiddleCenter;
            modBrowserButtonText.font = modBrowserDescText.font;
            modBrowserButtonText.fontSize = 12;
            modBrowserButtonText.text = "Mod Browser";

            ModBrowserPanel.BrowserButtonText = modBrowserButtonText;

            RectTransform listLoadingTextBackground = new GameObject("List Loading Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<RectTransform>();
            listLoadingTextBackground.GetComponent<RectTransform>().SetParent(ModBrowserPanel.transform);
            listLoadingTextBackground.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);
            listLoadingTextBackground.GetComponent<RectTransform>().anchorMax = new Vector2(0.3f, 1f);
            listLoadingTextBackground.GetComponent<RectTransform>().offsetMin = new Vector2(10, 10);
            listLoadingTextBackground.GetComponent<RectTransform>().offsetMax = new Vector2(0, -10);
            listLoadingTextBackground.GetComponent<Image>().sprite = BoxSprite;
            listLoadingTextBackground.GetComponent<Image>().type = Image.Type.Sliced;
            listLoadingTextBackground.GetComponent<Image>().fillCenter = true;
            ModBrowserPanel.ListLoadingText = new GameObject("List Loading Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
            ModBrowserPanel.ListLoadingText.rectTransform.SetParent(listLoadingTextBackground);
            ModBrowserPanel.ListLoadingText.rectTransform.anchorMin = new Vector2(0f, 0f);
            ModBrowserPanel.ListLoadingText.rectTransform.anchorMax = new Vector2(1f, 1f);
            ModBrowserPanel.ListLoadingText.rectTransform.offsetMin = Vector2.zero;
            ModBrowserPanel.ListLoadingText.rectTransform.offsetMax = Vector2.zero;
            ModBrowserPanel.ListLoadingText.alignment = TextAnchor.MiddleCenter;
            ModBrowserPanel.ListLoadingText.font = ModConfigMenuText.GetComponent<TextMesh>().font;
            ModBrowserPanel.ListLoadingText.fontSize = 20;
            ModBrowserPanel.ListLoadingText.text = string.Empty;
            RectTransform descLoadingTextBackground = new GameObject("Desc Loading Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<RectTransform>();
            descLoadingTextBackground.GetComponent<RectTransform>().SetParent(ModBrowserPanel.transform);
            descLoadingTextBackground.GetComponent<RectTransform>().anchorMin = new Vector2(0.4f, 0.25f);
            descLoadingTextBackground.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
            descLoadingTextBackground.GetComponent<RectTransform>().offsetMin = new Vector2(0, 5);
            descLoadingTextBackground.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -10);
            descLoadingTextBackground.GetComponent<Image>().sprite = BoxSprite;
            descLoadingTextBackground.GetComponent<Image>().type = Image.Type.Sliced;
            descLoadingTextBackground.GetComponent<Image>().fillCenter = true;
            ModBrowserPanel.DescLoadingText = new GameObject("Desc Loading Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
            ModBrowserPanel.DescLoadingText.rectTransform.SetParent(descLoadingTextBackground);
            ModBrowserPanel.DescLoadingText.rectTransform.anchorMin = new Vector2(0f, 0f);
            ModBrowserPanel.DescLoadingText.rectTransform.anchorMax = new Vector2(1f, 1f);
            ModBrowserPanel.DescLoadingText.rectTransform.offsetMin = Vector2.zero;
            ModBrowserPanel.DescLoadingText.rectTransform.offsetMax = Vector2.zero;
            ModBrowserPanel.DescLoadingText.alignment = TextAnchor.MiddleCenter;
            ModBrowserPanel.DescLoadingText.font = ModConfigMenuText.GetComponent<TextMesh>().font;
            ModBrowserPanel.DescLoadingText.fontSize = 20;
            ModBrowserPanel.DescLoadingText.text = string.Empty;
        }

        private static void BuildPersistantCanvas()
        {
            GadgetCore.CoreLogger.Log("Building Persistant Canvas");
            EventSystem eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule)).GetComponent<EventSystem>();
            UnityEngine.Object.DontDestroyOnLoad(eventSystem.gameObject);
            StandaloneInputModule inputModule = eventSystem.GetComponent<StandaloneInputModule>();
            inputModule.horizontalAxis = "Horizontal1";
            inputModule.verticalAxis = "Vertical1";
            inputModule.submitButton = "Jump";
            inputModule.cancelButton = "Cancel";

            PersistantCanvas = new GameObject("Persistant Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup)).GetComponent<Canvas>();
            UnityEngine.Object.DontDestroyOnLoad(PersistantCanvas.gameObject);
            PersistantCanvas.GetComponent<CanvasGroup>().alpha = 1;
            PersistantCanvas.GetComponent<CanvasGroup>().interactable = true;
            PersistantCanvas.GetComponent<CanvasGroup>().blocksRaycasts = true;
            PersistantCanvas.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
            PersistantCanvas.sortingOrder = 100;
            PersistantCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            PersistantCanvas.pixelPerfect = true;
            CanvasScaler scaler = PersistantCanvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.referencePixelsPerUnit = 100;

            GadgetConsole console = new GameObject("Console", typeof(RectTransform), typeof(GadgetConsole)).GetComponent<GadgetConsole>();
            console.gameObject.SetActive(false);
            console.transform.SetParent(PersistantCanvas.transform);
            console.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);
            console.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
            console.GetComponent<RectTransform>().offsetMin = new Vector2(10f, 10f);
            console.GetComponent<RectTransform>().offsetMax = new Vector2(-10f, -10f);
            console.InputField = new GameObject("Input Field", typeof(RectTransform), typeof(InputField), typeof(CanvasRenderer), typeof(Image)).GetComponent<InputField>();
            console.InputField.GetComponent<RectTransform>().SetParent(console.transform);
            console.InputField.GetComponent<RectTransform>().anchorMin = new Vector2(0.1f, 0.1f);
            console.InputField.GetComponent<RectTransform>().anchorMax = new Vector2(0.9f, 0.1f);
            console.InputField.GetComponent<RectTransform>().offsetMin = new Vector2(0f, 0f);
            console.InputField.GetComponent<RectTransform>().offsetMax = new Vector2(0f, 50f);
            console.InputField.GetComponent<Image>().sprite = BoxSprite;
            console.InputField.GetComponent<Image>().type = Image.Type.Sliced;
            console.InputField.GetComponent<Image>().fillCenter = true;
            Text inputText = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
            inputText.rectTransform.SetParent(console.InputField.transform);
            inputText.rectTransform.anchorMin = new Vector2(0f, 0f);
            inputText.rectTransform.anchorMax = new Vector2(1f, 1f);
            inputText.rectTransform.offsetMin = new Vector2(10f, 0f);
            inputText.rectTransform.offsetMax = new Vector2(-10f, 0f);
            inputText.alignment = TextAnchor.MiddleLeft;
            inputText.horizontalOverflow = HorizontalWrapMode.Overflow;
            inputText.font = ModMenuPanel.descText.font;
            inputText.fontSize = 24;
            console.InputField.textComponent = inputText;

            ScrollRect scrollableChat = new GameObject("Scrollable Chat", typeof(RectTransform), typeof(ScrollRect), typeof(CanvasRenderer), typeof(Image)).GetComponent<ScrollRect>();
            scrollableChat.GetComponent<RectTransform>().SetParent(console.transform);
            scrollableChat.GetComponent<RectTransform>().anchorMin = new Vector2(0.1f, 0.1f);
            scrollableChat.GetComponent<RectTransform>().anchorMax = new Vector2(0.9f, 1f);
            scrollableChat.GetComponent<RectTransform>().offsetMin = new Vector2(0f, 50f);
            scrollableChat.GetComponent<RectTransform>().offsetMax = new Vector2(0f, 0f);
            scrollableChat.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
            scrollableChat.GetComponent<Image>().sprite = BoxSprite;
            scrollableChat.GetComponent<Image>().type = Image.Type.Sliced;
            scrollableChat.GetComponent<Image>().fillCenter = true;
            scrollableChat.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.75f);
            scrollableChat.inertia = false;
            scrollableChat.horizontal = false;
            scrollableChat.scrollSensitivity = 5;
            scrollableChat.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            scrollableChat.movementType = ScrollRect.MovementType.Clamped;
            Scrollbar chatScrollBar = new GameObject("Scrollbar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Scrollbar)).GetComponent<Scrollbar>();
            chatScrollBar.GetComponent<RectTransform>().SetParent(console.transform);
            chatScrollBar.GetComponent<RectTransform>().anchorMin = new Vector2(0.9f, 0.1f);
            chatScrollBar.GetComponent<RectTransform>().anchorMax = new Vector2(0.9f, 1f);
            chatScrollBar.GetComponent<RectTransform>().offsetMin = new Vector2(0f, 0f);
            chatScrollBar.GetComponent<RectTransform>().offsetMax = new Vector2(50f, 0f);
            chatScrollBar.GetComponent<Image>().sprite = BoxSprite;
            chatScrollBar.GetComponent<Image>().type = Image.Type.Sliced;
            chatScrollBar.GetComponent<Image>().fillCenter = true;
            RectTransform charScrollBarHandle = new GameObject("Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<RectTransform>();
            charScrollBarHandle.SetParent(chatScrollBar.transform);
            charScrollBarHandle.anchorMin = new Vector2(0.05f, 0.05f);
            charScrollBarHandle.anchorMax = new Vector2(0.95f, 0.95f);
            charScrollBarHandle.offsetMin = Vector2.zero;
            charScrollBarHandle.offsetMax = Vector2.zero;
            charScrollBarHandle.GetComponent<Image>().sprite = BoxSprite;
            charScrollBarHandle.GetComponent<Image>().type = Image.Type.Sliced;
            charScrollBarHandle.GetComponent<Image>().fillCenter = true;
            chatScrollBar.targetGraphic = charScrollBarHandle.GetComponent<Image>();
            chatScrollBar.handleRect = charScrollBarHandle;
            chatScrollBar.direction = Scrollbar.Direction.BottomToTop;
            Mask chatViewport = new GameObject("Viewport", typeof(RectTransform), typeof(Mask), typeof(CanvasRenderer), typeof(Image)).GetComponent<Mask>();
            chatViewport.rectTransform.SetParent(scrollableChat.transform);
            chatViewport.rectTransform.anchorMin = new Vector2(0f, 0f);
            chatViewport.rectTransform.anchorMax = new Vector2(1f, 1f);
            chatViewport.rectTransform.offsetMin = new Vector2(0f, 0f);
            chatViewport.rectTransform.offsetMax = new Vector2(0f, 0f);
            chatViewport.rectTransform.pivot = Vector2.zero;
            chatViewport.showMaskGraphic = false;
            chatViewport.GetComponent<Image>().sprite = BoxSprite;
            chatViewport.GetComponent<Image>().type = Image.Type.Sliced;
            chatViewport.GetComponent<Image>().fillCenter = true;
            console.TextPanel = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter)).GetComponent<RectTransform>();
            console.TextPanel.SetParent(chatViewport.rectTransform);
            console.TextPanel.anchorMin = new Vector2(0f, 0f);
            console.TextPanel.anchorMax = new Vector2(1f, 1f);
            console.TextPanel.offsetMin = new Vector2(0f, 0f);
            console.TextPanel.offsetMax = new Vector2(0f, 0f);
            console.TextPanel.pivot = Vector2.zero;
            console.TextPanel.GetComponent<VerticalLayoutGroup>().padding = new RectOffset(10, 10, 10, 10);
            console.TextPanel.GetComponent<VerticalLayoutGroup>().spacing = 10;
            console.TextPanel.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.LowerLeft;
            console.TextPanel.GetComponent<VerticalLayoutGroup>().childControlWidth = true;
            console.TextPanel.GetComponent<VerticalLayoutGroup>().childControlHeight = true;
            console.TextPanel.GetComponent<VerticalLayoutGroup>().childForceExpandWidth = true;
            console.TextPanel.GetComponent<VerticalLayoutGroup>().childForceExpandHeight = true;
            console.TextPanel.GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            console.TextPanel.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scrollableChat.verticalScrollbar = chatScrollBar;
            scrollableChat.viewport = chatViewport.rectTransform;
            scrollableChat.content = console.TextPanel;
            console.AlwaysActivePanel = new GameObject("Always Active Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter)).GetComponent<RectTransform>();
            console.AlwaysActivePanel.SetParent(PersistantCanvas.transform);
            console.AlwaysActivePanel.anchorMin = new Vector2(0.1f, 0.1f);
            console.AlwaysActivePanel.anchorMax = new Vector2(0.9f, 1f);
            console.AlwaysActivePanel.offsetMin = new Vector2(10f, 60f);
            console.AlwaysActivePanel.offsetMax = new Vector2(-10f, -10f);
            console.AlwaysActivePanel.pivot = Vector2.zero;
            console.AlwaysActivePanel.GetComponent<VerticalLayoutGroup>().padding = new RectOffset(10, 10, 10, 10);
            console.AlwaysActivePanel.GetComponent<VerticalLayoutGroup>().spacing = 10;
            console.AlwaysActivePanel.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.LowerLeft;
            console.AlwaysActivePanel.GetComponent<VerticalLayoutGroup>().childControlWidth = true;
            console.AlwaysActivePanel.GetComponent<VerticalLayoutGroup>().childControlHeight = true;
            console.AlwaysActivePanel.GetComponent<VerticalLayoutGroup>().childForceExpandWidth = true;
            console.AlwaysActivePanel.GetComponent<VerticalLayoutGroup>().childForceExpandHeight = true;
            console.AlwaysActivePanel.GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            console.AlwaysActivePanel.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            GadgetConsole.PrintQueuedMessages();

            ConfirmationDialog = new GameObject("Confirmation Dialog", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            ConfirmationDialog.GetComponent<RectTransform>().SetParent(PersistantCanvas.transform);
            ConfirmationDialog.GetComponent<RectTransform>().anchorMin = new Vector2(0.25f, 0.25f);
            ConfirmationDialog.GetComponent<RectTransform>().anchorMax = new Vector2(0.75f, 0.75f);
            ConfirmationDialog.GetComponent<RectTransform>().offsetMin = new Vector2(10, 10);
            ConfirmationDialog.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -10);
            ConfirmationDialog.GetComponent<Image>().sprite = BoxSprite;
            ConfirmationDialog.GetComponent<Image>().type = Image.Type.Sliced;
            ConfirmationDialog.GetComponent<Image>().fillCenter = true;
            ConfirmationDialog.SetActive(false);
            Button yesButton = new GameObject("Yes Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button)).GetComponent<Button>();
            yesButton.GetComponent<RectTransform>().SetParent(ConfirmationDialog.transform);
            yesButton.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);
            yesButton.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.25f);
            yesButton.GetComponent<RectTransform>().offsetMin = new Vector2(10, 10);
            yesButton.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -10);
            yesButton.GetComponent<Image>().sprite = BoxSprite;
            yesButton.GetComponent<Image>().type = Image.Type.Sliced;
            yesButton.GetComponent<Image>().fillCenter = true;
            yesButton.targetGraphic = yesButton.GetComponent<Image>();
            ConfirmationYesText = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
            ConfirmationYesText.rectTransform.SetParent(yesButton.transform);
            ConfirmationYesText.rectTransform.anchorMin = new Vector2(0f, 0f);
            ConfirmationYesText.rectTransform.anchorMax = new Vector2(1f, 1f);
            ConfirmationYesText.rectTransform.offsetMin = Vector2.zero;
            ConfirmationYesText.rectTransform.offsetMax = Vector2.zero;
            ConfirmationYesText.alignment = TextAnchor.MiddleCenter;
            ConfirmationYesText.font = ModMenuPanel.descText.font;
            ConfirmationYesText.fontSize = 24;
            ConfirmationYesText.text = "Yes";
            Button noButton = new GameObject("No Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button)).GetComponent<Button>();
            noButton.GetComponent<RectTransform>().SetParent(ConfirmationDialog.transform);
            noButton.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0f);
            noButton.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 0.25f);
            noButton.GetComponent<RectTransform>().offsetMin = new Vector2(10, 10);
            noButton.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -10);
            noButton.GetComponent<Image>().sprite = BoxSprite;
            noButton.GetComponent<Image>().type = Image.Type.Sliced;
            noButton.GetComponent<Image>().fillCenter = true;
            noButton.targetGraphic = noButton.GetComponent<Image>();
            ConfirmationNoText = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
            ConfirmationNoText.rectTransform.SetParent(noButton.transform);
            ConfirmationNoText.rectTransform.anchorMin = new Vector2(0f, 0f);
            ConfirmationNoText.rectTransform.anchorMax = new Vector2(1f, 1f);
            ConfirmationNoText.rectTransform.offsetMin = Vector2.zero;
            ConfirmationNoText.rectTransform.offsetMax = Vector2.zero;
            ConfirmationNoText.alignment = TextAnchor.MiddleCenter;
            ConfirmationNoText.font = ModMenuPanel.descText.font;
            ConfirmationNoText.fontSize = 24;
            ConfirmationNoText.text = "No";
            yesButton.onClick.AddListener(() => {
                GadgetCoreAPI.CloseDialog();
                ConfirmationYesAction?.Invoke();
            });
            noButton.onClick.AddListener(() => {
                GadgetCoreAPI.CloseDialog();
                ConfirmationNoAction?.Invoke();
            });
            ConfirmationText = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
            ConfirmationText.rectTransform.SetParent(ConfirmationDialog.transform);
            ConfirmationText.rectTransform.anchorMin = new Vector2(0f, 0.25f);
            ConfirmationText.rectTransform.anchorMax = new Vector2(1f, 1f);
            ConfirmationText.rectTransform.offsetMin = Vector2.zero;
            ConfirmationText.rectTransform.offsetMax = Vector2.zero;
            ConfirmationText.alignment = TextAnchor.MiddleCenter;
            ConfirmationText.font = ModMenuPanel.descText.font;
            ConfirmationText.fontSize = 24;
            ConfirmationText.text = "";
        }
    }
}
