using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GadgetCore.API.ConfigMenu
{
    /// <summary>
    /// This is an implementation of <see cref="GadgetConfigComponent"/> that serves as a button that can execute arbitrary code when pressed.
    /// </summary>
    public class GadgetConfigButtonComponent : GadgetConfigComponent
    {
        /// <summary>
        /// The label on this button.
        /// </summary>
        public readonly string Label;

        private readonly Action Trigger;

        /// <summary>
        /// Constructs a new <see cref="GadgetConfigButtonComponent"/> that serves as a button that can execute arbitrary code when pressed. The given <paramref name="trigger"/> will be called whenever the button is pressed.
        /// </summary>
        public GadgetConfigButtonComponent(BasicGadgetConfigMenu configMenu, string name, string label, Action trigger, float height = 0.1f) : base(configMenu, name, height)
        {
            Label = label;
            Trigger = trigger;
        }

        /// <summary>
        /// Called when <see cref="BasicGadgetConfigMenu.Build"/> is called on the config menu containing this component.
        /// </summary>
        public override void Build(RectTransform parent)
        {
            if (!string.IsNullOrEmpty(Name))
            {
                StringBuilder nameString = new StringBuilder();
                int spacesAdded = 0;
                foreach (char c in Name)
                {
                    if (nameString.Length > 0 && char.IsUpper(c) && !char.IsUpper(Name[nameString.Length - spacesAdded - 1]) && (Name.Length == 1 || !char.IsUpper(Name[nameString.Length - spacesAdded - 2]) || (Name.Length > 2 && !char.IsUpper(Name[nameString.Length - spacesAdded - 3]))))
                    {
                        spacesAdded++;
                        nameString.Append(' ');
                    }
                    nameString.Append(nameString.Length > 0 ? c : char.ToUpper(c));
                }
                Text label = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
                label.rectTransform.SetParent(parent);
                label.rectTransform.anchorMin = new Vector2(0f, 0f);
                label.rectTransform.anchorMax = new Vector2(0.25f, 1f);
                label.rectTransform.offsetMin = new Vector2(0, 0);
                label.rectTransform.offsetMax = new Vector2(-10, 0);
                label.text = nameString + ":";
                label.font = SceneInjector.ModConfigMenuText.GetComponent<TextMesh>().font;
                label.fontSize = 12;
                label.horizontalOverflow = HorizontalWrapMode.Wrap;
                label.verticalOverflow = VerticalWrapMode.Overflow;
                label.alignment = TextAnchor.MiddleLeft;
            }
            RectTransform button = new GameObject("Button", typeof(RectTransform), typeof(Button), typeof(CanvasRenderer), typeof(Image)).GetComponent<RectTransform>();
            button.SetParent(parent);
            button.anchorMin = new Vector2(0.25f, 0f);
            button.anchorMax = new Vector2(0.75f, 1f);
            button.offsetMin = new Vector2(10, 0);
            button.offsetMax = new Vector2(-10, 0);
            button.GetComponent<Image>().sprite = SceneInjector.BoxSprite;
            button.GetComponent<Image>().type = Image.Type.Sliced;
            button.GetComponent<Image>().fillCenter = true;
            button.GetComponent<Button>().onClick.AddListener(() => Trigger());
            Text buttonLabel = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
            buttonLabel.rectTransform.SetParent(button);
            buttonLabel.rectTransform.anchorMin = new Vector2(0f, 0f);
            buttonLabel.rectTransform.anchorMax = new Vector2(1f, 1f);
            buttonLabel.rectTransform.offsetMin = new Vector2(2.5f, 2.5f);
            buttonLabel.rectTransform.offsetMax = new Vector2(-2.5f, -2.5f);
            buttonLabel.font = SceneInjector.ModConfigMenuText.GetComponent<TextMesh>().font;
            buttonLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
            buttonLabel.verticalOverflow = VerticalWrapMode.Overflow;
            buttonLabel.alignment = TextAnchor.MiddleCenter;
            buttonLabel.text = Label;
        }
    }
}
