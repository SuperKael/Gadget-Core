using IniParser;
using IniParser.Model;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace GadgetCore.API.ConfigMenu
{
    /// <summary>
    /// This is an extension of <see cref="BasicGadgetConfigMenu"/> that auto-generates its contents from an INI-based config file. This will always be used for non-Gadgets, and is also the default if you don't specify your own config menu.
    /// </summary>
    public class INIGadgetConfigMenu : BasicGadgetConfigMenu
    {
        /// <summary>
        /// Regex for inserting spaces into PascalCase strings.
        /// </summary>
        public const string PASCAL_CASE_SPACING_REGEX = @"(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])";

        /// <summary>
        /// The exception that occurs if the given config file has no configurable data.
        /// </summary>
        public const string NO_CONFIGURABLE_DATA = "This mod's config file has no configurable data!";

        /// <summary>
        /// The path to the config file that this menu is for.
        /// </summary>
        protected string ConfigFilePath;
        /// <summary>
        /// The Ini section of the config file that this config menu is for.
        /// </summary>
        protected string ConfigFileSection;
        /// <summary>
        /// All config entries that match names in this list are displayed as read-only.
        /// </summary>
        protected string[] ReadonlyEntries;
        /// <summary>
        /// The <see cref="FileIniDataParser"/> used for accessing the config file.
        /// </summary>
        protected FileIniDataParser IniParser;
        /// <summary>
        /// The <see cref="IniData"/> for the config file.
        /// </summary>
        protected IniData Ini;
        /// <summary>
        /// If not null, auto-reloads the configs for the given <see cref="ModMenuEntry"/>.
        /// </summary>
        protected ModMenuEntry autoReload;

        /// <summary>
        /// Constructs a UMFGadgetConfigMenu, optionally for a given ini file section and given config file path. Throws an <see cref="InvalidOperationException"/> with the message <see cref="NO_CONFIGURABLE_DATA"/> if the given config file has no configurable data.
        /// </summary>
        /// <param name="section">The Ini file section to reference. If the specified section is not present, then the section matching the config file's name will be used instead.</param>
        /// <param name="hidesModMenu">Specifies whether the mod menu should be hidden when this config menu is opened.</param>
        /// <param name="configFilePath">Specifies the location of the config file to reference. If left unspecified, the standard config file for your mod will be used.</param>
        /// <param name="autoReload">If not null, auto-reloads the configs for the given <see cref="ModMenuEntry"/>.</param>
        /// <param name="readonlyEntries">All config entries that match names in this list will be displayed as read-only.</param>
        public INIGadgetConfigMenu(string section, bool hidesModMenu = false, string configFilePath = null, ModMenuEntry autoReload = null, params string[] readonlyEntries) : base(hidesModMenu)
        {
            if (configFilePath == null) configFilePath = Path.Combine(GadgetPaths.ConfigsPath, Assembly.GetCallingAssembly().GetName().Name) + ".ini";
            this.autoReload = autoReload;
            LoadConfigFile(configFilePath, section, readonlyEntries);
        }

        /// <summary>
        /// Loads the given config file, and adds all of the entries within to this <see cref="INIGadgetConfigMenu"/>
        /// </summary>
        protected virtual void LoadConfigFile(string configFilePath, string section, params string[] readonlyEntries)
        {
            ConfigFilePath = configFilePath;
            ConfigFileSection = section;
            ReadonlyEntries = readonlyEntries;
            if (!File.Exists(configFilePath))
            {
                GadgetCore.CoreLogger.LogConsole(configFilePath);
                AddComponent(new GadgetConfigLabelComponent(this, "No Config File", "This mod had no config file when this menu was generated."));
                AddComponent(new GadgetConfigLabelComponent(this, "No Config File", "Perhaps it was just installed?"));
                AddComponent(new GadgetConfigLabelComponent(this, "No Config File", "Click the button to reload this config menu."));
                AddComponent(new GadgetConfigSpacerComponent(this, "Spacer"));
                AddComponent(new GadgetConfigButtonComponent(this, null, "Refresh Config File", () =>
                {
                    Reset();
                }, 0.25f));
                return;
            }
            IniParser = new FileIniDataParser();
            Ini = IniParser.ReadFile(configFilePath);
            Ini.Configuration.SkipInvalidLines = true;
            Ini.Configuration.AssigmentSpacer = "";
            if (section == null || !Ini.Sections.ContainsSection(section))
            {
                section = Path.GetFileNameWithoutExtension(configFilePath);
                if (!Ini.Sections.ContainsSection(section))
                {
                    throw new InvalidOperationException(NO_CONFIGURABLE_DATA);
                }
            }
            ConfigFileSection = section;
            if (Ini.Sections.ContainsSection("uModFramework") && Ini["uModFramework"].ContainsKey("ConfigVersion")) AddComponent(new GadgetConfigLabelComponent(this, "ConfigVersion", "Config Version (not to be confused with mod version): " + Ini["uModFramework"]["ConfigVersion"], allowHeightResize: true), GadgetConfigComponentAlignment.HEADER);
            bool firstStandard = true, firstHeader = false, firstFooter = true;
            foreach (KeyData keyData in Ini[section])
            {
                if (keyData == null) continue;
                if (keyData.KeyName == "ConfigVersion")
                {
                    AddComponent(new GadgetConfigLabelComponent(this, "ConfigVersion", "Config Version (not to be confused with mod version): " + keyData.Value, allowHeightResize: true), GadgetConfigComponentAlignment.HEADER);
                    continue;
                }
                GadgetConfigComponentAlignment alignment = default;
                bool seperatorMade = false, commentsMade = false;
                try
                {
                    string dataTypeString = keyData.Comments.SingleOrDefault((x) => x.StartsWith("[Type:"));
                    string[] dataTypeStrings;
                    string dataRangeString = null;
                    if (!string.IsNullOrEmpty(dataTypeString))
                    {
                        dataTypeStrings = dataTypeString.Split(new char[] { '|' }, 2);
                        dataTypeString = dataTypeStrings[0];
                        if (dataTypeStrings.Length == 2 && dataTypeStrings[1].StartsWith(" Range: "))
                        {
                            dataRangeString = dataTypeStrings[1].Substring(8, dataTypeStrings[1].Length - 9);
                        }
                        dataTypeString = dataTypeString.Substring(7, dataTypeString.Length - 8);
                    }
                    else
                    {
                        dataTypeString = "None";
                    }
                    string defaultValueString = keyData.Comments.SingleOrDefault((x) => x.StartsWith("[Default(s):"));
                    string vanillaValueString = keyData.Comments.SingleOrDefault((x) => x.StartsWith("[Vanilla:"));
                    if (!string.IsNullOrEmpty(defaultValueString) && defaultValueString != "[Default(s): n/a]")
                    {
                        string[] defaultValueStrings = defaultValueString.Split('|');
                        defaultValueString = defaultValueStrings[0];
                        if (defaultValueStrings.Length > 1)
                        {
                            vanillaValueString = defaultValueStrings[1].Substring(10, defaultValueStrings[1].Length - 11);
                        }
                        defaultValueString = defaultValueString.Substring(13, defaultValueString.Length - 14);
                    }
                    else if (!string.IsNullOrEmpty(vanillaValueString) && vanillaValueString != "[Vanilla: n/a]")
                    {
                        vanillaValueString = vanillaValueString.Substring(10, vanillaValueString.Length - 11);
                    }
                    else
                    {
                        defaultValueString = null;
                    }
                    string allowedValuesString = keyData.Comments.SingleOrDefault((x) => x.StartsWith("[Allowed:"));
                    if (!string.IsNullOrEmpty(allowedValuesString))
                    {
                        allowedValuesString = allowedValuesString.Substring(10, allowedValuesString.Length - 11);
                    }
                    else
                    {
                        allowedValuesString = null;
                    }
                    string requiresRestartString = keyData.Comments.SingleOrDefault((x) => x.StartsWith("[Restart:"));
                    if (!string.IsNullOrEmpty(requiresRestartString))
                    {
                        requiresRestartString = requiresRestartString.Substring(10, requiresRestartString.Length - 11);
                    }
                    else
                    {
                        requiresRestartString = null;
                    }
                    bool requiresRestart = requiresRestartString != null && bool.Parse(requiresRestartString);
                    GadgetConfigComponentAlignment? nullableAlignment = AlignConfigEntry(keyData.KeyName);
                    if (nullableAlignment == null) continue;
                    alignment = (GadgetConfigComponentAlignment)nullableAlignment;
                    if ((alignment == GadgetConfigComponentAlignment.STANDARD && firstStandard == true) || (alignment == GadgetConfigComponentAlignment.HEADER && firstHeader == true) || (alignment == GadgetConfigComponentAlignment.FOOTER && firstFooter == true))
                    {
                        switch (alignment)
                        {
                            case GadgetConfigComponentAlignment.STANDARD:
                                firstStandard = false;
                                break;
                            case GadgetConfigComponentAlignment.HEADER:
                                firstHeader = false;
                                break;
                            case GadgetConfigComponentAlignment.FOOTER:
                                firstFooter = false;
                                break;
                        }
                    }
                    else
                    {
                        AddComponent(new GadgetConfigSeparatorComponent(this, "Seperator"), alignment);
                    }
                    seperatorMade = true;
                    foreach (string comment in keyData.Comments)
                    {
                        if (!string.IsNullOrEmpty(comment) && comment[0] != '[')
                        {
                            AddComponent(new GadgetConfigLabelComponent(this, "Comment For: " + keyData.KeyName, comment, 0.05f, true), alignment);
                        }
                    }
                    commentsMade = true;
                    dataTypeStrings = dataTypeString.Split(new char[] { '-' }, 2);
                    switch (dataTypeStrings[0])
                    {
                        case "Boolean":
                            bool boolValue = bool.Parse(keyData.Value);
                            bool? boolDefaultValue = defaultValueString != null ? (bool?)bool.Parse(defaultValueString) : null;
                            bool? boolVanillaValue = vanillaValueString != null ? (bool?)bool.Parse(vanillaValueString) : null;
                            AddComponent(new GadgetConfigBoolComponent(this, keyData.KeyName, boolValue, (b) => SetConfigValue(section, keyData.KeyName, b.ToString()), readonlyEntries.Contains(keyData.KeyName), boolDefaultValue, boolVanillaValue), alignment);
                            break;
                        case "Int32":
                            int intValue = int.Parse(keyData.Value);
                            int[] intRange = dataRangeString?.Split(',').Select(x => int.Parse(x)).ToArray();
                            if (intRange != null && intRange.Length != 2)
                            {
                                throw new InvalidDataException("The range '" + dataRangeString + "' is not valid!");
                            }
                            int? intDefaultValue = defaultValueString != null ? (int?)int.Parse(defaultValueString) : null;
                            int? intVanillaValue = vanillaValueString != null ? (int?)int.Parse(vanillaValueString) : null;
                            AddComponent(new GadgetConfigIntComponent(this, keyData.KeyName, intValue, (s) => SetConfigValue(section, keyData.KeyName, s.ToString()), intRange != null ? intRange[0] : 0, intRange != null ? intRange[1] : 0, readonlyEntries.Contains(keyData.KeyName), intDefaultValue, intVanillaValue), alignment);
                            break;
                        case "Single":
                            float floatValue = float.Parse(keyData.Value);
                            float[] floatRange = dataRangeString?.Split(',').Select(x => float.Parse(x)).ToArray();
                            if (floatRange != null && floatRange.Length != 2 && floatRange.Length != 3)
                            {
                                throw new InvalidDataException("The range '" + dataRangeString + "' is not valid!");
                            }
                            float? floatDefaultValue = defaultValueString != null ? (float?)float.Parse(defaultValueString) : null;
                            float? floatVanillaValue = vanillaValueString != null ? (float?)float.Parse(vanillaValueString) : null;
                            AddComponent(new GadgetConfigFloatComponent(this, keyData.KeyName, floatValue, (s) => SetConfigValue(section, keyData.KeyName, s.ToString()), floatRange != null ? floatRange[0] : 0, floatRange != null ? floatRange[1] : 0, floatRange != null && floatRange.Length == 3 ? (int)floatRange[2] : -1, readonlyEntries.Contains(keyData.KeyName), floatDefaultValue, floatVanillaValue), alignment);
                            break;
                        case "Double":
                            double doubleValue = double.Parse(keyData.Value);
                            double[] doubleRange = dataRangeString?.Split(',').Select(x => double.Parse(x)).ToArray();
                            if (doubleRange != null && doubleRange.Length != 2 && doubleRange.Length != 3)
                            {
                                throw new InvalidDataException("The range '" + dataRangeString + "' is not valid!");
                            }
                            double? doubleDefaultValue = defaultValueString != null ? (double?)double.Parse(defaultValueString) : null;
                            double? doubleVanillaValue = vanillaValueString != null ? (double?)double.Parse(vanillaValueString) : null;
                            AddComponent(new GadgetConfigDoubleComponent(this, keyData.KeyName, doubleValue, (s) => SetConfigValue(section, keyData.KeyName, s.ToString()), doubleRange != null ? doubleRange[0] : 0, doubleRange != null ? doubleRange[1] : 0, doubleRange != null && doubleRange.Length == 3 ? (int)doubleRange[2] : -1, readonlyEntries.Contains(keyData.KeyName), doubleDefaultValue, doubleVanillaValue), alignment);
                            break;
                        case "String":
                            string stringValue = keyData.Value;
                            string stringDefaultValue = defaultValueString;
                            string stringVanillaValue = vanillaValueString;
                            if (keyData.Comments.Contains("[IsKeyBind: True]"))
                            {
                                AddComponent(new GadgetConfigKeybindComponent(this, keyData.KeyName, stringValue, (s) => SetConfigValue(section, keyData.KeyName, s), true, readonlyEntries.Contains(keyData.KeyName), stringDefaultValue, stringVanillaValue), alignment);
                            }
                            else
                            {
                                AddComponent(new GadgetConfigStringComponent(this, keyData.KeyName, stringValue, (s) => SetConfigValue(section, keyData.KeyName, s), readonlyEntries.Contains(keyData.KeyName), stringDefaultValue, stringVanillaValue), alignment);
                            }
                            break;
                        case "String[]":
                            string[] stringArrayValue = keyData.Value.Split(',');
                            string[] stringArrayDefaultValue = defaultValueString?.Split(',');
                            string[] stringArrayVanillaValue = vanillaValueString?.Split(',');
                            if (keyData.Comments.Contains("[IsKeyBind: True]"))
                            {
                                AddComponent(new GadgetConfigMultiKeybindComponent(this, keyData.KeyName, stringArrayValue, (s) => SetConfigValue(section, keyData.KeyName, s?.Aggregate(new StringBuilder(), (x, y) => { if (x.Length > 0) x.Append(','); x.Append(y); return x; })?.ToString()), true, readonlyEntries.Contains(keyData.KeyName), stringArrayDefaultValue, stringArrayVanillaValue), alignment);
                            }
                            else
                            {
                                AddComponent(new GadgetConfigMultiStringComponent(this, keyData.KeyName, stringArrayValue, (s) => SetConfigValue(section, keyData.KeyName, s?.Aggregate(new StringBuilder(), (x, y) => { if (x.Length > 0) x.Append(','); x.Append(y); return x; })?.ToString()), readonlyEntries.Contains(keyData.KeyName), stringArrayDefaultValue, stringArrayVanillaValue), alignment);
                            }
                            break;
                        case "KeyCode":
                            string keyCodeValue = keyData.Value;
                            string keyCodeDefaultValue = defaultValueString;
                            string keyCodeVanillaValue = vanillaValueString;
                            AddComponent(new GadgetConfigKeybindComponent(this, keyData.KeyName, keyCodeValue, (s) => SetConfigValue(section, keyData.KeyName, s), false, readonlyEntries.Contains(keyData.KeyName), keyCodeDefaultValue, keyCodeVanillaValue), alignment);
                            break;
                        case "Enum":
                            Type enumType;
                            try
                            {
                                if (dataTypeStrings.Length == 2 && (enumType = Type.GetType(dataTypeStrings[1], false)) != null)
                                {
                                    string enumValue = keyData.Value;
                                    string enumDefaultValue = defaultValueString;
                                    string enumVanillaValue = vanillaValueString;
                                    AddComponent(new GadgetConfigDropdownComponent(this, keyData.KeyName, Regex.Replace(enumValue, PASCAL_CASE_SPACING_REGEX, " $1"), Enum.GetNames(enumType).Select(x => Regex.Replace(x, PASCAL_CASE_SPACING_REGEX, " $1")).ToArray(), (s) => SetConfigValue(section, keyData.KeyName, s.Replace(" ", "")), readonlyEntries.Contains(keyData.KeyName), enumDefaultValue, enumVanillaValue), alignment);
                                    break;
                                }
                            }
                            catch (Exception) { }
                            goto default;
                        default:
                            string unknownValue = keyData.Value;
                            string unknownDefaultValue = defaultValueString;
                            string unknownVanillaValue = vanillaValueString;
                            AddComponent(new GadgetConfigStringComponent(this, keyData.KeyName, unknownValue, (s) => SetConfigValue(section, keyData.KeyName, s), readonlyEntries.Contains(keyData.KeyName), unknownDefaultValue, unknownVanillaValue), alignment);
                            break;
                    }
                }
                catch (Exception e)
                {
                    GadgetCore.CoreLogger.Log("WARNING: Exception parsing config entry '" + keyData.KeyName + "' with value '" + keyData.Value + "': " + e);
                    try
                    {
                        if (!seperatorMade)
                        {
                            GadgetConfigComponentAlignment? nullableAlignment = AlignConfigEntry(keyData.KeyName);
                            if (nullableAlignment == null) continue;
                            alignment = (GadgetConfigComponentAlignment)nullableAlignment;
                            if ((alignment == GadgetConfigComponentAlignment.STANDARD && firstStandard == true) || (alignment == GadgetConfigComponentAlignment.HEADER && firstHeader == true) || (alignment == GadgetConfigComponentAlignment.FOOTER && firstFooter == true))
                            {
                                switch (alignment)
                                {
                                    case GadgetConfigComponentAlignment.STANDARD:
                                        firstStandard = false;
                                        break;
                                    case GadgetConfigComponentAlignment.HEADER:
                                        firstHeader = false;
                                        break;
                                    case GadgetConfigComponentAlignment.FOOTER:
                                        firstFooter = false;
                                        break;
                                }
                            }
                            else
                            {
                                AddComponent(new GadgetConfigSeparatorComponent(this, "Seperator"), alignment);
                            }
                            seperatorMade = true;
                        }
                        if (!commentsMade)
                        {
                            foreach (string comment in keyData.Comments)
                            {
                                if (!string.IsNullOrEmpty(comment) && comment[0] != '[')
                                {
                                    AddComponent(new GadgetConfigLabelComponent(this, "Comment For: " + keyData.KeyName, comment, 0.05f, true), alignment);
                                }
                            }
                            commentsMade = true;
                        }
                        AddComponent(new GadgetConfigSpacerComponent(this, "Parsing Error Spacer for: " + keyData.KeyName));
                        AddComponent(new GadgetConfigLabelComponent(this, "Parsing Error Label for: " + keyData.KeyName, "An error occured parsing the following config entry: " + keyData.KeyName, 0.05f, true), alignment);
                        AddComponent(new GadgetConfigLabelComponent(this, "Parsing Error Label for: " + keyData.KeyName, "As such, a raw-text editor is being provided as a fallback.", 0.05f, true), alignment);
                        AddComponent(new GadgetConfigLabelComponent(this, "Parsing Error Label for: " + keyData.KeyName, "Additionally, the entry's meta-comments are being displayed.", 0.05f, true), alignment);
                        AddComponent(new GadgetConfigSpacerComponent(this, "Parsing Error Spacer for: " + keyData.KeyName));
                        foreach (string comment in keyData.Comments)
                        {
                            if (!string.IsNullOrEmpty(comment) && comment[0] == '[')
                            {
                                AddComponent(new GadgetConfigLabelComponent(this, "Meta-Comment For: " + keyData.KeyName, comment, 0.05f, true), alignment);
                            }
                        }
                        AddComponent(new GadgetConfigStringComponent(this, keyData.KeyName, keyData.Value, (s) => SetConfigValue(section, keyData.KeyName, s), readonlyEntries.Contains(keyData.KeyName)), alignment);
                    }
                    catch (Exception ee)
                    {
                        GadgetCore.CoreLogger.Log("WARNING: Failed fallback parsing of config entry '" + keyData.KeyName + "' with value '" + keyData.Value + "': " + ee);
                        try
                        {
                            if (!seperatorMade)
                            {
                                try
                                {
                                    GadgetConfigComponentAlignment? nullableAlignment = AlignConfigEntry(keyData.KeyName);
                                    if (nullableAlignment == null) continue;
                                    alignment = (GadgetConfigComponentAlignment)nullableAlignment;
                                    if ((alignment == GadgetConfigComponentAlignment.STANDARD && firstStandard == true) || (alignment == GadgetConfigComponentAlignment.HEADER && firstHeader == true) || (alignment == GadgetConfigComponentAlignment.FOOTER && firstFooter == true))
                                    {
                                        switch (alignment)
                                        {
                                            case GadgetConfigComponentAlignment.STANDARD:
                                                firstStandard = false;
                                                break;
                                            case GadgetConfigComponentAlignment.HEADER:
                                                firstHeader = false;
                                                break;
                                            case GadgetConfigComponentAlignment.FOOTER:
                                                firstFooter = false;
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        AddComponent(new GadgetConfigSeparatorComponent(this, "Seperator"), alignment);
                                    }
                                    seperatorMade = true;
                                }
                                catch (Exception se)
                                {
                                    GadgetCore.CoreLogger.Log("WARNING: Failed creating seperator for failed config entry '" + keyData.KeyName + "': " + se);
                                }
                            }
                            AddComponent(new GadgetConfigLabelComponent(this, "Fatal Error Name Label for: " + keyData.KeyName, "A FATAL ERROR OCCURED PROCESSING THE FOLLOWING CONFIG ENTRY: " + keyData.KeyName, 0.05f, true), alignment);
                            AddComponent(new GadgetConfigLabelComponent(this, "Fatal Error Value Label for: " + keyData.KeyName, "THE VALUE OF THE CONFIG ENTRY IS: " + keyData.Value, 0.05f, true), alignment);
                        }
                        catch (Exception fe)
                        {
                            GadgetCore.CoreLogger.Log("WARNING: Failed creating label for failed config entry '" + keyData.KeyName + "': " + fe);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Called to make the config menu totally reset itself.
        /// </summary>
        public override void Reset()
        {
            Clear();
            LoadConfigFile(ConfigFilePath, ConfigFileSection, ReadonlyEntries);
            Rebuild();
        }

        /// <summary>
        /// Used for validation and alignment of config entries being read from the config file. Return an alignment to align, or return null to prevent the entry from appearing in the menu.
        /// </summary>
        /// <param name="name">The name of the config entry</param>
        protected virtual GadgetConfigComponentAlignment? AlignConfigEntry(string name)
        {
            return GadgetConfigComponentAlignment.STANDARD;
        }

        private void SetConfigValue(string section, string entryName, string value)
        {
            Ini[section][entryName] = value;
            Update();
        }

        /// <summary>
        /// Called whenever the config menu's contents are updated in some way. This will be called whenever a default component's value is changed.
        /// </summary>
        public override void Update()
        {
            base.Update();
            IniParser.WriteFile(ConfigFilePath, Ini);
            if (ConfigFileSection == "GadgetCore")
            {
                GadgetCoreConfig.EarlyLoad();
                GadgetCoreConfig.Load();
                GadgetCore.CoreLogger.Log("Finished reloading config.");
            }
            if (autoReload != null)
            {
                if (autoReload.Name == ConfigFileSection)
                {
                    GadgetCoreAPI.GetUMFAPI()?.SendCommand("cfgReload " + Path.GetFileNameWithoutExtension(ConfigFilePath));
                }
                autoReload.Gadgets.FirstOrDefault(x => x.Attribute.Name == ConfigFileSection && x.Attribute.AllowConfigReloading)?.Gadget.ReloadConfig();
            }
        }
    }
}
