using GadgetCore.API;
using GadgetCore.API.ConfigMenu;
using System;
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
        public static RectTransform ModConfigMenus { get; internal set; }
        public static GameObject ModMenuBackButtonBeam { get; internal set; }
        public static GameObject ModMenuBackButtonHolder { get; internal set; }
        public static GameObject ModConfigMenuText { get; internal set; }
        public static ScrollRect ModMenuDescPanel { get; internal set; }

        public static Sprite BoxSprite { get; internal set; }
        public static Sprite BoxMask { get; internal set; }
        public static Sprite BarSprite { get; internal set; }

        internal static void InjectMainMenu()
        {
            GadgetCore.Log("Injecting objects into Main Menu");

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
            ModMenuButtonHolder.transform.position = new Vector3(0, -13.5f, 0);
            ModMenuButtonHolder.GetComponent<Animation>().RemoveClip("enterr1");
            ModMenuButtonHolder.GetComponent<Animation>().AddClip(BuildModMenuButtonAnimClip(false), "enterr1");
            ModMenuButtonHolder.GetComponent<Animation>().clip = ModMenuButtonHolder.GetComponent<Animation>().GetClip("enterr1");
            GameObject bModMenu = ModMenuButtonHolder.transform.GetChild(0).gameObject;
            bModMenu.name = "bModMenu";
            Array.ForEach(bModMenu.GetComponentsInChildren<TextMesh>(), x => x.text = "MOD MENU");
            ModMenuBeam.GetComponent<Animation>().Play();
            ModMenuButtonHolder.GetComponent<Animation>().Play();
            BuildModMenu();
            if (PersistantCanvas == null) BuildPersistantCanvas();
        }

        internal static void InjectIngame()
        {
            GadgetCoreAPI.menus = new List<GameObject>();
            for (int i = 0;i < GadgetCoreAPI.menuPaths.Count;i++)
            {
                GameObject menu = Resources.Load(GadgetCoreAPI.menuPaths[i]) as GameObject;
                menu.transform.SetParent(InstanceTracker.MainCamera.transform);
                menu.SetActive(false);
                GadgetCoreAPI.menus.Add(menu);
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
            GadgetCore.Log("Injecting Mod Menu into Main Menu");

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
            scaler.scaleFactor = 2;
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
            ModMenuPanel = new GameObject("Panel", typeof(RectTransform), typeof(ModMenuController)).GetComponent<ModMenuController>();
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
        }

        private static void BuildPersistantCanvas()
        {
            GadgetCore.Log("Building Persistant Canvas");
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

            ScrollRect scrollableChat = new GameObject("Scrollable Chat", typeof(RectTransform), typeof(ScrollRect)).GetComponent<ScrollRect>();
            scrollableChat.GetComponent<RectTransform>().SetParent(console.transform);
            scrollableChat.GetComponent<RectTransform>().anchorMin = new Vector2(0.1f, 0.1f);
            scrollableChat.GetComponent<RectTransform>().anchorMax = new Vector2(0.9f, 1f);
            scrollableChat.GetComponent<RectTransform>().offsetMin = new Vector2(0f, 50f);
            scrollableChat.GetComponent<RectTransform>().offsetMax = new Vector2(0f, 0f);
            scrollableChat.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
            scrollableChat.inertia = false;
            scrollableChat.horizontal = false;
            scrollableChat.scrollSensitivity = 5;
            scrollableChat.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            scrollableChat.movementType = ScrollRect.MovementType.Clamped;
            Scrollbar chatScrollBar = new GameObject("Scrollbar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Scrollbar)).GetComponent<Scrollbar>();
            chatScrollBar.GetComponent<RectTransform>().SetParent(console.transform);
            chatScrollBar.GetComponent<RectTransform>().anchorMin = new Vector2(1f, 0.1f);
            chatScrollBar.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
            chatScrollBar.GetComponent<RectTransform>().offsetMin = new Vector2(-50f, 0f);
            chatScrollBar.GetComponent<RectTransform>().offsetMax = new Vector2(0f, 0f);
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
