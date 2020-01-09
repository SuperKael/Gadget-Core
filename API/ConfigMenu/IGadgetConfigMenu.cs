using UnityEngine;

namespace GadgetCore.API.ConfigMenu
{
    /// <summary>
    /// Interface for Gadget Config Menu renderers. Most likely, you would be better of using <see cref="BasicGadgetConfigMenu"/> or <see cref="UMFGadgetConfigMenu"/>.
    /// </summary>
    public interface IGadgetConfigMenu
    {
        /// <summary>
        /// Called for every configurable mod and Gadget when the Mod Menu is injected into the title screen. This is called again if the player returns to the title screen after having selected a character. You should never call this yourself.
        /// </summary>
        /// <param name="parent">A <see cref="RectTransform"/> on the Mod Menu canvas intended to be used as the parent object of your config menu. This object will have a large background <see cref="UnityEngine.UI.Image">Image</see> on it, intended to be the background of your config menu. Feel free to change or remove this background. This object will also be enabled and disabled as needed to open and close the config menu.</param>
        void Build(RectTransform parent);

        /// <summary>
        /// Called to make the config menu totally reset itself.
        /// </summary>
        void Reset();

        /// <summary>
        /// Called to make the config menu rebuild itself. 
        /// </summary>
        void Rebuild();

        /// <summary>
        /// Called whenever this mod's config menu is opened. The parent <see cref="RectTransform"/> that was passed to <see cref="Build(RectTransform)"/> will be enabled immediately after this method is called. You should never call this yourself.
        /// </summary>
        void Render();

        /// <summary>
        /// Called whenever this mod's config menu is closed. The parent <see cref="RectTransform"/> that was passed to <see cref="Build(RectTransform)"/> will be disabled immediately before this method is called. You should never call this yourself.
        /// </summary>
        void Derender();

        /// <summary>
        /// Called whenever the config menu's contents are updated on some way. You should probably call this yourself whenever the config menu's contents are updated somehow.
        /// </summary>
        void Update();

        /// <summary>
        /// Called when this config menu is opened, to check whether the mod menu behind your config menu should be hidden when your config menu is opened.
        /// </summary>
        bool ShouldHideModMenu();
    }
}
