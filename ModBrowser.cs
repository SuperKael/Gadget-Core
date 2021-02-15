using GadgetCore.API;
using GadgetCore.Loader;
using GadgetCore.Util;
using IniParser;
using IniParser.Model;
using IniParser.Parser;
using Ionic.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace GadgetCore
{
    internal class ModBrowser : MonoBehaviour
    {
        /// <summary>
        /// Regex for inserting spaces into PascalCase strings.
        /// </summary>
        public const string PASCAL_CASE_SPACING_REGEX = @"(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])";

        public const string REPO_URL = @"https://github.com/SuperKael/Roguelands-Mods";
        public const string GIT_RAW_URL = @"https://raw.githubusercontent.com/SuperKael/Roguelands-Mods/master";
        public const string GIT_API_URL = @"https://api.github.com/repos/{0}/{1}/releases";
        public const string MODS_URL = GIT_RAW_URL + @"/Mods.ini";

        private static ModBrowser Singleton;

        internal Text ListLoadingText;
        internal Text DescLoadingText;
        internal Text DescText;
        internal Text BrowserButtonText;
        internal Button InstallButton;
        internal Button ActivateButton;
        internal Button VersionsButton;

        internal static bool RestartNeeded { get; private set; } = false;

        private List<ModBrowserEntry> modEntries = new List<ModBrowserEntry>();
        private bool listLoading, descLoading, downloading, built;

        private Toggle toggle;
        private int modIndex = 0;
        private float scrollPositionCache = -1;

        private ScrollRect versionSelectorScrollRect;

        /// <summary>
        /// Toggles the mod browser. Only works if the mod menu is open.
        /// Returns true if the mod browser is now open. (Assuming the mod menu is open).
        /// </summary>
        public static bool ToggleModBrowser()
        {
            if (SceneInjector.ModBrowserPanel.gameObject.activeSelf)
            {
                CloseModBrowser();
                return false;
            }
            else
            {
                OpenModBrowser();
                return true;
            }
        }

        /// <summary>
        /// Opens the mod browser. Only works if the mod menu is open.
        /// </summary>
        public static void OpenModBrowser()
        {
            if (SceneInjector.ModMenuPanel.gameObject.activeInHierarchy)
            {
                if (ModMenuController.RestartNeeded)
                {
                    foreach (System.Diagnostics.Process process in ModMenuController.ConfigHandles)
                    {
                        if (process != null && !process.HasExited) process.Kill();
                    }
                    GadgetCoreAPI.Quit();
                    return;
                }
                SceneInjector.ModMenuPanel.gameObject.SetActive(false);
                Array.ForEach(SceneInjector.ModConfigMenuText.GetComponentsInChildren<TextMesh>(), x => { x.text = "MOD BROWSER"; x.anchor = TextAnchor.UpperCenter; });
                Singleton.BrowserButtonText.text = "Mod Menu";
                SceneInjector.ModBrowserPanel.gameObject.SetActive(true);
                Singleton.StartCoroutine(Singleton.LoadModList());
            }
        }

        /// <summary>
        /// Closes the mod browser.
        /// </summary>
        public static void CloseModBrowser()
        {
            if (SceneInjector.ModBrowserPanel.gameObject.activeSelf)
            {
                if (RestartNeeded)
                {
                    foreach (System.Diagnostics.Process process in ModMenuController.ConfigHandles)
                    {
                        if (process != null && !process.HasExited) process.Kill();
                    }
                    GadgetCoreAPI.Quit();
                    return;
                }
                SceneInjector.ModBrowserPanel.gameObject.SetActive(false);
                Array.ForEach(SceneInjector.ModConfigMenuText.GetComponentsInChildren<TextMesh>(), x => { x.text = "MOD CONFIG MENU"; x.anchor = TextAnchor.UpperCenter; });
                Singleton.BrowserButtonText.text = "Mod Browser";
                SceneInjector.ModMenuPanel.Rebuild();
                SceneInjector.ModMenuPanel.gameObject.SetActive(true);
                Singleton.Clean();
            }
        }

        internal void OnDownloadButton()
        {
            StartCoroutine(DownloadModFile(modEntries[modIndex]));
        }

        private IEnumerator DownloadModFile(ModBrowserEntry entry)
        {
            if (downloading) yield break;
            if (!entry.Info.ContainsKey("File")) yield break;
            downloading = true;
            GadgetCore.CoreLogger.LogConsole("Initiating download for " + entry.Info["Name"] + "!");
            using (WWW modFileWWW = new WWW(entry.Info["File"]))
            {
                yield return new WaitUntil(() => modFileWWW.isDone);
                if (modFileWWW.text == "404: Not Found")
                {
                    downloading = false;
                    GadgetCore.CoreLogger.LogWarning("Download Failed: File does not exist!");
                    yield break;
                }
                string filePath = Path.Combine(GadgetPaths.ModsPath, Path.GetFileName(entry.Info["File"]));
                foreach (GadgetMod existingMod in GadgetMods.ListAllMods())
                {
                    if (existingMod != null && (existingMod.Name == entry.Info["Name"] || (entry.Info.TryGetValue("OldNames", out string oldNames) && oldNames.Split(',').Any(x => x == existingMod.Name))))
                    {
                        try
                        {
                            GadgetLoader.UnloadMod(existingMod);
                            if (existingMod.IsArchive)
                            {
                                File.Delete(existingMod.ModPath);
                            }
                            else
                            {
                                Directory.Delete(existingMod.ModPath, true);
                            }
                        }
                        catch (Exception e)
                        {
                            GadgetCore.CoreLogger.LogWarning("Failed to unload and delete old version: " + Path.GetFileName(existingMod.ModPath) + ". This may result in having multiple versions of the same mod installed! ");
                            GadgetCore.CoreLogger.Log("Mod unload and deletion failed due to error: " + e);
                        }
                        RestartNeeded = true;
                        Array.ForEach(SceneInjector.ModMenuBackButtonHolder.GetComponentsInChildren<TextMesh>(), x => x.text = "QUIT");
                        BrowserButtonText.text = "Quit Game";
                        SceneInjector.ModMenuPanel.restartRequiredText.SetActive(RestartNeeded);
                    }
                }
                File.WriteAllBytes(filePath, modFileWWW.bytes);
                bool isValid;
                try
                {
                    isValid = ZipFile.CheckZip(filePath);
                }
                catch
                {
                    isValid = false;
                }
                if (!isValid)
                {
                    File.Delete(filePath);
                    downloading = false;
                    GadgetCore.CoreLogger.LogWarning("Download Failed: File corrupt!");
                    yield break;
                }
            }
            GadgetCore.CoreLogger.LogConsole("Download of " + entry.Info["Name"] + " complete!" + (RestartNeeded ? " Restart Required." : ""));
            downloading = false;
            UpdateInfo(toggle, modIndex);
        }

        internal void OnActivateButton()
        {
            ModBrowserEntry modEntry = modEntries[modIndex];
            if (modEntry.Info.ContainsKey("File"))
            {
                GadgetCore.CoreLogger.LogConsole("Activating " + modEntry.Info["Name"] + "!");
                GadgetMod mod = GadgetLoader.LoadModFile(Path.Combine(GadgetPaths.ModsPath, Path.GetFileName(modEntry.Info["File"])));
                if (mod != null && mod.IsLoaded)
                {
                    GadgetCore.CoreLogger.LogConsole("Activation Complete!");
                }
                else
                {
                    GadgetCore.CoreLogger.LogWarning("Something went wrong during the activation process. A restart is now required.");
                    RestartNeeded = true;
                    Array.ForEach(SceneInjector.ModMenuBackButtonHolder.GetComponentsInChildren<TextMesh>(), x => x.text = "QUIT");
                    BrowserButtonText.text = "Quit Game";
                    SceneInjector.ModMenuPanel.restartRequiredText.SetActive(RestartNeeded);
                }
                UpdateInfo(toggle, modIndex);
            }
        }

        internal void OnVersionsButton()
        {
            if (versionSelectorScrollRect == null)
            {
                ModBrowserEntry modEntry = modEntries[modIndex];

                versionSelectorScrollRect = new GameObject("Version Selector", typeof(RectTransform), typeof(ScrollRect), typeof(CanvasRenderer), typeof(Image)).GetComponent<ScrollRect>();
                versionSelectorScrollRect.GetComponent<RectTransform>().SetParent(transform);
                versionSelectorScrollRect.GetComponent<RectTransform>().anchorMin = new Vector2(0.7f, 0.25f);
                versionSelectorScrollRect.GetComponent<RectTransform>().anchorMax = new Vector2(1.0f, modEntry.OtherVersions.Count >= 6 ? 1.0f : (0.375f + (0.625f / 6 * modEntry.OtherVersions.Count)));
                versionSelectorScrollRect.GetComponent<RectTransform>().offsetMin = new Vector2(10, 10);
                versionSelectorScrollRect.GetComponent<RectTransform>().offsetMax = new Vector2(0, -10);
                versionSelectorScrollRect.GetComponent<Image>().sprite = SceneInjector.BoxSprite;
                versionSelectorScrollRect.GetComponent<Image>().type = Image.Type.Sliced;
                versionSelectorScrollRect.GetComponent<Image>().fillCenter = true;
                Mask versionSelectorScrollViewMask = new GameObject("Mask", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask)).GetComponent<Mask>();
                versionSelectorScrollViewMask.GetComponent<RectTransform>().SetParent(versionSelectorScrollRect.transform);
                versionSelectorScrollViewMask.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);
                versionSelectorScrollViewMask.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
                versionSelectorScrollViewMask.GetComponent<RectTransform>().offsetMin = Vector2.zero;
                versionSelectorScrollViewMask.GetComponent<RectTransform>().offsetMax = Vector2.zero;
                versionSelectorScrollViewMask.GetComponent<Image>().sprite = SceneInjector.BoxMask;
                versionSelectorScrollViewMask.GetComponent<Image>().type = Image.Type.Sliced;
                versionSelectorScrollViewMask.GetComponent<Image>().fillCenter = true;
                versionSelectorScrollViewMask.showMaskGraphic = false;
                RectTransform versionSelectorViewport = new GameObject("Viewport", typeof(RectTransform)).GetComponent<RectTransform>();
                versionSelectorViewport.SetParent(versionSelectorScrollViewMask.transform);
                versionSelectorViewport.anchorMin = new Vector2(0f, 0f);
                versionSelectorViewport.anchorMax = new Vector2(1f, 1f);
                versionSelectorViewport.offsetMin = new Vector2(10, 10);
                versionSelectorViewport.offsetMax = new Vector2(-10, -10);
                RectTransform versionSelector = new GameObject("ModList", typeof(RectTransform), typeof(ToggleGroup)).GetComponent<RectTransform>();
                versionSelector.SetParent(versionSelectorViewport);
                versionSelector.anchorMin = new Vector2(0f, modEntry.OtherVersions.Count <= 6 ? 0f : (1f - (modEntry.OtherVersions.Count / 6f)));
                versionSelector.anchorMax = new Vector2(1f, 1f);
                versionSelector.offsetMin = Vector2.zero;
                versionSelector.offsetMax = Vector2.zero;
                if (modEntry.OtherVersions.Count > 6)
                {
                    Scrollbar versionSelectorScrollBar = new GameObject("Scrollbar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Scrollbar)).GetComponent<Scrollbar>();
                    versionSelectorScrollBar.GetComponent<RectTransform>().SetParent(versionSelectorScrollRect.transform);
                    versionSelectorScrollBar.GetComponent<RectTransform>().anchorMin = new Vector2(1f, 0f);
                    versionSelectorScrollBar.GetComponent<RectTransform>().anchorMax = new Vector2(1.25f, 1f);
                    versionSelectorScrollBar.GetComponent<RectTransform>().offsetMin = Vector2.zero;
                    versionSelectorScrollBar.GetComponent<RectTransform>().offsetMax = Vector2.zero;
                    versionSelectorScrollBar.GetComponent<Image>().sprite = SceneInjector.BoxSprite;
                    versionSelectorScrollBar.GetComponent<Image>().type = Image.Type.Sliced;
                    versionSelectorScrollBar.GetComponent<Image>().fillCenter = true;
                    RectTransform versionSelectorScrollBarHandle = new GameObject("Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<RectTransform>();
                    versionSelectorScrollBarHandle.SetParent(versionSelectorScrollBar.transform);
                    versionSelectorScrollBarHandle.anchorMin = new Vector2(0.05f, 0.05f);
                    versionSelectorScrollBarHandle.anchorMax = new Vector2(0.95f, 0.95f);
                    versionSelectorScrollBarHandle.offsetMin = Vector2.zero;
                    versionSelectorScrollBarHandle.offsetMax = Vector2.zero;
                    versionSelectorScrollBarHandle.GetComponent<Image>().sprite = SceneInjector.BoxSprite;
                    versionSelectorScrollBarHandle.GetComponent<Image>().type = Image.Type.Sliced;
                    versionSelectorScrollBarHandle.GetComponent<Image>().fillCenter = true;
                    versionSelectorScrollBar.targetGraphic = versionSelectorScrollBarHandle.GetComponent<Image>();
                    versionSelectorScrollBar.handleRect = versionSelectorScrollBarHandle;
                    versionSelectorScrollBar.direction = Scrollbar.Direction.BottomToTop;
                    versionSelectorScrollRect.content = versionSelector;
                    versionSelectorScrollRect.horizontal = false;
                    versionSelectorScrollRect.scrollSensitivity = 5;
                    versionSelectorScrollRect.movementType = ScrollRect.MovementType.Clamped;
                    versionSelectorScrollRect.viewport = versionSelectorViewport;
                    versionSelectorScrollRect.verticalScrollbar = versionSelectorScrollBar;
                }
                float entryHeight = 1f / modEntry.OtherVersions.Count;
                int i = 0;
                foreach (string version in modEntry.OtherVersions.Keys.OrderByDescending(x => x))
                {
                    RectTransform versionEntryRect = new GameObject("Version Entry: " + version, typeof(RectTransform), typeof(Toggle), typeof(CanvasRenderer), typeof(Image), typeof(Mask)).GetComponent<RectTransform>();
                    versionEntryRect.SetParent(versionSelector);
                    versionEntryRect.anchorMin = new Vector2(0f, 1 - ((i + 1) * entryHeight));
                    versionEntryRect.anchorMax = new Vector2(1f, 1 - (i * entryHeight));
                    versionEntryRect.offsetMin = Vector2.zero;
                    versionEntryRect.offsetMax = Vector2.zero;
                    Toggle versionToggle = versionEntryRect.GetComponent<Toggle>();
                    Image versionSelected = new GameObject("Selected", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
                    versionSelected.rectTransform.SetParent(versionEntryRect);
                    versionSelected.rectTransform.anchorMin = new Vector2(0f, 0f);
                    versionSelected.rectTransform.anchorMax = new Vector2(1f, 1f);
                    versionSelected.rectTransform.offsetMin = Vector2.zero;
                    versionSelected.rectTransform.offsetMax = Vector2.zero;
                    versionSelected.sprite = SceneInjector.BoxSprite;
                    versionSelected.type = Image.Type.Sliced;
                    Text versionLabel = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
                    versionLabel.rectTransform.SetParent(versionEntryRect);
                    versionLabel.rectTransform.anchorMin = new Vector2(0f, 0f);
                    versionLabel.rectTransform.anchorMax = new Vector2(1f, 1f);
                    versionLabel.rectTransform.offsetMin = new Vector2(10, 10);
                    versionLabel.rectTransform.offsetMax = new Vector2(-10, -10);
                    versionLabel.font = SceneInjector.ModConfigMenuText.GetComponent<TextMesh>().font;
                    versionLabel.fontSize = 12;
                    versionLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
                    versionLabel.verticalOverflow = VerticalWrapMode.Overflow;
                    versionLabel.alignment = TextAnchor.MiddleLeft;
                    versionToggle.GetComponent<Image>().sprite = SceneInjector.BoxSprite;
                    versionToggle.GetComponent<Image>().type = Image.Type.Sliced;
                    versionToggle.transition = Selectable.Transition.None;
                    versionToggle.isOn = version == modEntry.Info["Version"];
                    versionToggle.toggleTransition = Toggle.ToggleTransition.None;
                    versionToggle.graphic = versionSelected;
                    versionToggle.group = versionSelector.GetComponent<ToggleGroup>();
                    string localVersion = version;
                    versionToggle.onValueChanged.AddListener((toggled) => { if (toggled) { StartCoroutine(SelectVersion(modEntry, localVersion)); } });

                    versionSelected.color = new Color(1f, 1f, 0.5f, 1f);
                    versionToggle.GetComponent<Image>().color = new Color(1f, 1f, 0.5f, 0.25f);
                    versionLabel.color = new Color(1f, 1f, 1f, 1f);
                    versionLabel.text = "Version " + version;

                    i++;
                }

                versionSelectorScrollRect.GetComponent<RectTransform>().localScale = Vector3.one;
            }
            else
            {
                Destroy(versionSelectorScrollRect.gameObject);
            }
        }

        private IEnumerator SelectVersion(ModBrowserEntry modEntry, string version)
        {
            modEntry.Info["Version"] = version;
            string versionType = modEntry.OtherVersions[version].Substring(0, 3);
            switch (versionType)
            {
                case "url":
                    yield return StartCoroutine(ProcessMetadataURL(modEntry.ID, modEntry.OtherVersions[version].Substring(4), modEntry));
                    break;
                case "git":
                    yield return StartCoroutine(ProcessGitVersion(modEntry.ID, modEntry.OtherVersions[version].Substring(4), modEntry));
                    break;
            }
            UpdateInfo(toggle, modIndex);
            yield break;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity Message")]
        private void Awake()
        {
            if (Singleton != null) Destroy(Singleton);
            Singleton = this;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity Message")]
        private void Update()
        {
            if (listLoading)
            {
                if (!ListLoadingText.transform.parent.gameObject.activeSelf)
                {
                    ListLoadingText.transform.parent.gameObject.SetActive(true);
                }
                switch ((int)(Time.time % 1 * 4))
                {
                    case 0:
                        ListLoadingText.text = "Loading   ";
                        break;
                    case 1:
                        ListLoadingText.text = "Loading.  ";
                        break;
                    case 2:
                        ListLoadingText.text = "Loading.. ";
                        break;
                    case 3:
                        ListLoadingText.text = "Loading...";
                        break;
                }
            }
            else
            {
                ListLoadingText.text = built ? string.Empty : "Load Failed!";
                if (built && ListLoadingText.transform.parent.gameObject.activeSelf)
                {
                    ListLoadingText.transform.parent.gameObject.SetActive(false);
                }
            }
            if (listLoading || descLoading)
            {
                if (!DescLoadingText.transform.parent.gameObject.activeSelf)
                {
                    DescLoadingText.transform.parent.gameObject.SetActive(true);
                }
                switch ((int)(Time.time % 1 * 4))
                {
                    case 0:
                        DescLoadingText.text = "Loading   ";
                        break;
                    case 1:
                        DescLoadingText.text = "Loading.  ";
                        break;
                    case 2:
                        DescLoadingText.text = "Loading.. ";
                        break;
                    case 3:
                        DescLoadingText.text = "Loading...";
                        break;
                }
            }
            else
            {
                DescLoadingText.text = !string.IsNullOrEmpty(DescText.text) || !built ? string.Empty : "Load Failed!";
                if (!string.IsNullOrEmpty(DescText.text) && DescLoadingText.transform.parent.gameObject.activeSelf)
                {
                    DescLoadingText.transform.parent.gameObject.SetActive(false);
                }
            }
        }

        private IEnumerator LoadModList()
        {
            if (listLoading) yield break;
            listLoading = true;
            Singleton.Clean();
            using (WWW modsWWW = new WWW(MODS_URL))
            {
                yield return new WaitUntil(() => modsWWW.isDone);
                IniData modsIni;
                SectionData modsSection = null;
                try
                {
                    modsIni = new StringIniParser().ParseString(modsWWW.text);
                    modsSection = modsIni.Sections.SingleOrDefault(x => x.SectionName == "Mods");
                }
                catch (Exception e)
                {
                    GadgetCore.CoreLogger.LogError("An error occured while reading the Mods.ini file on the Roguelands-Mods repository: " + e);
                }
                if (modsSection != null)
                {
                    foreach (KeyData modKey in modsSection.Keys)
                    {
                        try
                        {
                            Dictionary<string, string> modInfo = modKey.Value.Split(',').Select(x => x.Split(new char[] { ':' }, 2)).Where(x => x.Length == 2).ToDictionary(x => x[0], x => x[1].Replace("\\n", "\n"));
                            string modURL = null;
                            if (modInfo.TryGetValue("URL", out modURL))
                            {
                                if (modURL.Length > 0 && modURL[0] == '/') modInfo["URL"] = modURL = GIT_RAW_URL + modURL;
                            }
                            if (!modInfo.ContainsKey("Name")) modInfo.Add("Name", modKey.KeyName);
                            modEntries.Add(new ModBrowserEntry(modKey.KeyName, modURL, modInfo));
                        }
                        catch (Exception e)
                        {
                            GadgetCore.CoreLogger.LogError("An error occured while reading the entry for " + modKey.KeyName + " in the Mods.ini file on the Roguelands-Mods repository: " + e);
                        }
                    }
                }
                else
                {
                    GadgetCore.CoreLogger.LogWarning("The Mods.ini file on the Roguelands-Mods repository is not formatted as expected, and cannot be read!");
                }
            }
            modEntries.Sort((a, b) => string.Compare(a.Info["Name"], b.Info["Name"]));
            listLoading = false;
            Build();
            yield break;
        }

        private IEnumerator ProcessMetadataURL(string ID, string URL, ModBrowserEntry modEntry)
        {
            using (WWW modWWW = new WWW(URL))
            {
                yield return new WaitUntil(() => modWWW.isDone);
                modEntry.Info.Remove("URL");
                try
                {
                    IniData modIni = new StringIniParser().ParseString(modWWW.text);
                    SectionData modSection = modIni.Sections.SingleOrDefault(x => x.SectionName == ID);
                    if (modSection != null)
                    {
                        foreach (KeyData modKey in modSection.Keys)
                        {
                            if (modKey.KeyName == "OtherVersions")
                            {
                                Dictionary<string, string> otherVersions = modKey.Value.Split(',').Select(x => x.Split(new char[] { ':' }, 2)).Where(x => x.Length == 2).ToDictionary(x => x[0], x => x[1].Replace("\\n", "\n"));
                                foreach (KeyValuePair<string, string> otherVersion in otherVersions)
                                {
                                    string versionURL = otherVersion.Value;
                                    if (versionURL.Length > 0 && versionURL[0] == '/') versionURL = (modEntry.Info.TryGetValue("RootPath", out string rootPath) ? rootPath : GIT_RAW_URL) + versionURL;
                                    if (!modEntry.OtherVersions.ContainsKey(otherVersion.Key)) modEntry.OtherVersions.Add(otherVersion.Key, "url:" + versionURL);
                                }
                            }
                            else
                            {
                                modEntry.Info[modKey.KeyName] = modKey.Value.Replace("\\n", "\n");
                            }
                        }
                    }
                    else
                    {
                        GadgetCore.CoreLogger.Log("The ini file at the URL '" + URL + "' does not contain a section matching the ID '" + ID + "'!");
                    }
                }
                catch (Exception e)
                {
                    GadgetCore.CoreLogger.Log("An error occured while reading the ini file at the URL '" + URL + "': " + e);
                    yield break;
                }
                if (modEntry.Info.TryGetValue("URL", out string newURL) && newURL.Length > 0 && newURL[0] == '/')
                {
                    modEntry.Info["URL"] = (modEntry.Info.TryGetValue("RootPath", out string rootPath) ? rootPath : GIT_RAW_URL) + newURL;
                }
                if (modEntry.Info.TryGetValue("File", out string fileURL))
                {
                    if (fileURL.Length > 0 && fileURL[0] == '/')
                    {
                        modEntry.Info["File"] = (modEntry.Info.TryGetValue("RootPath", out string rootPath) ? rootPath : GIT_RAW_URL) + fileURL;
                    }
                }
                if (modEntry.Info.TryGetValue("Version", out string version))
                {
                    modEntry.OtherVersions[version] = "url:" + URL;
                }
                if (modEntry.Info.TryGetValue("Git", out string gitURL))
                {
                    string[] splitString = gitURL.Split(new char[] { ':' }, 3);
                    if (splitString.Length == 3)
                    {
                        using (WWW gitWWW = new WWW(string.Format(GIT_API_URL, splitString[0], splitString[1])))
                        {
                            yield return new WaitUntil(() => gitWWW.isDone);
                            try
                            {
                                JObject lastVersionJSON = null;
                                string downloadURL = null;
                                JToken responseToken = JToken.Parse(gitWWW.text);
                                if (responseToken is JArray responseArray)
                                {
                                    foreach (JObject versionJSON in responseArray.Reverse())
                                    {
                                        lastVersionJSON = versionJSON;
                                        downloadURL = versionJSON
                                                .Value<JArray>("assets")
                                                .ToObject<List<JObject>>().FirstOrDefault(x => Regex.IsMatch(x.Value<string>("name"), splitString[2]))
                                                ?.Value<string>("browser_download_url");
                                        if (downloadURL == null) continue;
                                        modEntry.OtherVersions[versionJSON.Value<string>("tag_name").TrimStart('v')] = "git:" + versionJSON.Value<string>("id");
                                    }
                                    if (downloadURL != null)
                                    {
                                        modEntry.Info["File"] = downloadURL;
                                        string body = lastVersionJSON.Value<string>("body");
                                        if (body.StartsWith("GCVersion:") || body.StartsWith("GC Version:") ||
                                            body.StartsWith("GadgetCoreVersion:") || body.StartsWith("GadgetCore Version:") ||
                                            body.StartsWith("RequiredGCVersion:") || body.StartsWith("Required GC Version:") ||
                                            body.StartsWith("RequiredGadgetCoreVersion:") || body.StartsWith("Required GadgetCore Version:"))
                                        {
                                            string[] splitBody = body.Split(new char[] { '\r', '\n' }, 2);
                                            modEntry.Info["GCVersion"] = splitBody[0].Split(new char[] { ':' }, 2)[1].Trim();
                                            body = splitBody[1];
                                        }
                                        modEntry.Info["Version"] = lastVersionJSON.Value<string>("tag_name").TrimStart('v');
                                        string[] bodyLines = body.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                                        int indexOfDescription = bodyLines.IndexOf(x => x.StartsWith("Desc:") || x.StartsWith("Description:"));
                                        if (indexOfDescription >= 0)
                                        {
                                            string firstLine = bodyLines[indexOfDescription].Split(new char[] { ':' }, 2)[1].Trim();
                                            string[] changeLines = new string[indexOfDescription];
                                            string[] descLines = new string[bodyLines.Length - indexOfDescription - (string.IsNullOrEmpty(firstLine) ? 1 : 0)];
                                            if (changeLines.Length > 0)
                                            {
                                                Array.Copy(bodyLines, changeLines, changeLines.Length);
                                                modEntry.Info["Changed"] = changeLines.Concat("\n");
                                            }
                                            Array.Copy(bodyLines, indexOfDescription + (string.IsNullOrEmpty(firstLine) ? 1 : 0), descLines, 0, descLines.Length);
                                            modEntry.Info["Description"] = descLines.Concat("\n");
                                        }
                                        else
                                        {
                                            modEntry.Info["Changed"] = body;
                                        }
                                    }
                                }
                                else if (responseToken is JObject responseObject)
                                {
                                    string message = responseObject.Value<string>("message");
                                    if (message != null && message.StartsWith("API rate limit exceeded"))
                                    {
                                        GadgetCore.CoreLogger.LogWarning("GitHub API Rate limit exceeded for " + modEntry.Info["Name"] + "!");
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                GadgetCore.CoreLogger.Log("An error occured while trying to fetch GitHub releases with the target '" + gitURL + "': " + e);
                            }
                        }
                    }
                }
            }
        }

        private IEnumerator ProcessGitVersion(string ID, string gitID, ModBrowserEntry modEntry)
        {
            if (modEntry.Info.TryGetValue("Git", out string gitURL))
            {
                string[] splitString = gitURL.Split(new char[] { ':' }, 3);
                if (splitString.Length == 3)
                {
                    using (WWW gitWWW = new WWW(string.Format(GIT_API_URL + '/' + gitID, splitString[0], splitString[1])))
                    {
                        yield return new WaitUntil(() => gitWWW.isDone);
                        try
                        {
                            JObject versionJSON = JsonConvert.DeserializeObject<JObject>(gitWWW.text);
                            string message = versionJSON.Value<string>("message");
                            if (message != null && message.StartsWith("API rate limit exceeded"))
                            {
                                GadgetCore.CoreLogger.LogWarning("GitHub API Rate limit exceeded for " + modEntry.Info["Name"] + "!");
                            }
                            else
                            {
                                string downloadURL = versionJSON
                                    .Value<JArray>("assets")
                                    .ToObject<List<JObject>>().FirstOrDefault(x => Regex.IsMatch(x.Value<string>("name"), splitString[2]))
                                    ?.Value<string>("browser_download_url");
                                modEntry.Info["File"] = downloadURL;
                                string body = versionJSON.Value<string>("body");
                                if (body.StartsWith("GCVersion:") || body.StartsWith("GC Version:") ||
                                    body.StartsWith("GadgetCoreVersion:") || body.StartsWith("GadgetCore Version:") ||
                                    body.StartsWith("RequiredGCVersion:") || body.StartsWith("Required GC Version:") ||
                                    body.StartsWith("RequiredGadgetCoreVersion:") || body.StartsWith("Required GadgetCore Version:"))
                                {
                                    string[] splitBody = body.Split(new char[] { '\r', '\n' }, 2);
                                    modEntry.Info["GCVersion"] = splitBody[0].Split(new char[] { ':' }, 2)[1].Trim();
                                    body = splitBody[1];
                                }
                                modEntry.Info["Version"] = versionJSON.Value<string>("tag_name").TrimStart('v');
                                string[] bodyLines = body.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                                int indexOfDescription = bodyLines.IndexOf(x => x.StartsWith("Desc:") || x.StartsWith("Description:"));
                                if (indexOfDescription >= 0)
                                {
                                    string firstLine = bodyLines[indexOfDescription].Split(new char[] { ':' }, 2)[1].Trim();
                                    string[] changeLines = new string[indexOfDescription];
                                    string[] descLines = new string[bodyLines.Length - indexOfDescription - (string.IsNullOrEmpty(firstLine) ? 1 : 0)];
                                    if (changeLines.Length > 0)
                                    {
                                        Array.Copy(bodyLines, changeLines, changeLines.Length);
                                        modEntry.Info["Changed"] = changeLines.Concat("\n");
                                    }
                                    Array.Copy(bodyLines, indexOfDescription + (string.IsNullOrEmpty(firstLine) ? 1 : 0), descLines, 0, descLines.Length);
                                    modEntry.Info["Description"] = descLines.Concat("\n");
                                }
                                else
                                {
                                    modEntry.Info["Changed"] = body;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            GadgetCore.CoreLogger.Log("An error occured while trying to fetch GitHub release " + gitID + " with the target '" + gitURL + "': " + e);
                            yield break;
                        }
                    }
                }
            }
        }

        private void Build()
        {
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
            modList.anchorMin = new Vector2(0f, modEntries.Count <= 12 ? 0f : (1f - (modEntries.Count / 12f)));
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
            if (modEntries.Count <= 12) modListScrollBar.interactable = false;
            modListScrollView.content = modList;
            modListScrollView.horizontal = false;
            modListScrollView.scrollSensitivity = 5;
            modListScrollView.movementType = ScrollRect.MovementType.Clamped;
            modListScrollView.viewport = modListViewport;
            modListScrollView.verticalScrollbar = modListScrollBar;
            float entryHeight = modEntries.Count <= 12 ? (1f / 12f) : (1f / modEntries.Count);
            Toggle selectedToggle = null;
            int bonusOffset = 0;
            for (int i = 0; i < modEntries.Count; i++)
            {
                ModBrowserEntry modEntry = modEntries[i];
                RectTransform modEntryRect = new GameObject("Mod Entry: " + modEntry.Info["Name"], typeof(RectTransform), typeof(Toggle), typeof(CanvasRenderer), typeof(Image), typeof(Mask)).GetComponent<RectTransform>();
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
                modLabel.rectTransform.offsetMin = new Vector2(10, 0);
                modLabel.rectTransform.offsetMax = new Vector2(-10, 0);
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
                modToggle.onValueChanged.AddListener((toggled) =>
                {
                    if (toggled)
                    {
                        GadgetCoreAPI.CloseDialog();
                        StartCoroutine(ProcessEntryMetadata(modEntry, modToggle, toggleIndex));
                    }
                });

                GadgetMod existingMod = GadgetMods.GetModByName(modEntry.Info["Name"]);

                if (existingMod != null)
                {
                    modSelected.color = new Color(0.75f, 1f, 0.5f, 1f);
                    modToggle.GetComponent<Image>().color = new Color(0.75f, 1f, 0.5f, 0.25f);
                }
                else
                {
                    modSelected.color = new Color(1f, 0.75f, 0.5f, 1f);
                    modToggle.GetComponent<Image>().color = new Color(1f, 0.75f, 0.5f, 0.25f);
                }
                modLabel.color = new Color(1f, 1f, 1f, 1f);
                modLabel.text = modEntry.Info["Name"];
            }

            modListScrollView.GetComponent<RectTransform>().localScale = Vector3.one;
            if (scrollPositionCache >= 0) modListScrollView.verticalNormalizedPosition = 1 - (scrollPositionCache / (modList.anchorMax.y - modList.anchorMin.y));
            scrollPositionCache = -1;

            built = true;

            StartCoroutine(ProcessEntryMetadata(modEntries[modIndex], selectedToggle, modIndex));
        }

        private IEnumerator ProcessEntryMetadata(ModBrowserEntry modEntry, Toggle toggle, int modIndex)
        {
            descLoading = true;
            DescText.text = string.Empty;
            InstallButton.interactable = false;
            InstallButton.GetComponentInChildren<Text>().color = new Color(1f, 1f, 1f, 0.25f);
            ActivateButton.interactable = false;
            ActivateButton.GetComponentInChildren<Text>().color = new Color(1f, 1f, 1f, 0.25f);
            VersionsButton.interactable = false;
            VersionsButton.GetComponentInChildren<Text>().color = new Color(1f, 1f, 1f, 0.25f);
            while (modEntry.Info.TryGetValue("URL", out string modURL))
            {
                if (modURL.Length > 0 && modURL[0] == '/') modEntry.Info["URL"] = modURL = GIT_RAW_URL + modURL;
                yield return StartCoroutine(ProcessMetadataURL(modEntry.ID, modURL, modEntry));
            }
            UpdateInfo(toggle, modIndex);
            descLoading = false;
        }

        private void Clean()
        {
            built = false;
            RectTransform modListScrollView = transform.Find("Scroll View") as RectTransform;
            if (modListScrollView != null)
            {
                RectTransform modList = modListScrollView.Find("Mask").Find("Viewport").Find("ModList") as RectTransform;
                scrollPositionCache = (1 - modListScrollView.GetComponent<ScrollRect>().verticalNormalizedPosition) * (modList.anchorMax.y - modList.anchorMin.y);
                Destroy(modListScrollView.gameObject);
            }
            modEntries.Clear();
            DescText.text = string.Empty;
        }

        private void UpdateInfo(Toggle toggle, int modIndex)
        {
            try
            {
                if (versionSelectorScrollRect != null) Destroy(versionSelectorScrollRect.gameObject);

                this.toggle = toggle;
                this.modIndex = modIndex;

                StringBuilder DescTextBuilder = new StringBuilder();

                ModBrowserEntry modEntry = modEntries[modIndex];

                InstallButton.interactable = modEntry.Info.ContainsKey("File");
                InstallButton.GetComponentInChildren<Text>().color = InstallButton.interactable ? new Color(1f, 1f, 1f, 1f) : new Color(1f, 1f, 1f, 0.25f);
                ActivateButton.interactable = modEntry.Info.ContainsKey("File") && File.Exists(Path.Combine(GadgetPaths.ModsPath, Path.GetFileName(modEntry.Info["File"]))) && GadgetMods.GetModByPath(Path.Combine(GadgetPaths.ModsPath, Path.GetFileName(modEntry.Info["File"]))) == null;
                ActivateButton.GetComponentInChildren<Text>().color = ActivateButton.interactable ? new Color(1f, 1f, 1f, 1f) : new Color(1f, 1f, 1f, 0.25f);
                VersionsButton.interactable = modEntry.Info.ContainsKey("Version") && modEntry.OtherVersions.Count > 1;
                VersionsButton.GetComponentInChildren<Text>().color = VersionsButton.interactable ? new Color(1f, 1f, 1f, 1f) : new Color(1f, 1f, 1f, 0.25f);
                GadgetMod existingMod = GadgetMods.GetModByName(modEntry.Info["Name"]);
                string existingVersionString = existingMod != null && modEntry.Info.ContainsKey("Version") ? existingMod.Version.ToString(4) : null;
                string paddedVersion = modEntry.Info.ContainsKey("Version") ? modEntry.Info["Version"] + Enumerable.Repeat(".0", 3 - modEntry.Info["Version"].Count(x => x == '.')).Concat(string.Empty) : null;
                InstallButton.GetComponentInChildren<Text>().text = existingMod != null ? modEntry.Info.ContainsKey("Version") && paddedVersion != null && paddedVersion != existingVersionString ? string.Compare(paddedVersion, existingVersionString) > 0 ? "Update" : "Downgrade" : "Reinstall" : "Install";
                VersionsButton.GetComponentInChildren<Text>().text = VersionsButton.interactable ? "Version " + modEntry.Info["Version"] : "Only Version";
                DescTextBuilder.Append((existingMod != null ? paddedVersion != null ? string.Compare(paddedVersion, existingVersionString) > 0 ? "Installed - Update Available!" : "Installed - Up To Date" : "Installed - Version Unknown" : "Not Installed") + "\n");
                foreach (KeyValuePair<string, string> info in modEntries[modIndex].Info)
                {
                    if (info.Key == "Description" || info.Key == "URL") continue;
                    DescTextBuilder.Append('\n');
                    DescTextBuilder.Append(Regex.Replace(info.Key, PASCAL_CASE_SPACING_REGEX, " $1") + ": " + info.Value);
                }
                if (modEntries[modIndex].Info.ContainsKey("Description")) DescTextBuilder.Append("\n\n" + modEntries[modIndex].Info["Description"]);
                DescText.text = DescTextBuilder.ToString();
            }
            catch (Exception e)
            {
                DescText.text = "An error occured while populating the info panel for this mod! Check GadgetCore.log for details.";
                GadgetCore.CoreLogger.LogError("An error occured while populating the info panel for the mod in the browser with the index " + modIndex + ": " + e.ToString(), false);
            }
        }

        private sealed class ModBrowserEntry
        {
            public readonly string ID;
            public readonly string URL;
            public Dictionary<string, string> Info { get; private set; }
            public Dictionary<string, string> OtherVersions { get; private set; }

            internal ModBrowserEntry(string ID, string URL, Dictionary<string, string> Info = null)
            {
                this.ID = ID;
                this.URL = URL;
                this.Info = Info ?? new Dictionary<string, string>();
                OtherVersions = new Dictionary<string, string>();
            }

            internal void AddInfo(Dictionary<string, string> Info)
            {
                this.Info = Info.Concat(this.Info.Where(kvp => !Info.ContainsKey(kvp.Key))).ToDictionary(x => x.Key, x => x.Value);
            }
        }
    }
}
