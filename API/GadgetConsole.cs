using GadgetCore.API;
using GadgetCore.Loader;
using GadgetCore.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
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

        private static GadgetConsole console;
        private static List<GadgetConsoleMessage> messages = new List<GadgetConsoleMessage>();
        private static Dictionary<string, ConsoleCommand> commands = new Dictionary<string, ConsoleCommand>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, bool> isOperatorOnly = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, string> helpDescriptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, string> fullHelps = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, string> commandAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static List<GadgetConsoleMessage> queuedMessages = new List<GadgetConsoleMessage>();
        private static List<string> messageHistory = new List<string>();
        private static int historyIndex;

        internal static List<string> operators = new List<string>();

        private static bool wasSelected, wasOpen;

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

        /// <summary>
        /// Opens the console
        /// </summary>
        public static void ShowConsole()
        {
            GadgetCoreAPI.FreezeInput("Gadget Console Open");
            wasOpen = true;
            console.gameObject.SetActive(true);
            console.AlwaysActivePanel.gameObject.SetActive(false);
            console.InputField.ActivateInputField();
            console.InputField.Select();
            historyIndex = messageHistory.Count;
        }

        /// <summary>
        /// Closes the console
        /// </summary>
        public static void HideConsole()
        {
            GadgetCoreAPI.UnfreezeInput("Gadget Console Open");
            wasOpen = false;
            console.gameObject.SetActive(false);
            console.AlwaysActivePanel.gameObject.SetActive(true);
            console.InputField.text = "";
            console.InputField.DeactivateInputField();
        }

        /// <summary>
        /// Toggles the console
        /// </summary>
        public static void ToggleConsole()
        {
            console.gameObject.SetActive(!console.gameObject.activeSelf);
            console.AlwaysActivePanel.gameObject.SetActive(!console.gameObject.activeSelf);
            if (console.gameObject.activeSelf)
            {
                GadgetCoreAPI.FreezeInput("Gadget Console Open");
                wasOpen = true;
                console.InputField.ActivateInputField();
                console.InputField.Select();
                historyIndex = messageHistory.Count;
            }
            else
            {
                GadgetCoreAPI.UnfreezeInput("Gadget Console Open");
                wasOpen = false;
                console.InputField.text = "";
                console.InputField.DeactivateInputField();
            }
        }

        /// <summary>
        /// Returns a value indicating whether the console is currently open.
        /// </summary>
        public static bool IsOpen()
        {
            return console.gameObject.activeSelf;
        }
        /// <summary>
        /// Returns a value indicating whether the console is was open one frame ago.
        /// </summary>
        public static bool WasOpen()
        {
            return wasOpen;
        }

        /// <summary>
        /// Registers a command. If <paramref name="operatorOnly"/> is true, then only operators will be able to execute this command.
        /// </summary>
        public static void RegisterCommand(string name, bool operatorOnly, ConsoleCommand command, string helpDesc, string fullHelp = null, params string[] aliases)
        {
            name = name.ToLowerInvariant();
            if (commands.ContainsKey(name))
            {
                GadgetCore.CoreLogger.LogWarning("A command had already been registered with the name " + name + ", but it has been overwritten by another mod!");
            }
            if (name == "all") throw new InvalidOperationException(name + " is a reserved command name!");
            if (name.All(x => x >= '0' && x <= '9')) throw new InvalidOperationException("A command name may not be a number!");
            commands[name] = command;
            isOperatorOnly[name] = operatorOnly;
            helpDescriptions[name] = helpDesc;
            if (fullHelp != null) fullHelps[name] = fullHelp;
            commandAliases[name] = name;
            foreach (string alias in aliases) commandAliases[alias.ToLowerInvariant()] = name;
        }

        /// <summary>
        /// Prints the given text to the console. Returns the index of the message on the console, which can be used to change or remove it later. Will return null if a debug message is printed, but <see cref="Debug"/> is false.
        /// </summary>
        public static int Print(string text, string sender = null, MessageSeverity severity = MessageSeverity.RAW)
        {
            return Print(new GadgetConsoleMessage(text, sender, severity));
        }

        /// <summary>
        /// Prints the given <see cref="GadgetConsoleMessage"/> to the console. Returns the index of the message on the console, which can be used to change or remove it later. Will return null if a debug message is printed, but <see cref="Debug"/> is false.
        /// </summary>
        public static int Print(GadgetConsoleMessage message)
        {
            return AddMessage(message);
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
                textComponent.rectTransform.SetParent(console.TextPanel.transform);
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
                if (messageText.All(x => SceneInjector.ModMenuPanel.descText.font.HasCharacter(x)))
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
            if (console == null)
            {
                queuedMessages.Add(message);
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
                textComponent.rectTransform.SetParent(i == 0 ? console.TextPanel.transform : console.AlwaysActivePanel.transform);
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
                if (messageText.All(x => SceneInjector.ModMenuPanel.descText.font.HasCharacter(x)))
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
                InstanceTracker.MainCamera.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("au/click2"), Menuu.soundLevel / 10f);
            }
            else
            {
                InstanceTracker.MainCamera.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("au/pickup"), Menuu.soundLevel / 10f);
            }
            return messages.Count - 1;
        }

        internal static void PrintQueuedMessages()
        {
            foreach (GadgetConsoleMessage message in queuedMessages)
            {
                AddMessage(message);
            }
            queuedMessages.Clear();
        }

        /// <summary>
        /// Sends a message to the console, as if it were typed by the player. Can trigger commands. If <paramref name="printCommandFeedback"/> is false, then response feedback from commands will be supressed.
        /// </summary>
        public static int SendConsoleMessage(string message, string sender, bool printCommandFeedback = true)
        {
            if (!string.IsNullOrEmpty(message))
            {
                if (message.Length > 1 && message[0] == '/')
                {
                    string[] args = ParseArgs(message.Substring(1));
                    if (commandAliases.ContainsKey(args[0]))
                    {
                        if (SceneManager.GetActiveScene().buildIndex == 0 || Network.isServer || !isOperatorOnly[commandAliases[args[0]]] || operators.Contains(Menuu.curName))
                        {
                            GadgetConsoleMessage feedback = commands[commandAliases[args[0]]](sender, args);
                            if (printCommandFeedback) return Print(feedback);
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
            wasOpen = console.gameObject.activeSelf;
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

            console = this;
            GadgetCoreAPI.RegisterKeyDownListener(KeyCode.Return, TextSubmitAction);
            GadgetCoreAPI.RegisterKeyDownListener(KeyCode.UpArrow, HistoryUpAction);
            GadgetCoreAPI.RegisterKeyDownListener(KeyCode.DownArrow, HistoryDownAction);
        }

        [SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Unity Method")]
        private void OnDestroy()
        {
            console = null;
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
            RegisterCommand("help", false, CoreCommands.Help,
                "Provides the help page you are seeing right now.",
                "Can list available commands, as well as provide extended information about specific commands.\nUses the syntax: /help <page/command>",
                "h");
            RegisterCommand("op", true, CoreCommands.Op,
                "Grants operator permissions to a given player.",
                "Grants operator permissions to a given player. These permissions only last as long as the multiplayer session continues.\nThis command, along with /deop, can only be used by the game host.\nUses the syntax: /op <player>");
            RegisterCommand("deop", true, CoreCommands.Deop,
                "Removes operator permissions from a given player.",
                "Removes operator permissions from a given player. These permissions only last as long as the multiplayer session continues.\nThis command, along with /op, can only be used by the game host.\nUses the syntax: /deop <player>");
            RegisterCommand("give", true, CoreCommands.Give,
                "Used to spawn items into your or another player's inventory.",
                "Used to spawn items into your or another player's inventory. Note that you can set multiple properties,\nand they work like, I.E.: \"tier=3\", where 3 represents legendary.\nUses the syntax: /give [player] <item name/id> [quantity] [property]=[value]...",
                "g");
            RegisterCommand("givechip", true, CoreCommands.GiveChip,
                "Used to spawn chips into your or another player's inventory.",
                "Used to spawn chips into your or another player's inventory.\nUses the syntax: /givechip [player] <chip name/id>",
                "gc");
            RegisterCommand("reloadmod", true, CoreCommands.ReloadMod,
                "Reloads the specified mod.",
                "Relodas the specified mod. Will ask for confirmation, and lists other mods that will be reloaded as a consequence.\nUses the syntax: /reloadmod <mod>",
                "rlm");
            RegisterCommand("reloadgadget", true, CoreCommands.ReloadGadget,
                "Reloads the specified Gadget.",
                "Relodas the specified Gadget. Will ask for confirmation, and lists other Gadgets that will be reloaded as a consequence.\nUses the syntax: /reloadgadget <gadget>",
                "rlg");
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
                            case "aspect1Lvl":
                            case "mod1Lvl":
                            case "aspect1Count":
                            case "mod1Count":
                                if (!int.TryParse(splitProperty[1], out aspectLvl[0]))
                                {
                                    return new GadgetConsoleMessage("`" + splitProperty[1] + "` is not a valid number!", null, MessageSeverity.ERROR);
                                }
                                break;
                            case "aspect2Lvl":
                            case "mod2Lvl":
                            case "aspect2Count":
                            case "mod2Count":
                                if (!int.TryParse(splitProperty[1], out aspectLvl[1]))
                                {
                                    return new GadgetConsoleMessage("`" + splitProperty[1] + "` is not a valid number!", null, MessageSeverity.ERROR);
                                }
                                break;
                            case "aspect3Lvl":
                            case "mod3Lvl":
                            case "aspect3Count":
                            case "mod3Count":
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
                    if (!ItemRegistry.GetSingleton().HasEntry(id))
                    {
                        return new GadgetConsoleMessage("There is no item with the ID `" + id + "`", null, MessageSeverity.ERROR);
                    }
                }
                else
                {
                    id = ItemRegistry.GetItemIDByName(itemName);
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
                    if (!ChipRegistry.GetSingleton().HasEntry(id))
                    {
                        return new GadgetConsoleMessage("There is no chip with the ID `" + id + "`", null, MessageSeverity.ERROR);
                    }
                }
                else
                {
                    id = ChipRegistry.GetChipIDByName(itemName);
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
