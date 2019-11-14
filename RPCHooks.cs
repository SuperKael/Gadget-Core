using GadgetCore.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GadgetCore
{
    [RequireComponent(typeof(NetworkView))]
    internal class RPCHooks : MonoBehaviour
    {
        public static RPCHooks Singleton { get; private set; }
        private static NetworkView view;

        internal void Start()
        {
            if (Singleton != null && Singleton != this) Destroy(Singleton);
            Singleton = this;
            view = GetComponent<NetworkView>();
            if (Network.isServer)
            {
                GadgetCore.Log("Listening for client connections...");
                IdentifyNewClients();
            }
        }

        internal void IdentifyNewClients()
        {
            view.RPC("RequestModList", RPCMode.AllBuffered);
        }

        [RPC]
        internal void RequestModList(NetworkMessageInfo info)
        {
            if (Network.isServer)
            {
                SendRequiredModList(GadgetMods.ListAllModInfos().Where(x => x.Mod.Enabled && x.Attribute.RequiredOnClients).Select(x => x.Attribute.Name + ":" + x.Mod.GetModVersionString()).Aggregate(new StringBuilder(), (x, y) => { if (x.Length > 0) x.Append(","); x.Append(y); return x; }).ToString(), info);
            }
            else
            {
                GadgetCore.Log("Host is requesting local mod list. Sending...");
                GadgetNetwork.connectTime = Time.realtimeSinceStartup;
                if (view == null) view = GetComponent<NetworkView>();
                view.RPC("SendRequiredModList", RPCMode.Server, GadgetMods.ListAllModInfos().Where(x => x.Mod.Enabled && x.Attribute.RequiredOnClients).Select(x => x.Attribute.Name + ":" + x.Mod.GetModVersionString()).Aggregate(new StringBuilder(), (x, y) => { if (x.Length > 0) x.Append(","); x.Append(y); return x; }).ToString());
            }
        }

        [RPC]
        internal void SendRequiredModList(string modList, NetworkMessageInfo info)
        {
            try
            {
                bool isCompatible = true;
                int modCount = 0;
                string[][] splitModList = string.IsNullOrEmpty(modList) ? new string[0][] : modList.Split(',').Select(x => x.Split(':')).ToArray();
                foreach (GadgetModInfo mod in GadgetMods.ListAllModInfos().Where(x => x.Mod.Enabled && x.Attribute.RequiredOnClients))
                {
                    modCount++;
                    int[] hostVersionNums = mod.Mod.GetModVersionString().Split('.').Select(x => int.Parse(x)).ToArray();
                    int[] clientVersionNums = splitModList.Single(x => x[0] == mod.Attribute.Name)[1].Split('.').Select(x => int.Parse(x)).Take(4).ToArray();
                    hostVersionNums = hostVersionNums.Concat(Enumerable.Repeat(0, 4 - hostVersionNums.Length)).ToArray();
                    clientVersionNums = clientVersionNums.Concat(Enumerable.Repeat(0, 4 - clientVersionNums.Length)).ToArray();
                    if (!((mod.Attribute.GadgetVersionSpecificity == VersionSpecificity.MAJOR && clientVersionNums[0] == hostVersionNums[0] && (clientVersionNums[1] > hostVersionNums[1] || (clientVersionNums[1] == hostVersionNums[1] && (clientVersionNums[2] > hostVersionNums[2] || (clientVersionNums[2] == hostVersionNums[2] && clientVersionNums[3] >= hostVersionNums[3]))))) ||
                        (mod.Attribute.GadgetVersionSpecificity == VersionSpecificity.MINOR && clientVersionNums[0] == hostVersionNums[0] && clientVersionNums[1] == hostVersionNums[1] && (clientVersionNums[2] > hostVersionNums[2] || (clientVersionNums[2] == hostVersionNums[2] && clientVersionNums[3] >= hostVersionNums[3]))) ||
                        (mod.Attribute.GadgetVersionSpecificity == VersionSpecificity.NONBREAKING && clientVersionNums[0] == hostVersionNums[0] && clientVersionNums[1] == hostVersionNums[1] && clientVersionNums[2] == hostVersionNums[2] && clientVersionNums[3] >= hostVersionNums[3]) ||
                        (mod.Attribute.GadgetVersionSpecificity == VersionSpecificity.BUGFIX && clientVersionNums[0] == hostVersionNums[0] && clientVersionNums[1] == hostVersionNums[1] && clientVersionNums[2] == hostVersionNums[2] && clientVersionNums[3] == hostVersionNums[3])))
                    {
                        isCompatible = false;
                    }
                }
                if (isCompatible && modCount != splitModList.Length)
                {
                    GadgetCore.Log(modCount + ":" + splitModList.Length);
                    isCompatible = false;
                }
                if (isCompatible)
                {
                    if (string.IsNullOrEmpty(info.sender.ipAddress))
                    {
                        GadgetCore.Log("Self-connection succesfully established and identified.");
                        ReceiveIDMatrixData(GadgetNetwork.GenerateIDMatrixData());
                    }
                    else
                    {
                        GadgetCore.Log("A client connected with compatible mods: " + info.sender.ipAddress);
                        view.RPC("ReceiveIDMatrixData", info.sender, GadgetNetwork.GenerateIDMatrixData());
                    }
                }
                else
                {
                    GadgetCore.Log("A client tried to connect with incompatible mods: " + info.sender.ipAddress + Environment.NewLine + modList);
                    if (Network.isServer)
                    {
                        Network.Disconnect();
                    }
                    else
                    {
                        Network.CloseConnection(info.sender, true);
                    }
                }
            }
            catch (Exception e)
            {
                GadgetCore.Log("The following error occured processing the client's mod list: " + info.sender.ipAddress + Environment.NewLine + e.ToString());
                if (Network.isServer)
                {
                    Network.CloseConnection(info.sender, true);
                }
                else
                {
                    Network.Disconnect();
                }
            }
        }

        [RPC]
        internal void ReceiveIDMatrixData(string IDMatrixData)
        {
            if (!Network.isServer)
            {
                GadgetCore.Log("Received Host ID Conversion Matrix data.");
                GadgetNetwork.connectTime = Time.realtimeSinceStartup;
            }
            GadgetNetwork.ParseIDMatrixData(IDMatrixData);
        }

        internal static GameObject Instantiate(string path, Vector3 position, Quaternion rotation, int group)
        {
            if (view == null) view = Singleton.GetComponent<NetworkView>();
            NetworkViewID viewID = Network.AllocateViewID();
            view.RPC("NetworkInstantiate", RPCMode.OthersBuffered, path, position, rotation, group, viewID);
            GameObject obj = Instantiate(Resources.Load(path), position, rotation) as GameObject;
            if (obj != null)
            {
                NetworkView view = obj.GetComponent<NetworkView>();
                if (view == null) view = obj.AddComponent<NetworkView>();
                view.group = group;
                view.viewID = viewID;
            }
            return obj;
        }

        [RPC]
        internal void NetworkInstantiate(string path, Vector3 position, Quaternion rotation, int group, NetworkViewID viewID)
        {
            GameObject obj = Instantiate(Resources.Load(path), position, rotation) as GameObject;
            if (obj != null)
            {
                NetworkView view = obj.GetComponent<NetworkView>();
                if (view == null) view = obj.AddComponent<NetworkView>();
                view.group = group;
                view.viewID = viewID;
            }
        }
    }
}
