using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GadgetCore.API.ConfigMenu
{
    /// <summary>
    /// A component intended to be used in a <see cref="BasicGadgetConfigMenu"/> representing a single config entry.
    /// </summary>
    public abstract class GadgetConfigComponent
    {
        /// <summary>
        /// The <see cref="BasicGadgetConfigMenu"/> that this component is a part of.
        /// </summary>
        public BasicGadgetConfigMenu ConfigMenu { get; protected set; }

        /// <summary>
        /// The name of this component.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Indicates how tall this component is, as a percentage of the total height of the config menu.
        /// </summary>
        public float Height { get; protected set; }

        /// <summary>
        /// Specifies the alignment of this component. 
        /// </summary>
        public GadgetConfigComponentAlignment Alignment { get; protected set; } = GadgetConfigComponentAlignment.STANDARD;

        /// <summary>
        /// Constructs a new component with the given name and height.
        /// </summary>
        protected GadgetConfigComponent(BasicGadgetConfigMenu configMenu, string name, float height)
        {
            ConfigMenu = configMenu;
            Name = name;
            Height = height;
        }

        /// <summary>
        /// Called when <see cref="BasicGadgetConfigMenu.Build"/> is called on the config menu containing this component.
        /// </summary>
        public abstract void Build(RectTransform parent);

        /// <summary>
        /// Called when <see cref="BasicGadgetConfigMenu.Render"/> is called on the config menu containing this component.
        /// </summary>
        public virtual void Render() { }

        /// <summary>
        /// Called when <see cref="BasicGadgetConfigMenu.Derender"/> is called on the config menu containing this component.
        /// </summary>
        public virtual void Derender() { }

        /// <summary>
        /// Called when <see cref="BasicGadgetConfigMenu.Update"/> is called on the config menu containing this component.
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// Use to change the height of this component. Will trigger the config menu to update all components. As such, do not call this as part of <see cref="Update"/>, or you will cause a <see cref="StackOverflowException"/>.
        /// </summary>
        protected void UpdateHeight(float height)
        {
            Height = height;
            ConfigMenu.Update();
        }

        /// <summary>
        /// Sets the alignment of this component. You should not call this yourself, and this must not be called after the component has been built.
        /// </summary>
        public void SetAlignment(GadgetConfigComponentAlignment alignment)
        {
            if (ConfigMenu.MenuParent != null) throw new InvalidOperationException("A GadgetConfigComponent's alignment cannot be set after it has been built!");
            Alignment = alignment;
        }
    }

    /// <summary>
    /// Specifies the alignment of a component in the config menu.
    /// </summary>
    public enum GadgetConfigComponentAlignment
    {
        /// <summary>
        /// The component will be positioned in the main body of the config menu.
        /// </summary>
        STANDARD,
        /// <summary>
        /// The component will be positioned as a non-scrolling header to the config menu. 
        /// </summary>
        HEADER,
        /// <summary>
        /// The component will be positioned as a non-scrolling footer to the config menu.
        /// </summary>
        FOOTER
    }
}