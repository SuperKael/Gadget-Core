using UnityEngine;

namespace GadgetCore.API.ConfigMenu
{
    /// <summary>
    /// This is an implementation of <see cref="GadgetConfigComponent"/> that creates a blank space in the config menu.
    /// </summary>
    public class GadgetConfigSpacerComponent : GadgetConfigComponent
    {
        /// <summary>
        /// Constructs a new <see cref="GadgetConfigSpacerComponent"/> that creates a blank space in the config menu.
        /// </summary>
        public GadgetConfigSpacerComponent(BasicGadgetConfigMenu configMenu, string name, float height = 0.05f) : base(configMenu, name, height) { }

        /// <summary>
        /// Called when <see cref="BasicGadgetConfigMenu.Build"/> is called on the config menu containing this component.
        /// </summary>
        public override void Build(RectTransform parent)
        {
            
        }
    }
}
