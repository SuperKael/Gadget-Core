using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GadgetCore.API.ConfigMenu
{
    /// <summary>
    /// This is an implementation of <see cref="GadgetConfigComponent"/> that serves as a pair of toggles for representing a bool-based config value.
    /// </summary>
    public class GadgetConfigBoolComponent : GadgetConfigComponent
    {
        /// <summary>
        /// The current value of this component.
        /// </summary>
        public bool Value { get; protected set; }
        /// <summary>
        /// The default value of this component.
        /// </summary>
        public readonly bool? DefaultValue;
        /// <summary>
        /// The vanilla value of this component.
        /// </summary>
        public readonly bool? VanillaValue;
        /// <summary>
        /// Whether this component is read-only.
        /// </summary>
        public readonly bool ReadOnly;

        private readonly Action<bool> ValueSetter;

        /// <summary>
        /// Constructs a new <see cref="GadgetConfigBoolComponent"/> that serves as a pair of toggles for representing a bool-based config value. The given <paramref name="valueSetter"/> will be called whenever the value is changed.
        /// </summary>
        public GadgetConfigBoolComponent(BasicGadgetConfigMenu configMenu, string name, bool value, Action<bool> valueSetter, bool readOnly = false, bool? defaultValue = null, bool? vanillaValue = null, float height = 0.1f) : base(configMenu, name, height)
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
            ToggleGroup toggleGroup = new GameObject("ToggleGroup", typeof(RectTransform), typeof(ToggleGroup)).GetComponent<ToggleGroup>();
            toggleGroup.GetComponent<RectTransform>().SetParent(parent);
            toggleGroup.GetComponent<RectTransform>().anchorMin = new Vector2(0.25f, 0f);
            toggleGroup.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1f);
            toggleGroup.GetComponent<RectTransform>().offsetMin = new Vector2(10, 0);
            toggleGroup.GetComponent<RectTransform>().offsetMax = new Vector2(-10, 0);
            RectTransform trueButton = new GameObject("True", typeof(RectTransform), typeof(Toggle), typeof(CanvasRenderer), typeof(Image), typeof(Mask)).GetComponent<RectTransform>();
            trueButton.SetParent(toggleGroup.transform);
            trueButton.anchorMin = new Vector2(0f, 0f);
            trueButton.anchorMax = new Vector2(Math.Max(Math.Min(Height * 3f, 0.5f), 0.25f), 1f);
            trueButton.offsetMin = new Vector2(0, 0);
            trueButton.offsetMax = new Vector2(-2.5f, 0);
            Toggle trueToggle = trueButton.GetComponent<Toggle>();
            Image trueSelected = new GameObject("Selected", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
            trueSelected.rectTransform.SetParent(trueButton);
            trueSelected.rectTransform.anchorMin = new Vector2(0f, 0f);
            trueSelected.rectTransform.anchorMax = new Vector2(1f, 1f);
            trueSelected.rectTransform.offsetMin = Vector2.zero;
            trueSelected.rectTransform.offsetMax = Vector2.zero;
            trueSelected.sprite = SceneInjector.BoxSprite;
            trueSelected.type = Image.Type.Sliced;
            Text trueLabel = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
            trueLabel.rectTransform.SetParent(trueButton);
            trueLabel.rectTransform.anchorMin = new Vector2(0f, 0f);
            trueLabel.rectTransform.anchorMax = new Vector2(1f, 1f);
            trueLabel.rectTransform.offsetMin = new Vector2(2.5f, 2.5f);
            trueLabel.rectTransform.offsetMax = new Vector2(-2.5f, -2.5f);
            trueLabel.font = SceneInjector.ModConfigMenuText.GetComponent<TextMesh>().font;
            trueLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
            trueLabel.verticalOverflow = VerticalWrapMode.Overflow;
            trueLabel.alignment = TextAnchor.MiddleCenter;
            trueLabel.text = "T";
            trueToggle.GetComponent<Image>().sprite = SceneInjector.BoxSprite;
            trueToggle.GetComponent<Image>().type = Image.Type.Sliced;
            trueToggle.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.25f);
            trueToggle.transition = Selectable.Transition.None;
            trueToggle.isOn = Value;
            trueToggle.interactable = !ReadOnly;
            trueToggle.toggleTransition = Toggle.ToggleTransition.None;
            trueToggle.graphic = trueSelected;
            trueToggle.group = toggleGroup;
            trueToggle.onValueChanged.AddListener((toggled) => { if (toggled) ValueSetter(true); });
            RectTransform falseButton = new GameObject("False", typeof(RectTransform), typeof(Toggle), typeof(CanvasRenderer), typeof(Image), typeof(Mask)).GetComponent<RectTransform>();
            falseButton.SetParent(toggleGroup.transform);
            falseButton.anchorMin = new Vector2(Math.Max(Math.Min(Height * 3f, 0.5f), 0.25f), 0f);
            falseButton.anchorMax = new Vector2(Math.Max(Math.Min(Height * 3f, 0.5f), 0.25f) * 2, 1f);
            falseButton.offsetMin = new Vector2(2.5f, 0);
            falseButton.offsetMax = new Vector2(0, 0);
            Toggle falseToggle = falseButton.GetComponent<Toggle>();
            Image falseSelected = new GameObject("Selected", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
            falseSelected.rectTransform.SetParent(falseButton);
            falseSelected.rectTransform.anchorMin = new Vector2(0f, 0f);
            falseSelected.rectTransform.anchorMax = new Vector2(1f, 1f);
            falseSelected.rectTransform.offsetMin = Vector2.zero;
            falseSelected.rectTransform.offsetMax = Vector2.zero;
            falseSelected.sprite = SceneInjector.BoxSprite;
            falseSelected.type = Image.Type.Sliced;
            Text falseLabel = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
            falseLabel.rectTransform.SetParent(falseButton);
            falseLabel.rectTransform.anchorMin = new Vector2(0f, 0f);
            falseLabel.rectTransform.anchorMax = new Vector2(1f, 1f);
            falseLabel.rectTransform.offsetMin = new Vector2(2.5f, 2.5f);
            falseLabel.rectTransform.offsetMax = new Vector2(-2.5f, -2.5f);
            falseLabel.font = SceneInjector.ModConfigMenuText.GetComponent<TextMesh>().font;
            falseLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
            falseLabel.verticalOverflow = VerticalWrapMode.Overflow;
            falseLabel.alignment = TextAnchor.MiddleCenter;
            falseLabel.text = "F";
            falseToggle.GetComponent<Image>().sprite = SceneInjector.BoxSprite;
            falseToggle.GetComponent<Image>().type = Image.Type.Sliced;
            falseToggle.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.25f);
            falseToggle.transition = Selectable.Transition.None;
            falseToggle.isOn = !Value;
            falseToggle.interactable = !ReadOnly;
            falseToggle.toggleTransition = Toggle.ToggleTransition.None;
            falseToggle.graphic = falseSelected;
            falseToggle.group = toggleGroup;
            falseToggle.onValueChanged.AddListener((toggled) => { if (toggled) ValueSetter(false); });
        }
    }
}
