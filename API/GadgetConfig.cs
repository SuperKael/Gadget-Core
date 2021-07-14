using GadgetCore.Util;
using IniParser;
using IniParser.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GadgetCore.API
{
    /// <summary>
    /// Class for managing your mod's config.
    /// </summary>
    public class GadgetConfig
    {
        private static Dictionary<string, IniData> configFiles = new Dictionary<string, IniData>();

        private static FileIniDataParser configParser = new FileIniDataParser();

        /// <summary>
        /// The KeyDataCollection for the section of the config file that this GadgetConfig uses.
        /// </summary>
        protected KeyDataCollection ConfigData
        {
            get
            {
                return configFiles[configFile][configSection];
            }
        }

        /// <summary>
        /// The path to the config file that this GadgetConfig uses.
        /// </summary>
        public readonly string configFile;
        /// <summary>
        /// The section of the config file that this GadgetConfig uses.
        /// </summary>
        public readonly string configSection;

        /// <summary>
        /// Instantiates a new GadgetConfig for manipulating the given config file and section.
        /// </summary>
        internal GadgetConfig(string configFile, string configSection = null)
        {
            this.configFile = configFile;
            this.configSection = configSection ?? Path.GetFileNameWithoutExtension(configFile);
            if (!File.Exists(configFile)) File.Create(configFile).Dispose();
            Load();
        }

        /// <summary>
        /// Loads the current contents of the config file into this GadgetConfig instance. Destroys any changes to the data made before a call to <see cref="Save"/>.
        /// </summary>
        public void Load()
        {
            configFiles[configFile] = configParser.ReadFile(configFile, Encoding.UTF8);
            if (ConfigData == null) configFiles[configFile].Sections.AddSection(configSection);
        }
        /// <summary>
        /// Saves the current config data as represented in this GadgetConfig instance to the config file. Destroys any changes to the file made before a call to <see cref="Load"/>.
        /// </summary>
        public void Save()
        {
            configParser.WriteFile(configFile, configFiles[configFile], Encoding.UTF8);
        }

        /// <summary>
        /// Removes all values from the config section.
        /// </summary>
        public void Reset()
        {
            configFiles[configFile].Sections.RemoveSection(configSection);
        }

        /// <summary>
        /// Reads an <see cref="int"/> value from the config.
        /// </summary>
        public int ReadInt(string key, int defaultValue, int? vanillaValue = default, bool requiresRestart = false, int? minValue = default, int? maxValue = default, params string[] comments)
        {
            List<string> commentList = new List<string>();
            commentList.AddRange(comments);
            GenerateMetaComments(commentList, defaultValue, vanillaValue, requiresRestart, range: new int?[] { minValue, maxValue });
            if (!ConfigData.ContainsKey(key)) ConfigData[key] = defaultValue.ToString();
            ConfigData.GetKeyData(key).Comments = commentList;
            return int.Parse(ConfigData[key]);
        }

        /// <summary>
        /// Reads a <see cref="long"/> value from the config.
        /// </summary>
        public long ReadLong(string key, long defaultValue, long? vanillaValue = default, bool requiresRestart = false, long? minValue = default, long? maxValue = default, params string[] comments)
        {
            List<string> commentList = new List<string>();
            commentList.AddRange(comments);
            GenerateMetaComments(commentList, defaultValue, vanillaValue, requiresRestart, range: new long?[] { minValue, maxValue });
            if (!ConfigData.ContainsKey(key)) ConfigData[key] = defaultValue.ToString();
            ConfigData.GetKeyData(key).Comments = commentList;
            return long.Parse(ConfigData[key]);
        }

        /// <summary>
        /// Reads a <see cref="float"/> value from the config.
        /// </summary>
        public float ReadFloat(string key, float defaultValue, float? vanillaValue = default, bool requiresRestart = false, float? minValue = default, float? maxValue = default, int? decimals = default, params string[] comments)
        {
            List<string> commentList = new List<string>();
            commentList.AddRange(comments);
            GenerateMetaComments(commentList, defaultValue, vanillaValue, requiresRestart, range: new float?[] { minValue, maxValue, decimals });
            if (!ConfigData.ContainsKey(key)) ConfigData[key] = defaultValue.ToString();
            ConfigData.GetKeyData(key).Comments = commentList;
            return float.Parse(ConfigData[key]);
        }

        /// <summary>
        /// Reads a <see cref="double"/> value from the config.
        /// </summary>
        public double ReadDouble(string key, double defaultValue, double? vanillaValue = default, bool requiresRestart = false, double? minValue = default, double? maxValue = default, int? decimals = default, params string[] comments)
        {
            List<string> commentList = new List<string>();
            commentList.AddRange(comments);
            GenerateMetaComments(commentList, defaultValue, vanillaValue, requiresRestart, range: new double?[] { minValue, maxValue, decimals });
            if (!ConfigData.ContainsKey(key)) ConfigData[key] = defaultValue.ToString();
            ConfigData.GetKeyData(key).Comments = commentList;
            return double.Parse(ConfigData[key]);
        }

        /// <summary>
        /// Reads a <see cref="bool"/> value from the config.
        /// </summary>
        public bool ReadBool(string key, bool defaultValue, bool? vanillaValue = default, bool requiresRestart = false, params string[] comments)
        {
            List<string> commentList = new List<string>();
            commentList.AddRange(comments);
            GenerateMetaComments(commentList, defaultValue, vanillaValue);
            if (!ConfigData.ContainsKey(key)) ConfigData[key] = defaultValue.ToString();
            ConfigData.GetKeyData(key).Comments = commentList;
            return bool.Parse(ConfigData[key]);
        }

        /// <summary>
        /// Reads a <see cref="string"/> value from the config.
        /// </summary>
        public string ReadString(string key, string defaultValue, string vanillaValue = default, bool requiresRestart = false, string[] allowed = null, params string[] comments)
        {
            List<string> commentList = new List<string>();
            commentList.AddRange(comments);
            GenerateMetaComments(commentList, defaultValue, vanillaValue, requiresRestart, allowed);
            if (!ConfigData.ContainsKey(key)) ConfigData[key] = defaultValue.ToString();
            ConfigData.GetKeyData(key).Comments = commentList;
            return ConfigData[key];
        }

        /// <summary>
        /// Reads a <see cref="string"/> array value from the config.
        /// </summary>
        public string[] ReadStringArray(string key, string[] defaultValue, string[] vanillaValue = default, bool requiresRestart = false, string[][] allowed = null, params string[] comments)
        {
            List<string> commentList = new List<string>();
            commentList.AddRange(comments);
            GenerateMetaComments(commentList, defaultValue, vanillaValue, requiresRestart, allowed);
            if (!ConfigData.ContainsKey(key)) ConfigData[key] = defaultValue.ToString();
            ConfigData.GetKeyData(key).Comments = commentList;
            return ConfigData[key].Split(',');
        }

        /// <summary>
        /// Reads a <see cref="KeyCode"/> value from the config.
        /// </summary>
        public KeyCode ReadKeyCode(string key, KeyCode defaultValue, KeyCode vanillaValue = default, bool requiresRestart = false, KeyCode[] allowed = null, params string[] comments)
        {
            List<string> commentList = new List<string>();
            commentList.AddRange(comments);
            GenerateMetaComments(commentList, defaultValue, vanillaValue, requiresRestart, allowed);
            if (!ConfigData.ContainsKey(key)) ConfigData[key] = defaultValue.ToString();
            ConfigData.GetKeyData(key).Comments = commentList;
            return (KeyCode)Enum.Parse(typeof(KeyCode), ConfigData[key]);
        }

        /// <summary>
        /// Reads an <see cref="Enum"/> value from the config.
        /// </summary>
        public T ReadEnum<T>(string key, T defaultValue, T vanillaValue = default, bool requiresRestart = false, T[] allowed = null, params string[] comments) where T : Enum
        {
            List<string> commentList = new List<string>();
            commentList.AddRange(comments);
            GenerateMetaComments(commentList, defaultValue, vanillaValue, requiresRestart, allowed);
            if (!ConfigData.ContainsKey(key)) ConfigData[key] = defaultValue.ToString();
            ConfigData.GetKeyData(key).Comments = commentList;
            return (T)Enum.Parse(typeof(T), ConfigData[key]);
        }

        /// <summary>
        /// Reads a generic value from the config.
        /// </summary>
        public T Read<T>(string key, T defaultValue = default, T vanillaValue = default, bool requiresRestart = false, T[] allowed = null, T[] range = default, params string[] comments) where T : IConvertible
        {
            List<string> commentList = new List<string>();
            commentList.AddRange(comments);
            GenerateMetaComments(commentList, defaultValue, vanillaValue, requiresRestart, allowed, range);
            if (!ConfigData.ContainsKey(key)) ConfigData[key] = defaultValue.ToString();
            ConfigData.GetKeyData(key).Comments = commentList;
            return (T)Convert.ChangeType(ConfigData[key], typeof(T));
        }

        /// <summary>
        /// Reads a generic value from the config using a specified type converter.
        /// </summary>
        public T Read<T>(string key, Func<string, T> typeConverter, T defaultValue = default, T vanillaValue = default, bool requiresRestart = false, T[] allowed = null, T[] range = default, params string[] comments)
        {
            List<string> commentList = new List<string>();
            commentList.AddRange(comments);
            GenerateMetaComments(commentList, defaultValue, vanillaValue, requiresRestart, allowed, range);
            if (!ConfigData.ContainsKey(key)) ConfigData[key] = defaultValue.ToString();
            ConfigData.GetKeyData(key).Comments = commentList;
            return typeConverter(ConfigData[key]);
        }

        /// <summary>
        /// Writes an <see cref="int"/> value to the config.
        /// </summary>
        public void WriteInt(string key, int value, int? defaultValue = default, int? vanillaValue = default, bool requiresRestart = false, int? minValue = default, int? maxValue = default, params string[] comments)
        {
            List<string> commentList = new List<string>();
            commentList.AddRange(comments);
            GenerateMetaComments(commentList, defaultValue, vanillaValue, requiresRestart, range: new object[] { minValue, maxValue });
            ConfigData[key] = value.ToString();
            ConfigData.GetKeyData(key).Comments = commentList;
        }

        /// <summary>
        /// Writes a <see cref="long"/> value to the config.
        /// </summary>
        public void WriteLong(string key, long value, long? defaultValue = default, long? vanillaValue = default, bool requiresRestart = false, long? minValue = default, long? maxValue = default, params string[] comments)
        {
            List<string> commentList = new List<string>();
            commentList.AddRange(comments);
            GenerateMetaComments(commentList, defaultValue, vanillaValue, requiresRestart, range: new object[] { minValue, maxValue });
            ConfigData[key] = value.ToString();
            ConfigData.GetKeyData(key).Comments = commentList;
        }

        /// <summary>
        /// Writes a <see cref="float"/> value to the config.
        /// </summary>
        public void WriteFloat(string key, float value, float? defaultValue = default, float? vanillaValue = default, bool requiresRestart = false, float? minValue = default, float? maxValue = default, int? decimals = default, params string[] comments)
        {
            List<string> commentList = new List<string>();
            commentList.AddRange(comments);
            GenerateMetaComments(commentList, defaultValue, vanillaValue, requiresRestart, range: new object[] { minValue, maxValue, decimals });
            ConfigData[key] = value.ToString();
            ConfigData.GetKeyData(key).Comments = commentList;
        }

        /// <summary>
        /// Writes a <see cref="double"/> value to the config.
        /// </summary>
        public void WriteDouble(string key, double value, double? defaultValue = default, double? vanillaValue = default, bool requiresRestart = false, double? minValue = default, double? maxValue = default, int? decimals = default, params string[] comments)
        {
            List<string> commentList = new List<string>();
            commentList.AddRange(comments);
            GenerateMetaComments(commentList, defaultValue, vanillaValue, requiresRestart, range: new object[] { minValue, maxValue, decimals });
            ConfigData[key] = value.ToString();
            ConfigData.GetKeyData(key).Comments = commentList;
        }

        /// <summary>
        /// Writes a <see cref="bool"/> value to the config.
        /// </summary>
        public void WriteBool(string key, bool value, bool? defaultValue = default, bool? vanillaValue = default, bool requiresRestart = false, params string[] comments)
        {
            List<string> commentList = new List<string>();
            commentList.AddRange(comments);
            GenerateMetaComments(commentList, defaultValue, vanillaValue);
            ConfigData[key] = value.ToString();
            ConfigData.GetKeyData(key).Comments = commentList;
        }

        /// <summary>
        /// Writes a <see cref="string"/> value to the config.
        /// </summary>
        public void WriteString(string key, string value, string defaultValue = default, string vanillaValue = default, bool requiresRestart = false, string[] allowed = null, params string[] comments)
        {
            List<string> commentList = new List<string>();
            commentList.AddRange(comments);
            GenerateMetaComments(commentList, defaultValue, vanillaValue, requiresRestart, allowed);
            ConfigData[key] = value.ToString();
            ConfigData.GetKeyData(key).Comments = commentList;
        }

        /// <summary>
        /// Writes a <see cref="string"/> array value to the config.
        /// </summary>
        public void WriteStringArray(string key, string[] value, string[] defaultValue = default, string[] vanillaValue = default, bool requiresRestart = false, string[][] allowed = null, params string[] comments)
        {
            List<string> commentList = new List<string>();
            commentList.AddRange(comments);
            GenerateMetaComments(commentList, defaultValue, vanillaValue, requiresRestart, allowed);
            ConfigData[key] = value.Concat(",");
            ConfigData.GetKeyData(key).Comments = commentList;
        }

        /// <summary>
        /// Writes a <see cref="KeyCode"/> value to the config.
        /// </summary>
        public void WriteKeyCode(string key, KeyCode value, KeyCode defaultValue = default, KeyCode vanillaValue = default, bool requiresRestart = false, KeyCode[] allowed = null, params string[] comments)
        {
            List<string> commentList = new List<string>();
            commentList.AddRange(comments);
            GenerateMetaComments(commentList, defaultValue, vanillaValue, requiresRestart, allowed);
            ConfigData[key] = value.ToString();
            ConfigData.GetKeyData(key).Comments = commentList;
        }

        /// <summary>
        /// Writes an <see cref="Enum"/> value to the config.
        /// </summary>
        public void WriteEnum<T>(string key, T value, T defaultValue = default, T vanillaValue = default, bool requiresRestart = false, T[] allowed = null, params string[] comments) where T : Enum
        {
            List<string> commentList = new List<string>();
            commentList.AddRange(comments);
            GenerateMetaComments<T>(commentList, defaultValue, vanillaValue, requiresRestart, allowed);
            ConfigData[key] = value.ToString();
            ConfigData.GetKeyData(key).Comments = commentList;
        }

        /// <summary>
        /// Writes a generic value to the config.
        /// </summary>
        public void Write<T>(string key, T value, T defaultValue = default, T vanillaValue = default, bool requiresRestart = false, T[] allowed = null, T[] range = default, params string[] comments) where T : IConvertible
        {
            List<string> commentList = new List<string>();
            commentList.AddRange(comments);
            GenerateMetaComments<T>(commentList, defaultValue, vanillaValue, requiresRestart, allowed, range);
            ConfigData[key] = value.ToString();
            ConfigData.GetKeyData(key).Comments = commentList;
        }

        private void GenerateMetaComments<T>(List<string> commentList, T defaultValue = default, T vanillaValue = default, bool requiresRestart = false, T[] allowed = null, params T[] range)
        {
            if (!EqualityComparer<T>.Default.Equals(defaultValue, default) && defaultValue != null)
            {
                if (!EqualityComparer<T>.Default.Equals(vanillaValue, default) && vanillaValue != null)
                {
                    commentList.Add("[Default(s): " + defaultValue + " | Vanilla: " + vanillaValue + "]");
                }
                else
                {
                    commentList.Add("[Default(s): " + defaultValue + "]");
                }
            }
            else if (!EqualityComparer<T>.Default.Equals(vanillaValue, default) && vanillaValue != null)
            {
                commentList.Add("[Vanilla: " + vanillaValue + "]");
            }
            Type underlyingT = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            if (range != null && range.Length > 0 && range.Any(x => x != null))
            {
                commentList.Add("[Type: " + (underlyingT.IsEnum && !typeof(KeyCode).IsAssignableFrom(underlyingT) ? "Enum-" + underlyingT.AssemblyQualifiedName : underlyingT.Name) + " | Range: " + range.Where(x => x != null).Select(x => x.ToString()).Concat() + "]");
            }
            else
            {
                commentList.Add("[Type: " + (underlyingT.IsEnum && !typeof(KeyCode).IsAssignableFrom(underlyingT) ? "Enum-" + underlyingT.AssemblyQualifiedName : underlyingT.Name) + "]");
            }
            if (allowed != null && allowed.Length > 0 && allowed.Any(x => x != null))
            {
                commentList.Add("[Allowed: " + allowed.Where(x => x != null).Select(x => x.ToString()).Concat() + "]");
            }
            if (allowed != null && allowed.Length > 0)
            {
                commentList.Add("[Restart: " + requiresRestart + "]");
            }
        }
    }
}
