using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GadgetCore.API.ConfigMenu
{
    /// <summary>
    /// This is an implementation of <see cref="GadgetConfigComponent"/> that serves as a input field for representing an float-based config value.
    /// </summary>
    public class GadgetConfigFloatComponent : GadgetConfigComponent
    {
        /// <summary>
        /// The current value of this component.
        /// </summary>
        public float Value { get; protected set; }
        /// <summary>
        /// The minimum value of this component. If <see cref="MinValue"/> is not less than <see cref="MaxValue"/>, then they are ignored.
        /// </summary>
        public readonly float MinValue;
        /// <summary>
        /// The maximum value of this component. If <see cref="MinValue"/> is not less than <see cref="MaxValue"/>, then they are ignored.
        /// </summary>
        public readonly float MaxValue;
        /// <summary>
        /// The level of decimal precision to allow in the input. If this is negative, then it is ignored.
        /// </summary>
        public readonly int Decimals;
        /// <summary>
        /// The default value of this component.
        /// </summary>
        public readonly float? DefaultValue;
        /// <summary>
        /// The vanilla value of this component.
        /// </summary>
        public readonly float? VanillaValue;
        /// <summary>
        /// Whether this component is read-only.
        /// </summary>
        public readonly bool ReadOnly;

        private readonly Action<float> ValueSetter;

        /// <summary>
        /// Constructs a new <see cref="GadgetConfigFloatComponent"/> that serves as a input field for representing an float-based config value. The given <paramref name="valueSetter"/> will be called whenever the input field's contents are changed.
        /// </summary>
        public GadgetConfigFloatComponent(BasicGadgetConfigMenu configMenu, string name, float value, Action<float> valueSetter, float minValue = 0, float maxValue = 0, int decimals = -1, bool readOnly = false, float? defaultValue = null, float? vanillaValue = null, float height = 0.1f) : base(configMenu, name, height)
        {
            Value = value;
            MinValue = minValue;
            MaxValue = maxValue;
            Decimals = decimals;
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
            label.rectTransform.offsetMin = new Vector2(0f, 0f);
            label.rectTransform.offsetMax = new Vector2(-10f, 0f);
            label.text = nameString + ":";
            label.font = SceneInjector.ModConfigMenuText.GetComponent<TextMesh>().font;
            label.fontSize = 12;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.alignment = TextAnchor.MiddleLeft;
            InputField textbox = new GameObject("Textbox", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(InputField)).GetComponent<InputField>();
            textbox.GetComponent<RectTransform>().SetParent(parent);
            textbox.GetComponent<RectTransform>().anchorMin = new Vector2(0.25f, 0f);
            textbox.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1f);
            textbox.GetComponent<RectTransform>().offsetMin = new Vector2(10f, 0f);
            textbox.GetComponent<RectTransform>().offsetMax = new Vector2(0f, 0f);
            textbox.GetComponent<Image>().sprite = SceneInjector.BoxSprite;
            textbox.GetComponent<Image>().type = Image.Type.Sliced;
            textbox.GetComponent<Image>().fillCenter = true;
            Text placeholder = new GameObject("Placeholder", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
            placeholder.rectTransform.SetParent(textbox.transform);
            placeholder.rectTransform.anchorMin = new Vector2(0f, 0f);
            placeholder.rectTransform.anchorMax = new Vector2(1f, 1f);
            placeholder.rectTransform.offsetMin = new Vector2(10f, 0f);
            placeholder.rectTransform.offsetMax = new Vector2(-10f, 0f);
            placeholder.text = (DefaultValue != null ? "Default: " + DefaultValue + " " : "") + (VanillaValue != null ? "Vanilla: " + VanillaValue : "");
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
            text.rectTransform.offsetMin = new Vector2(10f, 0f);
            text.rectTransform.offsetMax = new Vector2(-10f, 0f);
            text.font = SceneInjector.ModConfigMenuText.GetComponent<TextMesh>().font;
            text.fontSize = 12;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.supportRichText = true;
            text.alignment = TextAnchor.MiddleLeft;
            textbox.textComponent = text;
            textbox.readOnly = ReadOnly;
            textbox.interactable = !ReadOnly;
            textbox.text = Value.ToString();
            textbox.characterValidation = InputField.CharacterValidation.Decimal;
            if (MaxValue > MinValue && MinValue > float.MinValue && MaxValue < float.MaxValue)
            {
                Slider slider = new GameObject("Slider", typeof(RectTransform), typeof(Slider)).GetComponent<Slider>();
                slider.transform.SetParent(parent);
                slider.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
                slider.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 0.5f);
                slider.GetComponent<RectTransform>().offsetMin = new Vector2(10f, -10f);
                slider.GetComponent<RectTransform>().offsetMax = new Vector2(-10f, 10f);
                Image sliderBackground = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
                sliderBackground.rectTransform.SetParent(slider.transform);
                sliderBackground.rectTransform.anchorMin = new Vector2(0f, 0.25f);
                sliderBackground.rectTransform.anchorMax = new Vector2(1f, 0.75f);
                sliderBackground.rectTransform.offsetMin = new Vector2(10f, 0f);
                sliderBackground.rectTransform.offsetMax = new Vector2(0f, 0f);
                sliderBackground.sprite = SceneInjector.BoxSprite;
                sliderBackground.type = Image.Type.Sliced;
                sliderBackground.fillCenter = true;
                RectTransform sliderHandleArea = new GameObject("Handle Slide Area", typeof(RectTransform)).GetComponent<RectTransform>();
                sliderHandleArea.SetParent(slider.transform);
                sliderHandleArea.anchorMin = Vector2.zero;
                sliderHandleArea.anchorMax = Vector2.one;
                sliderHandleArea.offsetMin = new Vector2(10f, 0f);
                sliderHandleArea.offsetMax = new Vector2(0f, 0f);
                Image sliderHandle = new GameObject("Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
                sliderHandle.rectTransform.SetParent(sliderHandleArea);
                sliderHandle.rectTransform.anchorMin = new Vector2(0f, 0f);
                sliderHandle.rectTransform.anchorMax = new Vector2(0f, 1f);
                sliderHandle.rectTransform.offsetMin = new Vector2(-10f, 0f);
                sliderHandle.rectTransform.offsetMax = new Vector2(10f, 0f);
                sliderHandle.sprite = SceneInjector.BoxSprite;
                sliderHandle.type = Image.Type.Sliced;
                sliderHandle.fillCenter = true;
                slider.minValue = MinValue;
                slider.maxValue = MaxValue;
                slider.wholeNumbers = false;
                slider.value = Value;
                slider.handleRect = sliderHandle.rectTransform;
                slider.onValueChanged.AddListener((value) =>
                {
                    Value = Mathf.Clamp(Decimals > -1 ? (float)Math.Round(value, Decimals) : value, MinValue, MaxValue);
                    textbox.text = Value.ToString();
                    slider.value = Value;
                    ValueSetter(Value);
                });
                textbox.onEndEdit.AddListener((value) =>
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        if (float.TryParse(value, out float parsedValue))
                        {
                            Value = Mathf.Clamp(Decimals > -1 ? (float)Math.Round(parsedValue, Decimals) : parsedValue, MinValue, MaxValue);
                            textbox.text = Value.ToString();
                            slider.value = Value;
                            ValueSetter(Value);
                        }
                        else
                        {
                            textbox.text = value[0] == '-' ? MinValue.ToString() : MaxValue.ToString();
                        }
                    }
                });
            }
            else
            {
                textbox.onEndEdit.AddListener((value) =>
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        if (float.TryParse(value, out float parsedValue))
                        {
                            Value = Decimals > -1 ? (float)Math.Round(parsedValue, Decimals) : parsedValue;
                            if (MaxValue > MinValue) Value = Value < MinValue ? MinValue : Value > MaxValue ? MaxValue : Value;
                            textbox.text = Value.ToString();
                            ValueSetter(Value);
                        }
                        else
                        {
                            textbox.text = value[0] == '-' ? MaxValue > MinValue ? MinValue.ToString() : float.MinValue.ToString() : MaxValue > MinValue ? MaxValue.ToString() : float.MinValue.ToString();
                        }
                    }
                });
            }
        }
    }
}
