using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GadgetCore
{
    public static class InstanceTracker
    {
        public static GameObject mainCamera { get; internal set; }
        public static GameScript gameScript { get; internal set; }
        public static PlayerScript playerScript { get; internal set; }
        public static MenuScript menuScript { get; internal set; }
        public static Menuu menuu { get; internal set; }
        public static MusicBox musicBox { get; internal set; }
    }
}
