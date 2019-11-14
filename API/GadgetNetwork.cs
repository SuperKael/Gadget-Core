using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        /// True if you are currently connected as a client, and the host does not have Gadget Core installed. This can be a false positive, in the case of a slow connection.
        /// </summary>
        public static bool IsHostVanilla { get; private set; }
        private static readonly Dictionary<string, Dictionary<int, int>> IDConversionMatrixToHost = new Dictionary<string, Dictionary<int, int>>();
        private static readonly Dictionary<string, Dictionary<int, int>> IDConversionMatrixToLocal = new Dictionary<string, Dictionary<int, int>>();
        internal static float connectTime = -1;

        /// <summary>
        /// Gets the amount of time, in seconds, that has passed since the Network connection was established. Returns -1 if there is no currently active Network connection.
        /// </summary>
        public static float GetTimeSinceConnect()
        {
            return connectTime > 0 ? Time.realtimeSinceStartup - connectTime : -1;
        }

        /// <summary>
        /// Checks if the network host has the specified registry.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the network ID conversion matrix is not ready.</exception>
        public static bool HostHasReg(Registry reg)
        {
            if (!MatrixReady)
            {
                Debug.Log("Network ID conversion cannot be performed when the ID conversion matrix is not ready!");
                return false;
            }
            return IDConversionMatrixToHost.ContainsKey(reg.GetRegistryName());
        }

        /// <summary>
        /// Gets the host ID that matches the specified local ID. The given ID MUST be an int. Returns -1 if the host does not have a matching ID for the specified local ID.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the network ID conversion matrix is not ready.</exception>
        public static object ConvertIDToHost(this Registry reg, ref object ID)
        {
            int id = (int)ID;
            if (!MatrixReady)
            {
                Debug.Log("Network ID conversion cannot be performed when the ID conversion matrix is not ready!");
                return ID;
            }
            if (reg == null) return ID;
            if (IDConversionMatrixToHost.ContainsKey(reg.GetRegistryName()) && IDConversionMatrixToHost[reg.GetRegistryName()].TryGetValue(id, out id))
            {
                return id;
            }
            return ((int)ID) < reg.GetIDStart() ? ID : (ID = -1);
        }

        /// <summary>
        /// Gets the host ID that matches the specified local ID. Returns -1 if the host does not have a matching ID for the specified local ID.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the network ID conversion matrix is not ready.</exception>
        public static int ConvertIDToHost(this Registry reg, ref int ID)
        {
            if (!MatrixReady)
            {
                Debug.Log("Network ID conversion cannot be performed when the ID conversion matrix is not ready!");
                return ID;
            }
            if (reg == null) return ID;
            if (IDConversionMatrixToHost.ContainsKey(reg.GetRegistryName()) && IDConversionMatrixToHost[reg.GetRegistryName()].TryGetValue(ID, out int newID))
            {
                return ID = newID;
            }
            return ID < reg.GetIDStart() ? ID : (ID = -1);
        }

        /// <summary>
        /// Gets the host ID that matches the specified local ID. Returns -1 if the host does not have a matching ID for the specified local ID.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the network ID conversion matrix is not ready.</exception>
        public static int ConvertIDToHost(this Registry reg, int ID)
        {
            if (!MatrixReady)
            {
                Debug.Log("Network ID conversion cannot be performed when the ID conversion matrix is not ready!");
                return ID;
            }
            if (reg == null) return ID;
            if (IDConversionMatrixToHost.ContainsKey(reg.GetRegistryName()) && IDConversionMatrixToHost[reg.GetRegistryName()].TryGetValue(ID, out int newID))
            {
                return newID;
            }
            return ID < reg.GetIDStart() ? ID : -1;
        }

        /// <summary>
        /// Gets the local ID that matches the specified host ID. The given ID must be an int. Returns -1 if there is no matching local ID for the specified host ID.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the network ID conversion matrix is not ready.</exception>
        public static object ConvertIDToLocal(this Registry reg, ref object ID)
        {
            int id = (int)ID;
            if (!MatrixReady)
            {
                Debug.Log("Network ID conversion cannot be performed when the ID conversion matrix is not ready!");
                return ID;
            }
            if (reg == null) return ID;
            if (IDConversionMatrixToLocal.ContainsKey(reg.GetRegistryName()) && IDConversionMatrixToLocal[reg.GetRegistryName()].TryGetValue(id, out id))
            {
                return id;
            }
            return ((int)ID) < reg.GetIDStart() ? ID : (ID = -1);
        }

        /// <summary>
        /// Gets the local ID that matches the specified host ID. Returns -1 if there is no matching local ID for the specified host ID.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the network ID conversion matrix is not ready.</exception>
        public static int ConvertIDToLocal(this Registry reg, ref int ID)
        {
            if (!MatrixReady)
            {
                Debug.Log("Network ID conversion cannot be performed when the ID conversion matrix is not ready!");
                return ID;
            }
            if (reg == null) return ID;
            if (IDConversionMatrixToLocal.ContainsKey(reg.GetRegistryName()) && IDConversionMatrixToLocal[reg.GetRegistryName()].TryGetValue(ID, out int newID))
            {
                return ID = newID;
            }
            return ID < reg.GetIDStart() ? ID : (ID = -1);
        }

        /// <summary>
        /// Gets the local ID that matches the specified host ID. Returns -1 if there is no matching local ID for the specified host ID.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the network ID conversion matrix is not ready.</exception>
        public static int ConvertIDToLocal(this Registry reg, int ID)
        {
            if (!MatrixReady)
            {
                Debug.Log("Network ID conversion cannot be performed when the ID conversion matrix is not ready!");
                return ID;
            }
            if (reg == null) return ID;
            if (IDConversionMatrixToLocal.ContainsKey(reg.GetRegistryName()) && IDConversionMatrixToLocal[reg.GetRegistryName()].TryGetValue(ID, out int newID))
            {
                return newID;
            }
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
        }

        internal static void ParseIDMatrixData(string packedData)
        {
            IsHostVanilla = false;
            MatrixReady = false;
            try
            {
                foreach (string dicEntry in packedData.Split('|'))
                {
                    string[] splitDic = dicEntry.Split(new char[] { ':' }, 2);
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
            }
            catch (Exception e)
            {
                GadgetCore.Log("Received bad host ID conversion data: " + packedData);
                GadgetCore.Log("Exception that occured while parsing host ID conversion data:" + Environment.NewLine + e.ToString());
                Network.Disconnect();
            }
        }
    }
}
