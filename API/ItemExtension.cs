using GadgetCore.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace GadgetCore.API
{
    /// <summary>
    /// Extends the Item class to add additional data to it.
    /// </summary>
    public static class ItemExtension
    {
        private static Dictionary<Item, Dictionary<string, object>> extraItemData = new Dictionary<Item, Dictionary<string, object>>();
        private static Dictionary<Type, Tuple<Func<object, string>, Func<string, object>>> typeSerializers = new Dictionary<Type, Tuple<Func<object, string>, Func<string, object>>>();
        
        /// <summary>
        /// Registers a serializer/deserializer pair to use when handling a given Type.
        /// </summary>
        public static void RegisterTypeSerializer(Type type, Func<object, string> serializer, Func<string, object> deserializer)
        {
            typeSerializers[type] = Tuple.Create(serializer, deserializer);
        }

        /// <summary>
        /// Returns true of the Item has any extra data at all.
        /// </summary>
        public static bool HasAnyExtraData(this Item item)
        {
            return extraItemData.ContainsKey(item);
        }

        /// <summary>
        /// Returns true of the Item has extra data with the specified key.
        /// </summary>
        public static bool HasExtraData(this Item item, string dataKey)
        {
            return extraItemData.ContainsKey(item) && extraItemData[item].ContainsKey(dataKey);
        }

        /// <summary>
        /// Adds or replaces a piece of extra data on this Item. The data key should be of the format GadgetName:Key, and the data value must be a serializable type or a type serializer must have been registered for it. If adding an ID as extra data, <see cref="PutExtraData(Item, string, int, Registry)"/> should be used instead.
        /// </summary>
        public static void PutExtraData<T>(this Item item, string dataKey, T dataValue)
        {
            if (dataKey.IndexOf(':') == -1) throw new ArgumentException("dataKey must be of the format GadgetName:Key");
            Type type = typeof(T);
            if (!typeSerializers.ContainsKey(type) && !type.IsSerializable && !typeof(ISerializable).IsAssignableFrom(type)) throw new ArgumentException("dataValue must be serializable!");
            if (!extraItemData.ContainsKey(item)) extraItemData.Add(item, new Dictionary<string, object>());
            extraItemData[item][dataKey] = dataValue;
        }

        /// <summary>
        /// Adds or replaces a piece of extra data on this Item. The data key should be of the format GadgetName:Key. If the provided Registry is not null, then it will be used for automatic conversion of IDs.
        /// </summary>
        public static void PutExtraData(this Item item, string dataKey, int dataValue, Registry reg)
        {
            if (dataKey.IndexOf(':') == -1) throw new ArgumentException("dataKey must be of the format GadgetName:Key");
            if (!extraItemData.ContainsKey(item)) extraItemData.Add(item, new Dictionary<string, object>());
            extraItemData[item][dataKey] = reg != null ? reg.ConvertIDToHost(dataValue) : dataValue;
        }

        /// <summary>
        /// Gets a piece of extra data from this Item. If retrieving an ID as extra data, <see cref="GetExtraData(Item, string, Registry)"/> should be used instead.
        /// </summary>
        public static T GetExtraData<T>(this Item item, string dataKey)
        {
            if (extraItemData.ContainsKey(item) && extraItemData[item].TryGetValue(dataKey, out object value))
            {
                return (T) value;
            }
            else
            {
                return default;
            }
        }

        /// <summary>
        /// Gets a piece of extra data from this Item. If the provided Registry is not null, then it will be used for automatic conversion of IDs.
        /// </summary>
        public static int GetExtraData(this Item item, string dataKey, Registry reg)
        {
            if (extraItemData.ContainsKey(item) && extraItemData[item].TryGetValue(dataKey, out object value))
            {
                return reg != null ? reg.ConvertIDToLocal((int)value) : (int)value;
            }
            else
            {
                return default;
            }
        }

        /// <summary>
        /// Returns all extra data stored on this Item. Returns null if the item has no extra data.
        /// </summary>
        public static Dictionary<string, object> GetAllExtraData(this Item item)
        {
            if (extraItemData.TryGetValue(item, out Dictionary<string, object> value))
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Removes a piece of extra data from this Item.
        /// </summary>
        public static bool RemoveExtraData(this Item item, string dataKey)
        {
            if (extraItemData.ContainsKey(item))
            {
                Dictionary<string, object> existingDic = extraItemData[item];
                bool success = existingDic.Remove(dataKey);
                if (existingDic.Count == 0)
                {
                    extraItemData.Remove(item);
                }
                return success;
            }
            return false;
        }

        /// <summary>
        /// Removes any and all extra data from this Item.
        /// </summary>
        public static bool RemoveAllExtraData(this Item item)
        {
            return extraItemData.Remove(item);
        }

        /// <summary>
        /// Sets the extra data dictionary for this item.
        /// </summary>
        public static void SetAllExtraData(this Item item, Dictionary<string, object> dic)
        {
            if (dic == null || dic.Count == 0)
            {
                extraItemData.Remove(item);
                return;
            }
            if (!extraItemData.ContainsKey(item)) extraItemData.Add(item, new Dictionary<string, object>());
            Dictionary<string, object> existingDic = extraItemData[item];
            existingDic.Clear();
            foreach (KeyValuePair<string, object> entry in dic)
            {
                if (!typeSerializers.ContainsKey(entry.Value.GetType()) && !entry.Value.GetType().IsSerializable && !typeof(ISerializable).IsAssignableFrom(entry.Value.GetType()))
                {
                    existingDic.Clear();
                    throw new ArgumentException("Dictionary contains non-serializable values!");
                }
                existingDic[entry.Key] = entry.Value;
            }
        }

        /// <summary>
        /// Serializes all of the extra data on an Item, for transmission across the Network, or saving into PlayerPrefs.
        /// </summary>
        public static string SerializeExtraData(this Item item)
        {
            BinaryFormatter br = new BinaryFormatter();
            return GetAllExtraData(item)?.Select(x => {
                string serializedString;
                if (x.Value == null)
                {
                    serializedString = "Null";
                }
                else if (typeSerializers.TryGetValue(x.Value.GetType(), out Tuple<Func<object, string>, Func<string, object>> serializerPair))
                {
                    serializedString = x.Value.GetType().FullName + "\",\" " + x.Value.GetType().Assembly.GetName().Name + ":" + serializerPair.Item1(x.Value);
                }
                else
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        br.Serialize(ms, x.Value);
                        serializedString = "Raw:" + Convert.ToBase64String(ms.ToArray());
                    }
                }
                return x.Key + "=" + serializedString.Replace("\"", "\"\"");
            }).Aggregate(new StringBuilder(), (x, y) => { if (x.Length > 0) x.Append(','); x.Append(y); return x; }).ToString() ?? string.Empty;
        }

        /// <summary>
        /// Deserializes the given extra data, and associates it with the Item. Replaces any extra data the Item may already have.
        /// </summary>
        public static void DeserializeExtraData(this Item item, string serializedData)
        {
            if (string.IsNullOrEmpty(serializedData))
            {
                RemoveAllExtraData(item);
                return;
            }
            Dictionary<string, object> extraData = new Dictionary<string, object>();
            StringBuilder builder = new StringBuilder();
            string key = null, type = null;
            bool inQuotes = false;
            IFormatter br = new BinaryFormatter();
            for (int i = 0;i < serializedData.Length;i++)
            {
                if (serializedData[i] == '"')
                {
                    inQuotes = !inQuotes;
                    if (i < 1 || serializedData[i - 1] != '"')
                    {
                        continue;
                    }
                }
                if (!inQuotes)
                {
                    if (key == null && serializedData[i] == '=')
                    {
                        key = builder.ToString();
                        builder = new StringBuilder();
                        continue;
                    }
                    else if (key != null && type == null && serializedData[i] == ':')
                    {
                        type = builder.ToString();
                        builder = new StringBuilder();
                        continue;
                    }
                    else if (serializedData[i] == ',' || i == serializedData.Length - 1)
                    {
                        if (serializedData[i] != ',') builder.Append(serializedData[i]);
                        if (key != null)
                        {
                            string value = builder.ToString();
                            if (type == null)
                            {
                                if (value == "Null")
                                {
                                    extraData[key] = null;
                                }
                            }
                            else if (type == "Raw")
                            {
                                try
                                {
                                    using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(value)))
                                    {
                                        extraData[key] = br.Deserialize(ms);
                                    }
                                }
                                catch (Exception e)
                                {
                                    GadgetCore.CoreLogger.LogWarning($"Deserialization error for Item '{GadgetCoreAPI.GetItemName(item.id)}'! Check GadgetCore.log for more details.");
                                    GadgetCore.CoreLogger.LogWarning($"Error deserializing data: '{key}={type}:{value}'. Exception: {e}", false);
                                }
                            }
                            else
                            {
                                Type t = Type.GetType(type);
                                if (t != null && typeSerializers.TryGetValue(t, out Tuple<Func<object, string>, Func<string, object>> serializerPair))
                                {
                                    extraData[key] = serializerPair.Item2(value);
                                }
                                else
                                {
                                    try
                                    {
                                        using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(value)))
                                        {
                                            extraData[key] = br.Deserialize(ms);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        GadgetCore.CoreLogger.LogWarning($"Deserialization error for Item '{GadgetCoreAPI.GetItemName(item.id)}'! Check GadgetCore.log for more details.");
                                        GadgetCore.CoreLogger.LogWarning($"Error deserializing data: '{key}={type}:{value}'. Exception: {e}", false);
                                    }
                                }
                            }
                        }
                        key = null;
                        type = null;
                        builder = new StringBuilder();
                        continue;
                    }
                }
                builder.Append(serializedData[i]);
            }
            SetAllExtraData(item, extraData);
        }
    }
}
