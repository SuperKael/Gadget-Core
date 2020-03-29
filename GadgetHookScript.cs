using GadgetCore.API;
using UnityEngine;

namespace GadgetCore
{
    /// <summary>
    /// This component is used to hook a Gadget into Unity's update cycle.
    /// </summary>
    public class GadgetHookScript : MonoBehaviour
    {
        /// <summary>
        /// The GadgetInfo that is updated by this hook script.
        /// </summary>
        public GadgetInfo Mod { get; internal set; }
        internal void Start()
        {
            if (Mod == null || Mod.Gadget == null)
            {
                DestroyImmediate(this);
                return;
            }
            if (Mod.Gadget.Enabled) Mod.Gadget.ScriptStart();
        }
        internal void Update()
        {
            if (Mod.Gadget.Enabled) Mod.Gadget.ScriptUpdate();
        }
        internal void FixedUpdate()
        {
            if (Mod.Gadget.Enabled) Mod.Gadget.ScriptFixedUpdate();
        }
    }
}
