using GadgetCore.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GadgetCore
{
    /// <summary>
    /// This component is used to hook a Gadget mod into Unity's update cycle.
    /// </summary>
    public class GadgetModHookScript : MonoBehaviour
    {
        /// <summary>
        /// The GadgetModInfo that is updated by this hook script.
        /// </summary>
        public GadgetModInfo Mod { get; internal set; }
        internal void Start()
        {
            if (Mod == null || Mod.Mod == null)
            {
                DestroyImmediate(this);
                return;
            }
            if (Mod.Mod.Enabled) Mod.Mod.ScriptStart();
        }
        internal void Update()
        {
            if (Mod.Mod.Enabled) Mod.Mod.ScriptUpdate();
        }
        internal void FixedUpdate()
        {
            if (Mod.Mod.Enabled) Mod.Mod.ScriptFixedUpdate();
        }
    }
}
