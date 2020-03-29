using UnityEngine;
using UnityEngine.UI;

namespace GadgetCore.API.ConfigMenu
{
    /// <summary>
    /// This is an implementation of <see cref="GadgetConfigComponent"/> that simply displays a piece of text.
    /// </summary>
    public class GadgetConfigLabelComponent : GadgetConfigComponent
    {
        /// <summary>
        /// The text that is displayed by this label.
        /// </summary>
        public string Text { get; protected set; }
        
        /// <summary>
        /// Whether the text height may be resized in the case of text overflow. Be aware that this resizing operation is not perfect - as such, you should avoid using it when possible.
        /// </summary>
        public bool AllowHeightResize { get; protected set; }

        /// <summary>
        /// The height of this component before resizing the height to match the text height.
        /// </summary>
        protected float InitialHeight;

        /// <summary>
        /// Constructs a new <see cref="GadgetConfigLabelComponent"/> that simply displays a piece of text.
        /// </summary>
        public GadgetConfigLabelComponent(BasicGadgetConfigMenu configMenu, string name, string text, float height = 0.05f, bool allowHeightResize = false) : base(configMenu, name, height)
        {
            Text = text;
            AllowHeightResize = allowHeightResize;
            InitialHeight = height;
        }

        /// <summary>
        /// Called when <see cref="BasicGadgetConfigMenu.Build"/> is called on the config menu containing this component.
        /// </summary>
        public override void Build(RectTransform parent)
        {
            Text label = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
            label.rectTransform.SetParent(parent);
            label.rectTransform.anchorMin = new Vector2(0f, 0f);
            label.rectTransform.anchorMax = new Vector2(1f, 1f);
            label.rectTransform.offsetMin = new Vector2(0, 0);
            label.rectTransform.offsetMax = new Vector2(0, 0);
            label.text = Text;
            label.font = SceneInjector.ModConfigMenuText.GetComponent<TextMesh>().font;
            label.fontSize = 12;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = AllowHeightResize ? VerticalWrapMode.Overflow : VerticalWrapMode.Truncate;
            label.alignment = TextAnchor.MiddleLeft;
            if (AllowHeightResize)
            {
                Height = InitialHeight * Mathf.CeilToInt(label.preferredHeight / label.fontSize);
            }
        }
    }
}
