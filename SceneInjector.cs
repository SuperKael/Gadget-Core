using GadgetCore.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UModFramework.API;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GadgetCore
{
    internal static class SceneInjector
    {
        public static GameObject modMenuBeam { get; internal set; }
        public static GameObject modMenuButtonHolder { get; internal set; }
        public static GameObject modMenu { get; internal set; }
        public static GameObject modMenuBackButtonBeam { get; internal set; }
        public static GameObject modMenuBackButtonHolder { get; internal set; }
        public static ModDescPanelController modMenuDescPanel { get; internal set; }

        internal static void InjectMainMenu()
        {
            GadgetCore.Log("Injecting Mod Menu into Main Menu");

            GameObject mainMenu = InstanceTracker.menuu.menuMain;
            Array.ForEach(mainMenu.GetComponentsInChildren<Animation>(), x => x.enabled = true);
            modMenuBeam = UnityEngine.Object.Instantiate(mainMenu.transform.Find("beamm").gameObject, mainMenu.transform);
            modMenuBeam.name = "beamm";
            modMenuBeam.transform.localScale = new Vector3(30, 0, 1);
            modMenuBeam.transform.position = new Vector3(0, -13.5f, 1);
            modMenuButtonHolder = UnityEngine.Object.Instantiate(mainMenu.transform.Find("BUTTONHOLDER").gameObject, mainMenu.transform);
            modMenuButtonHolder.name = "BUTTONHOLDER";
            modMenuButtonHolder.transform.position = new Vector3(0, -13.5f, 0);
            modMenuButtonHolder.GetComponent<Animation>().RemoveClip("enterr1");
            modMenuButtonHolder.GetComponent<Animation>().AddClip(BuildModMenuButtonAnimClip(false), "enterr1");
            modMenuButtonHolder.GetComponent<Animation>().clip = modMenuButtonHolder.GetComponent<Animation>().GetClip("enterr1");
            GameObject bModMenu = modMenuButtonHolder.transform.GetChild(0).gameObject;
            bModMenu.name = "bModMenu";
            Array.ForEach(bModMenu.GetComponentsInChildren<TextMesh>(), x => x.text = "MOD MENU");
            modMenuBeam.GetComponent<Animation>().Play();
            modMenuButtonHolder.GetComponent<Animation>().Play();
            BuildModMenu();
        }

        internal static void InjectIngame()
        {

        }

        private static AnimationClip BuildModMenuButtonAnimClip(bool reverse)
        {
            int reverseMult = reverse ? -1 : 1;
            AnimationClip clip = new AnimationClip();
            clip.legacy = true;
            clip.SetCurve("", typeof(Transform), "m_LocalPosition.x", new AnimationCurve(new Keyframe(0f, reverseMult * 26.09399f, 288.2641f, 288.2641f), new Keyframe(0.1f, reverseMult * -2.732423f, 123.6389f, 123.6389f), new Keyframe(0.1666667f, 0f, -40.98634f, -40.98634f)));
            clip.SetCurve("", typeof(Transform), "m_LocalPosition.y", AnimationCurve.Linear(0, -13.5f, 0.1666667f, -13.5f));
            return clip;
        }

        private static void BuildModMenu()
        {
            modMenu = new GameObject("MODMENU");
            modMenu.SetActive(false);
            modMenuBackButtonBeam = UnityEngine.Object.Instantiate(InstanceTracker.menuu.menuOptions.transform.Find("beamm").gameObject, modMenu.transform);
            modMenuBackButtonBeam.name = "beamm";
            modMenuBackButtonBeam.transform.localScale = new Vector3(30, 0, 1);
            modMenuBackButtonBeam.transform.position = new Vector3(0, -13.5f, 1);
            modMenuBackButtonHolder = UnityEngine.Object.Instantiate(InstanceTracker.menuu.menuOptions.transform.Find("BUTTONHOLDER").gameObject, modMenu.transform);
            modMenuBackButtonHolder.name = "BUTTONHOLDER";
            modMenuBackButtonHolder.transform.position = new Vector3(0, -13.5f, 0);
            modMenuBackButtonHolder.GetComponent<Animation>().RemoveClip("bbbac2");
            modMenuBackButtonHolder.GetComponent<Animation>().AddClip(BuildModMenuButtonAnimClip(true), "bbbac2");
            modMenuBackButtonHolder.GetComponent<Animation>().clip = modMenuBackButtonHolder.GetComponent<Animation>().GetClip("bbbac2");
            Canvas canvas = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();
            canvas.transform.SetParent(modMenu.transform);
            canvas.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            scaler.scaleFactor = 2;
            scaler.referencePixelsPerUnit = 100;
            GameObject modConfigMenuText = UnityEngine.Object.Instantiate(InstanceTracker.menuu.menuOptions.transform.Find("txt0").gameObject, modMenu.transform);
            modConfigMenuText.name = "txt0";
            modConfigMenuText.transform.localPosition = new Vector3(0, 14, -1);
            Array.ForEach(modConfigMenuText.GetComponentsInChildren<TextMesh>(), x => { x.text = "MOD CONFIG MENU"; x.anchor = TextAnchor.UpperCenter; });
            RectTransform panel = new GameObject("Panel", typeof(RectTransform)).GetComponent<RectTransform>();
            panel.SetParent(canvas.transform);
            panel.anchorMin = new Vector2(0.15f, 0.15f);
            panel.anchorMax = new Vector2(0.85f, 0.85f);
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;
            Image background = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
            background.transform.SetParent(panel);
            background.rectTransform.anchorMin = new Vector2(0f, 0f);
            background.rectTransform.anchorMax = new Vector2(01f, 1f);
            background.rectTransform.offsetMin = Vector2.zero;
            background.rectTransform.offsetMax = Vector2.zero;
            background.type = Image.Type.Sliced;
            background.fillCenter = true;
            Texture2D boxTex = UMFAsset.LoadTexture2D("modmenu.png");
            boxTex.filterMode = FilterMode.Point;
            Sprite boxSprite = Sprite.Create(boxTex, new Rect(0, 0, boxTex.width, boxTex.height), new Vector2(0.5f, 0.5f), 100, 1, SpriteMeshType.Tight, new Vector4(15, 15, 15, 15));
            background.sprite = boxSprite;

            EventSystem eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule)).GetComponent<EventSystem>();
            StandaloneInputModule inputModule = eventSystem.GetComponent<StandaloneInputModule>();
            inputModule.horizontalAxis = "Horizontal1";
            inputModule.verticalAxis = "Vertical1";
            inputModule.submitButton = "Jump";
            inputModule.cancelButton = "Cancel";

            modMenuDescPanel = new GameObject("Mod Desc Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(ModDescPanelController), typeof(Mask), typeof(ScrollRect)).GetComponent<ModDescPanelController>();
            modMenuDescPanel.GetComponent<RectTransform>().SetParent(panel);
            modMenuDescPanel.GetComponent<RectTransform>().anchorMin = new Vector2(0.4f, 0.25f);
            modMenuDescPanel.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
            modMenuDescPanel.GetComponent<RectTransform>().offsetMin = new Vector2(0, 5);
            modMenuDescPanel.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -10);
            modMenuDescPanel.GetComponent<Image>().sprite = boxSprite;
            modMenuDescPanel.GetComponent<Image>().type = Image.Type.Sliced;
            modMenuDescPanel.GetComponent<Image>().fillCenter = true;
            RectTransform modMenuDescViewport = new GameObject("Viewport", typeof(RectTransform)).GetComponent<RectTransform>();
            modMenuDescViewport.SetParent(modMenuDescPanel.transform);
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
            modMenuDescText.font = modConfigMenuText.GetComponent<TextMesh>().font;
            modMenuDescText.fontSize = 12;
            modMenuDescText.horizontalOverflow = HorizontalWrapMode.Wrap;
            modMenuDescText.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            modMenuDescPanel.GetComponent<ScrollRect>().content = modMenuDescText.rectTransform;
            modMenuDescPanel.GetComponent<ScrollRect>().horizontal = false;
            modMenuDescPanel.GetComponent<ScrollRect>().scrollSensitivity = 5;
            modMenuDescPanel.GetComponent<ScrollRect>().viewport = modMenuDescViewport;
            modMenuDescPanel.descText = modMenuDescText;
            Image modMenuButtonPanel = new GameObject("Button Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
            modMenuButtonPanel.GetComponent<RectTransform>().SetParent(panel);
            modMenuButtonPanel.GetComponent<RectTransform>().anchorMin = new Vector2(0.4f, 0f);
            modMenuButtonPanel.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 0.25f);
            modMenuButtonPanel.GetComponent<RectTransform>().offsetMin = new Vector2(0, 10);
            modMenuButtonPanel.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -5);
            modMenuButtonPanel.GetComponent<Image>().sprite = boxSprite;
            modMenuButtonPanel.GetComponent<Image>().type = Image.Type.Sliced;
            modMenuButtonPanel.GetComponent<Image>().fillCenter = true;
            modMenuDescPanel.configButton = new GameObject("Config Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button)).GetComponent<Button>();
            modMenuDescPanel.configButton.GetComponent<RectTransform>().SetParent(modMenuButtonPanel.transform);
            modMenuDescPanel.configButton.GetComponent<RectTransform>().anchorMin = new Vector2(2f / 3f, 0f);
            modMenuDescPanel.configButton.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
            modMenuDescPanel.configButton.GetComponent<RectTransform>().offsetMin = new Vector2(10, 10);
            modMenuDescPanel.configButton.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -10);
            modMenuDescPanel.configButton.GetComponent<Image>().sprite = boxSprite;
            modMenuDescPanel.configButton.GetComponent<Image>().type = Image.Type.Sliced;
            modMenuDescPanel.configButton.GetComponent<Image>().fillCenter = true;
            modMenuDescPanel.configButton.targetGraphic = modMenuDescPanel.configButton.GetComponent<Image>();
            modMenuDescPanel.configButton.onClick.AddListener(modMenuDescPanel.ConfigButton);
            Text configButtonText = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
            configButtonText.rectTransform.SetParent(modMenuDescPanel.configButton.transform);
            configButtonText.rectTransform.anchorMin = new Vector2(0f, 0f);
            configButtonText.rectTransform.anchorMax = new Vector2(1f, 1f);
            configButtonText.rectTransform.offsetMin = Vector2.zero;
            configButtonText.rectTransform.offsetMax = Vector2.zero;
            configButtonText.alignment = TextAnchor.MiddleCenter;
            configButtonText.font = modMenuDescText.font;
            configButtonText.fontSize = 12;
            configButtonText.text = "Configure";
            modMenuDescPanel.enableUMFButton = new GameObject("Enable UMF Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button)).GetComponent<Button>();
            modMenuDescPanel.enableUMFButton.GetComponent<RectTransform>().SetParent(modMenuButtonPanel.transform);
            modMenuDescPanel.enableUMFButton.GetComponent<RectTransform>().anchorMin = new Vector2(1f / 3f, 0f);
            modMenuDescPanel.enableUMFButton.GetComponent<RectTransform>().anchorMax = new Vector2(2f / 3f, 1f);
            modMenuDescPanel.enableUMFButton.GetComponent<RectTransform>().offsetMin = new Vector2(10, 10);
            modMenuDescPanel.enableUMFButton.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -10);
            modMenuDescPanel.enableUMFButton.GetComponent<Image>().sprite = boxSprite;
            modMenuDescPanel.enableUMFButton.GetComponent<Image>().type = Image.Type.Sliced;
            modMenuDescPanel.enableUMFButton.GetComponent<Image>().fillCenter = true;
            modMenuDescPanel.enableUMFButton.targetGraphic = modMenuDescPanel.enableUMFButton.GetComponent<Image>();
            modMenuDescPanel.enableUMFButton.onClick.AddListener(modMenuDescPanel.EnableUMFButton);
            Text enableUMFButtonText = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
            enableUMFButtonText.rectTransform.SetParent(modMenuDescPanel.enableUMFButton.transform);
            enableUMFButtonText.rectTransform.anchorMin = new Vector2(0f, 0f);
            enableUMFButtonText.rectTransform.anchorMax = new Vector2(1f, 1f);
            enableUMFButtonText.rectTransform.offsetMin = Vector2.zero;
            enableUMFButtonText.rectTransform.offsetMax = Vector2.zero;
            enableUMFButtonText.alignment = TextAnchor.MiddleCenter;
            enableUMFButtonText.font = modMenuDescText.font;
            enableUMFButtonText.material = configButtonText.font.material;
            enableUMFButtonText.fontSize = 12;
            enableUMFButtonText.text = "Enable Mod";
            modMenuDescPanel.enableButton = new GameObject("Enable UMF Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button)).GetComponent<Button>();
            modMenuDescPanel.enableButton.GetComponent<RectTransform>().SetParent(modMenuButtonPanel.transform);
            modMenuDescPanel.enableButton.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);
            modMenuDescPanel.enableButton.GetComponent<RectTransform>().anchorMax = new Vector2(1f / 3f, 1f);
            modMenuDescPanel.enableButton.GetComponent<RectTransform>().offsetMin = new Vector2(10, 10);
            modMenuDescPanel.enableButton.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -10);
            modMenuDescPanel.enableButton.GetComponent<Image>().sprite = boxSprite;
            modMenuDescPanel.enableButton.GetComponent<Image>().type = Image.Type.Sliced;
            modMenuDescPanel.enableButton.GetComponent<Image>().fillCenter = true;
            modMenuDescPanel.enableButton.targetGraphic = modMenuDescPanel.enableButton.GetComponent<Image>();
            modMenuDescPanel.enableButton.onClick.AddListener(modMenuDescPanel.EnableButton);
            Text enableButtonText = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
            enableButtonText.rectTransform.SetParent(modMenuDescPanel.enableButton.transform);
            enableButtonText.rectTransform.anchorMin = new Vector2(0f, 0f);
            enableButtonText.rectTransform.anchorMax = new Vector2(1f, 1f);
            enableButtonText.rectTransform.offsetMin = Vector2.zero;
            enableButtonText.rectTransform.offsetMax = Vector2.zero;
            enableButtonText.alignment = TextAnchor.MiddleCenter;
            enableButtonText.font = modMenuDescText.font;
            enableButtonText.fontSize = 12;
            enableButtonText.text = "Enable";

            GadgetModInfo[] gadgetMods = GadgetMods.ListAllModInfos();
            string[] allMods = gadgetMods.Select(x => x.Attribute.Name).Concat(GadgetCore.nonGadgetMods).Concat(GadgetCore.disabledMods).ToArray();
            int gadgetModCount = GadgetMods.CountMods();
            int normalModCount = GadgetCore.nonGadgetMods.Count;
            int disabledModCount = GadgetCore.disabledMods.Count;
            ScrollRect modListScrollView = new GameObject("Scroll View", typeof(RectTransform), typeof(ScrollRect), typeof(CanvasRenderer), typeof(Image)).GetComponent<ScrollRect>();
            modListScrollView.GetComponent<RectTransform>().SetParent(panel);
            modListScrollView.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);
            modListScrollView.GetComponent<RectTransform>().anchorMax = new Vector2(0.3f, 1f);
            modListScrollView.GetComponent<RectTransform>().offsetMin = new Vector2(10, 10);
            modListScrollView.GetComponent<RectTransform>().offsetMax = new Vector2(0, -10);
            modListScrollView.GetComponent<Image>().sprite = boxSprite;
            modListScrollView.GetComponent<Image>().type = Image.Type.Sliced;
            modListScrollView.GetComponent<Image>().fillCenter = true;
            RectTransform modListViewport = new GameObject("Viewport", typeof(RectTransform), typeof(Mask), typeof(CanvasRenderer), typeof(Image)).GetComponent<RectTransform>();
            modListViewport.SetParent(modListScrollView.transform);
            modListViewport.anchorMin = new Vector2(0f, 0f);
            modListViewport.anchorMax = new Vector2(1f, 1f);
            modListViewport.offsetMin = Vector2.zero;
            modListViewport.offsetMax = Vector2.zero;
            modListViewport.GetComponent<Image>().sprite = boxSprite;
            modListViewport.GetComponent<Image>().type = Image.Type.Sliced;
            modListViewport.GetComponent<Image>().fillCenter = true;
            RectTransform modList = new GameObject("Content", typeof(RectTransform), typeof(ToggleGroup)).GetComponent<RectTransform>();
            modList.SetParent(modListViewport);
            modList.anchorMin = new Vector2(0f, 0f);
            modList.anchorMax = new Vector2(1f, allMods.Length <= 5 ? 1f : (allMods.Length / 5f));
            modList.offsetMin = new Vector2(10, 10);
            modList.offsetMax = new Vector2(-10, -10);
            Scrollbar modListScrollBar = new GameObject("Scrollbar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Scrollbar)).GetComponent<Scrollbar>();
            modListScrollBar.GetComponent<RectTransform>().SetParent(modListScrollView.transform);
            modListScrollBar.GetComponent<RectTransform>().anchorMin = new Vector2(1f, 0f);
            modListScrollBar.GetComponent<RectTransform>().anchorMax = new Vector2(1.25f, 1f);
            modListScrollBar.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            modListScrollBar.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            modListScrollBar.GetComponent<Image>().sprite = boxSprite;
            modListScrollBar.GetComponent<Image>().type = Image.Type.Sliced;
            modListScrollBar.GetComponent<Image>().fillCenter = true;
            RectTransform modListScrollBarHandle = new GameObject("Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<RectTransform>();
            modListScrollBarHandle.SetParent(modListScrollBar.transform);
            modListScrollBarHandle.anchorMin = new Vector2(0.05f, 0.05f);
            modListScrollBarHandle.anchorMax = new Vector2(0.95f, 0.95f);
            modListScrollBarHandle.offsetMin = Vector2.zero;
            modListScrollBarHandle.offsetMax = Vector2.zero;
            modListScrollBarHandle.GetComponent<Image>().sprite = boxSprite;
            modListScrollBarHandle.GetComponent<Image>().type = Image.Type.Sliced;
            modListScrollBarHandle.GetComponent<Image>().fillCenter = true;
            modListScrollBar.targetGraphic = modListScrollBarHandle.GetComponent<Image>();
            modListScrollBar.handleRect = modListScrollBarHandle;
            modListScrollBar.direction = Scrollbar.Direction.TopToBottom;
            if (allMods.Length <= 5) modListScrollBar.interactable = false;
            modListScrollView.content = modList;
            modListScrollView.horizontal = false;
            modListScrollView.scrollSensitivity = 5;
            modListScrollView.movementType = ScrollRect.MovementType.Clamped;
            modListScrollView.viewport = modListViewport;
            modListScrollView.verticalScrollbar = modListScrollBar;
            float entryHeight = allMods.Length <= 5 ? 0.2f : 1f / allMods.Length;
            Toggle firstToggle = null;
            for (int i = 0; i < allMods.Length; i++)
            {
                RectTransform modEntry = new GameObject("Mod Entry: " + allMods[i], typeof(RectTransform), typeof(Toggle), typeof(CanvasRenderer), typeof(Image), typeof(Mask)).GetComponent<RectTransform>();
                modEntry.SetParent(modList);
                modEntry.anchorMin = new Vector2(0f, 1 - ((i + 1) * entryHeight));
                modEntry.anchorMax = new Vector2(1f, 1 - (i * entryHeight));
                modEntry.offsetMin = Vector2.zero;
                modEntry.offsetMax = Vector2.zero;
                Toggle modToggle = modEntry.GetComponent<Toggle>();
                Image modSelected = new GameObject("Selected", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
                modSelected.rectTransform.SetParent(modEntry);
                modSelected.rectTransform.anchorMin = new Vector2(0f, 0f);
                modSelected.rectTransform.anchorMax = new Vector2(1f, 1f);
                modSelected.rectTransform.offsetMin = Vector2.zero;
                modSelected.rectTransform.offsetMax = Vector2.zero;
                modSelected.sprite = boxSprite;
                modSelected.type = Image.Type.Sliced;
                Text modLabel = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
                modLabel.rectTransform.SetParent(modEntry);
                modLabel.rectTransform.anchorMin = new Vector2(0f, 0f);
                modLabel.rectTransform.anchorMax = new Vector2(1f, 1f);
                modLabel.rectTransform.offsetMin = new Vector2(10, 10);
                modLabel.rectTransform.offsetMax = new Vector2(-10, -10);
                modLabel.font = modConfigMenuText.GetComponent<TextMesh>().font;
                modLabel.fontSize = 12;
                modLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
                modLabel.text = allMods[i] + Environment.NewLine + (i < gadgetModCount ? ("Gadget Mod (" + GadgetMods.GetModInfo(allMods[i]).UMFName + ")") : (i < gadgetModCount + GadgetCore.nonGadgetMods.Count ? "Non-Gadget Mod" : "Disabled"));
                if ((i < gadgetModCount && !gadgetMods[i].Enabled) || i >= gadgetModCount + GadgetCore.nonGadgetMods.Count) modLabel.color = new Color(1f, 1f, 1f, 0.5f);
                modToggle.GetComponent<Image>().sprite = boxSprite;
                modToggle.GetComponent<Image>().type = Image.Type.Sliced;
                modToggle.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.25f);
                modToggle.transition = Selectable.Transition.None;
                modToggle.isOn = i == 0;
                modToggle.toggleTransition = Toggle.ToggleTransition.None;
                modToggle.graphic = modSelected;
                modToggle.group = modList.GetComponent<ToggleGroup>();
                int toggleIndex = i;
                if (i == 0) firstToggle = modToggle;
                modToggle.onValueChanged.AddListener((toggled) => { if (toggled) modMenuDescPanel.UpdateInfo(modToggle, toggleIndex); });
            }

            if (allMods.Length > 0) modMenuDescPanel.UpdateInfo(firstToggle, 0);
        }
    }
}
