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
        private static Dictionary<WeakReference<Item>, Dictionary<string, object>> extraItemData = new Dictionary<WeakReference<Item>, Dictionary<string, object>>();

        /// <summary>
        /// Returns true of the Item has any extra data at all.
        /// </summary>
        public static bool HasAnyExtraData(this Item item)
        {
            return extraItemData.ContainsKey(new WeakReference<Item>(item));
        }

        /// <summary>
        /// Returns true of the Item has extra data with the specified key.
        /// </summary>
        public static bool HasExtraData(this Item item, string dataKey)
        {
            if (dataKey.IndexOf(':') == -1) throw new ArgumentException("dataKey must be of the format ModName:Key");
            return extraItemData.ContainsKey(new WeakReference<Item>(item)) ? extraItemData[new WeakReference<Item>(item)].ContainsKey(dataKey) : false;
        }

        /// <summary>
        /// Adds or replaces a piece of extra data to this Item. The data key should be of the format ModName:Key, and the data value must be a serializable type. If adding an ID as extra data, make sure to use GadgetNetwork.ConvertIDToHost on it first.
        /// </summary>
        public static void PutExtraData<T>(this Item item, string dataKey, T dataValue)
        {
            if (dataKey.IndexOf(':') == -1) throw new ArgumentException("dataKey must be of the format ModName:Key");
            if (!typeof(T).IsSerializable && !typeof(ISerializable).IsAssignableFrom(typeof(T))) throw new ArgumentException("dataValue must be serializable!");
            if (!extraItemData.ContainsKey(new WeakReference<Item>(item))) extraItemData.Add(new WeakReference<Item>(item), new Dictionary<string, object>());
            extraItemData[new WeakReference<Item>(item)][dataKey] = dataValue;
        }

        /// <summary>
        /// Gets a piece of extra data from this Item. If retrieving an ID as extra data, make sure to use GadgetNetwork.ConvertIDToLocal on it first.
        /// </summary>
        public static T GetExtraData<T>(this Item item, string dataKey)
        {
            if (dataKey.IndexOf(':') == -1) throw new ArgumentException("dataKey must be of the format ModName:Key");
            if (extraItemData.ContainsKey(new WeakReference<Item>(item)) && extraItemData[new WeakReference<Item>(item)].TryGetValue(dataKey, out object value))
            {
                return (T) value;
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
            if (extraItemData.ContainsKey(new WeakReference<Item>(item)))
            {
                return extraItemData[new WeakReference<Item>(item)];
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
            if (dataKey.IndexOf(':') == -1) throw new ArgumentException("dataKey must be of the format ModName:Key");
            if (extraItemData.ContainsKey(new WeakReference<Item>(item)))
            {
                Dictionary<string, object> existingDic = extraItemData[new WeakReference<Item>(item)];
                bool success = existingDic.Remove(dataKey);
                if (existingDic.Count == 0)
                {
                    extraItemData.Remove(new WeakReference<Item>(item));
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
            return extraItemData.Remove(new WeakReference<Item>(item));
        }

        /// <summary>
        /// Sets the extra data dictionary for this item.
        /// </summary>
        public static void SetAllExtraData(this Item item, Dictionary<string, object> dic)
        {
            if (!extraItemData.ContainsKey(new WeakReference<Item>(item))) extraItemData.Add(new WeakReference<Item>(item), new Dictionary<string, object>());
            Dictionary<string, object> existingDic = extraItemData[new WeakReference<Item>(item)];
            existingDic.Clear();
            foreach (KeyValuePair<string, object> entry in dic)
            {
                if (!entry.Value.GetType().IsSerializable && !typeof(ISerializable).IsAssignableFrom(entry.Value.GetType()))
                {
                    existingDic.Clear();
                    throw new ArgumentException("Dictionary contains non-serializable values!");
                }
                existingDic[entry.Key] = entry.Value;
            }
        }

        /// <summary>
        /// Serializes all of the extra data on an Item, for transmission across the Network.
        /// </summary>
        public static string SerializeExtraData(this Item item)
        {
            BinaryFormatter br = new BinaryFormatter();
            return GetAllExtraData(item)?.Select(x => {
                using (MemoryStream ms = new MemoryStream())
                {
                    br.Serialize(ms, x.Value);
                    return x.Key + "=" + Convert.ToBase64String(ms.ToArray()).Replace("\"", "\"\"");
                }
            })?.Aggregate(string.Empty, (x, y) => x + "," + y) ?? "";
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
            string key = null;
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
                    else if (serializedData[i] == ',')
                    {
                        using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(builder.ToString())))
                        {
                            extraData[key] = br.Deserialize(ms);
                        }
                        key = null;
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
