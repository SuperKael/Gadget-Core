using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GadgetCore.API
{
    /// <summary>
    /// Provides access to instances of some commonly used scripts. Be wary of using these during scene load, as it may take a moment for their values to get set.
    /// </summary>
    public static class InstanceTracker
    {
        /// <summary>
        /// The Main Camera.
        /// </summary>
        public static Camera MainCamera { get; internal set; }
        /// <summary>
        /// The GameScript. Only valid when in scene 1.
        /// </summary>
        public static GameScript GameScript { get; internal set; }
        /// <summary>
        /// The PlayerScript. Note that there may be multiple PlayerScripts at one time in multiplayer, but this one is for your own player. Only valid when in scene 1.
        /// </summary>
        public static PlayerScript PlayerScript { get; internal set; }
        /// <summary>
        /// The MenuScript. Note that this is used for the in-game pause menu, and the singleplayer/multiplayer choice. Only valid when in scene 1.
        /// </summary>
        public static MenuScript MenuScript { get; internal set; }
        /// <summary>
        /// The Menuu. Note that this is used for everything on the title screen. Only valid when in scene 0.
        /// </summary>
        public static Menuu Menuu { get; internal set; }
        /// <summary>
        /// The MusicBox. This is used to control what music is playing.
        /// </summary>
        public static MusicBox MusicBox { get; internal set; }
    }
}
