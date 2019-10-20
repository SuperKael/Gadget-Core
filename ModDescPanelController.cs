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

        internal static List<Process> configHandles { get; private set; } = new List<Process>();

        private Toggle toggle;
        private int modIndex;
        internal Text descText;
        internal Button configButton;
        internal Button enableUMFButton;
        internal Button enableButton;

        internal static bool RestartNeeded { get; private set; } = false;

        internal void Start()
        {
            string[] disabledMods = File.ReadAllLines(UMFData.DisabledModsFile).Where(x => !string.IsNullOrEmpty(x)).ToArray();
            for (int i = 0;i < GadgetMods.CountMods();i++)
            {
                wasEnabled[i] = GadgetMods.GetModInfo(i).Enabled;
            }
            for (int i = 0; i < GadgetCore.nonGadgetMods.Count; i++)
            {
                wasEnabled[GadgetMods.CountMods() + i] = !disabledMods.Contains(GadgetCore.nonGadgetMods[i]);
            }
            for (int i = 0; i < GadgetCore.disabledMods.Count; i++)
            {
                wasEnabled[GadgetMods.CountMods() + GadgetCore.nonGadgetMods.Count + i] = !disabledMods.Contains(GadgetCore.nonGadgetMods[i]);
            }
            StartCoroutine(WatchForRestartNeeded());
        }

        public void UpdateInfo(Toggle toggle, int modIndex)
        {
            if (!File.Exists(UMFData.DisabledModsFile)) File.Create(UMFData.DisabledModsFile).Dispose();
            this.toggle = toggle;
            this.modIndex = modIndex;
            if (modIndex < GadgetMods.CountMods())
            {
                enableUMFButton.interactable = true;
                enableButton.interactable = true;
                GadgetModInfo mod = GadgetMods.ListAllModInfos()[modIndex];
                enableButton.GetComponentInChildren<Text>().text = mod.Enabled ? "Disable" : "Enable";
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
                    mod.Attribute.Name + " v" + UMFMod.GetModVersion(mod.UMFName).ToString() + (mod.Enabled ? " (Enabled)" : " (Disabled)") + Environment.NewLine +
                    "UMF Mod: " + mod.UMFName + Environment.NewLine +
                    (isModInfo ? "UMF Mod Info: " : "Description: ") + UMFMod.GetModDescription(mod.UMFName) + Environment.NewLine + Environment.NewLine + desc;
            }
            else
            {
                if (modIndex - GadgetMods.CountMods() < GadgetCore.nonGadgetMods.Count && GadgetCore.nonGadgetMods[modIndex - GadgetMods.CountMods()].Equals("GadgetCore"))
                {
                    enableUMFButton.interactable = false;
                }
                else
                {
                    enableUMFButton.interactable = true;
                }
                enableButton.interactable = false;
                bool enabled = !File.ReadAllLines(UMFData.DisabledModsFile).Any(x => x.Equals(modIndex - GadgetMods.CountMods() < GadgetCore.nonGadgetMods.Count ? GadgetCore.nonGadgetMods[modIndex - GadgetMods.CountMods()] : GadgetCore.disabledMods[modIndex - GadgetMods.CountMods() - GadgetCore.nonGadgetMods.Count]));
                enableButton.GetComponentInChildren<Text>().text = enabled ? "Disable" : "Enable";
                enableUMFButton.GetComponentInChildren<Text>().text = enabled ? "Disable Mod" : "Enable Mod";
                string mod = modIndex - GadgetMods.CountMods() < GadgetCore.nonGadgetMods.Count ? GadgetCore.nonGadgetMods[modIndex - GadgetMods.CountMods()] : GadgetCore.disabledMods[modIndex - GadgetMods.CountMods() - GadgetCore.nonGadgetMods.Count];
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
            else
            {
                filePath = UMFData.ConfigsPath + "/" + GadgetCore.disabledMods[modIndex - GadgetMods.CountMods() - GadgetCore.nonGadgetMods.Count] + ".ini";
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
                if (fileLines.Any(x => x.Equals(GadgetCore.nonGadgetMods[modIndex - GadgetMods.CountMods()])))
                {
                    File.WriteAllLines(UMFData.DisabledModsFile, fileLines.Where(x => !x.Equals(GadgetMods.GetModInfo(modIndex).UMFName)).ToArray());
                    enableButton.GetComponentInChildren<Text>().text = "Disable";
                    enableUMFButton.GetComponentInChildren<Text>().text = "Disable Mod";
                    toggle.GetComponentInChildren<Text>().color = new Color(1f, 1f, 1f, 1f);
                }
                else
                {
                    File.WriteAllText(UMFData.DisabledModsFile, fileText + Environment.NewLine + GadgetMods.GetModInfo(modIndex).UMFName);
                    enableButton.GetComponentInChildren<Text>().text = "Enable";
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
            else
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
            UpdateRestartNeeded();
        }

        internal void EnableButton()
        {
            if (modIndex < GadgetMods.CountMods())
            {
                bool enabled = !GadgetMods.GetModInfo(modIndex).Enabled;
                GadgetMods.SetEnabled(modIndex, enabled);
                enableButton.GetComponentInChildren<Text>().text = enabled ? "Disable" : "Enable";
                toggle.GetComponentInChildren<Text>().color = enabled ? new Color(1f, 1f, 1f, 1f) : new Color(1f, 1f, 1f, 0.5f);
                if (enabled)
                {
                    if (!File.Exists(UMFData.DisabledModsFile)) File.Create(UMFData.DisabledModsFile).Dispose();
                    File.WriteAllText(UMFData.DisabledModsFile, File.ReadAllText(UMFData.DisabledModsFile).Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Where(x => x != GadgetCore.nonGadgetMods[modIndex - GadgetMods.CountMods() - GadgetCore.nonGadgetMods.Count]).Aggregate((x, y) => x + Environment.NewLine + y));
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
                UpdateInfo(toggle, modIndex);
            }
        }

        private bool IsRestartNeeded()
        {
            if (!File.Exists(UMFData.DisabledModsFile)) File.Create(UMFData.DisabledModsFile).Dispose();
            for (int i = 0; i < GadgetMods.CountMods(); i++)
            {
                if (wasEnabled.ContainsKey(i))
                {
                    if (wasEnabled[i] != GadgetMods.GetModInfo(i).Enabled) return true;
                }
                if (originalConfig.ContainsKey(i))
                {
                    if (!originalConfig[i].Equals(File.ReadAllText(UMFData.ConfigsPath + "/" + GadgetMods.GetModInfo(modIndex).UMFName + ".ini"))) return true;
                }
            }
            for (int i = 0; i < GadgetCore.nonGadgetMods.Count; i++)
            {
                if (wasEnabled.ContainsKey(GadgetMods.CountMods() + i))
                {
                    if (wasEnabled[GadgetMods.CountMods() + i] ? File.ReadAllLines(UMFData.DisabledModsFile).Any(x => x.Equals(GadgetCore.nonGadgetMods[i])) : !File.ReadAllLines(UMFData.DisabledModsFile).Any(x => x.Equals(GadgetCore.nonGadgetMods[i]))) return true;
                }
                if (originalConfig.ContainsKey(GadgetMods.CountMods() + i))
                {
                    if (!originalConfig[GadgetMods.CountMods() + i].Equals(File.ReadAllText(UMFData.ConfigsPath + "/" + GadgetCore.nonGadgetMods[i] + ".ini"))) return true;
                }
            }
            for (int i = 0; i < GadgetCore.disabledMods.Count; i++)
            {
                if (wasEnabled.ContainsKey(GadgetMods.CountMods() + GadgetCore.nonGadgetMods.Count + i))
                {
                    if (wasEnabled[GadgetMods.CountMods() + GadgetCore.nonGadgetMods.Count + i] ? File.ReadAllLines(UMFData.DisabledModsFile).Any(x => x.Equals(GadgetCore.disabledMods[i])) : !File.ReadAllLines(UMFData.DisabledModsFile).Any(x => x.Equals(GadgetCore.disabledMods[i]))) return true;
                }
                if (originalConfig.ContainsKey(GadgetMods.CountMods() + GadgetCore.nonGadgetMods.Count + i))
                {
                    if (!originalConfig[GadgetMods.CountMods() + GadgetCore.nonGadgetMods.Count + i].Equals(File.ReadAllText(UMFData.ConfigsPath + "/" + GadgetCore.disabledMods[i] + ".ini"))) return true;
                }
            }
            return false;
        }
    }
}
