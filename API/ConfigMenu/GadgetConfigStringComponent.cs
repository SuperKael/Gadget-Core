using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GadgetCore.API.ConfigMenu
{
    /// <summary>
    /// This is an implementation of <see cref="GadgetConfigComponent"/> that serves as a textbox for representing a string-based config value.
    /// </summary>
    public class GadgetConfigStringComponent : GadgetConfigComponent
    {
        /// <summary>
        /// The current value of this component.
        /// </summary>
        public string Value { get; protected set; }
        /// <summary>
        /// The default value of this component.
        /// </summary>
        public readonly string DefaultValue;
        /// <summary>
        /// The vanilla value of this component.
        /// </summary>
        public readonly string VanillaValue;
        /// <summary>
        /// Whether this component is read-only.
        /// </summary>
        public readonly bool ReadOnly;

        private readonly Action<string> ValueSetter;

        /// <summary>
        /// Constructs a new <see cref="GadgetConfigStringComponent"/> that serves as a textbox for representing a string-based config value. The given <paramref name="valueSetter"/> will be called whenever the textbox's contents are changed.
        /// </summary>
        public GadgetConfigStringComponent(BasicGadgetConfigMenu configMenu, string name, string value, Action<string> valueSetter, bool readOnly = false, string defaultValue = null, string vanillaValue = null, float height = 0.1f) : base(configMenu, name, height)
        {
            Value = value;
            DefaultValue = defaultValue;
            VanillaValue = vanillaValue;
            ReadOnly = readOnly;
            ValueSetter = valueSetter;
        }

        /// <summary>
        /// Called when <see cref="BasicGadgetConfigMenu.Build"/> is called on the config menu containing this component.
        /// </summary>
        public override void Build(RectTransform parent)
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
            label.alignment = TextAnchor.MiddleLeft;
            InputField textbox = new GameObject("Textbox", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(InputField)).GetComponent<InputField>();
            textbox.GetComponent<RectTransform>().SetParent(parent);
            textbox.GetComponent<RectTransform>().anchorMin = new Vector2(0.25f, 0f);
            textbox.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
            textbox.GetComponent<RectTransform>().offsetMin = new Vector2(10, 0);
            textbox.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
            textbox.GetComponent<Image>().sprite = SceneInjector.BoxSprite;
            textbox.GetComponent<Image>().type = Image.Type.Sliced;
            textbox.GetComponent<Image>().fillCenter = true;
            Text placeholder = new GameObject("Placeholder", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
            placeholder.rectTransform.SetParent(textbox.transform);
            placeholder.rectTransform.anchorMin = new Vector2(0f, 0f);
            placeholder.rectTransform.anchorMax = new Vector2(1f, 1f);
            placeholder.rectTransform.offsetMin = new Vector2(10, 0);
            placeholder.rectTransform.offsetMax = new Vector2(-10, 0);
            placeholder.text = (!string.IsNullOrEmpty(DefaultValue) ? "Default: " + DefaultValue + " " : "") + (!string.IsNullOrEmpty(VanillaValue) ? "Vanilla: " + VanillaValue : "");
            placeholder.font = SceneInjector.ModConfigMenuText.GetComponent<TextMesh>().font;
            placeholder.fontSize = 12;
            placeholder.horizontalOverflow = HorizontalWrapMode.Wrap;
            placeholder.alignment = TextAnchor.MiddleLeft;
            placeholder.color = new Color(1f, 1f, 1f, 0.5f);
            textbox.placeholder = placeholder;
            Text text = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
            text.rectTransform.SetParent(textbox.transform);
            text.rectTransform.anchorMin = new Vector2(0f, 0f);
            text.rectTransform.anchorMax = new Vector2(1f, 1f);
            text.rectTransform.offsetMin = new Vector2(10, 0);
            text.rectTransform.offsetMax = new Vector2(-10, 0);
            text.font = SceneInjector.ModConfigMenuText.GetComponent<TextMesh>().font;
            text.fontSize = 12;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.supportRichText = true;
            text.alignment = TextAnchor.MiddleLeft;
            textbox.textComponent = text;
            textbox.readOnly = ReadOnly;
            textbox.interactable = !ReadOnly;
            textbox.text = Value;
            textbox.onEndEdit.AddListener((value) =>
            {
                Value = value;
                ValueSetter(Value);
            });
        }
    }
}
