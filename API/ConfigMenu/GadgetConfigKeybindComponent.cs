using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GadgetCore.API.ConfigMenu
{
    /// <summary>
    /// This is an implementation of <see cref="GadgetConfigComponent"/> that serves as a button where you can enter a keybind.
    /// </summary>
    public class GadgetConfigKeybindComponent : GadgetConfigComponent
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
        /// Constructs a new <see cref="GadgetConfigKeybindComponent"/> that serves as a button where you can enter a keybind. The given <paramref name="valueSetter"/> will be called whenever the keybind is changed.
        /// </summary>
        public GadgetConfigKeybindComponent(BasicGadgetConfigMenu configMenu, string name, string value, Action<string> valueSetter, bool readOnly = false, string defaultValue = null, string vanillaValue = null, float height = 0.1f) : base(configMenu, name, height)
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
            Text defaults = new GameObject("Defaults", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
            defaults.rectTransform.SetParent(parent);
            defaults.rectTransform.anchorMin = new Vector2(0.5f, 0f);
            defaults.rectTransform.anchorMax = new Vector2(1f, 1f);
            defaults.rectTransform.offsetMin = new Vector2(10, 0);
            defaults.rectTransform.offsetMax = new Vector2(0, 0);
            defaults.text = (DefaultValue != null ? "Default: " + DefaultValue + " " : "") + (VanillaValue != null ? "Vanilla: " + VanillaValue : "");
            defaults.font = SceneInjector.ModConfigMenuText.GetComponent<TextMesh>().font;
            defaults.fontSize = 12;
            defaults.horizontalOverflow = HorizontalWrapMode.Wrap;
            defaults.alignment = TextAnchor.MiddleLeft;
            RectTransform button = new GameObject("Keybind Button", typeof(RectTransform), typeof(Toggle), typeof(CanvasRenderer), typeof(Image), typeof(Mask), typeof(KeybindToggle)).GetComponent<RectTransform>();
            button.SetParent(parent);
            button.GetComponent<RectTransform>().anchorMin = new Vector2(0.25f, 0f);
            button.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1f);
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
            buttonLabel.text = Value;
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
                Value = keybind;
                ValueSetter(Value);
            });
        }
    }
}
