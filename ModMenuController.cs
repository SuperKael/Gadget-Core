using GadgetCore.API;
using GadgetCore.API.ConfigMenu;
using GadgetCore.Loader;
using GadgetCore.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GadgetCore
{
    internal partial class ModMenuController : MonoBehaviour
    {
        private static Dictionary<string, string> originalConfig = new Dictionary<string, string>();
        private static Dictionary<string, bool> wasEnabled = new Dictionary<string, bool>();
        private static List<string> unpackedMods = new List<string>();
        private static List<string> modsToToggle = new List<string>();

        internal static List<ModMenuEntry> modEntries = new List<ModMenuEntry>();
        internal static List<ModMenuEntry> umfModEntries = new List<ModMenuEntry>();
        internal static List<Process> ConfigHandles { get; private set; } = new List<Process>();

        private Toggle toggle;
        private int modIndex = 0;
        private int gadgetIndex = -1;
        private float scrollPositionCache = -1;
        internal Text descText;
        internal GameObject restartRequiredText;
        internal Button umfConfigButton;
        internal Button enableButton;
        internal Button reloadButton;
        internal Button configButton;

        private Vector2 canvasSize;

        internal static bool RestartNeeded { get; private set; } = false;

        internal void Build()
        {
            string modInfo = null;
            foreach (GadgetMod mod in GadgetMods.ListAllMods().Where(x => x.LoadedGadgets.Count > 0))
            {
                if (mod.Name == "GadgetCore") modInfo = CoreMod.GadgetCoreMod.GetDesc();
                else if (mod.HasModFile("ModInfo.txt")) using (GadgetModFile infoFile = mod.GetModFile("ModInfo.txt")) modInfo = infoFile.ReadAllText();
                if (string.IsNullOrEmpty(modInfo) || modInfo == "Insert the description for your mod here!") modInfo = "This Gadget mod does not have a ModInfo file.";
                Dictionary<string, string> info = new Dictionary<string, string>
                {
                    ["Name"] = mod.Name,
                    ["Version"] = mod.Version.ToString(),
                    ["File Name"] = Path.GetFileName(mod.ModPath),
                    ["Dependencies"] = mod.ModDependencies.Count > 0 ? mod.ModDependencies.Concat() : "None",
                    ["Gadgets"] = mod.LoadedGadgets.Select(x => x.Attribute.Name).Concat()
                };
                if (mod.UnloadedGadgets.Count > 0)
                {
                    info.Add("Unloaded/Errored Gadgets", mod.UnloadedGadgets.Select(x => x.Attribute.Name).Concat());
                }
                using (GadgetModFile infoFile = mod.GetModFile("ModInfo.txt"))
                    modEntries.Add(new ModMenuEntry(mod.Name, ModMenuEntryType.GADGET, modInfo, info, mod.LoadedGadgets.ToArray()));
            }
            foreach (GadgetMod mod in GadgetLoader.EmptyMods)
            {
                if (mod.HasModFile("ModInfo.txt")) using (GadgetModFile infoFile = mod.GetModFile("ModInfo.txt")) modInfo = infoFile.ReadAllText();
                if (string.IsNullOrEmpty(modInfo) || modInfo == "Insert the description for your mod here!") modInfo = "This Gadget mod does not have a ModInfo file.";
                Dictionary<string, string> info = new Dictionary<string, string>
                {
                    ["Name"] = mod.Name,
                    ["Version"] = mod.Version.ToString(),
                    ["File Name"] = Path.GetFileName(mod.ModPath),
                    ["Dependencies"] = mod.ModDependencies.Count > 0 ? mod.ModDependencies.Concat() : "None",
                };
                if (mod.UnloadedGadgets.Count > 0)
                {
                    info.Add("Unloaded/Errored Gadgets", mod.UnloadedGadgets.Select(x => x.Attribute.Name).Concat());
                }
                modEntries.Add(new ModMenuEntry(mod.Name, ModMenuEntryType.EMPTY_GADGET, modInfo, info));
            }
            foreach (GadgetMod mod in GadgetLoader.IncompatibleMods)
            {
                if (mod.HasModFile("ModInfo.txt")) using (GadgetModFile infoFile = mod.GetModFile("ModInfo.txt")) modInfo = infoFile.ReadAllText();
                if (string.IsNullOrEmpty(modInfo) || modInfo == "Insert the description for your mod here!") modInfo = "This Gadget mod does not have a ModInfo file.";
                Dictionary<string, string> info = new Dictionary<string, string>
                {
                    ["Name"] = mod.Name,
                    ["Version"] = mod.Version.ToString(),
                    ["File Name"] = Path.GetFileName(mod.ModPath),
                    ["Dependencies"] = mod.ModDependencies.Count > 0 ? mod.ModDependencies.Concat() : "None"
                };
                modEntries.Add(new ModMenuEntry(mod.Name, ModMenuEntryType.INCOMPATIBLE_GADGET, modInfo, info));
            }
            foreach (GadgetMod mod in GadgetMods.ListAllMods().Where(x => x.LoadedGadgets.Count == 0))
            {
                if (mod.HasModFile("ModInfo.txt")) using (GadgetModFile infoFile = mod.GetModFile("ModInfo.txt")) modInfo = infoFile.ReadAllText();
                if (string.IsNullOrEmpty(modInfo) || modInfo == "Insert the description for your mod here!") modInfo = "This Gadget mod does not have a ModInfo file.";
                Dictionary<string, string> info = new Dictionary<string, string>
                {
                    ["Name"] = mod.Name,
                    ["Version"] = mod.Version.ToString(),
                    ["File Name"] = Path.GetFileName(mod.ModPath),
                    ["Dependencies"] = mod.ModDependencies.Count > 0 ? mod.ModDependencies.Concat() : "None",
                };
                if (mod.UnloadedGadgets.Count > 0)
                {
                    info.Add("Unloaded/Errored Gadgets", mod.UnloadedGadgets.Select(x => x.Attribute.Name).Concat());
                }
                modEntries.Add(new ModMenuEntry(mod.Name, ModMenuEntryType.ERRORED_GADGET, modInfo, info));
            }
            foreach (Tuple<string, string> mod in GadgetLoader.ErroredMods)
            {
                modInfo = "As there was an error loading this mod, its description could not be loaded.\nThe error that prevented this mod from loading should be in the log.\nYou should report this to the mod author.";
                Dictionary<string, string> info = new Dictionary<string, string>
                {
                    ["Name"] = mod.Item1,
                    ["File Name"] = Path.GetFileName(mod.Item2)
                };
                modEntries.Add(new ModMenuEntry(mod.Item1, ModMenuEntryType.ERRORED_GADGET, modInfo, info));
            }
            if (GadgetCoreAPI.GetUMFAPI() != null)
            {
                foreach (string mod in GadgetCoreAPI.GetUMFAPI().GetModNames())
                {
                    try
                    {
                        modInfo = File.ReadAllText(GadgetCoreAPI.GetUMFAPI().GetModInfosPath() + "/" + mod + "_v" + GadgetCoreAPI.GetUMFAPI().GetModVersion(mod) + "_ModInfo.txt");
                    }
                    catch (Exception) { }
                    if (string.IsNullOrEmpty(modInfo) || modInfo == "A UMF Mod(umodframework.com) for Roguelands") modInfo = "This UMF mod does not have a ModInfo file.";
                    Dictionary<string, string> info = new Dictionary<string, string>()
                    {
                        ["Name"] = mod,
                        ["Version"] = GadgetCoreAPI.GetUMFAPI().GetModVersion(mod).ToString(),
                        ["Description"] = GadgetCoreAPI.GetUMFAPI().GetModDescription(mod),
                    };
                    if (GadgetCoreAPI.GetUMFAPI().GetModNamesEnabled().Contains(mod))
                    {
                        modEntries.Add(new ModMenuEntry(mod, ModMenuEntryType.UMF, modInfo, info));
                    }
                    else
                    {
                        modEntries.Add(new ModMenuEntry(mod, ModMenuEntryType.DISABLED_UMF, modInfo, info));
                    }
                    umfModEntries.Add(modEntries[modEntries.Count - 1]);
                }
                foreach (string mod in GadgetCoreAPI.GetUMFAPI().GetModNamesMissingDependencies())
                {
                    try
                    {
                        modInfo = File.ReadAllText(GadgetCoreAPI.GetUMFAPI().GetModInfosPath() + "/" + mod + "_v" + GadgetCoreAPI.GetUMFAPI().GetModVersion(mod) + "_ModInfo.txt");
                    }
                    catch (Exception) { }
                    if (string.IsNullOrEmpty(modInfo) || modInfo == "A UMF Mod(umodframework.com) for Roguelands") modInfo = "This UMF mod does not have a ModInfo file.";
                    Dictionary<string, string> info = new Dictionary<string, string>()
                    {
                        ["Name"] = mod,
                        ["Version"] = GadgetCoreAPI.GetUMFAPI().GetModVersion(mod).ToString(),
                        ["Description"] = GadgetCoreAPI.GetUMFAPI().GetModDescription(mod),
                    };
                    modEntries.Add(new ModMenuEntry(mod, ModMenuEntryType.INCOMPATIBLE_UMF, modInfo, info));
                }
            }

            int modCount = modEntries.Count + modEntries.Where(x => x.Type == ModMenuEntryType.GADGET).SelectMany(x => x.Gadgets).Count();

            ScrollRect modListScrollView = new GameObject("Scroll View", typeof(RectTransform), typeof(ScrollRect), typeof(CanvasRenderer), typeof(Image)).GetComponent<ScrollRect>();
            modListScrollView.GetComponent<RectTransform>().SetParent(transform);
            modListScrollView.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);
            modListScrollView.GetComponent<RectTransform>().anchorMax = new Vector2(0.3f, 1f);
            modListScrollView.GetComponent<RectTransform>().offsetMin = new Vector2(10, 10);
            modListScrollView.GetComponent<RectTransform>().offsetMax = new Vector2(0, -10);
            modListScrollView.GetComponent<Image>().sprite = SceneInjector.BoxSprite;
            modListScrollView.GetComponent<Image>().type = Image.Type.Sliced;
            modListScrollView.GetComponent<Image>().fillCenter = true;
            Mask modListScrollViewMask = new GameObject("Mask", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask)).GetComponent<Mask>();
            modListScrollViewMask.GetComponent<RectTransform>().SetParent(modListScrollView.transform);
            modListScrollViewMask.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);
            modListScrollViewMask.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
            modListScrollViewMask.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            modListScrollViewMask.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            modListScrollViewMask.GetComponent<Image>().sprite = SceneInjector.BoxMask;
            modListScrollViewMask.GetComponent<Image>().type = Image.Type.Sliced;
            modListScrollViewMask.GetComponent<Image>().fillCenter = true;
            modListScrollViewMask.showMaskGraphic = false;
            RectTransform modListViewport = new GameObject("Viewport", typeof(RectTransform)).GetComponent<RectTransform>();
            modListViewport.SetParent(modListScrollViewMask.transform);
            modListViewport.anchorMin = new Vector2(0f, 0f);
            modListViewport.anchorMax = new Vector2(1f, 1f);
            modListViewport.offsetMin = new Vector2(10, 10);
            modListViewport.offsetMax = new Vector2(-10, -10);
            RectTransform modList = new GameObject("ModList", typeof(RectTransform), typeof(ToggleGroup)).GetComponent<RectTransform>();
            modList.SetParent(modListViewport);
            modList.anchorMin = new Vector2(0f, modCount <= 6 ? 0f : (1f - (modCount / 6f)));
            modList.anchorMax = new Vector2(1f, 1f);
            modList.offsetMin = Vector2.zero;
            modList.offsetMax = Vector2.zero;
            Scrollbar modListScrollBar = new GameObject("Scrollbar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Scrollbar)).GetComponent<Scrollbar>();
            modListScrollBar.GetComponent<RectTransform>().SetParent(modListScrollView.transform);
            modListScrollBar.GetComponent<RectTransform>().anchorMin = new Vector2(1f, 0f);
            modListScrollBar.GetComponent<RectTransform>().anchorMax = new Vector2(1.25f, 1f);
            modListScrollBar.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            modListScrollBar.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            modListScrollBar.GetComponent<Image>().sprite = SceneInjector.BoxSprite;
            modListScrollBar.GetComponent<Image>().type = Image.Type.Sliced;
            modListScrollBar.GetComponent<Image>().fillCenter = true;
            RectTransform modListScrollBarHandle = new GameObject("Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<RectTransform>();
            modListScrollBarHandle.SetParent(modListScrollBar.transform);
            modListScrollBarHandle.anchorMin = new Vector2(0.05f, 0.05f);
            modListScrollBarHandle.anchorMax = new Vector2(0.95f, 0.95f);
            modListScrollBarHandle.offsetMin = Vector2.zero;
            modListScrollBarHandle.offsetMax = Vector2.zero;
            modListScrollBarHandle.GetComponent<Image>().sprite = SceneInjector.BoxSprite;
            modListScrollBarHandle.GetComponent<Image>().type = Image.Type.Sliced;
            modListScrollBarHandle.GetComponent<Image>().fillCenter = true;
            modListScrollBar.targetGraphic = modListScrollBarHandle.GetComponent<Image>();
            modListScrollBar.handleRect = modListScrollBarHandle;
            modListScrollBar.direction = Scrollbar.Direction.BottomToTop;
            if (modCount <= 5) modListScrollBar.interactable = false;
            modListScrollView.content = modList;
            modListScrollView.horizontal = false;
            modListScrollView.scrollSensitivity = 5;
            modListScrollView.movementType = ScrollRect.MovementType.Clamped;
            modListScrollView.viewport = modListViewport;
            modListScrollView.verticalScrollbar = modListScrollBar;
            float entryHeight = modCount <= 6 ? (1f / 6f) : (1f / modCount);
            Toggle selectedToggle = null;
            int bonusOffset = 0;
            for (int i = 0; i < modEntries.Count; i++)
            {
                ModMenuEntry modEntry = modEntries[i];
                if (modEntry.Gadgets.Length > 0)
                {
                    int gadgetCount = modEntry.Gadgets.Count();
                    float gadgetHeight = 1f / (gadgetCount + 1);
                    RectTransform modEntryRect = new GameObject("Gadget-Containing Mod Entry: " + modEntry.Name, typeof(RectTransform), typeof(Toggle), typeof(CanvasRenderer), typeof(Image), typeof(Mask)).GetComponent<RectTransform>();
                    modEntryRect.SetParent(modList);
                    modEntryRect.anchorMin = new Vector2(0f, 1 - ((i + bonusOffset + 1 + gadgetCount) * entryHeight));
                    modEntryRect.anchorMax = new Vector2(1f, 1 - ((i + bonusOffset) * entryHeight));
                    modEntryRect.offsetMin = Vector2.zero;
                    modEntryRect.offsetMax = Vector2.zero;
                    Toggle modToggle = modEntryRect.GetComponent<Toggle>();
                    Image modSelected = new GameObject("Selected", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
                    modSelected.rectTransform.SetParent(modEntryRect);
                    modSelected.rectTransform.anchorMin = new Vector2(0f, 0f);
                    modSelected.rectTransform.anchorMax = new Vector2(1f, 1f);
                    modSelected.rectTransform.offsetMin = Vector2.zero;
                    modSelected.rectTransform.offsetMax = Vector2.zero;
                    modSelected.sprite = SceneInjector.BoxSprite;
                    modSelected.type = Image.Type.Sliced;
                    Text modLabel = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
                    modLabel.rectTransform.SetParent(modEntryRect);
                    modLabel.rectTransform.anchorMin = new Vector2(0f, 1f - gadgetHeight);
                    modLabel.rectTransform.anchorMax = new Vector2(1f, 1f);
                    modLabel.rectTransform.offsetMin = new Vector2(10, 10);
                    modLabel.rectTransform.offsetMax = new Vector2(-10, -10);
                    modLabel.font = SceneInjector.ModConfigMenuText.GetComponent<TextMesh>().font;
                    modLabel.fontSize = 12;
                    modLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
                    modLabel.verticalOverflow = VerticalWrapMode.Overflow;
                    modLabel.alignment = TextAnchor.MiddleLeft;
                    modToggle.GetComponent<Image>().sprite = SceneInjector.BoxSprite;
                    modToggle.GetComponent<Image>().type = Image.Type.Sliced;
                    modToggle.transition = Selectable.Transition.None;
                    modToggle.isOn = i == modIndex && gadgetIndex == -1;
                    modToggle.toggleTransition = Toggle.ToggleTransition.None;
                    modToggle.graphic = modSelected;
                    modToggle.group = modList.GetComponent<ToggleGroup>();
                    int toggleIndex = i;
                    if (i == modIndex) selectedToggle = modToggle;
                    modToggle.onValueChanged.AddListener((toggled) => { if (toggled) { UpdateInfo(modToggle, toggleIndex); GadgetCoreAPI.CloseDialog(); } });

                    switch (modEntry.Type)
                    {
                        case ModMenuEntryType.GADGET:
                            modSelected.color = new Color(1f, 1f, 0.5f, 1f);
                            modToggle.GetComponent<Image>().color = new Color(1f, 1f, 0.5f, 0.25f);
                            modLabel.color = new Color(1f, 1f, 1f, 1f);
                            modLabel.text = modEntry.Name + "\nGadget Mod";
                            break;
                        case ModMenuEntryType.EMPTY_GADGET:
                            modSelected.color = new Color(0.5f, 0.5f, 0.125f, 1f);
                            modToggle.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.125f, 0.25f);
                            modLabel.color = new Color(1f, 1f, 1f, 0.5f);
                            modLabel.text = modEntry.Name + "\nEmpty Gadget Mod";
                            break;
                        case ModMenuEntryType.INCOMPATIBLE_GADGET:
                            modSelected.color = new Color(1f, 0.5f, 0f, 1f);
                            modToggle.GetComponent<Image>().color = new Color(1f, 0.5f, 0f, 0.25f);
                            modLabel.color = new Color(1f, 1f, 1f, 0.5f);
                            modLabel.text = modEntry.Name + "\nIncompatible Gadget Mod";
                            break;
                        case ModMenuEntryType.ERRORED_GADGET:
                            modSelected.color = new Color(1f, 0f, 0f, 1f);
                            modToggle.GetComponent<Image>().color = new Color(1f, 0f, 0f, 0.25f);
                            modLabel.color = new Color(1f, 1f, 1f, 0.5f);
                            modLabel.text = modEntry.Name + "\nErrored Gadget Mod";
                            break;
                        case ModMenuEntryType.UMF:
                            modSelected.color = new Color(139 / 255f, 69 / 255f, 19 / 255f, 1f);
                            modToggle.GetComponent<Image>().color = new Color(139 / 255f, 69 / 255f, 19 / 255f, 0.25f);
                            modLabel.color = new Color(139 / 255f, 69 / 255f, 19 / 255f, 1f);
                            modLabel.text = modEntry.Name + "\nUMF Mod";
                            break;
                        case ModMenuEntryType.DISABLED_UMF:
                            modSelected.color = new Color(69 / 255f, 34 / 255f, 9 / 255f, 1f);
                            modToggle.GetComponent<Image>().color = new Color(69 / 255f, 34 / 255f, 9 / 255f, 0.25f);
                            modLabel.color = new Color(139 / 255f, 69 / 255f, 19 / 255f, 0.5f);
                            modLabel.text = modEntry.Name + "\nDisabled UMF Mod";
                            break;
                        case ModMenuEntryType.INCOMPATIBLE_UMF:
                            modSelected.color = new Color(210 / 255f, 105 / 255f, 30 / 255f, 1f);
                            modToggle.GetComponent<Image>().color = new Color(210 / 255f, 105 / 255f, 30 / 255f, 0.25f);
                            modLabel.color = new Color(139 / 255f, 69 / 255f, 19 / 255f, 0.5f);
                            modLabel.text = modEntry.Name + "\nIncompatible UMF Mod";
                            break;
                    }

                    for (int g = 0;g < gadgetCount; g++)
                    {
                        GadgetInfo gadget = modEntry.Gadgets[g];
                        RectTransform gadgetRect = new GameObject("Gadget: " + gadget.Attribute.Name, typeof(RectTransform), typeof(Toggle), typeof(CanvasRenderer), typeof(Image), typeof(Mask)).GetComponent<RectTransform>();
                        gadgetRect.SetParent(modEntryRect);
                        gadgetRect.anchorMin = new Vector2(0f, 1 - ((g + 2) * gadgetHeight));
                        gadgetRect.anchorMax = new Vector2(1f, 1 - ((g + 1) * gadgetHeight));
                        gadgetRect.offsetMin = new Vector2(10f, 10f);
                        gadgetRect.offsetMax = new Vector2(-10f, 10f);
                        Toggle gadgetToggle = gadgetRect.GetComponent<Toggle>();
                        Image gadgetSelected = new GameObject("Selected", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
                        gadgetSelected.rectTransform.SetParent(gadgetRect);
                        gadgetSelected.rectTransform.anchorMin = new Vector2(0f, 0f);
                        gadgetSelected.rectTransform.anchorMax = new Vector2(1f, 1f);
                        gadgetSelected.rectTransform.offsetMin = Vector2.zero;
                        gadgetSelected.rectTransform.offsetMax = Vector2.zero;
                        gadgetSelected.sprite = SceneInjector.BoxSprite;
                        gadgetSelected.type = Image.Type.Sliced;
                        gadgetSelected.color = new Color(1f, 1f, 0.5f, 1f);
                        Text gadgetLabel = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
                        gadgetLabel.rectTransform.SetParent(gadgetRect);
                        gadgetLabel.rectTransform.anchorMin = new Vector2(0f, 0f);
                        gadgetLabel.rectTransform.anchorMax = new Vector2(1f, 1f);
                        gadgetLabel.rectTransform.offsetMin = new Vector2(10, 10);
                        gadgetLabel.rectTransform.offsetMax = new Vector2(-10, -10);
                        gadgetLabel.font = SceneInjector.ModConfigMenuText.GetComponent<TextMesh>().font;
                        gadgetLabel.fontSize = 12;
                        gadgetLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
                        gadgetLabel.verticalOverflow = VerticalWrapMode.Overflow;
                        gadgetLabel.alignment = TextAnchor.MiddleLeft;
                        gadgetLabel.text = gadget.Attribute.Name + "\n" + (gadget.Gadget.Enabled ? "Enabled" : "Disabled");
                        gadgetLabel.color = gadget.Gadget.Enabled ? new Color(1f, 1f, 1f, 1f) : new Color(1f, 1f, 1f, 0.5f);
                        gadgetToggle.GetComponent<Image>().sprite = SceneInjector.BoxSprite;
                        gadgetToggle.GetComponent<Image>().type = Image.Type.Sliced;
                        gadgetToggle.transition = Selectable.Transition.None;
                        gadgetToggle.isOn = i == modIndex && g == this.gadgetIndex;
                        gadgetToggle.toggleTransition = Toggle.ToggleTransition.None;
                        gadgetToggle.graphic = gadgetSelected;
                        gadgetToggle.group = modList.GetComponent<ToggleGroup>();
                        gadgetToggle.GetComponent<Image>().color = new Color(1f, 1f, 0.5f, 0.25f);
                        int gadgetIndex = g;
                        gadgetToggle.onValueChanged.AddListener((toggled) => { if (toggled) { UpdateInfo(modToggle, toggleIndex, gadgetIndex); GadgetCoreAPI.CloseDialog(); } });
                        bonusOffset++;
                    }
                }
                else
                {
                    RectTransform modEntryRect = new GameObject("Mod Entry: " + modEntry.Name, typeof(RectTransform), typeof(Toggle), typeof(CanvasRenderer), typeof(Image), typeof(Mask)).GetComponent<RectTransform>();
                    modEntryRect.SetParent(modList);
                    modEntryRect.anchorMin = new Vector2(0f, 1 - ((i + bonusOffset + 1) * entryHeight));
                    modEntryRect.anchorMax = new Vector2(1f, 1 - ((i + bonusOffset) * entryHeight));
                    modEntryRect.offsetMin = Vector2.zero;
                    modEntryRect.offsetMax = Vector2.zero;
                    Toggle modToggle = modEntryRect.GetComponent<Toggle>();
                    Image modSelected = new GameObject("Selected", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
                    modSelected.rectTransform.SetParent(modEntryRect);
                    modSelected.rectTransform.anchorMin = new Vector2(0f, 0f);
                    modSelected.rectTransform.anchorMax = new Vector2(1f, 1f);
                    modSelected.rectTransform.offsetMin = Vector2.zero;
                    modSelected.rectTransform.offsetMax = Vector2.zero;
                    modSelected.sprite = SceneInjector.BoxSprite;
                    modSelected.type = Image.Type.Sliced;
                    Text modLabel = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
                    modLabel.rectTransform.SetParent(modEntryRect);
                    modLabel.rectTransform.anchorMin = new Vector2(0f, 0f);
                    modLabel.rectTransform.anchorMax = new Vector2(1f, 1f);
                    modLabel.rectTransform.offsetMin = new Vector2(10, 10);
                    modLabel.rectTransform.offsetMax = new Vector2(-10, -10);
                    modLabel.font = SceneInjector.ModConfigMenuText.GetComponent<TextMesh>().font;
                    modLabel.fontSize = 12;
                    modLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
                    modLabel.verticalOverflow = VerticalWrapMode.Overflow;
                    modLabel.alignment = TextAnchor.MiddleLeft;
                    modToggle.GetComponent<Image>().sprite = SceneInjector.BoxSprite;
                    modToggle.GetComponent<Image>().type = Image.Type.Sliced;
                    modToggle.transition = Selectable.Transition.None;
                    modToggle.isOn = i == modIndex;
                    modToggle.toggleTransition = Toggle.ToggleTransition.None;
                    modToggle.graphic = modSelected;
                    modToggle.group = modList.GetComponent<ToggleGroup>();
                    int toggleIndex = i;
                    if (i == modIndex) selectedToggle = modToggle;
                    modToggle.onValueChanged.AddListener((toggled) => { if (toggled) { UpdateInfo(modToggle, toggleIndex); GadgetCoreAPI.CloseDialog(); } });

                    switch (modEntry.Type)
                    {
                        case ModMenuEntryType.EMPTY_GADGET:
                            modSelected.color = new Color(0.5f, 0.5f, 0.125f, 1f);
                            modToggle.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.125f, 0.25f);
                            modLabel.color = new Color(1f, 1f, 1f, 0.5f);
                            modLabel.text = modEntry.Name + "\nEmpty Gadget Mod";
                            break;
                        case ModMenuEntryType.INCOMPATIBLE_GADGET:
                            modSelected.color = new Color(1f, 0.5f, 0f, 1f);
                            modToggle.GetComponent<Image>().color = new Color(1f, 0.5f, 0f, 0.25f);
                            modLabel.color = new Color(1f, 1f, 1f, 0.5f);
                            modLabel.text = modEntry.Name + "\nIncompatible Gadget Mod";
                            break;
                        case ModMenuEntryType.ERRORED_GADGET:
                            modSelected.color = new Color(1f, 0f, 0f, 1f);
                            modToggle.GetComponent<Image>().color = new Color(1f, 0f, 0f, 0.25f);
                            modLabel.color = new Color(1f, 1f, 1f, 0.5f);
                            modLabel.text = modEntry.Name + "\nErrored Gadget Mod";
                            break;
                        case ModMenuEntryType.UMF:
                            modSelected.color = new Color(139 / 255f, 69 / 255f, 19 / 255f, 1f);
                            modToggle.GetComponent<Image>().color = new Color(139 / 255f, 69 / 255f, 19 / 255f, 0.25f);
                            modLabel.color = new Color(139 / 255f, 69 / 255f, 19 / 255f, 1f);
                            modLabel.text = modEntry.Name + "\nUMF Mod";
                            break;
                        case ModMenuEntryType.DISABLED_UMF:
                            modSelected.color = new Color(69 / 255f, 34 / 255f, 9 / 255f, 1f);
                            modToggle.GetComponent<Image>().color = new Color(69 / 255f, 34 / 255f, 9 / 255f, 0.25f);
                            modLabel.color = new Color(139 / 255f, 69 / 255f, 19 / 255f, 0.5f);
                            modLabel.text = modEntry.Name + "\nDisabled UMF Mod";
                            break;
                        case ModMenuEntryType.INCOMPATIBLE_UMF:
                            modSelected.color = new Color(210 / 255f, 105 / 255f, 30 / 255f, 1f);
                            modToggle.GetComponent<Image>().color = new Color(210 / 255f, 105 / 255f, 30 / 255f, 0.25f);
                            modLabel.color = new Color(139 / 255f, 69 / 255f, 19 / 255f, 0.5f);
                            modLabel.text = modEntry.Name + "\nIncompatible UMF Mod";
                            break;
                    }
                }
            }

            modListScrollView.GetComponent<RectTransform>().localScale = Vector3.one;
            if (scrollPositionCache >= 0) modListScrollView.verticalNormalizedPosition = 1 - (scrollPositionCache / (modList.anchorMax.y - modList.anchorMin.y));
            scrollPositionCache = -1;

            if (GadgetModConfigs.GetConfigMenuObject(0) != null)
            {
                GadgetModConfigs.ResetAllConfigMenus();
            }
            else
            {
                GadgetModConfigs.BuildConfigMenus(SceneInjector.ModConfigMenus);
            }

            if (modCount > 0)
            {
                UpdateInfo(selectedToggle, modIndex, gadgetIndex);
            }
        }

        internal void Rebuild()
        {
            if (modEntries.Count > 0)
            {
                RectTransform modListScrollView = transform.Find("Scroll View") as RectTransform;
                RectTransform modList = modListScrollView.Find("Mask").Find("Viewport").Find("ModList") as RectTransform;
                scrollPositionCache = (1 - modListScrollView.GetComponent<ScrollRect>().verticalNormalizedPosition) * (modList.anchorMax.y - modList.anchorMin.y);
                Destroy(modListScrollView.gameObject);
                modEntries.Clear();
            }
            Build();
        }

        internal void Start()
        {
            for (int i = 0; i < umfModEntries.Count; i++)
            {
                wasEnabled[umfModEntries[i].Name] = umfModEntries[i].Type != ModMenuEntryType.DISABLED_UMF;
            }
            for (int i = 0;i < Gadgets.CountGadgets(); i++)
            {
                wasEnabled[Gadgets.GetGadgetInfo(i).Attribute.Name] = Gadgets.GetGadgetInfo(i).Gadget.Enabled;
            }
            StartCoroutine(WatchForRestartNeeded());
        }

        internal void Update()
        {
            if (canvasSize.x != SceneInjector.ModMenuCanvas.GetComponent<RectTransform>().sizeDelta.x || canvasSize.y != SceneInjector.ModMenuCanvas.GetComponent<RectTransform>().sizeDelta.y)
            {
                if (canvasSize != default)
                {
                    GadgetModConfigs.RebuildAllConfigMenus();
                }
                canvasSize = new Vector2(SceneInjector.ModMenuCanvas.GetComponent<RectTransform>().sizeDelta.x, SceneInjector.ModMenuCanvas.GetComponent<RectTransform>().sizeDelta.y);
            }
        }

        public void UpdateInfo(Toggle toggle, int modIndex, int gadgetIndex = -1)
        {
            try
            {
                if (GadgetModConfigs.IsConfigMenuOpen((this.modIndex << 16) + (this.gadgetIndex & 0xFFFF))) GadgetModConfigs.CloseConfigMenu((this.modIndex << 16) + (this.gadgetIndex & 0xFFFF));
                this.toggle = toggle;
                this.modIndex = modIndex;
                this.gadgetIndex = gadgetIndex;

                string[] disabledMods = GadgetCoreConfig.enabledMods.Where(x => !x.Value).Select(x => x.Key).ToArray();

                StringBuilder descTextBuilder = new StringBuilder();
                bool modEnabled = modEntries[modIndex].Type == ModMenuEntryType.GADGET || modEntries[modIndex].Type == ModMenuEntryType.UMF;

                if (gadgetIndex >= 0)
                {
                    GadgetInfo mod = modEntries[modIndex].Gadgets[gadgetIndex];
                    enableButton.interactable = modEnabled;
                    reloadButton.interactable = mod.Attribute.AllowRuntimeReloading;
                    configButton.interactable = GadgetModConfigs.GetConfigMenuObject((modIndex << 16) + (gadgetIndex & 0xFFFF)) != null;
                    enableButton.GetComponentInChildren<Text>().text = mod.Gadget.Enabled ? "Disable" : "Enable";
                    descTextBuilder.Append((mod.Gadget.Enabled ? "Gadget Enabled" : "Gadget Disabled") + (modEnabled ? "" : " (Disabled by mod) ") + '\n');
                    Dictionary<string, string> gadgetInfo = new Dictionary<string, string>
                    {
                        ["Name"] = mod.Attribute.Name,
                        ["Version"] = mod.Mod.Version.ToString(),
                        ["Dependencies"] = mod.Attribute.Dependencies.Length > 0 ? mod.Attribute.Dependencies.Aggregate(new StringBuilder(), (a, b) => { if (a.Length == 0) a.Append(", "); a.Append(b); return a; }).ToString() : "None"
                    };
                    foreach (KeyValuePair<string, string> info in gadgetInfo)
                    {
                        descTextBuilder.Append('\n');
                        descTextBuilder.Append(info.Key + ": " + info.Value);
                    }
                    descTextBuilder.Append("\n\n" + mod.Gadget.GetModDescription() ?? modEntries[modIndex].Description);
                }
                else
                {
                    enableButton.interactable = modEntries[modIndex].Name != "GadgetCore" && (modEnabled || modEntries[modIndex].Type == ModMenuEntryType.DISABLED_UMF);
                    reloadButton.interactable = modEntries[modIndex].Name != "GadgetCore" && (modEntries[modIndex].Type == ModMenuEntryType.GADGET || modEntries[modIndex].Type == ModMenuEntryType.UMF);
                    configButton.interactable = GadgetModConfigs.GetConfigMenuObject((modIndex << 16) + (gadgetIndex & 0xFFFF)) != null;
                    enableButton.GetComponentInChildren<Text>().text = (modEnabled ^ modsToToggle.Contains(modEntries[modIndex].Name)) ? "Disable" : "Enable";
                    descTextBuilder.Append((modEnabled ? "Mod Enabled" : "Mod Disabled") + (modsToToggle.Contains(modEntries[modIndex].Name) ? (modEnabled ? " (Will disable after restart)" : " (Will enable after restart)") : "") + '\n');
                    foreach (KeyValuePair<string, string> info in modEntries[modIndex].Info)
                    {
                        descTextBuilder.Append('\n');
                        descTextBuilder.Append(info.Key + ": " + info.Value);
                    }
                    descTextBuilder.Append("\n\n" + modEntries[modIndex].Description);
                }
                descText.text = descTextBuilder.ToString();
            }
            catch (Exception e)
            {
                descText.text = "An error occured while populating the info panel for this mod! Check GadgetCore.log for details.";
                if (gadgetIndex >= 0)
                {
                    GadgetCore.CoreLogger.LogError("An error occured while populating the info panel for the gadget with the mod index " + modIndex + " and gadget index " + gadgetIndex + ": " + e.ToString(), false);
                }
                else
                {
                    GadgetCore.CoreLogger.LogError("An error occured while populating the info panel for the mod with the index " + modIndex + ": " + e.ToString(), false);
                }
            }
        }

        internal void UMFConfigButton()
        {
            if (GadgetModConfigs.IsConfigMenuOpen(-1))
            {
                GadgetModConfigs.CloseConfigMenu(-1);
            }
            else
            {
                GadgetModConfigs.OpenConfigMenu(-1);
            }
        }

        internal void ConfigButton()
        {
            if (GadgetModConfigs.IsConfigMenuOpen((modIndex << 16) + (gadgetIndex & 0xFFFF)))
            {
                GadgetModConfigs.CloseConfigMenu((modIndex << 16) + (gadgetIndex & 0xFFFF));
            }
            else
            {
                GadgetModConfigs.OpenConfigMenu((modIndex << 16) + (gadgetIndex & 0xFFFF));
            }
        }

        internal void EnableButton()
        {
            ModMenuEntry entry = modEntries[modIndex];
            if (gadgetIndex >= 0)
            {
                bool enabled = !entry.Gadgets[gadgetIndex].Gadget.Enabled;
                Gadgets.SetEnabled(entry.Gadgets[gadgetIndex], enabled);
            }
            else if (entry.Type == ModMenuEntryType.GADGET)
            {
                bool enabled = !GadgetMods.GetModByName(entry.Name).Enabled;
                GadgetMods.SetEnabled(entry.Name, enabled);
            }
            else
            {
                if (!File.Exists(GadgetCoreAPI.GetUMFAPI().GetDisabledModsFile())) File.Create(GadgetCoreAPI.GetUMFAPI().GetDisabledModsFile()).Dispose();
                string fileText = File.ReadAllText(GadgetCoreAPI.GetUMFAPI().GetDisabledModsFile());
                string[] fileLines = File.ReadAllLines(GadgetCoreAPI.GetUMFAPI().GetDisabledModsFile());
                if (fileLines.Any(x => x.Equals(entry.Name)))
                {
                    File.WriteAllLines(GadgetCoreAPI.GetUMFAPI().GetDisabledModsFile(), fileLines.Where(x => !x.Equals(entry.Name)).ToArray());
                }
                else
                {
                    File.WriteAllText(GadgetCoreAPI.GetUMFAPI().GetDisabledModsFile(), fileText + Environment.NewLine + entry.Name);
                }
                if (modsToToggle.Contains(entry.Name))
                {
                    modsToToggle.Remove(entry.Name);
                }
                else
                {
                    modsToToggle.Add(entry.Name);
                }
            }
            Rebuild();
            UpdateRestartNeeded();
        }

        internal void ReloadButton()
        {
            if (gadgetIndex >= 0)
            {
                GadgetCoreAPI.DisplayYesNoDialog("Are you sure you want to perform a Gadget reload?\nThe following Gadgets will be reloaded:\n\n" + modEntries[modIndex].Gadgets[gadgetIndex].Dependents.Union(Gadgets.LoadOrderTree.Find(modEntries[modIndex].Gadgets[gadgetIndex]).FlattenUniqueByBreadth()).Select(x => x.Attribute.Name).Concat("\n"), () => {
                    Gadgets.ReloadGadget(modEntries[modIndex].Gadgets[gadgetIndex]);
                });
            }
            else
            {
                if (modEntries[modIndex].Type == ModMenuEntryType.GADGET)
                {
                    GadgetCoreAPI.DisplayYesNoDialog("<color=red>WARNING: This feature does not work! Unexpected consequences may occur from attempting to use it!</color>\n\nAre you sure you want to perform a mod reload?\nThe following mods will be reloaded:\n\n" + modEntries[modIndex].Gadgets.SelectMany(x => Gadgets.LoadOrderTree.Find(x).FlattenUniqueByBreadth()).Where(x => x != null).Select(x => x.Mod).Distinct().Select(x => x.Name).Concat("\n"), () => {
                        GadgetLoader.ReloadMod(modEntries[modIndex].Gadgets[0].Mod);
                    });
                }
                else
                {
                    GadgetCoreAPI.DisplayYesNoDialog("UMF mods can only have their configs reloaded at runtime.\nWould you like to reload " + modEntries[modIndex].Name + "'s config?", () => {
                        GadgetCore.UMFAPI.SendCommand("cfgReload " + modEntries[modIndex].Name);
                    });
                }
            }
            Rebuild();
            UpdateRestartNeeded();
        }

        private IEnumerator WatchForRestartNeeded()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);
                UpdateRestartNeeded();
            }
        }

        private void UpdateRestartNeeded()
        {
            bool restartNeededOld = RestartNeeded;
            RestartNeeded = IsRestartNeeded();
            if (RestartNeeded != restartNeededOld)
            {
                Array.ForEach(SceneInjector.ModMenuBackButtonHolder.GetComponentsInChildren<TextMesh>(), x => x.text = RestartNeeded ? "QUIT" : "BACK");
                restartRequiredText.SetActive(RestartNeeded);
                UpdateInfo(toggle, modIndex);
            }
        }

        private bool IsRestartNeeded()
        {
            if (unpackedMods.Count > 0) return true;
            int umfModCount = umfModEntries?.Count ?? 0;
            foreach (GadgetInfo gadget in Gadgets.ListAllGadgetInfos())
            {
                if (GadgetCoreConfig.enabledGadgets[gadget.Attribute.Name] != gadget.Gadget.Enabled)
                {
                    return true;
                }
                if (originalConfig.ContainsKey(gadget.Attribute.Name))
                {
                    if (!originalConfig[gadget.Attribute.Name].Equals(File.ReadAllText(GadgetPaths.ConfigsPath + "/" + Gadgets.GetGadgetInfo(modIndex).Mod.Name + ".ini"))) return true;
                }
            }
            if (GadgetCoreAPI.GetUMFAPI() != null)
            {
                if (!File.Exists(GadgetCoreAPI.GetUMFAPI().GetDisabledModsFile())) File.Create(GadgetCoreAPI.GetUMFAPI().GetDisabledModsFile()).Dispose();
                string[] disabledMods = File.ReadAllLines(GadgetCoreAPI.GetUMFAPI().GetDisabledModsFile()).Where(x => !string.IsNullOrEmpty(x)).ToArray();
                for (int i = 0; i < umfModEntries.Count; i++)
                {
                    if (wasEnabled.ContainsKey(umfModEntries[i].Name))
                    {
                        if (wasEnabled[umfModEntries[i].Name] == disabledMods.Contains(umfModEntries[i].Name)) return true;
                    }
                    if (originalConfig.ContainsKey(umfModEntries[i].Name))
                    {
                        if (!originalConfig[umfModEntries[i].Name].Equals(File.ReadAllText(GadgetCoreAPI.GetUMFAPI().GetConfigsPath() + "/" + umfModEntries[i].Name + ".ini"))) return true;
                    }
                }
            }
            return false;
        }
    }
}
