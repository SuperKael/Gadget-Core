using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GadgetCore.API.ConfigMenu
{
    /// <summary>
    /// This is an implementation of <see cref="GadgetConfigComponent"/> that serves as one or more buttons where you can enter one or more keybinds.
    /// </summary>
    public class GadgetConfigMultiKeybindComponent : GadgetConfigComponent
    {
        /// <summary>
        /// The current value of this component.
        /// </summary>
        public string[] Value { get; protected set; }
        /// <summary>
        /// Whether to allow multi-key bindings.
        /// </summary>
        public readonly bool AllowMultiBind;
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
        /// Constructs a new <see cref="GadgetConfigMultiKeybindComponent"/> that serves as one or more buttons where you can enter one or more keybinds. The given <paramref name="valueSetter"/> will be called whenever the keybinds are changed. Be aware that the given height value is only the height of one entry in the list. The actual height can vary depending on the number of set keybinds.
        /// </summary>
        public GadgetConfigMultiKeybindComponent(BasicGadgetConfigMenu configMenu, string name, string[] value, Action<string[]> valueSetter, bool allowMultiBind = true, bool readOnly = false, string[] defaultValue = null, string[] vanillaValue = null, float height = 0.1f) : base(configMenu, name, height * value.Length)
        {
            Value = value;
            AllowMultiBind = allowMultiBind;
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
                label.alignment = TextAnchor.MiddleLeft;
            }
            addButtons.Clear();
            for (int i = 0;i < Value.Length;i++)
            {
                int valueIndex = i;
                Text defaults = new GameObject("Defaults", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
                defaults.rectTransform.SetParent(parent);
                defaults.rectTransform.anchorMin = new Vector2(0.5f, 1f - (((float)i + 1) / Value.Length));
                defaults.rectTransform.anchorMax = new Vector2(1f, 1f - ((float)i / Value.Length));
                defaults.rectTransform.offsetMin = new Vector2(10, 0);
                defaults.rectTransform.offsetMax = new Vector2(0, 0);
                defaults.text = ((DefaultValue?.Length > i && DefaultValue[i] != null) ? "Default: " + DefaultValue[i] + " " : "") + ((VanillaValue?.Length > i && VanillaValue[i] != null) ? "Vanilla: " + VanillaValue[i] : "");
                defaults.font = SceneInjector.ModConfigMenuText.GetComponent<TextMesh>().font;
                defaults.fontSize = 12;
                defaults.horizontalOverflow = HorizontalWrapMode.Wrap;
                defaults.alignment = TextAnchor.MiddleLeft;
                RectTransform button = new GameObject("Keybind Button", typeof(RectTransform), typeof(Toggle), typeof(CanvasRenderer), typeof(Image), typeof(Mask), typeof(KeybindToggle)).GetComponent<RectTransform>();
                button.SetParent(parent);
                button.GetComponent<RectTransform>().anchorMin = new Vector2(0.25f, 1f - (((float)i + 1) / Value.Length));
                button.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1f - ((float)i / Value.Length));
                button.GetComponent<RectTransform>().offsetMin = new Vector2(10, 0);
                button.GetComponent<RectTransform>().offsetMax = new Vector2(-10, 0);
                Toggle toggle = button.GetComponent<Toggle>();
                Image selected = new GameObject("Selected", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
                selected.rectTransform.SetParent(button);
                selected.rectTransform.anchorMin = new Vector2(0f, 0f);
                selected.rectTransform.anchorMax = new Vector2(1f, 1f);
                selected.rectTransform.offsetMin = Vector2.zero;
                selected.rectTransform.offsetMax = Vector2.zero;
                selected.sprite = SceneInjector.BoxSprite;
                selected.type = Image.Type.Sliced;
                selected.color = new Color(0.25f, 0.25f, 0.25f, 1f);
                Text buttonLabel = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
                buttonLabel.rectTransform.SetParent(button);
                buttonLabel.rectTransform.anchorMin = new Vector2(0f, 0f);
                buttonLabel.rectTransform.anchorMax = new Vector2(1f, 1f);
                buttonLabel.rectTransform.offsetMin = new Vector2(2.5f, 2.5f);
                buttonLabel.rectTransform.offsetMax = new Vector2(-2.5f, -2.5f);
                buttonLabel.font = SceneInjector.ModConfigMenuText.GetComponent<TextMesh>().font;
                buttonLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
                buttonLabel.verticalOverflow = VerticalWrapMode.Truncate;
                buttonLabel.alignment = TextAnchor.MiddleCenter;
                buttonLabel.text = Value[i];
                float buttonWidth = Math.Min(Height / Value.Length / (Screen.width / Screen.height), 0.125f);
                Button addBindButton = new GameObject("Add Bind Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button)).GetComponent<Button>();
                addBindButton.GetComponent<RectTransform>().SetParent(parent);
                addBindButton.GetComponent<RectTransform>().anchorMin = new Vector2(1f - (buttonWidth * 2), 1f - (((float)i + 1) / Value.Length));
                addBindButton.GetComponent<RectTransform>().anchorMax = new Vector2(1f - buttonWidth, 1f - ((float)i / Value.Length));
                addBindButton.GetComponent<RectTransform>().offsetMin = new Vector2(5, 0);
                addBindButton.GetComponent<RectTransform>().offsetMax = new Vector2(-5, 0);
                addBindButton.GetComponent<Image>().sprite = SceneInjector.BoxSprite;
                addBindButton.GetComponent<Image>().type = Image.Type.Sliced;
                addBindButton.GetComponent<Image>().fillCenter = true;
                addBindButton.targetGraphic = addBindButton.GetComponent<Image>();
                addBindButton.onClick.AddListener(() =>
                {
                    string[] newValue = new string[Value.Length + 1];
                    Array.Copy(Value, 0, newValue, 0, valueIndex + 1);
                    if (valueIndex < Value.Length - 1) Array.Copy(Value, valueIndex + 1, newValue, valueIndex + 2, Value.Length - valueIndex - 1);
                    Height *= (float)newValue.Length / Value.Length;
                    Value = newValue;
                    ConfigMenu.Rebuild();
                    foreach (Button addButton in addButtons) addButton.interactable = false;
                });
                addButtons.Add(addBindButton);
                Text addBindButtonText = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
                addBindButtonText.rectTransform.SetParent(addBindButton.transform);
                addBindButtonText.rectTransform.anchorMin = new Vector2(0f, 0f);
                addBindButtonText.rectTransform.anchorMax = new Vector2(1f, 1f);
                addBindButtonText.rectTransform.offsetMin = Vector2.zero;
                addBindButtonText.rectTransform.offsetMax = Vector2.zero;
                addBindButtonText.horizontalOverflow = HorizontalWrapMode.Overflow;
                addBindButtonText.verticalOverflow = VerticalWrapMode.Overflow;
                addBindButtonText.alignment = TextAnchor.MiddleCenter;
                addBindButtonText.font = SceneInjector.ModConfigMenuText.GetComponent<TextMesh>().font;
                addBindButtonText.fontSize = 12;
                addBindButtonText.text = "+";
                Button removeBindButton = new GameObject("Remove Bind Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button)).GetComponent<Button>();
                removeBindButton.GetComponent<RectTransform>().SetParent(parent);
                removeBindButton.GetComponent<RectTransform>().anchorMin = new Vector2(1f - buttonWidth, 1f - (((float)i + 1) / Value.Length));
                removeBindButton.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f - ((float)i / Value.Length));
                removeBindButton.GetComponent<RectTransform>().offsetMin = new Vector2(5, 0);
                removeBindButton.GetComponent<RectTransform>().offsetMax = new Vector2(-5, 0);
                removeBindButton.GetComponent<Image>().sprite = SceneInjector.BoxSprite;
                removeBindButton.GetComponent<Image>().type = Image.Type.Sliced;
                removeBindButton.GetComponent<Image>().fillCenter = true;
                removeBindButton.targetGraphic = removeBindButton.GetComponent<Image>();
                removeBindButton.interactable = Value.Length > 1;
                removeBindButton.onClick.AddListener(() =>
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
                Text removeBindButtonText = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
                removeBindButtonText.rectTransform.SetParent(removeBindButton.transform);
                removeBindButtonText.rectTransform.anchorMin = new Vector2(0f, 0f);
                removeBindButtonText.rectTransform.anchorMax = new Vector2(1f, 1f);
                removeBindButtonText.rectTransform.offsetMin = Vector2.zero;
                removeBindButtonText.rectTransform.offsetMax = Vector2.zero;
                removeBindButtonText.horizontalOverflow = HorizontalWrapMode.Overflow;
                removeBindButtonText.verticalOverflow = VerticalWrapMode.Overflow;
                removeBindButtonText.alignment = TextAnchor.MiddleCenter;
                removeBindButtonText.font = SceneInjector.ModConfigMenuText.GetComponent<TextMesh>().font;
                removeBindButtonText.fontSize = 12;
                removeBindButtonText.text = "-";
                toggle.GetComponent<Image>().sprite = SceneInjector.BoxSprite;
                toggle.GetComponent<Image>().type = Image.Type.Sliced;
                toggle.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
                toggle.transition = Selectable.Transition.None;
                toggle.isOn = false;
                toggle.interactable = !ReadOnly;
                toggle.toggleTransition = Toggle.ToggleTransition.None;
                toggle.graphic = selected;
                toggle.GetComponent<KeybindToggle>().Init(toggle, (keybind) =>
                {
                    buttonLabel.text = keybind;
                    Value[valueIndex] = keybind;
                    if (string.IsNullOrEmpty(keybind))
                    {
                        if (addButtons.Count == 1) ValueSetter(Value);
                        foreach (Button addButton in addButtons) addButton.interactable = false;
                    }
                    else if (!Value.Any(x => string.IsNullOrEmpty(x)))
                    {
                        ValueSetter(Value);
                        foreach (Button addButton in addButtons) addButton.interactable = true;
                    }
                }, AllowMultiBind);
            }
            if (Value.Any(x => string.IsNullOrEmpty(x)))
            {
                foreach (Button addButton in addButtons) addButton.interactable = false;
            }
        }
    }
}
