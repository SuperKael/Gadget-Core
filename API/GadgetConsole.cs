using GadgetCore.API;
using GadgetCore.Loader;
using GadgetCore.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GadgetCore
{
    /// <summary>
    /// The console used by GadgetCore. Use its static methods to interface with it from code. Note that console messages support rich text.
    /// </summary>
    public class GadgetConsole : MonoBehaviour
    {
        /// <summary>
        /// Indicates whether the console should show debug messages.
        /// </summary>
        public static bool Debug { get; internal set; } = false;
        private static GadgetLogger Logger = new GadgetLogger("GadgetCore", "Console");
        private static Thread mainThread;

        /// <summary>
        /// The Console object in the scene
        /// </summary>
        public static GadgetConsole Console { get; private set; }
        private static List<GadgetConsoleMessage> messages = new List<GadgetConsoleMessage>();
        private static Dictionary<string, ConsoleCommand> commands = new Dictionary<string, ConsoleCommand>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<int, List<string>> gadgetCommands = new Dictionary<int, List<string>>();
        private static Dictionary<string, bool> isOperatorOnly = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, string> helpDescriptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, string> fullHelps = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, string> commandAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static List<GadgetConsoleMessage> queuedMessages = new List<GadgetConsoleMessage>();
        private static List<string> messageHistory = new List<string>();
        private static HashSet<string> executeBlacklist = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static int historyIndex;

        internal static List<string> operators = new List<string>();

        private static bool suppressPrint;
        private static bool wasSelected, wasOpen;
        internal static bool hidThisFrame;

        /// <summary>
        /// The text field where input is entered into the console.
        /// </summary>
        public InputField InputField;
        /// <summary>
        /// The panel where the console's text content is placed
        /// </summary>
        public RectTransform TextPanel;
        /// <summary>
        /// The panel where the console places recently-sent messages.
        /// </summary>
        public RectTransform AlwaysActivePanel;

        private Action TextSubmitAction, HistoryUpAction, HistoryDownAction;

        private void Awake()
        {
            mainThread = Thread.CurrentThread;
        }

        /// <summary>
        /// Opens the console
        /// </summary>
        public static void ShowConsole()
        {
            if (hidThisFrame) return;
            GadgetCoreAPI.FreezeInput("Gadget Console Open");
            wasOpen = true;
            Console.gameObject.SetActive(true);
            Console.AlwaysActivePanel.gameObject.SetActive(false);
            Console.InputField.ActivateInputField();
            Console.InputField.Select();
            historyIndex = messageHistory.Count;
        }

        /// <summary>
        /// Closes the console
        /// </summary>
        public static void HideConsole()
        {
            GadgetCoreAPI.DelayUnfreezeInput("Gadget Console Open");
            if (wasOpen) hidThisFrame = true;
            wasOpen = false;
            Console.gameObject.SetActive(false);
            Console.AlwaysActivePanel.gameObject.SetActive(true);
            Console.InputField.text = "";
            Console.InputField.DeactivateInputField();
        }

        /// <summary>
        /// Toggles the console
        /// </summary>
        public static void ToggleConsole()
        {
            Console.gameObject.SetActive(!Console.gameObject.activeSelf);
            Console.AlwaysActivePanel.gameObject.SetActive(!Console.gameObject.activeSelf);
            if (Console.gameObject.activeSelf)
            {
                GadgetCoreAPI.FreezeInput("Gadget Console Open");
                wasOpen = true;
                Console.InputField.ActivateInputField();
                Console.InputField.Select();
                historyIndex = messageHistory.Count;
            }
            else
            {
                GadgetCoreAPI.DelayUnfreezeInput("Gadget Console Open");
                wasOpen = false;
                Console.InputField.text = "";
                Console.InputField.DeactivateInputField();
            }
        }

        /// <summary>
        /// Returns a value indicating whether the console is currently open.
        /// </summary>
        public static bool IsOpen()
        {
            return Console.gameObject.activeSelf;
        }
        /// <summary>
        /// Returns a value indicating whether the console is was open one frame ago.
        /// </summary>
        public static bool WasOpen()
        {
            return wasOpen;
        }

        /// <summary>
        /// Dealiases the given command.
        /// </summary>
        public static string DealiasCommand(string command)
        {
            return commandAliases.TryGetValue(command, out string dialiasedCommand) ? dialiasedCommand : command;
        }

        /// <summary>
        /// Returns whether the given command is on the /execute blacklist.
        /// </summary>
        public static bool IsCommandExecuteBlacklisted(string command)
        {
            return executeBlacklist.Contains(DealiasCommand(command));
        }

        /// <summary>
        /// Registers a command. If <paramref name="operatorOnly"/> is true, then only operators will be able to execute this command. If <paramref name="allowExecute"/> is false, then the /execute command cannot be used to force a player to run this command.
        /// </summary>
        public static void RegisterCommand(string name, bool operatorOnly, bool allowExecute, ConsoleCommand command, string helpDesc, string fullHelp = null, params string[] aliases)
        {
            if (!allowExecute)
            {
                executeBlacklist.Add(name);
                foreach (string alias in aliases) executeBlacklist.Add(alias);
            }
            RegisterCommand(name, operatorOnly, command, helpDesc, fullHelp, aliases);
        }

        /// <summary>
        /// Registers a command. If <paramref name="operatorOnly"/> is true, then only operators will be able to execute this command.
        /// </summary>
        public static void RegisterCommand(string name, bool operatorOnly, ConsoleCommand command, string helpDesc, string fullHelp = null, params string[] aliases)
        {
            if (!Registry.registeringVanilla && Registry.gadgetRegistering < 0) throw new InvalidOperationException("Command registration may only be performed by the Initialize method of a Gadget!");
            name = name.ToLowerInvariant();
            if (commands.ContainsKey(name))
            {
                GadgetCore.CoreLogger.LogWarning("A command had already been registered with the name " + name + ", but it has been overwritten by another mod!");
            }
            if (name == "all") throw new InvalidOperationException(name + " is a reserved command name!");
            if (name.All(x => x >= '0' && x <= '9')) throw new InvalidOperationException("A command name may not be a number!");
            commands[name] = command;
            if (!gadgetCommands.ContainsKey(Registry.gadgetRegistering)) gadgetCommands.Add(Registry.gadgetRegistering, new List<string>());
            if (!gadgetCommands[Registry.gadgetRegistering].Contains(name)) gadgetCommands[Registry.gadgetRegistering].Add(name);
            isOperatorOnly[name] = operatorOnly;
            helpDescriptions[name] = helpDesc;
            if (fullHelp != null) fullHelps[name] = fullHelp;
            commandAliases[name] = name;
            foreach (string alias in aliases) commandAliases[alias.ToLowerInvariant()] = name;
        }

        internal static void UnregisterGadgetCommands(int modID)
        {
            if (gadgetCommands.ContainsKey(modID))
            {
                foreach (string rpc in gadgetCommands[modID])
                {
                    commands.Remove(rpc);
                }
                gadgetCommands.Remove(modID);
            }
        }

        /// <summary>
        /// Prints the given text to the console. Returns the index of the message on the console, which can be used to change or remove it later. Will return -1 if the message is suppressed.
        /// </summary>
        public static int Print(string text, string sender = null, MessageSeverity severity = MessageSeverity.RAW)
        {
            return Print(new GadgetConsoleMessage(text, sender, severity));
        }

        /// <summary>
        /// Prints the given <see cref="GadgetConsoleMessage"/> to the console. Returns the index of the message on the console, which can be used to change or remove it later. Will return -1 if the message is suppressed.
        /// </summary>
        public static int Print(GadgetConsoleMessage message)
        {
            return suppressPrint ? -1 : AddMessage(message);
        }

        /// <summary>
        /// Like <see cref="Print(GadgetConsoleMessage)"/>, but broadcasts the message to every player's console.
        /// </summary>
        public static int BroadcastMessage(GadgetConsoleMessage message)
        {
            return BroadcastMessage(message.Text, message.Sender, message.Severity);
        }

        /// <summary>
        /// Like <see cref="Print(string, string, MessageSeverity)"/>, but broadcasts the message to every player's console. Note that the returned ID can only be used to manipulate the message on this player, not all players.
        /// </summary>
        public static int BroadcastMessage(string text, string sender = null, MessageSeverity severity = MessageSeverity.RAW)
        {
            int ID = Print(text, sender, severity);
            RPCHooks.Singleton.BroadcastConsoleMessage(text, sender, severity, GetMessage(ID).SendTime);
            return ID;
        }

        /// <summary>
        /// Gets the message with the given index. Will return null if no message with that index exists.
        /// </summary>
        public static GadgetConsoleMessage GetMessage(int index)
        {
            return index >= 0 && index < messages.Count ? messages[index] : null;
        }

        /// <summary>
        /// Replaces a previously sent message with a new one. Cannot replace messages that have been removed.
        /// </summary>
        public static void ReplaceMessage(int index, GadgetConsoleMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (index >= 0 && index < messages.Count && messages[index] != null)
            {
                messages[index] = message;
                message.SendTime = Time.realtimeSinceStartup;
                Text textComponent = new GameObject("Message " + messages.IndexOf(message) + ", sent at time: " + message.SendTime, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
                textComponent.rectTransform.SetParent(Console.TextPanel.transform);
                textComponent.rectTransform.anchorMin = new Vector2(0f, 1f);
                textComponent.rectTransform.anchorMax = new Vector2(0f, 1f);
                textComponent.rectTransform.offsetMin = new Vector2(0f, 0f);
                textComponent.rectTransform.offsetMax = new Vector2(0f, 0f);
                textComponent.horizontalOverflow = HorizontalWrapMode.Wrap;
                textComponent.verticalOverflow = VerticalWrapMode.Overflow;
                textComponent.supportRichText = true;
                textComponent.resizeTextForBestFit = false;
                textComponent.alignment = TextAnchor.LowerLeft;
                string messageText = message.GetDisplayedText();
                if (messageText.All(x => x == '\n' || SceneInjector.ModMenuPanel.descText.font.HasCharacter(x)))
                {
                    textComponent.font = SceneInjector.ModMenuPanel.descText.font;
                }
                else
                {
                    textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                }
                textComponent.fontSize = 24;
                textComponent.text = messageText;
                message.Component = textComponent;
            }
        }

        /// <summary>
        /// Permanently removes a message from the console.
        /// </summary>
        public static void RemoveMessage(int index)
        {
            if (index >= 0 && index < messages.Count && messages[index] != null)
            {
                if (messages[index]?.Component != null) Destroy(messages[index].Component.gameObject);
                messages[index] = null;
            }
        }

        /// <summary>
        /// Permanently removes a message using the exact time of its sending. Since message send-times are synchronized across the network, this can be used to remove broadcasts.
        /// </summary>
        public static void RemoveMessage(float sendTime)
        {
            RemoveMessage(messages.FindLastIndex(x => x.SendTime == sendTime));
        }

        /// <summary>
        /// Like <see cref="RemoveMessage(int)"/>, but removes a broadcast from every player's console.
        /// </summary>
        public static void RemoveBroadcast(int index)
        {
            RemoveBroadcast(GetMessage(index).SendTime);
        }

        /// <summary>
        /// Like <see cref="RemoveMessage(float)"/>, but removes a broadcast from every player's console.
        /// </summary>
        public static void RemoveBroadcast(float sendTime)
        {
            RPCHooks.Singleton.RemoveConsoleBroadcast(sendTime);
        }

        private static int AddMessage(GadgetConsoleMessage message)
        {
            if (message == null) return -1;
            if (!Debug && message.Severity == MessageSeverity.DEBUG) return -1;
            if (Console == null || !Thread.CurrentThread.Equals(mainThread))
            {
                lock (queuedMessages)
                {
                    queuedMessages.Add(message);
                }
                return -1;
            }
            if (messages.Contains(message))
            {
                if (messages[messages.IndexOf(message)]?.Component != null) Destroy(messages[messages.IndexOf(message)].Component.gameObject);
                messages[messages.IndexOf(message)] = null;
            }
            for (int i = 0;i < 2;i++)
            {
                message.SendTime = Time.realtimeSinceStartup;
                Text textComponent = new GameObject("Message " + messages.IndexOf(message) + ", sent at time: " + message.SendTime, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
                textComponent.rectTransform.SetParent(i == 0 ? Console.TextPanel.transform : Console.AlwaysActivePanel.transform);
                textComponent.rectTransform.anchorMin = new Vector2(0f, 1f);
                textComponent.rectTransform.anchorMax = new Vector2(0f, 1f);
                textComponent.rectTransform.offsetMin = new Vector2(0f, 0f);
                textComponent.rectTransform.offsetMax = new Vector2(0f, 0f);
                textComponent.raycastTarget = i == 0;
                textComponent.horizontalOverflow = HorizontalWrapMode.Wrap;
                textComponent.verticalOverflow = VerticalWrapMode.Overflow;
                textComponent.supportRichText = true;
                textComponent.resizeTextForBestFit = false;
                textComponent.alignment = TextAnchor.LowerLeft;
                string messageText = message.GetDisplayedText();
                if (messageText.All(x => x == '\n' || SceneInjector.ModMenuPanel.descText.font.HasCharacter(x)))
                {
                    textComponent.font = SceneInjector.ModMenuPanel.descText.font;
                }
                else
                {
                    textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                }
                textComponent.fontSize = 24;
                textComponent.text = messageText;
                if (i == 0)
                {
                    message.Component = textComponent;
                    messages.Add(message);
                }
                else
                {
                    Destroy(textComponent.gameObject, 5);
                }
            }
            if (IsOpen())
            {
                Camera.main.GetComponent<AudioSource>()?.PlayOneShot((AudioClip)Resources.Load("au/click2"), Menuu.soundLevel / 10f);
            }
            else
            {
                Camera.main.GetComponent<AudioSource>()?.PlayOneShot((AudioClip)Resources.Load("au/pickup"), Menuu.soundLevel / 10f);
            }
            return messages.Count - 1;
        }

        internal static IEnumerator PrintQueuedMessages()
        {
            while (true)
            {
                if (Console == null) yield return new WaitUntil(() => Console != null);
                lock (queuedMessages)
                {
                    if (queuedMessages.Count > 0)
                    {
                        foreach (GadgetConsoleMessage message in queuedMessages)
                        {
                            AddMessage(message);
                        }
                        queuedMessages.Clear();
                    }
                }
                yield return new WaitForEndOfFrame();
            }
        }

        /// <summary>
        /// Sends a message to the console, as if it were entered by the player. Can trigger commands. If <paramref name="printCommandFeedback"/> is false, then response feedback from commands will be supressed.
        /// </summary>
        public static int SendConsoleMessage(string message, string sender, bool printCommandFeedback = true)
        {
            return SendConsoleMessage(message, sender, printCommandFeedback, false);
        }

        /// <summary>
        /// Sends a message to the console, as if it were entered by the player. Can trigger commands. If <paramref name="printCommandFeedback"/> is false, then response feedback from commands will be supressed.
        /// </summary>
        public static int SendConsoleMessage(string message, string sender, bool printCommandFeedback, bool force)
        {
            if (!string.IsNullOrEmpty(message))
            {
                if (message.Length > 1 && message[0] == '/')
                {
                    string[] args = ParseArgs(message.Substring(1));
                    if (commandAliases.ContainsKey(args[0]))
                    {
                        if (force || SceneManager.GetActiveScene().buildIndex == 0 || Network.isServer || !isOperatorOnly[commandAliases[args[0]]] || operators.Contains(Menuu.curName))
                        {
                            GadgetConsoleMessage feedback;
                            try
                            {
                                if (!printCommandFeedback) suppressPrint = true;
                                feedback = commands[commandAliases[args[0]]](sender, args);
                            }
                            catch (Exception e)
                            {
                                feedback = new GadgetConsoleMessage("Error executing command: " + e);
                            }
                            finally
                            {
                                suppressPrint = false;
                            }
                            if (printCommandFeedback && feedback != null) return Print(feedback);
                        }
                        else
                        {
                            if (printCommandFeedback) return Print("You do not have permission to use this command!", null, MessageSeverity.ERROR);
                        }
                    }
                    else
                    {
                        if (printCommandFeedback) return Print("`" + args[0] + "` is not a valid command! Enter `/help` for a list of commands.", null, MessageSeverity.ERROR);
                    }
                }
                else
                {
                    if (SceneManager.GetActiveScene().buildIndex == 0)
                    {
                        return Print("You can't chat on the title screen!", null, MessageSeverity.WARN);
                    }
                    Logger.Log("[Chat] <" + sender + "> " + message);
                    return BroadcastMessage(message, sender, MessageSeverity.RAW);
                }
            }
            return -1;
        }

        /// <summary>
        /// Returns whether the given player name is an operator.
        /// </summary>
        public static bool IsOperator(string name)
        {
            return SceneManager.GetActiveScene().buildIndex == 0 || name == Menuu.curName && Network.isServer || operators.Contains(name);
        }

        /// <summary>
        /// Parses the given text as a string of command arguments.
        /// </summary>
        public static string[] ParseArgs(string text)
        {
            List<string> args = new List<string>();
            StringBuilder builder = new StringBuilder();
            bool isInQuotes = false;
            for (int i = 0;i < text.Length;i++)
            {
                if (text[i] == '\\' && i + 1 < text.Length && (text[i + 1] == '\\' || text[i + 1] == '"'))
                {
                    i++;
                    builder.Append(text[i]);
                }
                else if (text[i] == '"')
                {
                    if (i == 0 || text[i - 1] != '\\')
                    {
                        isInQuotes = !isInQuotes;
                    }
                }
                else if (!isInQuotes && text[i] == ' ')
                {
                    args.Add(builder.ToString());
                    builder = new StringBuilder();
                }
                else
                {
                    builder.Append(text[i]);
                }
            }
            if (builder.Length > 0) args.Add(builder.ToString());
            return args.ToArray();
        }

        [SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Unity Method")]
        private void LateUpdate()
        {
            wasSelected = InputField.isFocused;
            wasOpen = Console.gameObject.activeSelf;
        }

        private GadgetConsole()
        {
            TextSubmitAction = () =>
            {
                if (wasSelected)
                {
                    string text = InputField.text;
                    InputField.text = "";
                    InputField.ActivateInputField();
                    InputField.Select();
                    SendConsoleMessage(text, SceneManager.GetActiveScene().buildIndex == 0 ? "User" : Menuu.curName);
                    messageHistory.Add(text);
                    historyIndex = messageHistory.Count;
                }
            };

            HistoryUpAction = () =>
            {
                if (wasSelected)
                {
                    if (historyIndex > 0) historyIndex--;
                    InputField.text = historyIndex < messageHistory.Count ? messageHistory[historyIndex] : "";
                    InputField.selectionAnchorPosition = InputField.text.Length;
                }
            };

            HistoryDownAction = () =>
            {
                if (wasSelected)
                {
                    if (historyIndex < messageHistory.Count) historyIndex++;
                    InputField.text = historyIndex < messageHistory.Count ? messageHistory[historyIndex] : "";
                    InputField.selectionAnchorPosition = InputField.text.Length;
                }
            };

            Console = this;
            GadgetCoreAPI.RegisterKeyDownListener(KeyCode.Return, TextSubmitAction);
            GadgetCoreAPI.RegisterKeyDownListener(KeyCode.UpArrow, HistoryUpAction);
            GadgetCoreAPI.RegisterKeyDownListener(KeyCode.DownArrow, HistoryDownAction);
        }

        [SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Unity Method")]
        private void OnDestroy()
        {
            Console = null;
            GadgetCoreAPI.UnregisterKeyDownListener(KeyCode.Return, TextSubmitAction);
            GadgetCoreAPI.UnregisterKeyDownListener(KeyCode.UpArrow, HistoryUpAction);
            GadgetCoreAPI.UnregisterKeyDownListener(KeyCode.DownArrow, HistoryDownAction);
        }

        /// <summary>
        /// Deligate used for registering console commands. May return a message to send only to the person who ran this command. May return null to print nothing.
        /// </summary>
        /// <param name="sender">The sender for this command. May be null, if triggered directly through a call to <see cref="SendConsoleMessage(string, string, bool)"/></param>
        /// <param name="args">The arguments passed to this command. Note that the first one will always be the name of the command, as it was entered.</param>
        /// <returns>A message to send only to the person who ran this command. May return null to print nothing.</returns>
        public delegate GadgetConsoleMessage ConsoleCommand(string sender, params string[] args);

        /// <summary>
        /// Represents a particular message in the console.
        /// </summary>
        public sealed class GadgetConsoleMessage
        {
            /// <summary>
            /// The text of this message.
            /// </summary>
            public readonly string Text;
            /// <summary>
            /// The sender of this message. May be null.
            /// </summary>
            public readonly string Sender;
            /// <summary>
            /// The severity of this message.
            /// </summary>
            public readonly MessageSeverity Severity;
            /// <summary>
            /// The age, in seconds, of this message. Only starts incrementing once it has been posted to the console.
            /// </summary>
            public float Age
            {
                get
                {
                    return SendTime > -1 ? (Time.realtimeSinceStartup - SendTime) : -1;
                }
            }
            /// <summary>
            /// The time, in seconds since startup, since that message was sent.
            /// </summary>
            public float SendTime { get; internal set; } = -1;
            /// <summary>
            /// The <see cref="Text"/> component used to display this message in the console. Will be null until the message has been printed.
            /// </summary>
            public Text Component { get; internal set; }

            /// <summary>
            /// Constructs a new message just from the given text.
            /// </summary>
            public GadgetConsoleMessage(string text, string sender = null, MessageSeverity severity = MessageSeverity.RAW)
            {
                Text = text;
                Sender = sender;
                Severity = severity;
            }

            /// <summary>
            /// Builds the resultant text string when considering the severity, sender, and content.
            /// </summary>
            public string GetDisplayedText()
            {
                StringBuilder builder = new StringBuilder();
                switch (Severity)
                {
                    case MessageSeverity.DEBUG:
                        builder.Append("<color=grey>[Debug] ");
                        break;
                    case MessageSeverity.INFO:
                        builder.Append("<color=white>[Info] ");
                        break;
                    case MessageSeverity.WARN:
                        builder.Append("<color=yellow>[Warning] ");
                        break;
                    case MessageSeverity.ERROR:
                        builder.Append("<color=red>[Error] ");
                        break;
                }
                if (Sender != null) builder.Append("<" + Sender + "> ");
                builder.Append(Text);
                switch (Severity)
                {
                    case MessageSeverity.DEBUG:
                        builder.Append("</color>");
                        break;
                    case MessageSeverity.INFO:
                        builder.Append("</color>");
                        break;
                    case MessageSeverity.WARN:
                        builder.Append("</color>");
                        break;
                    case MessageSeverity.ERROR:
                        builder.Append("</color>");
                        break;
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// Generates a <see cref="GadgetConsoleMessage"/> representative of a standard command syntax error. Return this when the user uses your command wrong.
        /// Pass args[0] to <paramref name="command"/> to cause the error to display the alias that the user used.
        /// </summary>
        public static GadgetConsoleMessage CommandSyntaxError(string command, string correctSyntax)
        {
            return new GadgetConsoleMessage("Invalid command syntax! Syntax: /" + command.ToLowerInvariant() + " " + correctSyntax, null, MessageSeverity.ERROR);
        }

        static GadgetConsole()
        {
            bool wasRegisteringVanilla = Registry.registeringVanilla;
            int wasModRegistering = Registry.gadgetRegistering;
            Registry.registeringVanilla = true;
            Registry.gadgetRegistering = -1;

            RegisterCommand("help", false, false, CoreCommands.Help,
                "Provides the help page you are seeing right now.",
                "Can list available commands, as well as provide extended information about specific commands.\nUses the syntax: /help <page/command>",
                "h");
            RegisterCommand("op", true, false, CoreCommands.Op,
                "Grants operator permissions to a given player.",
                "Grants operator permissions to a given player. These permissions only last as long as the multiplayer session continues.\nThis command, along with /deop, can only be used by the game host.\nUses the syntax: /op <player>");
            RegisterCommand("deop", true, false, CoreCommands.Deop,
                "Removes operator permissions from a given player.",
                "Removes operator permissions from a given player. These permissions only last as long as the multiplayer session continues.\nThis command, along with /op, can only be used by the game host.\nUses the syntax: /deop <player>");
            RegisterCommand("give", true, true, CoreCommands.Give,
                "Used to spawn items into your or another player's inventory.",
                "Used to spawn items into your or another player's inventory. Note that you can set multiple properties,\nand they work like, I.E.: \"tier=3\", where 3 represents legendary.\nUses the syntax: /give [player] <item name/id> [quantity] [property]=[value]...",
                "g");
            RegisterCommand("givechip", true, true, CoreCommands.GiveChip,
                "Used to spawn chips into your or another player's inventory.",
                "Used to spawn chips into your or another player's inventory.\nUses the syntax: /givechip [player] <chip name/id>",
                "gc");
            RegisterCommand("giveexp", true, true, CoreCommands.GiveExp,
                "Used to give yourself exp.",
                "Used to give yourself exp for either your character, or your gear.\nSuffix the exp amount with 'L' to give an amount of levels rather than exp points.\nUses the syntax: /giveexp <character/weapon/offhand/helmet/armor/ring/droid> <amount>",
                "exp", "gxp", "givexp");
            RegisterCommand("giveportals", true, true, CoreCommands.GivePortals,
                "Used to give yourself portal uses.",
                "Used to give yourself portal uses.\nThe keyword 'Infinite' can be used instead of an amount as well - however, this is permanent and cannot be undone.\nUses the syntax: /giveexp <planet id> <amount>",
                "gp", "giveportal");
            RegisterCommand("reloadmod", true, false, CoreCommands.ReloadMod,
                "Reloads the specified mod.",
                "Reloads the specified mod. Will ask for confirmation, and lists other mods that will be reloaded as a consequence.\nUses the syntax: /reloadmod <mod>",
                "rlm");
            RegisterCommand("reloadgadget", true, false, CoreCommands.ReloadGadget,
                "Reloads the specified Gadget.",
                "Reloads the specified Gadget. Will ask for confirmation, and lists other Gadgets that will be reloaded as a consequence.\nUses the syntax: /reloadgadget <gadget>",
                "rlg");
            RegisterCommand("debugmode", true, false, CoreCommands.DebugMode,
                "Toggles debug mode.",
                "Toggles the game's built-in debug mode. Use with caution.\nUses the syntax: /debugmode",
                "debug");
            RegisterCommand("githublogin", true, false, CoreCommands.GitHubLogin,
                "Logs into or out of GitHub, for use in the Mod Browser.",
                "Logs into or out of GitHub, for use in the Mod Browser. Uses a Personal Access Token, created on this page:\nhttps://github.com/settings/tokens/new\nUses the syntax: /githublogin [auth token]",
                "ghl");
            RegisterCommand("spawnentity", true, true, CoreCommands.SpawnEntity,
                "Spawns an entity into the world.",
                "Spawns an entity into the world. Optionally accepts X and Y offsets to specify where to spawn the entity relative to the player's position.\nUses the syntax: /spawnentity <entity> [x offset] [y offset]",
                "spawn");
            RegisterCommand("godmode", true, true, CoreCommands.GodMode,
                "Toggles god mode.",
                "Toggles god mode. When active, your HP will be set to max every frame, thereby making it theoretically impossible to die.\nUses the syntax: /godmode",
                "tgm", "god");
            RegisterCommand("execute", true, false, CoreCommands.Execute,
                "Forces another player to execute a command.",
                "Forces another player to execute a command. Some commands are blacklisted, such as the /reflect command.\nIf the affected player is an operator, then they will be notified that you used the command on them.\nUses the syntax: /execute <player> <command> [args]...",
                "ex", "exe");
            RegisterCommand("reflect", true, false, CoreCommands.Reflect,
                "Console utility for performing some simple arbitrary Reflection at runtime.",
                "Console utility for performing some simple arbitrary Reflection at runtime, such as by monitoring the value of a field. Use /reflect help to see available modes of the command.\nUses the syntax: /reflect <mode> [parameters]...",
                "refl", "reflector");

            Registry.registeringVanilla = wasRegisteringVanilla;
            Registry.gadgetRegistering = wasModRegistering;
        }

        /// <summary>
        /// This class contains all of the standard GadgetCore commands.
        /// </summary>
        public static class CoreCommands
        {
            /// <summary>
            /// The /help command
            /// </summary>
            public static GadgetConsoleMessage Help(string sender, params string[] args)
            {
                if (args.Length > 2) return CommandSyntaxError(args[0], "<page/command>");
                string arg = args.Length > 1 ? args[1] : "1";
                if (int.TryParse(arg, out int page))
                {
                    page = Math.Max(page, 1);
                    if ((page - 1) * 10 < commands.Count)
                    {
                        StringBuilder response = new StringBuilder("Showing page " + page + " of " + ((commands.Count + 9) / 10) + ":\n");
                        List<string> sortedNames = commands.Keys.OrderBy(x => x).ToList();
                        for (int i = (page - 1) * 10;i < Math.Min(page * 10, commands.Count);i++)
                        {
                            response.Append("/" + sortedNames[i] + ": " + (helpDescriptions.ContainsKey(sortedNames[i]) && helpDescriptions[sortedNames[i]] != null ? helpDescriptions[sortedNames[i]] : "This command has no description") + (isOperatorOnly[sortedNames[i]] ? " (Operator Only)\n" : "\n"));
                        }
                        response.Append("Use `/help <page>` to display a given page of commands, or use `/help all` to display all commands at once.");
                        return new GadgetConsoleMessage(response.ToString(), null);
                    }
                    else
                    {
                        return new GadgetConsoleMessage("There is no page " + page + "!", null, MessageSeverity.WARN);
                    }
                }
                else
                {
                    arg = arg.ToLowerInvariant();
                    if (arg == "all")
                    {
                        StringBuilder response = new StringBuilder("Showing all commands:\n");
                        List<string> sortedNames = commands.Keys.OrderBy(x => x).ToList();
                        for (int i = 0; i < commands.Count; i++)
                        {
                            response.Append("/" + sortedNames[i] + ": " + (helpDescriptions.ContainsKey(sortedNames[i]) && helpDescriptions[sortedNames[i]] != null ? helpDescriptions[sortedNames[i]] : "This command has no description") + (isOperatorOnly[sortedNames[i]] ? " (Operator Only)\n" : "\n"));
                        }
                        response.Append("Use `/help <page>` to display a given page of commands, or use `/help all` to display all commands at once.");
                        return new GadgetConsoleMessage(response.ToString(), null);
                    }
                    else
                    {
                        if (commandAliases.ContainsKey(arg))
                        {
                            string command = commandAliases[arg];
                            return new GadgetConsoleMessage("Showing help for command: `" + command + "`: " + (commandAliases.Count > 1 ? "(Aliases: " + commandAliases.Where(x => x.Key != command && x.Value == command).Select(x => x.Key).Concat(",") + ")\n\n" : "\n\n") + fullHelps[command], null);
                        }
                        else
                        {
                            return new GadgetConsoleMessage("There is no command with the name `" + arg + "`!", null, MessageSeverity.WARN);
                        }
                    }
                }
            }

            /// <summary>
            /// The /op command
            /// </summary>
            public static GadgetConsoleMessage Op(string sender, params string[] args)
            {
                if (!Network.isServer) return new GadgetConsoleMessage("This command can only be used by the server host!", null, MessageSeverity.ERROR);
                if (args.Length != 2) return CommandSyntaxError(args[0], "<player>");
                if (GadgetCoreAPI.GetPlayerByName(args[1]) == null)
                {
                    return new GadgetConsoleMessage("There is no player with the name `" + args[1] + "`", null, MessageSeverity.ERROR);
                }
                RPCHooks.Singleton.SetOp(args[1], true);
                return new GadgetConsoleMessage("Granted operator status to " + args[1]);
            }

            /// <summary>
            /// The /deop command
            /// </summary>
            public static GadgetConsoleMessage Deop(string sender, params string[] args)
            {
                if (!Network.isServer) return new GadgetConsoleMessage("This command can only be used by the server host!", null, MessageSeverity.ERROR);
                if (args.Length != 2) return CommandSyntaxError(args[0], "<player>");
                if (GadgetCoreAPI.GetPlayerByName(args[1]) == null)
                {
                    return new GadgetConsoleMessage("There is no player with the name `" + args[1] + "`", null, MessageSeverity.ERROR);
                }
                RPCHooks.Singleton.SetOp(args[1], false);
                return new GadgetConsoleMessage("Removed operator status from " + args[1]);
            }

            /// <summary>
            /// The /give command
            /// </summary>
            public static GadgetConsoleMessage Give(string sender, params string[] args)
            {
                if (InstanceTracker.PlayerScript == null) return new GadgetConsoleMessage("This command may only be used in-game!", null, MessageSeverity.ERROR);
                if (args.Length < 2) return CommandSyntaxError(args[0], "[player] <item name/id> [quantity] [property]=[value]...");
                string player;
                string itemName;
                int quantity = 1;
                int exp = 0;
                int tier = 0;
                int corrupted = 0;
                int[] aspect = new int[] { 0, 0, 0 };
                int[] aspectLvl = new int[] { 0, 0, 0 };
                Dictionary<string, object> extraData = new Dictionary<string, object>();
                if (args.Length == 2)
                {
                    player = sender;
                    itemName = args[1];
                }
                else if (args.Length == 3)
                {
                    if (GadgetCoreAPI.GetPlayerByName(args[1]) != null)
                    {
                        player = args[1];
                        itemName = args[2];
                    }
                    else
                    {
                        player = sender;
                        itemName = args[1];
                        if (!int.TryParse(args[2], out quantity))
                        {
                            return new GadgetConsoleMessage("`" + args[2] + "` is not a valid number!", null, MessageSeverity.ERROR);
                        }
                    }
                }
                else
                {
                    bool isArgPlayer = GadgetCoreAPI.GetPlayerByName(args[1]) != null;
                    if (isArgPlayer)
                    {
                        player = args[1];
                        itemName = args[2];
                        if (!int.TryParse(args[3], out quantity))
                        {
                            return new GadgetConsoleMessage("`" + args[2] + "` is not a valid number!", null, MessageSeverity.ERROR);
                        }
                    }
                    else
                    {
                        player = sender;
                        itemName = args[1];
                        if (!int.TryParse(args[2], out quantity))
                        {
                            return new GadgetConsoleMessage("`" + args[2] + "` is not a valid number!", null, MessageSeverity.ERROR);
                        }
                    }
                    for (int i = isArgPlayer ? 4 : 3;i < args.Length;i++)
                    {
                        string[] splitProperty = args[i].Split(new char[] { '=' }, 2);
                        if (splitProperty.Length != 2)
                        {
                            return new GadgetConsoleMessage("Cannot parse property: " + args[i], null, MessageSeverity.ERROR);
                        }
                        switch (splitProperty[0].ToLowerInvariant())
                        {
                            case "exp":
                                if (!int.TryParse(splitProperty[1], out exp))
                                {
                                    return new GadgetConsoleMessage("`" + splitProperty[1] + "` is not a valid number!", null, MessageSeverity.ERROR);
                                }
                                break;
                            case "tier":
                                if (!int.TryParse(splitProperty[1], out tier))
                                {
                                    return new GadgetConsoleMessage("`" + splitProperty[1] + "` is not a valid number!", null, MessageSeverity.ERROR);
                                }
                                break;
                            case "corrupt":
                            case "corrupted":
                                if (!int.TryParse(splitProperty[1], out corrupted))
                                {
                                    return new GadgetConsoleMessage("`" + splitProperty[1] + "` is not a valid number!", null, MessageSeverity.ERROR);
                                }
                                break;
                            case "aspect1":
                            case "mod1":
                                if (!int.TryParse(splitProperty[1], out aspect[0]))
                                {
                                    return new GadgetConsoleMessage("`" + splitProperty[1] + "` is not a valid number!", null, MessageSeverity.ERROR);
                                }
                                break;
                            case "aspect2":
                            case "mod2":
                                if (!int.TryParse(splitProperty[1], out aspect[1]))
                                {
                                    return new GadgetConsoleMessage("`" + splitProperty[1] + "` is not a valid number!", null, MessageSeverity.ERROR);
                                }
                                break;
                            case "aspect3":
                            case "mod3":
                                if (!int.TryParse(splitProperty[1], out aspect[2]))
                                {
                                    return new GadgetConsoleMessage("`" + splitProperty[1] + "` is not a valid number!", null, MessageSeverity.ERROR);
                                }
                                break;
                            case "aspect1lvl":
                            case "mod1lvl":
                            case "aspect1count":
                            case "mod1count":
                                if (!int.TryParse(splitProperty[1], out aspectLvl[0]))
                                {
                                    return new GadgetConsoleMessage("`" + splitProperty[1] + "` is not a valid number!", null, MessageSeverity.ERROR);
                                }
                                break;
                            case "aspect2lvl":
                            case "mod2lvl":
                            case "aspect2count":
                            case "mod2count":
                                if (!int.TryParse(splitProperty[1], out aspectLvl[1]))
                                {
                                    return new GadgetConsoleMessage("`" + splitProperty[1] + "` is not a valid number!", null, MessageSeverity.ERROR);
                                }
                                break;
                            case "aspect3lvl":
                            case "mod3lvl":
                            case "aspect3count":
                            case "mod3count":
                                if (!int.TryParse(splitProperty[1], out aspectLvl[2]))
                                {
                                    return new GadgetConsoleMessage("`" + splitProperty[1] + "` is not a valid number!", null, MessageSeverity.ERROR);
                                }
                                break;
                            default:
                                if (!extraData.ContainsKey(splitProperty[0]))
                                {
                                    if (bool.TryParse(splitProperty[1], out bool boolValue))
                                    {
                                        extraData.Add(splitProperty[0], boolValue);
                                    }
                                    else if (int.TryParse(splitProperty[1], out int intValue))
                                    {
                                        extraData.Add(splitProperty[0], intValue);
                                    }
                                    else if (long.TryParse(splitProperty[1], out long longValue))
                                    {
                                        extraData.Add(splitProperty[0], longValue);
                                    }
                                    else if (float.TryParse(splitProperty[1], out float floatValue))
                                    {
                                        extraData.Add(splitProperty[0], floatValue);
                                    }
                                    else if (double.TryParse(splitProperty[1], out double doubleValue))
                                    {
                                        extraData.Add(splitProperty[0], doubleValue);
                                    }
                                    else
                                    {
                                        extraData.Add(splitProperty[0], splitProperty[1]);
                                    }
                                }
                                else
                                {
                                    return new GadgetConsoleMessage("Cannot set the value of the property `" + splitProperty[0] + "` multiple times!", null, MessageSeverity.ERROR);
                                }
                                break;
                        }
                    }
                }
                PlayerScript playerScript = GadgetCoreAPI.GetPlayerByName(player);
                if (playerScript == null)
                {
                    return new GadgetConsoleMessage("There is no player with the name `" + player + "`", null, MessageSeverity.ERROR);
                }
                if (int.TryParse(itemName, out int id))
                {
                    if (string.IsNullOrEmpty(GadgetCoreAPI.GetItemName(id)))
                    {
                        return new GadgetConsoleMessage("There is no item with the ID `" + id + "`", null, MessageSeverity.ERROR);
                    }
                }
                else
                {
                    id = ItemRegistry.GetItemIDByName(itemName);
                    if (id == -1)
                    {
                        id = ItemRegistry.Singleton[itemName]?.GetID() ?? -1;
                    }
                    if (id == -1)
                    {
                        return new GadgetConsoleMessage("There is no item with the name `" + itemName + "`", null, MessageSeverity.ERROR);
                    }
                }
                Item item = new Item(id, quantity, exp, tier, corrupted, aspect, aspectLvl);
                item.SetAllExtraData(extraData);
                RPCHooks.Singleton.GiveItem(item, playerScript.GetComponent<NetworkView>().owner);
                return new GadgetConsoleMessage("Given " + quantity + " `" + GadgetCoreAPI.GetItemName(id) + "` to " + player);
            }

            /// <summary>
            /// The /givechip command
            /// </summary>
            public static GadgetConsoleMessage GiveChip(string sender, params string[] args)
            {
                if (InstanceTracker.PlayerScript == null) return new GadgetConsoleMessage("This command may only be used in-game!", null, MessageSeverity.ERROR);
                if (args.Length < 2) return CommandSyntaxError(args[0], "[player] <chip name/id>");
                string player;
                string itemName;
                if (args.Length == 2)
                {
                    player = sender;
                    itemName = args[1];
                }
                else if (args.Length == 3)
                {
                    player = args[1];
                    itemName = args[2];
                }
                else
                {
                    return CommandSyntaxError(args[0], "[player] <chip name/id>");
                }
                PlayerScript playerScript = GadgetCoreAPI.GetPlayerByName(player);
                if (playerScript == null)
                {
                    return new GadgetConsoleMessage("There is no player with the name `" + player + "`", null, MessageSeverity.ERROR);
                }
                if (int.TryParse(itemName, out int id))
                {
                    if (string.IsNullOrEmpty(GadgetCoreAPI.GetChipName(id)))
                    {
                        return new GadgetConsoleMessage("There is no chip with the ID `" + id + "`", null, MessageSeverity.ERROR);
                    }
                }
                else
                {
                    id = ChipRegistry.GetChipIDByName(itemName);
                    if (id == -1)
                    {
                        id = ChipRegistry.Singleton[itemName]?.GetID() ?? -1;
                    }
                    if (id == -1)
                    {
                        return new GadgetConsoleMessage("There is no chip with the name `" + itemName + "`", null, MessageSeverity.ERROR);
                    }
                }
                Item item = new Item(id, 0, 0, 0, 0, new int[3], new int[3]);
                RPCHooks.Singleton.GiveChip(item, playerScript.GetComponent<NetworkView>().owner);
                return new GadgetConsoleMessage("Given a `" + GadgetCoreAPI.GetChipName(id) + "` to " + player);
            }

            /// <summary>
            /// The /giveexp command
            /// </summary>
            public static GadgetConsoleMessage GiveExp(string sender, params string[] args)
            {
                if (InstanceTracker.PlayerScript == null) return new GadgetConsoleMessage("This command may only be used in-game!", null, MessageSeverity.ERROR);
                if (args.Length != 3) return CommandSyntaxError(args[0], "<player/weapon/offhand/helmet/armor/rings/droids> <amount>");
                string amountString = args[2];
                bool isLevels = false;
                if (amountString[amountString.Length - 1] == 'L')
                {
                    amountString = amountString.Substring(0, amountString.Length - 1);
                    isLevels = true;
                }
                if (int.TryParse(amountString, out int amount))
                {
                    string typeName;
                    Item gearItem;
                    switch (args[1][0])
                    {
                        case 'p':
                            if (!args[1].Equals("player".Substring(0, args[1].Length), StringComparison.OrdinalIgnoreCase)) return new GadgetConsoleMessage($"`{args[1]}` is not a valid exp target!", null, MessageSeverity.ERROR);
                            typeName = "player";
                            InstanceTracker.GameScript.AddExp(isLevels ? GadgetCoreAPI.GetPlayerExp(GameScript.playerLevel + amount) - GadgetCoreAPI.GetPlayerExp(GameScript.playerLevel) : amount);
                            break;
                        case 'c':
                            if (!args[1].Equals("character".Substring(0, args[1].Length), StringComparison.OrdinalIgnoreCase)) return new GadgetConsoleMessage($"`{args[1]}` is not a valid exp target!", null, MessageSeverity.ERROR);
                            typeName = "character";
                            InstanceTracker.GameScript.AddExp(isLevels ? GadgetCoreAPI.GetPlayerExp(GameScript.playerLevel + amount) - GadgetCoreAPI.GetPlayerExp(GameScript.playerLevel) : amount);
                            break;
                        case 'w':
                            if (!args[1].Equals("weapon".Substring(0, args[1].Length), StringComparison.OrdinalIgnoreCase)) return new GadgetConsoleMessage($"`{args[1]}` is not a valid exp target!", null, MessageSeverity.ERROR);
                            typeName = "weapon";
                            gearItem = GadgetCoreAPI.GetInventory()[36];
                            Patches.Patch_GameScript_GetItemLevel.SpoofLevel(GadgetCoreAPI.GetGearLevel(gearItem));
                            if (gearItem != null && gearItem.id != 0)
                            {
                                int gearLevel = GadgetCoreAPI.GetGearLevel(gearItem.exp, -1);
                                gearItem.exp += isLevels ? GadgetCoreAPI.GetGearExp(gearLevel + amount) - GadgetCoreAPI.GetGearExp(gearLevel) : amount;
                            }
                            InstanceTracker.GameScript.EXPGEAR(new int[] { 0, 0 });
                            break;
                        case 'o':
                            if (!args[1].Equals("offhand".Substring(0, args[1].Length), StringComparison.OrdinalIgnoreCase)) return new GadgetConsoleMessage($"`{args[1]}` is not a valid exp target!", null, MessageSeverity.ERROR);
                            typeName = "offhand";
                            gearItem = GadgetCoreAPI.GetInventory()[37];
                            Patches.Patch_GameScript_GetItemLevel.SpoofLevel(GadgetCoreAPI.GetGearLevel(gearItem));
                            if (gearItem != null && gearItem.id != 0)
                            {
                                int gearLevel = GadgetCoreAPI.GetGearLevel(gearItem.exp, -1);
                                gearItem.exp += isLevels ? GadgetCoreAPI.GetGearExp(gearLevel + amount) - GadgetCoreAPI.GetGearExp(gearLevel) : amount;
                            }
                            InstanceTracker.GameScript.EXPGEAR(new int[] { 1, 0 });
                            break;
                        case 'h':
                            if (!args[1].Equals("helmet".Substring(0, args[1].Length), StringComparison.OrdinalIgnoreCase)) return new GadgetConsoleMessage($"`{args[1]}` is not a valid exp target!", null, MessageSeverity.ERROR);
                            typeName = "helmet";
                            gearItem = GadgetCoreAPI.GetInventory()[38];
                            Patches.Patch_GameScript_GetItemLevel.SpoofLevel(GadgetCoreAPI.GetGearLevel(gearItem));
                            if (gearItem != null && gearItem.id != 0)
                            {
                                int gearLevel = GadgetCoreAPI.GetGearLevel(gearItem.exp, -1);
                                gearItem.exp += isLevels ? GadgetCoreAPI.GetGearExp(gearLevel + amount) - GadgetCoreAPI.GetGearExp(gearLevel) : amount;
                            }
                            InstanceTracker.GameScript.EXPGEAR(new int[] { 2, 0 });
                            break;
                        case 'a':
                            if (!args[1].Equals("armor".Substring(0, args[1].Length), StringComparison.OrdinalIgnoreCase)) return new GadgetConsoleMessage($"`{args[1]}` is not a valid exp target!", null, MessageSeverity.ERROR);
                            typeName = "armor";
                            gearItem = GadgetCoreAPI.GetInventory()[39];
                            Patches.Patch_GameScript_GetItemLevel.SpoofLevel(GadgetCoreAPI.GetGearLevel(gearItem));
                            if (gearItem != null && gearItem.id != 0)
                            {
                                int gearLevel = GadgetCoreAPI.GetGearLevel(gearItem.exp, -1);
                                gearItem.exp += isLevels ? GadgetCoreAPI.GetGearExp(gearLevel + amount) - GadgetCoreAPI.GetGearExp(gearLevel) : amount;
                            }
                            InstanceTracker.GameScript.EXPGEAR(new int[] { 3, 0 });
                            break;
                        case 'r':
                            if (!args[1].Equals("rings".Substring(0, args[1].Length), StringComparison.OrdinalIgnoreCase)) return new GadgetConsoleMessage($"`{args[1]}` is not a valid exp target!", null, MessageSeverity.ERROR);
                            typeName = "ring";
                            gearItem = GadgetCoreAPI.GetInventory()[40];
                            Patches.Patch_GameScript_GetItemLevel.SpoofLevel(GadgetCoreAPI.GetGearLevel(gearItem));
                            if (gearItem != null && gearItem.id != 0)
                            {
                                int gearLevel = GadgetCoreAPI.GetGearLevel(gearItem.exp, -1);
                                gearItem.exp += isLevels ? GadgetCoreAPI.GetGearExp(gearLevel + amount) - GadgetCoreAPI.GetGearExp(gearLevel) : amount;
                            }
                            gearItem = GadgetCoreAPI.GetInventory()[41];
                            if (gearItem != null && gearItem.id != 0)
                            {
                                Patches.Patch_GameScript_GetItemLevel.SpoofLevel(GadgetCoreAPI.GetGearLevel(gearItem));
                                int gearLevel = GadgetCoreAPI.GetGearLevel(gearItem.exp, -1);
                                gearItem.exp += isLevels ? GadgetCoreAPI.GetGearExp(gearLevel + amount) - GadgetCoreAPI.GetGearExp(gearLevel) : amount;
                            }
                            InstanceTracker.GameScript.EXPGEAR(new int[] { 4, 0 });
                            break;
                        case 'd':
                            if (!args[1].Equals("droids".Substring(0, args[1].Length), StringComparison.OrdinalIgnoreCase)) return new GadgetConsoleMessage($"`{args[1]}` is not a valid exp target!", null, MessageSeverity.ERROR);
                            typeName = "droid";
                            gearItem = GadgetCoreAPI.GetInventory()[42];
                            if (gearItem != null && gearItem.id != 0)
                            {
                                int gearLevel = GadgetCoreAPI.GetGearLevel(gearItem.exp, -1);
                                gearItem.exp += isLevels ? GadgetCoreAPI.GetGearExp(gearLevel + amount) - GadgetCoreAPI.GetGearExp(gearLevel) : amount;
                            }
                            gearItem = GadgetCoreAPI.GetInventory()[43];
                            if (gearItem != null && gearItem.id != 0)
                            {
                                int gearLevel = GadgetCoreAPI.GetGearLevel(gearItem.exp, -1);
                                gearItem.exp += isLevels ? GadgetCoreAPI.GetGearExp(gearLevel + amount) - GadgetCoreAPI.GetGearExp(gearLevel) : amount;
                            }
                            gearItem = GadgetCoreAPI.GetInventory()[44];
                            if (gearItem != null && gearItem.id != 0)
                            {
                                int gearLevel = GadgetCoreAPI.GetGearLevel(gearItem.exp, -1);
                                gearItem.exp += isLevels ? GadgetCoreAPI.GetGearExp(gearLevel + amount) - GadgetCoreAPI.GetGearExp(gearLevel) : amount;
                            }
                            InstanceTracker.GameScript.EXPGEAR(new int[] { -1, 0 });
                            break;
                        default:
                            return new GadgetConsoleMessage($"`{args[1]}` is not a valid exp target!", null, MessageSeverity.ERROR);
                    }
                    return new GadgetConsoleMessage($"Given {(isLevels ? $"{amountString} levels of" : $"{amountString}")} {typeName} exp");
                }
                else
                {
                    return new GadgetConsoleMessage($"`{args[2]}` is not a valid exp amount!", null, MessageSeverity.ERROR);
                }
            }

            /// <summary>
            /// The /giveportals command
            /// </summary>
            public static GadgetConsoleMessage GivePortals(string sender, params string[] args)
            {
                if (InstanceTracker.PlayerScript == null) return new GadgetConsoleMessage("This command may only be used in-game!", null, MessageSeverity.ERROR);
                if (args.Length != 3) return CommandSyntaxError(args[0], "<planet id> <amount>");
                if (int.TryParse(args[1], out int planetID))
                {
                    if (string.IsNullOrEmpty(InstanceTracker.GameScript.GetPlanetName(planetID)))
                    {
                        return new GadgetConsoleMessage("There is no planet with the ID `" + planetID + "`", null, MessageSeverity.ERROR);
                    }
                }
                else
                {
                    planetID = PlanetRegistry.GetPlanetIDByName(args[1]);
                    if (planetID == -1)
                    {
                        planetID = PlanetRegistry.Singleton[args[1]]?.GetID() ?? -1;
                    }
                    if (planetID == -1)
                    {
                        return new GadgetConsoleMessage("There is no planet with the name `" + args[1] + "`", null, MessageSeverity.ERROR);
                    }
                }
                if (args[2].Equals("Infinite", StringComparison.OrdinalIgnoreCase) || args[2].Equals("Unlimited", StringComparison.OrdinalIgnoreCase))
                {
                    if (PlanetRegistry.Singleton[planetID] is PlanetInfo planet)
                    {
                        planet.PortalUses = -1;
                        planet.Relics = Math.Max(planet.Relics, 100);
                        PreviewLabs.PlayerPrefs.SetInt("planetRelics" + planetID, planet.Relics);
                    }
                    else if (planetID >= 0 && planetID < GameScript.planetRelics.Length)
                    {
                        GadgetCoreAPI.GetPortalUses()[planetID] = -1;
                        GameScript.planetRelics[planetID] = Math.Max(GameScript.planetRelics[planetID], 100);
                        PreviewLabs.PlayerPrefs.SetInt("planetRelics" + planetID, GameScript.planetRelics[planetID]);
                    }
                    else return new GadgetConsoleMessage($"{planetID} is not a valid planet ID!", null, MessageSeverity.ERROR);
                    PlanetRegistry.UpdatePlanetSelector();
                    return new GadgetConsoleMessage($"Granted Infinite portal uses to {InstanceTracker.GameScript.GetPlanetName(planetID)}");
                }
                if (args[2].Equals("Finite", StringComparison.OrdinalIgnoreCase) || args[2].Equals("Limited", StringComparison.OrdinalIgnoreCase))
                {
                    if (PlanetRegistry.Singleton[planetID] is PlanetInfo planet)
                    {
                        if (planet.Relics > 99)
                        {
                            planet.PortalUses = 3;
                            planet.Relics -= planet.Relics / 100 * 100;
                            PreviewLabs.PlayerPrefs.SetInt("portalUses" + planetID, planet.PortalUses);
                            PreviewLabs.PlayerPrefs.SetInt("planetRelics" + planetID, planet.Relics);
                        }
                    }
                    else if (planetID >= 0 && planetID < GameScript.planetRelics.Length)
                    {
                        if (GameScript.planetRelics[planetID] > 99)
                        {
                            GadgetCoreAPI.GetPortalUses()[planetID] = 3;
                            GameScript.planetRelics[planetID] -= GameScript.planetRelics[planetID] / 100 - 100;
                            PreviewLabs.PlayerPrefs.SetInt("portalUses" + planetID, planet.PortalUses);
                            PreviewLabs.PlayerPrefs.SetInt("planetRelics" + planetID, GameScript.planetRelics[planetID]);
                        }
                    }
                    else return new GadgetConsoleMessage($"{planetID} is not a valid planet ID!", null, MessageSeverity.ERROR);
                    PlanetRegistry.UpdatePlanetSelector();
                    return new GadgetConsoleMessage($"Granted Finite portal uses to {InstanceTracker.GameScript.GetPlanetName(planetID)}");
                }
                else if (int.TryParse(args[2], out int uses))
                {
                    if (PlanetRegistry.Singleton[planetID] is PlanetInfo planet)
                    {
                        planet.PortalUses += uses;
                        PreviewLabs.PlayerPrefs.SetInt("portalUses" + planetID, planet.PortalUses);
                    }
                    else if (planetID >= 0 && planetID < GadgetCoreAPI.GetPortalUses().Length)
                    {
                        GadgetCoreAPI.GetPortalUses()[planetID] += uses;
                        PreviewLabs.PlayerPrefs.SetInt("portalUses" + planetID, GadgetCoreAPI.GetPortalUses()[planetID]);
                    }
                    else return new GadgetConsoleMessage($"{planetID} is not a valid planet ID!", null, MessageSeverity.ERROR);
                    PlanetRegistry.UpdatePlanetSelector();
                    return new GadgetConsoleMessage($"Granted {uses} portal uses to {InstanceTracker.GameScript.GetPlanetName(planetID)}");
                }
                else return new GadgetConsoleMessage($"{args[2]} is not a valid number!", null, MessageSeverity.ERROR);
            }

            /// <summary>
            /// The /reloadmod command.
            /// </summary>
            public static GadgetConsoleMessage ReloadMod(string sender, params string[] args)
            {
                if (args.Length != 2) return CommandSyntaxError(args[0], "<mod>");
                GadgetMod mod = GadgetMods.GetModByName(args[1]);
                if (mod == null) return new GadgetConsoleMessage("There is no mod with the name `" + args[1] + "`", null, MessageSeverity.ERROR);
                GadgetCoreAPI.DisplayYesNoDialog("<color=red>WARNING: This feature does not work! Unexpected consequences may occur from attempting to use it!</color>\n\nAre you sure you want to perform a mod reload?\nThe following mods will be reloaded:\n\n" + mod.LoadedGadgets.SelectMany(x => Gadgets.LoadOrderTree.Find(x).FlattenUniqueByBreadth()).Where(x => x != null).Select(x => x.Mod).Distinct().Select(x => x.Name).Concat("\n"), () => {
                    GadgetLoader.ReloadMod(mod);
                });
                return null;
            }

            /// <summary>
            /// The /reloadgadget command.
            /// </summary>
            public static GadgetConsoleMessage ReloadGadget(string sender, params string[] args)
            {
                if (args.Length != 2) return CommandSyntaxError(args[0], "<gadget>");
                GadgetInfo gadget = Gadgets.GetGadgetInfo(args[1]);
                if (gadget == null) return new GadgetConsoleMessage("There is no Gadget with the name `" + args[1] + "`", null, MessageSeverity.ERROR);
                GadgetCoreAPI.DisplayYesNoDialog("Are you sure you want to perform a Gadget reload?\nThe following Gadgets will be reloaded:\n\n" + gadget.Dependents.Union(Gadgets.LoadOrderTree.Find(gadget).FlattenUniqueByBreadth()).Select(x => x.Attribute.Name).Concat("\n"), () => {
                    Gadgets.ReloadGadget(gadget);
                });
                return null;
            }

            /// <summary>
            /// The /debugmode command.
            /// </summary>
            public static GadgetConsoleMessage DebugMode(string sender, params string[] args)
            {
                GameScript.debugMode = !GameScript.debugMode;
                return new GadgetConsoleMessage("Debug Mode is now " + (GameScript.debugMode ? "ON" : "OFF"));
            }

            /// <summary>
            /// The /githublogin command.
            /// </summary>
            public static GadgetConsoleMessage GitHubLogin(string sender, params string[] args)
            {
                if (args.Length == 1)
                {
                    if (!string.IsNullOrEmpty(PlayerPrefs.GetString("GitHubAuthToken")))
                    {
                        PlayerPrefs.DeleteKey("GitHubAuthToken");
                        ModBrowser.gitHubAuthToken = null;
                        ModBrowser.gitHubAuthHeaders.Remove("Authorization");
                        return new GadgetConsoleMessage("Successfully deleted GitHub login information and logged you out.");
                    }
                    else
                    {
                        return new GadgetConsoleMessage("Cannot delete your login information as you have not logged in. " +
                            "If you meant to log in, use the version of this command with an argument for an auth token.", null, MessageSeverity.ERROR);
                    }
                }
                else if (args.Length == 2)
                {
                    Console.StartCoroutine(GitHubLoginRoutine(args[1]));
                    return null;
                }
                else return CommandSyntaxError(args[0], "[auth token]");
            }

            private static IEnumerator GitHubLoginRoutine(string authToken)
            {
                Print(new GadgetConsoleMessage("Attempting GitHub login..."));
                using (WWW testWWW = new WWW(ModBrowser.MODS_URL, null, new Dictionary<string, string>()
                {
                    ["User-Agent"] = ModBrowser.GITHUB_USER_AGENT,
                    ["Authorization"] = $"token {authToken}"
                }))
                {
                    yield return new WaitUntil(() => testWWW.isDone);
                    if (string.IsNullOrEmpty(testWWW.error) && !string.IsNullOrEmpty(testWWW.text))
                    {
                        PlayerPrefs.SetString("GitHubAuthToken", ModBrowser.gitHubAuthToken = authToken);
                        if (ModBrowser.gitHubAuthToken != null)
                        {
                            ModBrowser.gitHubAuthHeaders["Authorization"] = $"token {ModBrowser.gitHubAuthToken}";
                        }
                        if (SceneInjector.ModBrowserPanel != null) SceneInjector.ModBrowserPanel.UnlimitButton.gameObject.SetActive(false);
                        Print(new GadgetConsoleMessage("GitHub Login Successful! This login will be remembered even for future sessions."));
                        Print(new GadgetConsoleMessage("If you wish to log out, use `/githublogin` with no arguments."));
                    }
                    else
                    {
                        Print(new GadgetConsoleMessage("Login failed! Your auth token must have been invalid.", null, MessageSeverity.ERROR));
                    }
                }
                yield break;
            }

            /// <summary>
            /// The /spawnentity command.
            /// </summary>
            public static GadgetConsoleMessage SpawnEntity(string sender, params string[] args)
            {
                if (InstanceTracker.PlayerScript == null) return new GadgetConsoleMessage("This command may only be used in-game!", null, MessageSeverity.ERROR);
                if (args.Length < 2 || args.Length > 4) return CommandSyntaxError(args[0], "<entity> [x offset] [y offset]");
                float xOffset = 0, yOffset = 0;
                if (args.Length >= 3 && !float.TryParse(args[2], out xOffset))
                {
                    return new GadgetConsoleMessage("Invalid X Offset: " + args[2], null, MessageSeverity.ERROR);
                }
                if (args.Length >= 4 && !float.TryParse(args[3], out yOffset))
                {
                    return new GadgetConsoleMessage("Invalid Y Offset: " + args[2], null, MessageSeverity.ERROR);
                }
                GameObject entity = (GameObject) Instantiate((UnityEngine.Object) GadgetCoreAPI.GetEntityResource(args[1]), InstanceTracker.PlayerScript.transform.position + new Vector3(xOffset, yOffset), Quaternion.identity);
                return new GadgetConsoleMessage("Spawned " + entity.name.Replace("(Clone)", "").Trim());
            }

            private static readonly Action<GameScript, bool> cantdyingSetter = typeof(GameScript).GetField("cantdying", BindingFlags.NonPublic | BindingFlags.Instance).CreateSetter<GameScript, bool>();
            private static bool? godMode;
            /// <summary>
            /// The /godmode command.
            /// </summary>
            public static GadgetConsoleMessage GodMode(string sender, params string[] args)
            {
                if (!godMode.HasValue)
                {
                    godMode = true;
                    CoroutineHooker.StartCoroutine(GodModeRoutine());
                }
                else godMode = !godMode;
                Patches.Patch_GameScript_Die.godMode = godMode.Value;
                return new GadgetConsoleMessage("God Mode is now " + (godMode.Value ? "ON" : "OFF"));
            }
            
            private static IEnumerator GodModeRoutine()
            {
                int buildIndex = SceneManager.GetActiveScene().buildIndex;
                while (godMode.HasValue && SceneManager.GetActiveScene().buildIndex == buildIndex)
                {
                    if (godMode.Value)
                    {
                        bool refresh = GameScript.hp != GameScript.maxhp;
                        GameScript.hp = GameScript.maxhp;
                        cantdyingSetter(InstanceTracker.GameScript, false);
                        if (refresh) InstanceTracker.GameScript.UpdateHP();
                    }
                    yield return new WaitForEndOfFrame();
                }
                godMode = null;
                Patches.Patch_GameScript_Die.godMode = false;
            }

            /// <summary>
            /// The /execute command.
            /// </summary>
            public static GadgetConsoleMessage Execute(string sender, params string[] args)
            {
                if (args.Length < 3 || string.IsNullOrEmpty(args[2])) return CommandSyntaxError(args[0], "<player> <command> [args]...");
                string command = args[2];
                if (command[0] == '/') command = command.Substring(1);
                if (!IsCommandExecuteBlacklisted(command))
                {
                    if (!commandAliases.ContainsKey(command))
                    {
                        return new GadgetConsoleMessage("`/" + command + "` is not a valid command! Enter `/help` for a list of commands.", null, MessageSeverity.ERROR);
                    }
                    string player = args[1];
                    NetworkPlayer? netPlayer = GadgetNetwork.GetNetworkPlayerByName(player);
                    if (netPlayer == null)
                    {
                        return new GadgetConsoleMessage($"There is no player with the name `{player}`", null, MessageSeverity.ERROR);
                    }
                    string message = $"/{command} {args.Skip(3).Concat(" ")}";
                    RPCHooks.Singleton.SendConsoleMessage(message, netPlayer.Value);
                    return new GadgetConsoleMessage($"Forced {args[1]} to execute the command: {message}");
                }
                else if (command.Equals("help", StringComparison.OrdinalIgnoreCase))
                {
                    return new GadgetConsoleMessage("Forces another player to execute a command. Some commands are blacklisted, such as the /reflect command.\n" +
                                                    "If the affected player is an operator, then they will be notified that you used the command on them.\n" +
                                                    "Uses the syntax: /execute <player> <command> [args]...");
                }
                else
                {
                    return new GadgetConsoleMessage($"The /{command} command is blacklisted!", null, MessageSeverity.ERROR);
                }
            }

            private static Dictionary<string, ThreadedWatcher<string>> watchers = new Dictionary<string, ThreadedWatcher<string>>();
            private static Reflector reflector;
            /// <summary>
            /// The /reflect command.
            /// </summary>
            public static GadgetConsoleMessage Reflect(string sender, params string[] args)
            {
                if (args.Length < 2 || args.Length == 2 && args[1].Equals("help", StringComparison.CurrentCultureIgnoreCase))
                {
                    return new GadgetConsoleMessage("Available Modes (Case Insensitive): Help, ResolveType, ResolveMember, ResolveRef/ResolveReference, Using, UnUsing, Read, Write, Watch, EndWatch, Invoke.");
                }
                if (reflector == null) reflector = new Reflector();
                switch (args[1].ToLower())
                {
                    case "help":
                        if (args.Length != 3) return CommandSyntaxError(args[0], args[1] + " [mode]");
                        switch (args[2].ToLower())
                        {
                            case "help":
                                return new GadgetConsoleMessage("Provides information about the usage and syntax of the different modes of the /reflect command.\nSyntax: /" + args[0] + " " + args[2] + " [mode]");
                            case "resolvetype":
                                return new GadgetConsoleMessage("Resolves a specified type by its full name, so that later references can identify it by only its type name.\nSyntax: /" + args[0] + " " + args[2] + " <identifier>");
                            case "resolvemember":
                                return new GadgetConsoleMessage("Resolves a specified class member as a part of its container type, so that later references can identify it by only its name.\nSyntax: /" + args[0] + " " + args[2] + " <identifier>");
                            case "resolveref":
                            case "resolvereference":
                                return new GadgetConsoleMessage("Resolves a reference to a specified member, and optionally assigns it to a name to keep this reference for simpler access.\nTo use a reference by name, enter the reference's name prefixed by '$'\nThe container may be 'null' if the referenced member is static.\nSyntax: /" + args[0] + " " + args[2] + " <identifier> [container] [name]");
                            case "using":
                                return new GadgetConsoleMessage("Designates a namespace as being 'used', thereby simplyifing type resolution.\nSyntax: /" + args[0] + " " + args[2] + " <namespace>");
                            case "unusing":
                                return new GadgetConsoleMessage("Undesignates a namespace as bein 'used', and un-resolves all cached types from this namespace, even if the namespace was not previously 'used'.\nSyntax: /" + args[0] + " " + args[2] + " <namespace>");
                            case "read":
                                return new GadgetConsoleMessage("Reads the value of a field or reference and prints the value in the console.\nThe container may be omitted if the identifier is a reference, or 'null' if the referenced field is static.\nSyntax: /" + args[0] + " " + args[2] + " <identifier> [container]");
                            case "write":
                                return new GadgetConsoleMessage("Attempts to write the value of a field or reference from a string value. This is only possible for some types.\nThe container may be omitted if the identifier is a reference, or 'null' if the referenced field is static.\nSyntax: /" + args[0] + " " + args[2] + " <identifier> [container] <value>");
                            case "watch":
                                return new GadgetConsoleMessage("Starts watching the value of a member, and reports whenever the value changes.\nThe container may be omitted if the identifier is a reference, or 'null' if the referenced member is static.\nOptionally receives an interval to specify how many milliseconds between each change for changes in the member's value.\nUse EndWatch to stop receiving feedback on the value of the member.\nSyntax: /" + args[0] + " " + args[2] + " <identifier> [container] [interval]");
                            case "endwatch":
                                return new GadgetConsoleMessage("Stops watching the value of a member that was previously set to watch with the Watch mode.\nSyntax: /" + args[0] + " " + args[2] + " <identifier>");
                            case "invoke":
                                return new GadgetConsoleMessage("Attempts to invoke the specified method.\nThe container may be omitted if the identifier is a reference, or 'null' if the referenced member is static.\nReceives a list of zero or more arguments to pass to the method.\nThese arguments may be references prefixed with '$'\nSyntax: /" + args[0] + " " + args[2] + " <identifier> [container] [args]...");
                            default:
                                return new GadgetConsoleMessage("'" + args[2] + "' is not a valid mode of the /reflect command.", null, MessageSeverity.ERROR);
                        }
                    case "resolvetype":
                        if (args.Length != 3) return CommandSyntaxError(args[0], args[1] + " <identifier>");
                        try
                        {
                            Type t = reflector.ResolveType(args[2]);
                            return new GadgetConsoleMessage("Successfully Resolved Type: " + t.Assembly.GetName().Name + ":" + t.FullName);
                        }
                        catch (Reflector.ReflectorException e)
                        {
                            return new GadgetConsoleMessage(e.Message, null, MessageSeverity.ERROR);
                        }
                    case "resolvemember":
                        if (args.Length != 3) return CommandSyntaxError(args[0], args[1] + " <identifier>");
                        try
                        {
                            MemberInfo m = reflector.ResolveMember(args[2]);
                            return new GadgetConsoleMessage("Successfully Resolved Member: " + m.DeclaringType.Name + "." + m.Name);
                        }
                        catch (Reflector.ReflectorException e)
                        {
                            return new GadgetConsoleMessage(e.Message, null, MessageSeverity.ERROR);
                        }
                    case "resolveref":
                    case "resolvereference":
                        if (args.Length < 3 || args.Length > 5) return CommandSyntaxError(args[0], args[1] + " <identifier> [container] [name]");
                        try
                        {
                            Tuple<MemberInfo, object> oRef = reflector.ResolveReference(args[2], args.Length >= 4 ? args[3] : "null", args.Length == 5 ? args[4] : null);
                            return new GadgetConsoleMessage("Successfully Resolved Reference To: " + oRef.Item1.DeclaringType.Name + "." + oRef.Item1.Name + (args.Length == 5 ? " as $" + args[4].TrimStart('$') : string.Empty));
                        }
                        catch (Reflector.ReflectorException e)
                        {
                            return new GadgetConsoleMessage(e.Message, null, MessageSeverity.ERROR);
                        }
                    case "using":
                        if (args.Length != 3) return CommandSyntaxError(args[0], args[1] + " <namespace>");
                        try
                        {
                            reflector.UseNamespace(args[2]);
                            return new GadgetConsoleMessage("Successfully Added Using Namespace: " + args[2]);
                        }
                        catch (Reflector.ReflectorException e)
                        {
                            return new GadgetConsoleMessage(e.Message, null, MessageSeverity.ERROR);
                        }
                    case "unusing":
                        if (args.Length != 3) return CommandSyntaxError(args[0], args[1] + " <namespace>");
                        try
                        {
                            reflector.UnuseNamespace(args[2]);
                            return new GadgetConsoleMessage("Successfully Removed Using Namespace: " + args[2]);
                        }
                        catch (Reflector.ReflectorException e)
                        {
                            return new GadgetConsoleMessage(e.Message, null, MessageSeverity.ERROR);
                        }
                    case "read":
                        if (args.Length < 3 || args.Length > 4) return CommandSyntaxError(args[0], args[1] + " <identifier> [container]");
                        try
                        {
                            return new GadgetConsoleMessage("Read: " + args[2] + " = " + reflector.ReadValue(args[2], args.Length == 4 ? args[3] : "null"));
                        }
                        catch (Reflector.ReflectorException e)
                        {
                            return new GadgetConsoleMessage(e.Message, null, MessageSeverity.ERROR);
                        }
                    case "write":
                        if (args.Length < 4 || args.Length > 5) return CommandSyntaxError(args[0], args[1] + " <identifier> [container] <value>");
                        try
                        {
                            return new GadgetConsoleMessage("Write: " + args[2] + " = " + reflector.WriteValue(args[2], args.Length == 5 ? args[3] : "null", args[args.Length - 1]));
                        }
                        catch (Reflector.ReflectorException e)
                        {
                            return new GadgetConsoleMessage(e.Message, null, MessageSeverity.ERROR);
                        }
                    case "watch":
                        if (args.Length < 3 || args.Length > 5) return CommandSyntaxError(args[0], args[1] + " <identifier> [container] [interval]");
                        if (watchers.ContainsKey(args[2])) return new GadgetConsoleMessage("Already watching " + args[2] + "!", null, MessageSeverity.WARN);
                        int interval;
                        if (args.Length < 5 || !int.TryParse(args[4], out interval)) interval = 10;
                        watchers.Add(args[2], new ThreadedWatcher<string>(() => reflector.ReadValue(args[2], args.Length >= 4 ? args[3] : "null"), (before, after) =>
                        {
                            Print(new GadgetConsoleMessage(args[2] + " Watch: " + before + " -> " + after));
                            return true;
                        }, sleepInterval: interval, exceptionHandler: (e) =>
                        {
                            if (e is Reflector.ReflectorException re)
                            {
                                Print(new GadgetConsoleMessage(args[2] + " Watch Error: " + re.Message + "\nAborting Watch.", null, MessageSeverity.ERROR));
                            }
                            else
                            {
                                Print(new GadgetConsoleMessage(args[2] + " Watch Exception: " + e + "\nAborting Watch.", null, MessageSeverity.ERROR));
                            }
                            return false;
                        }));
                        return new GadgetConsoleMessage("Started Watching " + args[2]);
                    case "endwatch":
                        if (args.Length != 3) return CommandSyntaxError(args[0], args[1] + " <namespace>");
                        if (args[2].Equals("all", StringComparison.CurrentCultureIgnoreCase))
                        {
                            foreach (string key in watchers.Keys.ToArray())
                            {
                                watchers[key].Kill();
                                watchers.Remove(key);
                                Print(new GadgetConsoleMessage("Stopped Watching " + key + "..."));
                            }
                        }
                        else
                        {
                            if (!watchers.ContainsKey(args[2])) return new GadgetConsoleMessage("Not watching " + args[2] + "!", null, MessageSeverity.WARN);
                            watchers[args[2]].Kill();
                            watchers.Remove(args[2]);
                        }
                        return new GadgetConsoleMessage("Stopped Watching " + args[2]);
                    case "invoke":
                        if (args.Length < 3) return CommandSyntaxError(args[0], args[1] + " <identifier> [container] [args]...");
                        try
                        {
                            string returnTarget = null;
                            if (args.Length > 4 && args[3].Trim() == "=")
                            {
                                returnTarget = args[2];
                                string[] splicedArgs = new string[args.Length - 2];
                                Array.Copy(args, 0, splicedArgs, 0, 2);
                                Array.Copy(args, 4, splicedArgs, 2, splicedArgs.Length - 2);
                                args = splicedArgs;
                            }
                            string[] invokeArgs = new string[args.Length > 4 ? args.Length - 4 : 0];
                            if (invokeArgs.Length > 0)
                            {
                                Array.Copy(args, 4, invokeArgs, 0, invokeArgs.Length);
                            }
                            string val = returnTarget != null ? reflector.InvokeReturn(args[2], args.Length >= 4 ? args[3] : "null", returnTarget, invokeArgs) : reflector.Invoke(args[2], args.Length >= 4 ? args[3] : "null", invokeArgs);
                            if (val != "void")
                            {
                                return new GadgetConsoleMessage("Invoke: " + args[2] + "(" + invokeArgs.Concat() + ") = " + val);
                            }
                            else
                            {
                                return new GadgetConsoleMessage("Invoke: " + args[2] + "(" + invokeArgs.Concat() + ")");
                            }
                        }
                        catch (Reflector.ReflectorException e)
                        {
                            return new GadgetConsoleMessage(e.Message, null, MessageSeverity.ERROR);
                        }
                    default:
                        return CommandSyntaxError(args[0], "<mode> [parameters]...");
                }
            }
        }

        /// <summary>
        /// Used to indicate the severity of a GadgetConsole message.
        /// </summary>
        public enum MessageSeverity
        {
            /// <summary>
            /// Don't show a severity tag for this message.
            /// </summary>
            RAW,
            /// <summary>
            /// The message is simply informational. The default severity.
            /// </summary>
            INFO,
            /// <summary>
            /// The message is a warning.
            /// </summary>
            WARN,
            /// <summary>
            /// The message is indicitive of an error. Used for reporting mod exceptions.
            /// </summary>
            ERROR,
            /// <summary>
            /// The message is for debugging purposes only, and shouldn't be shown by default.
            /// </summary>
            DEBUG
        }
    }
}
