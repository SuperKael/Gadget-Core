﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GadgetCore.API
{
    /// <summary>
    /// Provides methods related to network communication
    /// </summary>
    public static class GadgetNetwork
    {
        /// <summary>
        /// How long the Gadget Network will wait for the Network ID Conversion Matrix to be created. If it has not been created after this timeout, the Gadget Network will assume you are connected to a vanilla player.
        /// </summary>
        public static float MatrixTimeout { get; internal set; } = 2.5f;
        /// <summary>
        /// True if the Network ID Conversion Matrix is ready for use. False otherwise. ID conversion methods will fail if the matrix is not ready.
        /// </summary>
        public static bool MatrixReady { get; private set; }
        /// <summary>
        /// True if the timeout period for the Network ID Conversion Matrix has passed, but the Matrix is not ready for use. Will always be false when MatrixReady is true.
        /// </summary>
        public static bool MatrixTimedOut => !MatrixReady && connectTime >= 0 && GetTimeSinceConnect() > MatrixTimeout;
        /// <summary>
        /// True if the Network ID Conversion Matrix is ready for use, OR the timeout period for the Matrix has passed.
        /// </summary>
        public static bool MatrixReadyOrTimedOut => MatrixReady || connectTime >= 0 && GetTimeSinceConnect() > MatrixTimeout;

        /// <summary>
        /// True if you are currently connected as a client, and the host does not have Gadget Core installed. This can be a false positive, in the case of a slow connection.
        /// </summary>
        public static bool IsHostVanilla { get; private set; }
        private static readonly Dictionary<string, Dictionary<int, int>> IDConversionMatrixToHost = new Dictionary<string, Dictionary<int, int>>();
        private static readonly Dictionary<string, Dictionary<int, int>> IDConversionMatrixToLocal = new Dictionary<string, Dictionary<int, int>>();
        internal static float connectTime = -1;

        internal static readonly Dictionary<string, NetworkPlayer> NetworkPlayersByName = new Dictionary<string, NetworkPlayer>(StringComparer.OrdinalIgnoreCase);
        internal static readonly Dictionary<NetworkPlayer, string> NamesByNetworkPlayer = new Dictionary<NetworkPlayer, string>();

        private static readonly Dictionary<string, SyncVar> SyncVars = new Dictionary<string, SyncVar>();
        private static readonly Dictionary<string, LocalSyncVar> LocalSyncVars = new Dictionary<string, LocalSyncVar>();

        /// <summary>
        /// The name of the server host's player
        /// </summary>
        public static string ServerPlayerName { get; internal set; }

        /// <summary>
        /// Gets the amount of time, in seconds, that has passed since the Network connection was established. Returns -1 if there is no currently active Network connection.
        /// </summary>
        public static float GetTimeSinceConnect()
        {
            return connectTime >= 0 ? Time.realtimeSinceStartup - connectTime : -1;
        }

        /// <summary>
        /// Checks if the network host has the specified registry.
        /// </summary>
        public static bool HostHasReg(Registry reg)
        {
            if (!MatrixReady) throw new InvalidOperationException("Network ID conversion cannot be performed when the ID conversion matrix is not ready!");
            return IDConversionMatrixToHost.ContainsKey(reg.GetRegistryName());
        }

        /// <summary>
        /// Gets the ID of this RegistryEntry on the multiplayer host, saves the trouble of calling <see cref="ConvertIDToHost(Registry, int)"/>
        /// </summary>
        public static int GetHostID<E, T>(this RegistryEntry<E, T> entry) where E : RegistryEntry<E, T> where T : Enum
        {
            return ConvertIDToHost(entry.GetRegistry(), entry.ID);
        }

        /// <summary>
        /// Gets the host IDs that match the local IDs in the given array. Produces values of -1 if the host does not have matching IDs for given IDs in the array.
        /// Does not effect the given array, and returns a new array of the same length as the given array.
        /// </summary>
        public static int[] ConvertIDsToHost(this Registry reg, ref int[] IDs)
        {
            return IDs = ConvertIDsToHost(reg, IDs);
        }

        /// <summary>
        /// Gets the host IDs that match the local IDs in the given array. Produces values of -1 if the host does not have matching IDs for given IDs in the array.
        /// Does not effect the given array, and returns a new array of the same length as the given array.
        /// </summary>
        public static int[] ConvertIDsToHost(this Registry reg, int[] IDs)
        {
            return IDs.Select(id => ConvertIDToHost(reg, id)).ToArray();
        }

        /// <summary>
        /// Gets the host ID that matches the specified local ID. The given ID MUST be an int. Returns -1 if the host does not have a matching ID for the specified local ID.
        /// </summary>
        public static object ConvertIDToHost(this Registry reg, ref object ID)
        {
            if (!(ID is int id)) throw new ArgumentException("ID must be an int!", nameof(ID));
            return ID = ConvertIDToHost(reg, id);
        }

        /// <summary>
        /// Gets the host ID that matches the specified local ID. Returns -1 if the host does not have a matching ID for the specified local ID.
        /// </summary>
        public static int ConvertIDToHost(this Registry reg, ref int ID)
        {
            return ID = ConvertIDToHost(reg, ID);
        }

        /// <summary>
        /// Gets the host ID that matches the specified local ID. Returns -1 if the host does not have a matching ID for the specified local ID.
        /// </summary>
        public static int ConvertIDToHost(this Registry reg, int ID)
        {
            if (Network.isServer) return ID;
            if (!MatrixReady) throw new InvalidOperationException("Network ID conversion cannot be performed when the ID conversion matrix is not ready!");
            if (reg == null) return ID;
            if (IDConversionMatrixToHost.ContainsKey(reg.GetRegistryName()) && IDConversionMatrixToHost[reg.GetRegistryName()].TryGetValue(ID, out int newID))
            {
                return newID;
            }
            if (ID >= reg.GetIDStart()) GadgetCore.CoreLogger.LogWarning($"ID Conversion to host failed: ID {ID} does not exist on the host. This has resulted in the corruption of {("aeiouAEIOU".Contains(reg.GetRegistryName()[0]) ? "an " : "a ") + reg.GetRegistryName()}!");
            return ID < reg.GetIDStart() ? ID : -1;
        }

        /// <summary>
        /// Gets the local IDs that match the host IDs in the given array. Produces values of -1 if there are no matching local IDs for given IDs in the array.
        /// Does not effect the given array, and returns a new array of the same length as the given array.
        /// </summary>
        public static int[] ConvertIDsToLocal(this Registry reg, ref int[] IDs)
        {
            return IDs = ConvertIDsToLocal(reg, IDs);
        }

        /// <summary>
        /// Gets the local IDs that match the host IDs in the given array. Produces values of -1 if there are no matching local IDs for given IDs in the array.
        /// Does not effect the given array, and returns a new array of the same length as the given array.
        /// </summary>
        public static int[] ConvertIDsToLocal(this Registry reg, int[] IDs)
        {
            return IDs.Select(id => ConvertIDToLocal(reg, id)).ToArray();
        }

        /// <summary>
        /// Gets the local ID that matches the specified host ID. The given ID must be an int. Returns -1 if there is no matching local ID for the specified host ID.
        /// </summary>
        public static object ConvertIDToLocal(this Registry reg, ref object ID)
        {
            if (!(ID is int id)) throw new ArgumentException("ID must be an int!", nameof(ID));
            return ID = ConvertIDToLocal(reg, id);
        }

        /// <summary>
        /// Gets the local ID that matches the specified host ID. Returns -1 if there is no matching local ID for the specified host ID.
        /// </summary>
        public static int ConvertIDToLocal(this Registry reg, ref int ID)
        {
            return ID = ConvertIDToLocal(reg, ID);
        }

        /// <summary>
        /// Gets the local ID that matches the specified host ID. Returns -1 if there is no matching local ID for the specified host ID.
        /// </summary>
        public static int ConvertIDToLocal(this Registry reg, int ID)
        {
            if (Network.isServer) return ID;
            if (!MatrixReady) throw new InvalidOperationException("Network ID conversion cannot be performed when the ID conversion matrix is not ready!");
            if (reg == null) return ID;
            if (IDConversionMatrixToLocal.ContainsKey(reg.GetRegistryName()) && IDConversionMatrixToLocal[reg.GetRegistryName()].TryGetValue(ID, out int newID))
            {
                return newID;
            }
            if (ID >= reg.GetIDStart()) GadgetCore.CoreLogger.LogWarning($"ID Conversion to local failed: ID {ID} does not exist on this client. This has resulted in the corruption of {("aeiouAEIOU".Contains(reg.GetRegistryName()[0]) ? "an " : "a ") + reg.GetRegistryName()}!");
            return ID < reg.GetIDStart() ? ID : -1;
        }

        internal static string GenerateIDMatrixData()
        {
            return GameRegistry.ListAllRegistries().Select(x => x.GetRegistryName() + ":" + x.reservedIDs.Select(w => w.Key + "=" + w.Value).Aggregate(string.Empty, (y, z) => y + "," + z)).Aggregate(string.Empty, (y, z) => y + "|" + z);
        }

        internal static void ResetIDMatrix()
        {
            connectTime = -1;
            IsHostVanilla = false;
            MatrixReady = false;
        }

        internal static void InitializeVanillaIDMatrix()
        {
            IsHostVanilla = true;
            MatrixReady = false;
            foreach (Registry reg in GameRegistry.ListAllRegistries())
            {
                IDConversionMatrixToHost[reg.GetRegistryName()] = new Dictionary<int, int>();
                IDConversionMatrixToLocal[reg.GetRegistryName()] = new Dictionary<int, int>();
            }
            MatrixReady = true;
            OnMatrixReady?.Invoke(false);
        }

        internal static void ParseIDMatrixData(string packedData)
        {
            IsHostVanilla = false;
            MatrixReady = false;
            try
            {
                foreach (string dicEntry in packedData.Split('|'))
                {
                    string[] splitDic = dicEntry.Split(new[] { ':' }, 2);
                    if (GameRegistry.IsRegistryRegistered(splitDic[0]))
                    {
                        Registry reg = GameRegistry.GetRegistry(splitDic[0]);
                        IDConversionMatrixToHost[reg.GetRegistryName()] = new Dictionary<int, int>();
                        IDConversionMatrixToLocal[reg.GetRegistryName()] = new Dictionary<int, int>();
                        foreach (string regEntry in splitDic[1].Split(','))
                        {
                            string[] splitReg = regEntry.Split('=');
                            if (reg.reservedIDs.ContainsKey(splitReg[0]))
                            {
                                int hostID = int.Parse(splitReg[1]);
                                IDConversionMatrixToHost[reg.GetRegistryName()][reg.reservedIDs[splitReg[0]]] = hostID;
                                IDConversionMatrixToLocal[reg.GetRegistryName()][hostID] = reg.reservedIDs[splitReg[0]];
                            }
                        }
                    }
                }
                MatrixReady = true;
                OnMatrixReady?.Invoke(true);
            }
            catch (Exception e)
            {
                GadgetCore.CoreLogger.LogError("Received bad host ID conversion data: " + packedData);
                GadgetCore.CoreLogger.LogError("Exception that occured while parsing host ID conversion data:" + Environment.NewLine + e.ToString());
                Network.Disconnect();
            }
        }

        /// <summary>
        /// Disconnects the target player from this server with a provided message.
        /// </summary>
        public static void DisconnectPlayerWithMessage(NetworkPlayer player, string message)
        {
            if (Network.isServer)
            {
                RPCHooks.DisconnectWithMessage(player, message);
            }
            else
            {
                throw new InvalidOperationException("Only the server may disconnect other players!");
            }
        }

        /// <summary>
        /// Returns the player name of the given <see cref="NetworkPlayer"/>
        /// </summary>
        public static string GetNameByNetworkPlayer(NetworkPlayer player)
        {
            return NamesByNetworkPlayer.TryGetValue(player, out string name) ? name : player == Network.player ? GadgetCoreAPI.GetPlayerName() : null;
        }

        /// <summary>
        /// Returns the <see cref="NetworkPlayer"/> with the given player name
        /// </summary>
        public static NetworkPlayer? GetNetworkPlayerByName(string name)
        {
            return NetworkPlayersByName.TryGetValue(name, out NetworkPlayer player) ? (NetworkPlayer?)player : name == GadgetCoreAPI.GetPlayerName() ? (NetworkPlayer?)Network.player : null;
        }

        /// <summary>
        /// Registers a new SyncVar. The value of this SyncVar can be updated using <see cref="UpdateSyncVar"/>,
        /// and retrieved using <see cref="GetSyncVar"/>. This value will always be synced across all clients.
        /// Must be called in the Initialize method of a Gadget.
        /// </summary>
        public static void RegisterSyncVar<T>(string name, T initialValue)
        {
            if (!Registry.registeringVanilla && Registry.gadgetRegistering < 0) throw new InvalidOperationException("SyncVars may only be registered in the Initialize method of a Gadget!");
            SyncVars.Add(name, new SyncVar(initialValue, Registry.gadgetRegistering));
        }

        /// <summary>
        /// Registers a new local SyncVar. The value of this SyncVar can be updated using <see cref="UpdateLocalSyncVar"/>,
        /// and retrieved using <see cref="GetLocalSyncVar(string, string)"/>. This value will always be synced across all clients.
        /// Each client has their own value for this SyncVar, but any client can retrieve any other client's value.
        /// Must be called in the Initialize method of a Gadget.
        /// </summary>
        public static void RegisterLocalSyncVar<T>(string name, T initialValue)
        {
            if (!Registry.registeringVanilla && Registry.gadgetRegistering < 0) throw new InvalidOperationException("SyncVars may only be registered in the Initialize method of a Gadget!");
            LocalSyncVars.Add(name, new LocalSyncVar(initialValue, Registry.gadgetRegistering));
        }

        internal static void UnregisterSyncVars(int modID)
        {
            foreach (string syncVar in SyncVars.Where(x => x.Value.ModID == modID).Select(x => x.Key).ToList())
            {
                SyncVars.Remove(syncVar);
            }
            foreach (string syncVar in LocalSyncVars.Where(x => x.Value.ModID == modID).Select(x => x.Key).ToList())
            {
                LocalSyncVars.Remove(syncVar);
            }
        }

        /// <summary>
        /// Updates the value of a SyncVar. The new value will be synced across all clients. May only be used by the server.
        /// </summary>
        public static void UpdateSyncVar<T>(string name, T value)
        {
            if (!Network.isServer) throw new InvalidOperationException("Only the server may set the value of a non-local SyncVar!");
            RPCHooks.Singleton.UpdateSyncVar(name, value, false);
        }

        internal static bool UpdateSyncVarInternal<T>(string name, T value)
        {
            if (SyncVars.ContainsKey(name))
            {
                SyncVars[name].Value = value;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Updates the value of a local SyncVar. The new value will be synced across all clients.
        /// </summary>
        public static void UpdateLocalSyncVar<T>(string name, T value)
        {
            RPCHooks.Singleton.UpdateSyncVar(name, value, true);
        }

        internal static bool UpdateLocalSyncVarInternal<T>(string name, T value, string playerName)
        {
            if (LocalSyncVars.ContainsKey(name))
            {
                LocalSyncVars[name].Values[playerName] = value;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the value of a SyncVar.
        /// </summary>
        public static T GetSyncVar<T>(string name)
        {
            return (T)SyncVars[name].Value;
        }

        /// <summary>
        /// Gets the value of a local SyncVar, for a given player.
        /// </summary>
        public static T GetLocalSyncVar<T>(string name, NetworkPlayer player)
        {
            return GetLocalSyncVar<T>(name, NamesByNetworkPlayer[player]);
        }

        /// <summary>
        /// Gets the value of a local SyncVar, for a given player.
        /// </summary>
        public static T GetLocalSyncVar<T>(string name, string player)
        {
            return LocalSyncVars[name].Values.ContainsKey(player) ? (T)LocalSyncVars[name].Values[player] : (T)LocalSyncVars[name].DefaultValue;
        }

        /// <summary>
        /// Invoked once the ID Conversion Matrix is ready for use. Bool parameter is true if the host has GadgetCore installed.
        /// Warning: In cases of high latency, this event may be invoked twice. The first time with a false parameter suggesting the host is vanilla,
        /// the second time with a true parameter once the modded Matrix connection is initialized. As such, you most likely only want to utilize
        /// this event when the parameter is true.
        /// </summary>
        public static event Action<bool> OnMatrixReady;

        internal enum SyncVarType
        {
            INT,
            FLOAT,
            STRING,
            VECTOR3,
            INT_ARRAY,
            FLOAT_ARRAY,
            STRING_ARRAY,
            VECTOR3_ARRAY
        }

        internal class SyncVar
        {
            internal object Value;
            internal readonly int ModID;

            internal SyncVar(object value, int modID)
            {
                Value = value;
                ModID = modID;
            }
        }

        internal class LocalSyncVar
        {
            internal readonly Dictionary<string, object> Values;
            internal object DefaultValue;
            internal readonly int ModID;

            internal LocalSyncVar(object value, int modID)
            {
                Values = new Dictionary<string, object>();
                DefaultValue = value;
                ModID = modID;
            }
        }
    }
}
