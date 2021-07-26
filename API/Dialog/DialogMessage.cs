using GadgetCore.Util;
using System;

namespace GadgetCore.API.Dialog
{
    /// <summary>
    /// Represents a single dialog message within a dialog chain.
    /// </summary>
    public class DialogMessage
    {
        public static TextMeshSize textboxSize { get; private set; }
        /// <summary>
        /// The text to display in the dialog box when this message is displayed.
        /// </summary>
        public readonly string Text;
        /// <summary>
        /// An action to trigger when thisa dialog message is displayed.
        /// </summary>
        public readonly Action Trigger;

        /// <summary>
        /// Displays this dialog message, if it exists, and triggers this message's action, if it exists.
        /// Also returns whether an actual dialog box was displayed, for convenience's sake.
        /// </summary>
        public virtual bool DisplayMessage()
        {
            bool displayText = Text != null;
            if (displayText)
            {
                if (textboxSize == null || !textboxSize.IsValid())
                {
                    textboxSize = new TextMeshSize(InstanceTracker.GameScript.menuTalking.GetComponent<MenuTalking>().txtTalkingText[0]);
                }
                string wordWrappedText = textboxSize.InsertNewlines(Text, 22f);
                InstanceTracker.GameScript.menuTalking.SendMessage("Set", wordWrappedText);
            }
            Trigger?.Invoke();
            return displayText;
        }

        /// <summary>
        /// Constructs a new DialogMessage
        /// </summary>
        public DialogMessage(string Text, Action Trigger)
        {
            this.Text = Text;
            this.Trigger = Trigger;
        }

        /// <summary>
        /// Constructs a new DialogMessage
        /// </summary>
        public DialogMessage(string Text)
        {
            this.Text = Text;
            Trigger = null;
        }

        /// <summary>
        /// Constructs a new DialogMessage
        /// </summary>
        public DialogMessage(Action Trigger)
        {
            Text = null;
            this.Trigger = Trigger;
        }

        /// <summary>
        /// Implicitly converts a string to a DialogMessage
        /// </summary>
        public static implicit operator DialogMessage(string Text)
        {
            return new DialogMessage(Text);
        }

        /// <summary>
        /// Implicitly converts a DialogMessage to a string
        /// </summary>
        public static implicit operator string(DialogMessage Message)
        {
            return Message.Text;
        }
    }
}
