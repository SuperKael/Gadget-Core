using GadgetCore.API;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UModFramework.API;
using UnityEngine;
using UnityEngine.UI;

namespace GadgetCore
{
    internal class ModDescPanelController : MonoBehaviour
    {
        private static Dictionary<int, string> originalConfig = new Dictionary<int, string>();
        private static Dictionary<int, bool> wasEnabled = new Dictionary<int, bool>();
        private static Dictionary<int, bool> wasUMFEnabled = new Dictionary<int, bool>();

        internal static List<Process> configHandles { get; private set; } = new List<Process>();

        private Toggle toggle;
        private int modIndex;
        internal Text descText;
        internal GameObject restartRequiredText;
        internal Button configButton;
        internal Button enableUMFButton;
        internal Button enableButton;

        internal static bool RestartNeeded { get; private set; } = false;

        internal void Start()
        {
            for (int i = 0;i < GadgetMods.CountMods();i++)
            {
                wasEnabled[i] = GadgetMods.GetModInfo(i).Mod.Enabled;
                wasUMFEnabled[i] = true;
            }
            for (int i = 0; i < GadgetCore.nonGadgetMods.Count; i++)
            {
                wasUMFEnabled[GadgetMods.CountMods() + i] = true;
            }
            for (int i = 0; i < GadgetCore.disabledMods.Count; i++)
            {
                wasUMFEnabled[GadgetMods.CountMods() + GadgetCore.nonGadgetMods.Count + i] = false;
            }
            for (int i = 0; i < GadgetCore.incompatibleMods.Count; i++)
            {
                wasUMFEnabled[GadgetMods.CountMods() + GadgetCore.nonGadgetMods.Count + GadgetCore.disabledMods.Count + i] = false;
            }
            StartCoroutine(WatchForRestartNeeded());
        }

        public void UpdateInfo(Toggle toggle, int modIndex)
        {
            if (!File.Exists(UMFData.DisabledModsFile)) File.Create(UMFData.DisabledModsFile).Dispose();
            this.toggle = toggle;
            this.modIndex = modIndex;
            string[] disabledMods = File.ReadAllLines(UMFData.DisabledModsFile).Where(x => !string.IsNullOrEmpty(x)).ToArray();
            if (modIndex < GadgetMods.CountMods())
            {
                GadgetModInfo mod = GadgetMods.ListAllModInfos()[modIndex];
                enableUMFButton.interactable = !GadgetCore.dependencies.Any(x => !disabledMods.Contains(x.Key) && x.Value.Any(d => { string[] split = d.Split(' '); return split[split.Length - 2].Equals(mod.UMFName); }));
                enableButton.interactable = true;
                enableButton.GetComponentInChildren<Text>().text = mod.Mod.Enabled ? "Disable Gadget" : "Enable Gadget";
                enableUMFButton.GetComponentInChildren<Text>().text = File.ReadAllLines(UMFData.DisabledModsFile).Any(x => x.Equals(mod.UMFName)) ? "Enable Mod" : "Disable Mod";
                string desc = mod.Mod.GetModDescription();
                bool isDescNull = desc == null;
                bool isModInfo = false;
                if (string.IsNullOrEmpty(desc))
                {
                    try
                    {
                        desc = File.ReadAllText(UMFData.ModInfosPath + "/" + mod.UMFName + "_v" + UMFMod.GetModVersion(mod.UMFName) + "_ModInfo.txt");
                    }
                    catch (IOException) { }
                    finally
                    {
                        if (string.IsNullOrEmpty(desc) || desc == "A UMF Mod(umodframework.com) for Roguelands")
                        {
                            desc = "This mod does not have a description, or a ModInfo file." + Environment.NewLine + (isDescNull ? "You should suggest to the mod author that they add a description." : "");
                        }
                        else
                        {
                            isModInfo = true;
                        }
                    }
                }
                descText.text =
                    mod.Attribute.Name + " v" + UMFMod.GetModVersion(mod.UMFName).ToString() + (mod.Mod.Enabled ? " (Enabled)" : " (Disabled)") + Environment.NewLine +
                    "UMF Mod: " + mod.UMFName + Environment.NewLine +
                    (GadgetCore.dependencies.ContainsKey(mod.UMFName) ? ("Dependencies: " + GadgetCore.dependencies[mod.UMFName].Aggregate((x, y) => x + ", " + y) + Environment.NewLine) : "") +
                    (isModInfo ? "UMF Mod Info: " : "Description: ") + UMFMod.GetModDescription(mod.UMFName) + Environment.NewLine + Environment.NewLine + desc;
            }
            else
            {
                bool enabled = !File.ReadAllLines(UMFData.DisabledModsFile).Any(x => x.Equals(modIndex - GadgetMods.CountMods() < GadgetCore.nonGadgetMods.Count ? GadgetCore.nonGadgetMods[modIndex - GadgetMods.CountMods()] : modIndex - GadgetMods.CountMods() - GadgetCore.nonGadgetMods.Count < GadgetCore.disabledMods.Count ? GadgetCore.disabledMods[modIndex - GadgetMods.CountMods() - GadgetCore.nonGadgetMods.Count] : GadgetCore.incompatibleMods[modIndex - GadgetMods.CountMods() - GadgetCore.nonGadgetMods.Count - GadgetCore.disabledMods.Count]));
                enableButton.GetComponentInChildren<Text>().text = enabled ? "Disable Gadget" : "Enable Gadget";
                enableUMFButton.GetComponentInChildren<Text>().text = enabled ? "Disable Mod" : "Enable Mod";
                string mod = modIndex - GadgetMods.CountMods() < GadgetCore.nonGadgetMods.Count ? GadgetCore.nonGadgetMods[modIndex - GadgetMods.CountMods()] : modIndex - GadgetMods.CountMods() - GadgetCore.nonGadgetMods.Count < GadgetCore.disabledMods.Count ? GadgetCore.disabledMods[modIndex - GadgetMods.CountMods() - GadgetCore.nonGadgetMods.Count] : GadgetCore.incompatibleMods[modIndex - GadgetMods.CountMods() - GadgetCore.nonGadgetMods.Count - GadgetCore.disabledMods.Count];
                if (modIndex - GadgetMods.CountMods() < GadgetCore.nonGadgetMods.Count && GadgetCore.nonGadgetMods[modIndex - GadgetMods.CountMods()].Equals("GadgetCore"))
                {
                    enableUMFButton.interactable = false;
                }
                else
                {
                    enableUMFButton.interactable = !GadgetCore.dependencies.Any(x => !disabledMods.Contains(x.Key) && x.Value.Any(d => { string[] split = d.Split(' '); return split[split.Length - 2].Equals(mod); }));
                }
                enableButton.interactable = false;
                string desc = null;
                try
                {
                    desc = File.ReadAllText(UMFData.ModInfosPath + "/" + mod + "_v" + UMFMod.GetModVersion(mod) + "_ModInfo.txt");
                }
                catch (IOException) { }
                finally
                {
                    if (string.IsNullOrEmpty(desc) || desc == "A UMF Mod(umodframework.com) for Roguelands") desc = "This mod does not have a ModInfo file.";
                    descText.text =
                        mod + " v" + UMFMod.GetModVersion(mod).ToString() + (enabled ? " (Enabled)" : " (Disabled)") + Environment.NewLine +
                        (GadgetCore.dependencies.ContainsKey(mod) ? ("Dependencies: " + GadgetCore.dependencies[mod].Aggregate((x, y) => x + ", " + y) + Environment.NewLine) : "") +
                        "UMF Mod Info: " + UMFMod.GetModDescription(mod) + Environment.NewLine + Environment.NewLine + desc;
                }
            }
        }

        internal void ConfigButton()
        {
            string filePath;
            if (modIndex < GadgetMods.CountMods())
            {
                filePath = UMFData.ConfigsPath + "/" + GadgetMods.GetModInfo(modIndex).UMFName + ".ini";
            }
            else if (modIndex - GadgetMods.CountMods() < GadgetCore.nonGadgetMods.Count)
            {
                filePath = UMFData.ConfigsPath + "/" + GadgetCore.nonGadgetMods[modIndex - GadgetMods.CountMods()] + ".ini";
            }
            else if (modIndex - GadgetMods.CountMods() - GadgetCore.nonGadgetMods.Count < GadgetCore.disabledMods.Count)
            {
                filePath = UMFData.ConfigsPath + "/" + GadgetCore.disabledMods[modIndex - GadgetMods.CountMods() - GadgetCore.nonGadgetMods.Count] + ".ini";
            }
            else
            {
                filePath = UMFData.ConfigsPath + "/" + GadgetCore.disabledMods[modIndex - GadgetMods.CountMods() - GadgetCore.nonGadgetMods.Count - GadgetCore.disabledMods.Count] + ".ini";
            }
            try
            {
                if (!originalConfig.ContainsKey(modIndex))
                {
                    originalConfig.Add(modIndex, File.ReadAllText(filePath));
                }
                configHandles.Add(Process.Start(filePath));
            }
            catch (Exception e) { GadgetCore.Log(e.ToString()); }
        }

        internal void EnableUMFButton()
        {
            if (!File.Exists(UMFData.DisabledModsFile)) File.Create(UMFData.DisabledModsFile).Dispose();
            string fileText = File.ReadAllText(UMFData.DisabledModsFile);
            string[] fileLines = File.ReadAllLines(UMFData.DisabledModsFile);
            if (modIndex < GadgetMods.CountMods())
            {
                if (fileLines.Any(x => x.Equals(GadgetMods.GetModInfo(modIndex).UMFName)))
                {
                    File.WriteAllLines(UMFData.DisabledModsFile, fileLines.Where(x => !x.Equals(GadgetMods.GetModInfo(modIndex).UMFName)).ToArray());
                    enableButton.GetComponentInChildren<Text>().text = GadgetMods.GetModInfo(modIndex).Mod.Enabled ? "Disable Gadget" : "Enable Gadget";
                    enableUMFButton.GetComponentInChildren<Text>().text = "Disable Mod";
                    toggle.GetComponentInChildren<Text>().color = GadgetMods.GetModInfo(modIndex).Mod.Enabled ? new Color(1f, 1f, 1f, 1f) : new Color(1f, 1f, 1f, 0.5f);
                }
                else
                {
                    File.WriteAllText(UMFData.DisabledModsFile, fileText + Environment.NewLine + GadgetMods.GetModInfo(modIndex).UMFName);
                    GadgetMods.SetEnabled(modIndex, false);
                    enableButton.GetComponentInChildren<Text>().text = "Enable Gadget";
                    enableUMFButton.GetComponentInChildren<Text>().text = "Enable Mod";
                    toggle.GetComponentInChildren<Text>().color = new Color(1f, 1f, 1f, 0.5f);
                }
            }
            else if (modIndex - GadgetMods.CountMods() < GadgetCore.nonGadgetMods.Count)
            {
                if (fileLines.Any(x => x.Equals(GadgetCore.nonGadgetMods[modIndex - GadgetMods.CountMods()])))
                {
                    File.WriteAllLines(UMFData.DisabledModsFile, fileLines.Where(x => !x.Equals(GadgetCore.nonGadgetMods[modIndex - GadgetMods.CountMods()])).ToArray());
                    enableUMFButton.GetComponentInChildren<Text>().text = "Disable Mod";
                    toggle.GetComponentInChildren<Text>().color = new Color(1f, 1f, 1f, 1f);
                }
                else
                {
                    File.WriteAllText(UMFData.DisabledModsFile, fileText + Environment.NewLine + GadgetCore.nonGadgetMods[modIndex - GadgetMods.CountMods()]);
                    enableUMFButton.GetComponentInChildren<Text>().text = "Enable Mod";
                    toggle.GetComponentInChildren<Text>().color = new Color(1f, 1f, 1f, 0.5f);
                }
            }
            else if (modIndex - GadgetMods.CountMods() - GadgetCore.nonGadgetMods.Count < GadgetCore.disabledMods.Count)
            {
                if (fileLines.Any(x => x.Equals(GadgetCore.disabledMods[modIndex - GadgetMods.CountMods() - GadgetCore.nonGadgetMods.Count])))
                {
                    File.WriteAllLines(UMFData.DisabledModsFile, fileLines.Where(x => !x.Equals(GadgetCore.disabledMods[modIndex - GadgetMods.CountMods() - GadgetCore.nonGadgetMods.Count])).ToArray());
                    enableUMFButton.GetComponentInChildren<Text>().text = "Disable Mod";
                    toggle.GetComponentInChildren<Text>().color = new Color(1f, 1f, 1f, 1f);
                }
                else
                {
                    File.WriteAllText(UMFData.DisabledModsFile, fileText + Environment.NewLine + GadgetCore.disabledMods[modIndex - GadgetMods.CountMods() - GadgetCore.nonGadgetMods.Count]);
                    enableUMFButton.GetComponentInChildren<Text>().text = "Enable Mod";
                    toggle.GetComponentInChildren<Text>().color = new Color(1f, 1f, 1f, 0.5f);
                }
            }
            else
            {
                if (fileLines.Any(x => x.Equals(GadgetCore.incompatibleMods[modIndex - GadgetMods.CountMods() - GadgetCore.nonGadgetMods.Count - GadgetCore.disabledMods.Count])))
                {
                    File.WriteAllLines(UMFData.DisabledModsFile, fileLines.Where(x => !x.Equals(GadgetCore.incompatibleMods[modIndex - GadgetMods.CountMods() - GadgetCore.nonGadgetMods.Count - GadgetCore.disabledMods.Count])).ToArray());
                    enableUMFButton.GetComponentInChildren<Text>().text = "Disable Mod";
                    toggle.GetComponentInChildren<Text>().color = new Color(1f, 1f, 1f, 1f);
                }
                else
                {
                    File.WriteAllText(UMFData.DisabledModsFile, fileText + Environment.NewLine + GadgetCore.incompatibleMods[modIndex - GadgetMods.CountMods() - GadgetCore.nonGadgetMods.Count - GadgetCore.disabledMods.Count]);
                    enableUMFButton.GetComponentInChildren<Text>().text = "Enable Mod";
                    toggle.GetComponentInChildren<Text>().color = new Color(1f, 1f, 1f, 0.5f);
                }
            }
            UpdateRestartNeeded();
        }

        internal void EnableButton()
        {
            if (modIndex < GadgetMods.CountMods())
            {
                bool enabled = !GadgetMods.GetModInfo(modIndex).Mod.Enabled;
                GadgetMods.SetEnabled(modIndex, enabled);
                enableButton.GetComponentInChildren<Text>().text = enabled ? "Disable Gadget" : "Enable Gadget";
                toggle.GetComponentInChildren<Text>().color = enabled ? new Color(1f, 1f, 1f, 1f) : new Color(1f, 1f, 1f, 0.5f);
                if (enabled)
                {
                    if (!File.Exists(UMFData.DisabledModsFile)) File.Create(UMFData.DisabledModsFile).Dispose();
                    File.WriteAllLines(UMFData.DisabledModsFile, File.ReadAllLines(UMFData.DisabledModsFile).Where(x => x != GadgetMods.GetModInfo(modIndex).UMFName).ToArray());
                    enableUMFButton.GetComponentInChildren<Text>().text = "Disable Mod";
                }
            }
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
                Array.ForEach(SceneInjector.modMenuBackButtonHolder.GetComponentsInChildren<TextMesh>(), x => x.text = RestartNeeded ? "QUIT" : "BACK");
                restartRequiredText.SetActive(RestartNeeded);
                UpdateInfo(toggle, modIndex);
            }
        }

        private bool IsRestartNeeded()
        {
            if (!File.Exists(UMFData.DisabledModsFile)) File.Create(UMFData.DisabledModsFile).Dispose();
            string[] disabledMods = File.ReadAllLines(UMFData.DisabledModsFile).Where(x => !string.IsNullOrEmpty(x)).ToArray();
            for (int i = 0; i < GadgetMods.CountMods(); i++)
            {
                if (wasEnabled.ContainsKey(i))
                {
                    if (wasEnabled[i] != GadgetMods.GetModInfo(i).Mod.Enabled) return true;
                }
                if (wasUMFEnabled.ContainsKey(i))
                {
                    if (wasUMFEnabled[i] != !disabledMods.Contains(GadgetMods.GetModInfo(i).UMFName)) return true;
                }
                if (originalConfig.ContainsKey(i))
                {
                    if (!originalConfig[i].Equals(File.ReadAllText(UMFData.ConfigsPath + "/" + GadgetMods.GetModInfo(modIndex).UMFName + ".ini"))) return true;
                }
            }
            for (int i = 0; i < GadgetCore.nonGadgetMods.Count; i++)
            {
                if (wasUMFEnabled.ContainsKey(GadgetMods.CountMods() + i))
                {
                    if (wasUMFEnabled[GadgetMods.CountMods() + i] != !disabledMods.Contains(GadgetCore.nonGadgetMods[i])) return true;
                }
                if (originalConfig.ContainsKey(GadgetMods.CountMods() + i))
                {
                    if (!originalConfig[GadgetMods.CountMods() + i].Equals(File.ReadAllText(UMFData.ConfigsPath + "/" + GadgetCore.nonGadgetMods[i] + ".ini"))) return true;
                }
            }
            for (int i = 0; i < GadgetCore.disabledMods.Count; i++)
            {
                if (wasUMFEnabled.ContainsKey(GadgetMods.CountMods() + GadgetCore.nonGadgetMods.Count + i))
                {
                    if (wasUMFEnabled[GadgetMods.CountMods() + GadgetCore.nonGadgetMods.Count + i] != !disabledMods.Contains(GadgetCore.disabledMods[i])) return true;
                }
                if (originalConfig.ContainsKey(GadgetMods.CountMods() + GadgetCore.nonGadgetMods.Count + i))
                {
                    if (!originalConfig[GadgetMods.CountMods() + GadgetCore.nonGadgetMods.Count + i].Equals(File.ReadAllText(UMFData.ConfigsPath + "/" + GadgetCore.disabledMods[i] + ".ini"))) return true;
                }
            }
            for (int i = 0; i < GadgetCore.incompatibleMods.Count; i++)
            {
                if (wasUMFEnabled.ContainsKey(GadgetMods.CountMods() + GadgetCore.nonGadgetMods.Count + GadgetCore.disabledMods.Count + i))
                {
                    if (wasUMFEnabled[GadgetMods.CountMods() + GadgetCore.nonGadgetMods.Count + GadgetCore.disabledMods.Count + i] != !disabledMods.Contains(GadgetCore.incompatibleMods[i])) return true;
                }
                if (originalConfig.ContainsKey(GadgetMods.CountMods() + GadgetCore.nonGadgetMods.Count + GadgetCore.disabledMods.Count + i))
                {
                    if (!originalConfig[GadgetMods.CountMods() + GadgetCore.nonGadgetMods.Count + GadgetCore.disabledMods.Count + i].Equals(File.ReadAllText(UMFData.ConfigsPath + "/" + GadgetCore.incompatibleMods[i] + ".ini"))) return true;
                }
            }
            return false;
        }
    }
}
