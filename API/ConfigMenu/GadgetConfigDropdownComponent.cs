using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GadgetCore.API.ConfigMenu
{
    /// <summary>
    /// This is an implementation of <see cref="GadgetConfigComponent"/> that serves as a dropdown for selecting from an array of choices.
    /// Based on code provided by "Zariteis" on the New Roguelands Discord server.
    /// </summary>
    public class GadgetConfigDropdownComponent : GadgetConfigComponent
    {
        /// <summary>
        /// The current value of this component.
        /// </summary>
        public string Value { get; protected set; }
        /// <summary>
        /// The selectable values of this component.
        /// </summary>
        public string[] Values { get; protected set; }
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
        private RectTransform[] Entries;

        /// <summary>
        /// Constructs a new <see cref="GadgetConfigDropdownComponent"/> that serves as a dropdown for selecting from an array of choices. The given <paramref name="valueSetter"/> will be called whenever the dropdown's selection is changed.
        /// </summary>
        public GadgetConfigDropdownComponent(BasicGadgetConfigMenu configMenu, string name, string value, string[] values, Action<string> valueSetter, bool readOnly = false, string defaultValue = null, string vanillaValue = null, float height = 0.1f) : base(configMenu, name, height)
        {
            Value = value;
            Values = values;
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
            var textureMenuTile = GadgetCoreAPI.LoadTexture2D("menu_tile.png");
            var MenuTile = Sprite.Create(textureMenuTile, SceneInjector.BoxSprite.rect, SceneInjector.BoxSprite.pivot, SceneInjector.BoxSprite.pixelsPerUnit, default, default, SceneInjector.BoxSprite.border);
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
            if (!ReadOnly)
            {
                button.GetComponent<Button>().onClick.AddListener(() =>
                {
                    bool set = Entries[0] != null && !Entries[0].gameObject.active;
                    for (int i = 0; i < Entries.Length; i++)
                    {
                        Entries[i].gameObject.SetActive(set);
                    }
                    parent.SetAsLastSibling();
                });
            }
            button.GetComponent<Button>().interactable = !ReadOnly;
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
            buttonLabel.text = Value;
            Entries = new RectTransform[Values.Length];
            for (int i = 0; i < Values.Length; i++)
            {
                int ic = i;
                RectTransform dropButton = new GameObject("Button", typeof(RectTransform), typeof(Button), typeof(CanvasRenderer), typeof(Image)).GetComponent<RectTransform>();
                dropButton.SetParent(button);
                dropButton.anchorMin = new Vector2(0.25f, 0f);
                dropButton.anchorMax = new Vector2(0.75f, 1f);
                dropButton.offsetMin = new Vector2(-42, -24 * i - 24);
                dropButton.offsetMax = new Vector2(42, -24 * i - 24);
                dropButton.GetComponent<Image>().sprite = MenuTile;
                dropButton.GetComponent<Image>().type = Image.Type.Sliced;
                dropButton.GetComponent<Image>().fillCenter = true;
                if (!ReadOnly)
                {
                    dropButton.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        buttonLabel.text = Values[ic];
                        Value = Values[ic];
                        ValueSetter.Invoke(Values[ic]);
                        for (int j = 0; j < Entries.Length; j++)
                        {
                            Entries[j].gameObject.SetActive(false);
                            Entries[j].gameObject.GetComponent<RectTransform>().SetAsLastSibling();
                        }
                    });
                }
                dropButton.GetComponent<Button>().interactable = !ReadOnly;

                Text dropButtonLabel = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
                dropButtonLabel.rectTransform.SetParent(dropButton);
                dropButtonLabel.rectTransform.anchorMin = new Vector2(0f, 0f);
                dropButtonLabel.rectTransform.anchorMax = new Vector2(1f, 1f);
                dropButtonLabel.rectTransform.offsetMin = new Vector2(2.5f, 2.5f);
                dropButtonLabel.rectTransform.offsetMax = new Vector2(-2.5f, -2.5f);
                dropButtonLabel.font = SceneInjector.ModConfigMenuText.GetComponent<TextMesh>().font;
                dropButtonLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
                dropButtonLabel.verticalOverflow = VerticalWrapMode.Overflow;
                dropButtonLabel.alignment = TextAnchor.MiddleCenter;
                dropButtonLabel.text = Values[i];
                Entries[i] = dropButton;
                dropButton.gameObject.SetActive(false);
            }
        }
    }
}
