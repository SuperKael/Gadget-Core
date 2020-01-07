using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GadgetCore.API.ConfigMenu
{
    /// <summary>
    /// This is an implementation of <see cref="GadgetConfigComponent"/> that creates a separator in the config menu.
    /// </summary>
    public class GadgetConfigSeparatorComponent : GadgetConfigComponent
    {
        /// <summary>
        /// Constructs a new <see cref="GadgetConfigSeparatorComponent"/> that creates a separator in the config menu.
        /// </summary>
        public GadgetConfigSeparatorComponent(BasicGadgetConfigMenu configMenu, string name, float height = 0.05f) : base(configMenu, name, height) { }

        /// <summary>
        /// Called when <see cref="BasicGadgetConfigMenu.Build"/> is called on the config menu containing this component.
        /// </summary>
        public override void Build(RectTransform parent)
        {
            Image seperator = new GameObject("Textbox", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
            seperator.GetComponent<RectTransform>().SetParent(parent);
            seperator.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.2f);
            seperator.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 0.8f);
            seperator.GetComponent<RectTransform>().offsetMin = new Vector2(-5, 0);
            seperator.GetComponent<RectTransform>().offsetMax = new Vector2(5, 0);
            seperator.sprite = SceneInjector.BarSprite;
            seperator.type = Image.Type.Sliced;
            seperator.fillCenter = true;
        }
    }
}
