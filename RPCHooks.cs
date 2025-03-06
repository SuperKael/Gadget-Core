using GadgetCore.API;
using GadgetCore.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GadgetCore
{
    [RequireComponent(typeof(NetworkView))]
    internal class RPCHooks : MonoBehaviour
    {
        public const int MaxChunkSize = 2048;

        public static RPCHooks Singleton { get; private set; }
        private static NetworkView view;

        private readonly HashSet<NetworkPlayer> initiatedClients = new HashSet<NetworkPlayer>();
        private readonly NetworkMessageInfo serverNMI = new NetworkMessageInfo();
        private string[] IDMatrixDataChunks;

        internal void Awake()
        {
            if (Singleton != null && Singleton != this) Destroy(Singleton);
            Singleton = this;
            initiatedClients.Clear();
            view = GetComponent<NetworkView>();
            if (Network.isServer)
            {
                GadgetCore.CoreLogger.Log("Awaiting for client connections...");
            }
        }

        internal static void InitiateGadgetNetwork()
        {
            if (Singleton == null) Singleton = InstanceTracker.GameScript.gameObject.AddComponent<RPCHooks>();
            if (view == null) view = InstanceTracker.GameScript.gameObject.GetComponent<NetworkView>();
            if (Network.isServer)
            {
                if (Singleton.initiatedClients.Contains(Network.player)) return;
                Singleton.initiatedClients.Add(Network.player);
                Singleton.RequestModList(Singleton.serverNMI);
            }
            else
            {
                view.RPC("InitiateGadgetNetwork", RPCMode.Server);
            }
        }

        [RPC]
        internal void InitiateGadgetNetwork(NetworkMessageInfo info)
        {
            if (!Network.isServer || initiatedClients.Contains(info.sender)) return;
            initiatedClients.Add(info.sender);
            GadgetCore.CoreLogger.Log("Client reports ready. Requesting mod list...");
            view.RPC("RequestModList", info.sender);
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
                IDMatrixDataChunks = null;
                view.RPC("SendRequiredModList", RPCMode.Server, Gadgets.ListAllGadgetInfos().Where(x => x.Gadget.Enabled && x.Attribute.RequiredOnClients).Select(x => x.Attribute.Name + ":" + x.Gadget.GetModVersionString()).Aggregate(new StringBuilder(), (x, y) => { if (x.Length > 0) x.Append(","); x.Append(y); return x; }).ToString());
            }
        }

        [RPC]
        internal void SendRequiredModList(string modList, NetworkMessageInfo info)
        {
            GadgetCore.CoreLogger.Log("Client has sent local mod list. Processing...");
            try
            {
                bool isCompatible = true;
                List<string> incompatibleReasons = new List<string>();
                HashSet<string> handledGadgets = new HashSet<string>();
                string[][] splitModList = string.IsNullOrEmpty(modList) ? new string[0][] : modList.Split(',').Select(x => x.Split(':')).ToArray();
                foreach (GadgetInfo mod in Gadgets.ListAllGadgetInfos().Where(x => x.Gadget.Enabled && x.Attribute.RequiredOnClients))
                {
                    handledGadgets.Add(mod.Attribute.Name);
                    string clientVersion = splitModList.SingleOrDefault(x => x[0] == mod.Attribute.Name)?[1];
                    if (clientVersion == null)
                    {
                        isCompatible = false;
                        incompatibleReasons.Add($"The Gadget '{mod.Attribute.Name}' (From the mod '{mod.ModName}') is not present on the client");
                        continue;
                    }
                    int[] clientVersionNums = clientVersion.Split('.').Select(x => int.Parse(x)).Take(4).ToArray();
                    int[] hostVersionNums = mod.Gadget.GetModVersionString().Split('.').Select(x => int.Parse(x)).ToArray();
                    hostVersionNums = hostVersionNums.Concat(Enumerable.Repeat(0, 4 - hostVersionNums.Length)).ToArray();
                    clientVersionNums = clientVersionNums.Concat(Enumerable.Repeat(0, 4 - clientVersionNums.Length)).ToArray();
                    if (!((mod.Attribute.GadgetCoreVersionSpecificity == VersionSpecificity.MAJOR && clientVersionNums[0] == hostVersionNums[0] && (clientVersionNums[1] > hostVersionNums[1] || (clientVersionNums[1] == hostVersionNums[1] && (clientVersionNums[2] > hostVersionNums[2] || (clientVersionNums[2] == hostVersionNums[2] && clientVersionNums[3] >= hostVersionNums[3]))))) ||
                        (mod.Attribute.GadgetCoreVersionSpecificity == VersionSpecificity.MINOR && clientVersionNums[0] == hostVersionNums[0] && clientVersionNums[1] == hostVersionNums[1] && (clientVersionNums[2] > hostVersionNums[2] || (clientVersionNums[2] == hostVersionNums[2] && clientVersionNums[3] >= hostVersionNums[3]))) ||
                        (mod.Attribute.GadgetCoreVersionSpecificity == VersionSpecificity.NONBREAKING && clientVersionNums[0] == hostVersionNums[0] && clientVersionNums[1] == hostVersionNums[1] && clientVersionNums[2] == hostVersionNums[2] && clientVersionNums[3] >= hostVersionNums[3]) ||
                        (mod.Attribute.GadgetCoreVersionSpecificity == VersionSpecificity.BUGFIX && clientVersionNums[0] == hostVersionNums[0] && clientVersionNums[1] == hostVersionNums[1] && clientVersionNums[2] == hostVersionNums[2] && clientVersionNums[3] == hostVersionNums[3])))
                    {
                        isCompatible = false;
                        incompatibleReasons.Add($"The Gadget '{mod.Attribute.Name}' (From the mod '{mod.ModName}') is of incompatible versions: Host: {mod.Gadget.GetModVersionString()}, Client: {clientVersion}");
                    }
                }
                if (handledGadgets.Count != splitModList.Length)
                {
                    isCompatible = false;
                    foreach (string[] modEntry in splitModList.Where(x => !handledGadgets.Contains(x[0])))
                    {
                        incompatibleReasons.Add($"The Gadget '{modEntry[0]}' (From an unknown mod) is present on the client, but not on the host");
                    }
                }
                if (isCompatible)
                {
                    if (info.Equals(serverNMI))
                    {
                        GadgetCore.CoreLogger.Log("Self-connection succesfully established and identified.");
                        ReceiveIDMatrixData(GadgetNetwork.GenerateIDMatrixData());
                    }
                    else
                    {
                        GadgetCore.CoreLogger.Log("A client connected with compatible mods: " + info.sender.ipAddress);
                        string matrixData = GadgetNetwork.GenerateIDMatrixData();
                        if (matrixData.Length <= 4096)
                        {
                            GadgetCore.CoreLogger.Log("Sending ID Matrix data as a single block...");
                            view.RPC("ReceiveIDMatrixData", info.sender, matrixData);
                        }
                        else
                        {
                            string[] splitMatrixData = matrixData.SplitOnLength(MaxChunkSize).ToArray();
                            GadgetCore.CoreLogger.Log($"Sending ID Matrix data as {splitMatrixData.Length} chunks...");
                            for (int i = 0; i < splitMatrixData.Length; i++)
                            {
                                view.RPC("ReceiveIDMatrixDataChunk", info.sender, splitMatrixData[i], i, splitMatrixData.Length);
                            }
                        }
                    }
                }
                else
                {
                    GadgetCore.CoreLogger.LogWarning("A client tried to connect with incompatible mods: " + info.sender.ipAddress +
                        Environment.NewLine + " - " + incompatibleReasons.Concat(Environment.NewLine + " - "));
                    if (Network.isServer)
                    {
                        DisconnectWithMessage(info.sender, "Your mods are incompatible with the server:" + Environment.NewLine + " - " + incompatibleReasons.Concat(Environment.NewLine + " - "));
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
                    DisconnectWithMessage(info.sender, "An error occured processing your mod list:" + Environment.NewLine + modList + Environment.NewLine + e.ToString());
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

        [RPC]
        internal void ReceiveIDMatrixDataChunk(string IDMatrixData, int chunkIndex, int chunkCount)
        {
            if (IDMatrixDataChunks == null)
            {
                if (!Network.isServer)
                {
                    GadgetCore.CoreLogger.Log($"Receiving Host ID Conversion Matrix data chunks. {chunkCount} chunks expected.");
                }
                IDMatrixDataChunks = new string[chunkCount];
            }
            else if (IDMatrixDataChunks.Length != chunkCount)
            {
                GadgetCore.CoreLogger.LogError("Received Host ID Conversion Matrix data chunk with invalid chunk count.");
                Network.Disconnect();
                return;
            }
            else if (IDMatrixDataChunks[chunkIndex] != null)
            {
                GadgetCore.CoreLogger.LogError("Received Host ID Conversion Matrix data chunk with duplicate chunk index.");
                Network.Disconnect();
                return;
            }
            IDMatrixDataChunks[chunkIndex] = IDMatrixData;
            if (IDMatrixDataChunks.All(x => x != null))
            {
                if (!Network.isServer)
                {
                    GadgetCore.CoreLogger.Log("Finished Receiving Host ID Conversion Matrix data chunks.");
                    GadgetNetwork.connectTime = Time.realtimeSinceStartup;
                }
                GadgetNetwork.ParseIDMatrixData(IDMatrixDataChunks.Concat(string.Empty));
            }
        }

        internal static void DisconnectWithMessage(NetworkPlayer target, string message)
        {
            Singleton.StartCoroutine(DisconnectWithMessageRoutine(target, message));
        }

        private static IEnumerator DisconnectWithMessageRoutine(NetworkPlayer target, string message)
        {
            view.RPC("RPCDisconnectWithMessage", target, message);
            yield return new WaitForSecondsRealtime(GadgetNetwork.MatrixTimeout);
            if (Network.connections.Contains(target))
            {
                Network.CloseConnection(target, true);
            }
        }

        [RPC]
        internal void RPCDisconnectWithMessage(string message)
        {
            GadgetCore.CoreLogger.LogWarning("You have been disconnected with the following message: " + message);
            Network.Disconnect((int)(GadgetNetwork.MatrixTimeout * 1000));
        }

        internal static GameObject InstantiateResource(string path, Vector3 position, Quaternion rotation, int group)
        {
            if (view == null) view = Singleton.GetComponent<NetworkView>();
            NetworkViewID viewID = Network.AllocateViewID();
            view.RPC("NetworkInstantiate", RPCMode.OthersBuffered, path, position, rotation, group, viewID);
            return InstantiateWithNetworkView(Resources.Load(path), position, rotation, group, viewID);
        }

        [RPC]
        internal void NetworkInstantiate(string path, Vector3 position, Quaternion rotation, int group, NetworkViewID viewID)
        {
            Object objectToInstantiate = Resources.Load(path);
            if (objectToInstantiate == null)
            {
                GadgetCore.CoreLogger.LogWarning($"Tried to instantiate resource {path} with viewID {viewID}, but that resource is invalid!");
                return;
            }
            InstantiateWithNetworkView(objectToInstantiate, position, rotation, group, viewID);
        }

        private static GameObject InstantiateWithNetworkView(Object original, Vector3 position, Quaternion rotation, int group, NetworkViewID viewID)
        {
            GameObject obj = Instantiate(original, position, rotation) as GameObject;
            if (obj != null)
            {
                NetworkView view = obj.GetComponent<NetworkView>();
                if (view == null) view = obj.AddComponent<NetworkView>();
                view.group = group;
                view.viewID = viewID;
            }
            return obj;
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

        internal GameObject CreateMarketStand(ItemInfo item, Vector2 pos, int cost, bool isBuild, bool isCredits, bool isTrophies)
        {
            Vector3 pos3d = new Vector3(pos.x, pos.y, SceneInjector.BuildStand.transform.position.z);
            NetworkViewID viewID = Network.AllocateViewID();
            Singleton.GetComponent<NetworkView>().RPC("RPCCreateMarketStand", RPCMode.OthersBuffered, item.GetHostID(), pos3d, cost, isBuild, isCredits, isTrophies, viewID);
            GameObject shopStand = Instantiate(SceneInjector.BuildStand, SceneInjector.BuildStand.transform.parent);
            NetworkView view = shopStand.GetComponent<NetworkView>();
            if (view == null) view = shopStand.AddComponent<NetworkView>();
            view.viewID = viewID;
            shopStand.transform.localPosition = pos3d;
            shopStand.name = isBuild ? "buildStand" : "kylockeStand";
            KylockeStand standScript = shopStand.GetComponent<KylockeStand>();
            standScript.isTrophies = isTrophies;
            standScript.isCredits = isCredits;
            standScript.isBuild = isBuild;
            standScript.itemID = item.ID;
            standScript.cost = cost;
            standScript.currency.GetComponent<MeshRenderer>().material = isBuild ? isCredits ? isTrophies ? GadgetCoreAPI.GetItemMaterial(59) : GadgetCoreAPI.GetItemMaterial(52) : GadgetCoreAPI.GetItemMaterial(57) : GadgetCoreAPI.GetItemMaterial(51);
            standScript.icon.GetComponent<MeshRenderer>().material = item.Mat;
            standScript.txtName[0].text = item.GetName();
            standScript.txtName[1].text = standScript.txtName[0].text;
            standScript.txtCost[0].text = string.Empty + standScript.cost;
            standScript.txtCost[1].text = standScript.txtCost[0].text;
            return shopStand;
        }

        [RPC]
        internal void RPCCreateMarketStand(int itemID, Vector3 pos, int cost, bool isBuild, bool isCredits, bool isTrophies, NetworkViewID viewID)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                StartCoroutine(GadgetUtils.WaitAndInvoke(typeof(RPCHooks).GetMethod("RPCCreateMarketStand", BindingFlags.NonPublic | BindingFlags.Instance), GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, this, itemID, pos, cost, isBuild, isCredits, isTrophies, viewID));
                return;
            }
            ItemInfo item = ItemRegistry.Singleton[ItemRegistry.Singleton.ConvertIDToLocal(itemID)];
            GameObject shopStand = Instantiate(SceneInjector.BuildStand, SceneInjector.BuildStand.transform.parent);
            NetworkView view = shopStand.GetComponent<NetworkView>();
            if (view == null) view = shopStand.AddComponent<NetworkView>();
            view.viewID = viewID;
            shopStand.transform.localPosition = new Vector3(pos.x, pos.y, SceneInjector.BuildStand.transform.position.z);
            shopStand.name = isBuild ? "buildStand" : "kylockeStand";
            KylockeStand standScript = shopStand.GetComponent<KylockeStand>();
            standScript.isTrophies = isTrophies;
            standScript.isCredits = isCredits;
            standScript.isBuild = isBuild;
            standScript.itemID = item.ID;
            standScript.cost = cost;
            standScript.currency.GetComponent<MeshRenderer>().material = isBuild ? isCredits ? isTrophies ? GadgetCoreAPI.GetItemMaterial(59) : GadgetCoreAPI.GetItemMaterial(52) : GadgetCoreAPI.GetItemMaterial(57) : GadgetCoreAPI.GetItemMaterial(51);
            standScript.icon.GetComponent<MeshRenderer>().material = item.Mat;
            standScript.txtName[0].text = item.GetName();
            standScript.txtName[1].text = standScript.txtName[0].text;
            standScript.txtCost[0].text = string.Empty + standScript.cost;
            standScript.txtCost[1].text = standScript.txtCost[0].text;
        }

        internal void BroadcastConsoleMessage(int index, string text, string sender, GadgetConsole.MessageSeverity severity, float sendTime)
        {
            view.RPC("RPCBroadcastConsoleMessage", RPCMode.Others, index, text, sender, (int)severity, sendTime);
        }

        private Dictionary<int, int> broadcastMessageIndices = new Dictionary<int, int>();

        [RPC]
        internal void RPCBroadcastConsoleMessage(int index, string text, string sender, int severity, float sendTime)
        {
            int messageIndex = GadgetConsole.Print(text, sender, (GadgetConsole.MessageSeverity)severity);
            GadgetConsole.GetMessage(messageIndex).SendTime = sendTime;
            broadcastMessageIndices[index] = messageIndex;
        }

        internal void ReplaceConsoleBroadcast(int index, string text, string sender, GadgetConsole.MessageSeverity severity, float sendTime)
        {
            view.RPC("RPCReplaceConsoleBroadcast", RPCMode.Others, index, text, sender, (int)severity, sendTime);
        }

        [RPC]
        internal void RPCReplaceConsoleBroadcast(int index, string text, string sender, int severity, float sendTime)
        {
            index = broadcastMessageIndices[index];
            GadgetConsole.ReplaceMessage(index, new GadgetConsole.GadgetConsoleMessage(text, sender, (GadgetConsole.MessageSeverity)severity));
            if (sendTime >= 0) GadgetConsole.GetMessage(index).SendTime = sendTime;
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
                    if (name == GadgetCoreAPI.GetPlayerName()) GadgetConsole.Print("You are now an operator!", null, GadgetConsole.MessageSeverity.INFO);
                }
            }
            else
            {
                if (GadgetConsole.operators.Contains(name))
                {
                    GadgetConsole.operators.Remove(name);
                    if (name == GadgetCoreAPI.GetPlayerName()) GadgetConsole.Print("You are no longer an operator!", null, GadgetConsole.MessageSeverity.WARN);
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
                view.RPC("RPCGiveItem", player, GadgetCoreAPI.ConstructIntArrayFromItem(item, true, false));
            }
        }

        [RPC]
        internal void RPCGiveItem(int[] st)
        {
            GadgetCoreAPI.SpawnItemLocal(InstanceTracker.PlayerScript.transform.position, GadgetCoreAPI.ConstructItemFromIntArray(st, true, false), false).gameObject.SendMessage("Request");
        }

        internal void GiveChip(Item item, NetworkPlayer player)
        {
            if (Network.player == player)
            {
                GadgetCoreAPI.SpawnItemLocal(InstanceTracker.PlayerScript.transform.position, item, true).gameObject.SendMessage("Request");
            }
            else
            {
                view.RPC("RPCGiveChip", player, GadgetCoreAPI.ConstructIntArrayFromItem(item, true, true));
            }
        }

        [RPC]
        internal void RPCGiveChip(int[] st)
        {
            GadgetCoreAPI.SpawnItemLocal(InstanceTracker.PlayerScript.transform.position, GadgetCoreAPI.ConstructItemFromIntArray(st, true, true), true).gameObject.SendMessage("Request");
        }

        internal void SendConsoleMessage(string message, NetworkPlayer player)
        {
            if (Network.player == player)
            {
                RPCSendConsoleMessage(message, new NetworkMessageInfo());
            }
            else
            {
                view.RPC("RPCSendConsoleMessage", player, message);
            }
        }

        [RPC]
        internal void RPCSendConsoleMessage(string message, NetworkMessageInfo info)
        {
            if (string.IsNullOrEmpty(message)) return;
            if (message.Length > 1 && message[0] == '/')
            {
                string command = GadgetConsole.ParseArgs(message.Substring(1))[0];
                if (GadgetConsole.IsCommandExecuteBlacklisted(command))
                {
                    GadgetConsole.Print($"{GadgetNetwork.GetNameByNetworkPlayer(info.sender) ?? GadgetCoreAPI.GetPlayerName()} attempted to force you to execute the blacklisted command: {message}", null, GadgetConsole.MessageSeverity.WARN);
                    return;
                }
            }
            bool isOperator = GadgetConsole.IsOperator(GadgetCoreAPI.GetPlayerName());
            if (isOperator)
            {
                if (message.Length > 1 && message[0] == '/')
                {
                    GadgetConsole.Print($"{GadgetNetwork.GetNameByNetworkPlayer(info.sender) ?? GadgetCoreAPI.GetPlayerName()} forced you to execute the command: {message}");
                }
                else
                {
                    GadgetConsole.Print($"{GadgetNetwork.GetNameByNetworkPlayer(info.sender) ?? GadgetCoreAPI.GetPlayerName()} forced you to say: {message}");
                }
            }
            GadgetConsole.SendConsoleMessage(message, GadgetCoreAPI.GetPlayerName(), isOperator, true);
        }

        internal void CallGeneral(string name, RPCMode mode, params object[] args)
        {
            if (args == null || args.Length == 0)
            {
                view.RPC("RPCCallGeneral", mode, name, 0, new byte[] { 0 });
                return;
            }
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            foreach (object arg in args)
            {
                formatter.Serialize(stream, arg);
            }
            view.RPC("RPCCallGeneral", mode, name, args.Length, stream.ToArray());
            stream.Close();
        }

        internal void CallGeneral(string name, NetworkPlayer target, params object[] args)
        {
            if (args == null || args.Length == 0)
            {
                view.RPC("RPCCallGeneral", target, name, 0, new byte[] { 0 });
                return;
            }
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            foreach (object arg in args)
            {
                formatter.Serialize(stream, arg);
            }
            stream.Position = 0;
            view.RPC("RPCCallGeneral", target, name, args.Length, stream.ToArray());
            stream.Close();
        }

        [RPC]
        internal void RPCCallGeneral(string name, int argCount, byte[] serializedArgs)
        {
            object[] args = new object[argCount];
            if (argCount > 0)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream stream = new MemoryStream();
                stream.Write(serializedArgs, 0, serializedArgs.Length);
                stream.Position = 0;
                for (int i = 0; i < argCount; i++)
                {
                    args[i] = formatter.Deserialize(stream);
                }
                stream.Close();
            }
            if (GadgetCoreAPI.customRPCActions.TryGetValue(name, out var customRPC))
            {
                customRPC.Invoke(args);
            }
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
