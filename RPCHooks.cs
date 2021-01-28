using GadgetCore.API;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

namespace GadgetCore
{
    [RequireComponent(typeof(NetworkView))]
    internal class RPCHooks : MonoBehaviour
    {
        public static RPCHooks Singleton { get; private set; }
        private static NetworkView view;

        internal void Awake()
        {
            if (Singleton != null && Singleton != this) Destroy(Singleton);
            Singleton = this;
            view = GetComponent<NetworkView>();
            if (Network.isServer)
            {
                GadgetCore.CoreLogger.Log("Listening for client connections...");
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
                SendRequiredModList(Gadgets.ListAllGadgetInfos().Where(x => x.Gadget.Enabled && x.Attribute.RequiredOnClients).Select(x => x.Attribute.Name + ":" + x.Gadget.GetModVersionString()).Aggregate(new StringBuilder(), (x, y) => { if (x.Length > 0) x.Append(","); x.Append(y); return x; }).ToString(), info);
            }
            else
            {
                GadgetCore.CoreLogger.Log("Host is requesting local mod list. Sending...");
                GadgetNetwork.connectTime = Time.realtimeSinceStartup;
                if (view == null) view = GetComponent<NetworkView>();
                view.RPC("SendRequiredModList", RPCMode.Server, Gadgets.ListAllGadgetInfos().Where(x => x.Gadget.Enabled && x.Attribute.RequiredOnClients).Select(x => x.Attribute.Name + ":" + x.Gadget.GetModVersionString()).Aggregate(new StringBuilder(), (x, y) => { if (x.Length > 0) x.Append(","); x.Append(y); return x; }).ToString());
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
                foreach (GadgetInfo mod in Gadgets.ListAllGadgetInfos().Where(x => x.Gadget.Enabled && x.Attribute.RequiredOnClients))
                {
                    modCount++;
                    int[] hostVersionNums = mod.Gadget.GetModVersionString().Split('.').Select(x => int.Parse(x)).ToArray();
                    int[] clientVersionNums = splitModList.SingleOrDefault(x => x[0] == mod.Attribute.Name)?[1].Split('.').Select(x => int.Parse(x)).Take(4).ToArray();
                    if (clientVersionNums == null)
                    {
                        isCompatible = false;
                        break;
                    }
                    hostVersionNums = hostVersionNums.Concat(Enumerable.Repeat(0, 4 - hostVersionNums.Length)).ToArray();
                    clientVersionNums = clientVersionNums.Concat(Enumerable.Repeat(0, 4 - clientVersionNums.Length)).ToArray();
                    if (!((mod.Attribute.GadgetCoreVersionSpecificity == VersionSpecificity.MAJOR && clientVersionNums[0] == hostVersionNums[0] && (clientVersionNums[1] > hostVersionNums[1] || (clientVersionNums[1] == hostVersionNums[1] && (clientVersionNums[2] > hostVersionNums[2] || (clientVersionNums[2] == hostVersionNums[2] && clientVersionNums[3] >= hostVersionNums[3]))))) ||
                        (mod.Attribute.GadgetCoreVersionSpecificity == VersionSpecificity.MINOR && clientVersionNums[0] == hostVersionNums[0] && clientVersionNums[1] == hostVersionNums[1] && (clientVersionNums[2] > hostVersionNums[2] || (clientVersionNums[2] == hostVersionNums[2] && clientVersionNums[3] >= hostVersionNums[3]))) ||
                        (mod.Attribute.GadgetCoreVersionSpecificity == VersionSpecificity.NONBREAKING && clientVersionNums[0] == hostVersionNums[0] && clientVersionNums[1] == hostVersionNums[1] && clientVersionNums[2] == hostVersionNums[2] && clientVersionNums[3] >= hostVersionNums[3]) ||
                        (mod.Attribute.GadgetCoreVersionSpecificity == VersionSpecificity.BUGFIX && clientVersionNums[0] == hostVersionNums[0] && clientVersionNums[1] == hostVersionNums[1] && clientVersionNums[2] == hostVersionNums[2] && clientVersionNums[3] == hostVersionNums[3])))
                    {
                        isCompatible = false;
                        break;
                    }
                }
                if (isCompatible && modCount != splitModList.Length)
                {
                    isCompatible = false;
                }
                if (isCompatible)
                {
                    if (string.IsNullOrEmpty(info.sender.ipAddress))
                    {
                        GadgetCore.CoreLogger.Log("Self-connection succesfully established and identified.");
                        ReceiveIDMatrixData(GadgetNetwork.GenerateIDMatrixData());
                    }
                    else
                    {
                        GadgetCore.CoreLogger.Log("A client connected with compatible mods: " + info.sender.ipAddress);
                        view.RPC("ReceiveIDMatrixData", info.sender, GadgetNetwork.GenerateIDMatrixData());
                    }
                }
                else
                {
                    GadgetCore.CoreLogger.LogWarning("A client tried to connect with incompatible mods: " + info.sender.ipAddress + Environment.NewLine + modList);
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
            catch (Exception e)
            {
                GadgetCore.CoreLogger.LogWarning("The following error occured processing an incoming client's mod list: " + info.sender.ipAddress + Environment.NewLine + modList + Environment.NewLine + e.ToString());
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
                GadgetCore.CoreLogger.Log("Received Host ID Conversion Matrix data.");
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

        internal static void Destroy(NetworkViewID viewID)
        {
            if (view == null) view = Singleton.GetComponent<NetworkView>();
            view.RPC("NetworkDestroy", RPCMode.AllBuffered, viewID);
        }

        [RPC]
        internal void NetworkDestroy(NetworkViewID viewID)
        {
            NetworkView view = NetworkView.Find(viewID);
            if (view != null)
            {
                Destroy(view.gameObject);
            }
        }

        internal void BroadcastConsoleMessage(string text, string sender, GadgetConsole.MessageSeverity severity, float sendTime)
        {
            view.RPC("RPCBroadcastConsoleMessage", RPCMode.Others, text, sender, (int)severity, sendTime);
        }

        [RPC]
        internal void RPCBroadcastConsoleMessage(string text, string sender, int severity, float sendTime)
        {
            GadgetConsole.GetMessage(GadgetConsole.Print(text, sender, (GadgetConsole.MessageSeverity)severity)).SendTime = sendTime;
        }

        internal void RemoveConsoleBroadcast(float sendTime)
        {
            view.RPC("RPCRemoveConsoleBroadcast", RPCMode.Others, sendTime);
        }

        [RPC]
        internal void RPCRemoveConsoleBroadcast(float sendTime)
        {
            GadgetConsole.RemoveMessage(sendTime);
        }

        internal void SetOp(string name, bool op)
        {
            view.RPC("RPCSetOp", RPCMode.AllBuffered, name, op);
        }

        [RPC]
        internal void RPCSetOp(string name, bool op)
        {
            if (op)
            {
                if (!GadgetConsole.operators.Contains(name))
                {
                    GadgetConsole.operators.Add(name);
                    GadgetConsole.Print("You are now an operator!", null, GadgetConsole.MessageSeverity.INFO);
                }
            }
            else
            {
                if (GadgetConsole.operators.Contains(name))
                {
                    GadgetConsole.operators.Remove(name);
                    GadgetConsole.Print("You are no longer an operator!", null, GadgetConsole.MessageSeverity.WARN);
                }
            }
        }

        internal void GiveItem(Item item, NetworkPlayer player)
        {
            if (Network.player == player)
            {
                GadgetCoreAPI.SpawnItemLocal(InstanceTracker.PlayerScript.transform.position, item, false).gameObject.SendMessage("Request");
            }
            else
            {
                view.RPC("RPCGiveItem", player, GadgetCoreAPI.ConstructIntArrayFromItem(item));
            }
        }

        [RPC]
        internal void RPCGiveItem(int[] st)
        {
            GadgetCoreAPI.SpawnItemLocal(InstanceTracker.PlayerScript.transform.position, GadgetCoreAPI.ConstructItemFromIntArray(st), false).gameObject.SendMessage("Request");
        }

        internal void GiveChip(Item item, NetworkPlayer player)
        {
            if (Network.player == player)
            {
                GadgetCoreAPI.SpawnItemLocal(InstanceTracker.PlayerScript.transform.position, item, true).gameObject.SendMessage("Request");
            }
            else
            {
                view.RPC("RPCGiveChip", player, GadgetCoreAPI.ConstructIntArrayFromItem(item));
            }
        }

        [RPC]
        internal void RPCGiveChip(int[] st)
        {
            GadgetCoreAPI.SpawnItemLocal(InstanceTracker.PlayerScript.transform.position, GadgetCoreAPI.ConstructItemFromIntArray(st), true).gameObject.SendMessage("Request");
        }

        internal void CallGeneral(string name, RPCMode mode, params object[] args)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            foreach (object arg in args)
            {
                formatter.Serialize(stream, arg);
            }
            view.RPC("RPCCallGeneral", mode, new object[] { name, args.Length, stream.ToArray()});
            stream.Close();
        }

        internal void CallGeneral(string name, NetworkPlayer target, params object[] args)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            foreach (object arg in args)
            {
                formatter.Serialize(stream, arg);
            }
            stream.Position = 0;
            view.RPC("RPCCallGeneral", target, new object[] { name, args.Length, stream.ToArray() });
            stream.Close();
        }

        [RPC]
        internal void RPCCallGeneral(string name, int argCount, byte[] serializedArgs)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            stream.Write(serializedArgs, 0, serializedArgs.Length);
            stream.Position = 0;
            object[] args = new object[argCount];
            for (int i = 0;i < argCount; i++)
            {
                args[i] = formatter.Deserialize(stream);
            }
            if (GadgetCoreAPI.customRPCs.ContainsKey(name))
            {
                GadgetCoreAPI.customRPCs[name].Invoke(args);
            }
            stream.Close();
        }

        internal void UpdateSyncVar(string name, object value, bool local)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, value);
            stream.Position = 0;
            view.RPC("RPCUpdateSyncVar", RPCMode.AllBuffered, new object[] { name, stream.ToArray(), local, InstanceTracker.PlayerScript.txtName[0].text });
            stream.Close();
        }

        [RPC]
        internal void RPCUpdateSyncVar(string name, byte[] valueBytes, bool local, string playerName)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            stream.Write(valueBytes, 0, valueBytes.Length);
            stream.Position = 0;
            object value = formatter.Deserialize(stream);
            stream.Close();
            if (local)
            {
                GadgetNetwork.UpdateLocalSyncVarInternal(name, value, playerName);
            }
            else if (playerName == GadgetNetwork.ServerPlayerName)
            {
                GadgetNetwork.UpdateSyncVarInternal(name, value);
            }
            else GadgetCore.CoreLogger.LogWarning("Received non-local UpdateSyncVar RPC from player other than the server: " + playerName);
        }
    }
}
