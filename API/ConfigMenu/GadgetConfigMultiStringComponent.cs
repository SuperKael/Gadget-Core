using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GadgetCore.API.ConfigMenu
{
    /// <summary>
    /// This is an implementation of <see cref="GadgetConfigComponent"/> that serves as one or more textboxes where you can enter one or more string values.
    /// </summary>
    public class GadgetConfigMultiStringComponent : GadgetConfigComponent
    {
        /// <summary>
        /// The current value of this component.
        /// </summary>
        public string[] Value { get; protected set; }
        /// <summary>
        /// The default value of this component.
        /// </summary>
        public readonly string[] DefaultValue;
        /// <summary>
        /// The vanilla value of this component.
        /// </summary>
        public readonly string[] VanillaValue;
        /// <summary>
        /// Whether this component is read-only.
        /// </summary>
        public readonly bool ReadOnly;

        private readonly Action<string[]> ValueSetter;

        private List<Button> addButtons = new List<Button>();

        /// <summary>
        /// Constructs a new <see cref="GadgetConfigMultiStringComponent"/> that serves as one or more textboxes where you can enter one or more string values. The given <paramref name="valueSetter"/> will be called whenever the values are changed. Be aware that the given height value is only the height of one entry in the list. The actual height can vary depending on the number of set values.
        /// </summary>
        public GadgetConfigMultiStringComponent(BasicGadgetConfigMenu configMenu, string name, string[] value, Action<string[]> valueSetter, bool readOnly = false, string[] defaultValue = null, string[] vanillaValue = null, float height = 0.1f) : base(configMenu, name, height * value.Length)
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
            addButtons.Clear();
            for (int i = 0;i < Value.Length;i++)
            {
                int valueIndex = i;
                InputField textbox = new GameObject("Textbox", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(InputField)).GetComponent<InputField>();
                textbox.GetComponent<RectTransform>().SetParent(parent);
                textbox.GetComponent<RectTransform>().anchorMin = new Vector2(0.25f, 1f - (((float)i + 1) / Value.Length));
                textbox.GetComponent<RectTransform>().anchorMax = new Vector2(0.75f, 1f - ((float)i / Value.Length));
                textbox.GetComponent<RectTransform>().offsetMin = new Vector2(10, 0);
                textbox.GetComponent<RectTransform>().offsetMax = new Vector2(-10, 0);
                textbox.GetComponent<Image>().sprite = SceneInjector.BoxSprite;
                textbox.GetComponent<Image>().type = Image.Type.Sliced;
                textbox.GetComponent<Image>().fillCenter = true;
                Text placeholder = new GameObject("Placeholder", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
                placeholder.rectTransform.SetParent(textbox.transform);
                placeholder.rectTransform.anchorMin = new Vector2(0f, 0f);
                placeholder.rectTransform.anchorMax = new Vector2(1f, 1f);
                placeholder.rectTransform.offsetMin = new Vector2(10, 0);
                placeholder.rectTransform.offsetMax = new Vector2(-10, 0);
                placeholder.text = ((DefaultValue?.Length > i && DefaultValue[i] != null) ? "Default: " + DefaultValue[i] + " " : "") + ((VanillaValue?.Length > i && VanillaValue[i] != null) ? "Vanilla: " + VanillaValue[i] : "");
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
                textbox.text = Value[i];
                textbox.onEndEdit.AddListener((value) =>
                {
                    if ((string.IsNullOrEmpty(value) && addButtons.Count == 1) || !Value.Any(x => string.IsNullOrEmpty(x)))
                    {
                        ValueSetter(Value);
                    }
                });
                textbox.onValueChanged.AddListener((value) =>
                {
                    Value[valueIndex] = value;
                    if (string.IsNullOrEmpty(value))
                    {
                        foreach (Button addButton in addButtons) addButton.interactable = false;
                    }
                    else if (!Value.Any(x => string.IsNullOrEmpty(x)))
                    {
                        foreach (Button addButton in addButtons) addButton.interactable = true;
                    }
                });
                float buttonWidth = Math.Min(Height / Value.Length / (Screen.width / Screen.height), 0.125f);
                Button addValueButton = new GameObject("Add Value Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button)).GetComponent<Button>();
                addValueButton.GetComponent<RectTransform>().SetParent(parent);
                addValueButton.GetComponent<RectTransform>().anchorMin = new Vector2(1f - (buttonWidth * 2), 1f - (((float)i + 1) / Value.Length));
                addValueButton.GetComponent<RectTransform>().anchorMax = new Vector2(1f - buttonWidth, 1f - ((float)i / Value.Length));
                addValueButton.GetComponent<RectTransform>().offsetMin = new Vector2(5, 0);
                addValueButton.GetComponent<RectTransform>().offsetMax = new Vector2(-5, 0);
                addValueButton.GetComponent<Image>().sprite = SceneInjector.BoxSprite;
                addValueButton.GetComponent<Image>().type = Image.Type.Sliced;
                addValueButton.GetComponent<Image>().fillCenter = true;
                addValueButton.targetGraphic = addValueButton.GetComponent<Image>();
                addValueButton.onClick.AddListener(() =>
                {
                    string[] newValue = new string[Value.Length + 1];
                    Array.Copy(Value, 0, newValue, 0, valueIndex + 1);
                    if (valueIndex < Value.Length - 1) Array.Copy(Value, valueIndex + 1, newValue, valueIndex + 2, Value.Length - valueIndex - 1);
                    Height *= (float)newValue.Length / Value.Length;
                    Value = newValue;
                    ConfigMenu.Rebuild();
                    foreach (Button addButton in addButtons) addButton.interactable = false;
                });
                addButtons.Add(addValueButton);
                Text addValueButtonText = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
                addValueButtonText.rectTransform.SetParent(addValueButton.transform);
                addValueButtonText.rectTransform.anchorMin = new Vector2(0f, 0f);
                addValueButtonText.rectTransform.anchorMax = new Vector2(1f, 1f);
                addValueButtonText.rectTransform.offsetMin = Vector2.zero;
                addValueButtonText.rectTransform.offsetMax = Vector2.zero;
                addValueButtonText.horizontalOverflow = HorizontalWrapMode.Overflow;
                addValueButtonText.verticalOverflow = VerticalWrapMode.Overflow;
                addValueButtonText.alignment = TextAnchor.MiddleCenter;
                addValueButtonText.font = SceneInjector.ModConfigMenuText.GetComponent<TextMesh>().font;
                addValueButtonText.fontSize = 12;
                addValueButtonText.text = "+";
                Button removeValueButton = new GameObject("Remove Value Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button)).GetComponent<Button>();
                removeValueButton.GetComponent<RectTransform>().SetParent(parent);
                removeValueButton.GetComponent<RectTransform>().anchorMin = new Vector2(1f - buttonWidth, 1f - (((float)i + 1) / Value.Length));
                removeValueButton.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f - ((float)i / Value.Length));
                removeValueButton.GetComponent<RectTransform>().offsetMin = new Vector2(5, 0);
                removeValueButton.GetComponent<RectTransform>().offsetMax = new Vector2(-5, 0);
                removeValueButton.GetComponent<Image>().sprite = SceneInjector.BoxSprite;
                removeValueButton.GetComponent<Image>().type = Image.Type.Sliced;
                removeValueButton.GetComponent<Image>().fillCenter = true;
                removeValueButton.targetGraphic = removeValueButton.GetComponent<Image>();
                removeValueButton.interactable = Value.Length > 1;
                removeValueButton.onClick.AddListener(() =>
                {
                    bool shouldSave = !string.IsNullOrEmpty(Value[valueIndex]);
                    string[] newValue = new string[Value.Length - 1];
                    Array.Copy(Value, 0, newValue, 0, valueIndex);
                    if (valueIndex < Value.Length - 1) Array.Copy(Value, valueIndex + 1, newValue, valueIndex, Value.Length - valueIndex - 1);
                    Height *= (float)newValue.Length / Value.Length;
                    Value = newValue;
                    if (shouldSave) ValueSetter(Value);
                    ConfigMenu.Rebuild();
                });
                Text removeValueButtonText = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
                removeValueButtonText.rectTransform.SetParent(removeValueButton.transform);
                removeValueButtonText.rectTransform.anchorMin = new Vector2(0f, 0f);
                removeValueButtonText.rectTransform.anchorMax = new Vector2(1f, 1f);
                removeValueButtonText.rectTransform.offsetMin = Vector2.zero;
                removeValueButtonText.rectTransform.offsetMax = Vector2.zero;
                removeValueButtonText.horizontalOverflow = HorizontalWrapMode.Overflow;
                removeValueButtonText.verticalOverflow = VerticalWrapMode.Overflow;
                removeValueButtonText.alignment = TextAnchor.MiddleCenter;
                removeValueButtonText.font = SceneInjector.ModConfigMenuText.GetComponent<TextMesh>().font;
                removeValueButtonText.fontSize = 12;
                removeValueButtonText.text = "-";
            }
            if (Value.Any(x => string.IsNullOrEmpty(x)))
            {
                foreach (Button addButton in addButtons) addButton.interactable = false;
            }
        }
    }
}
