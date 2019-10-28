using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GadgetCore.API
{
    /// <summary>
    /// Extend this class to have your mod be identified by Gadget Core. Must also have the <see cref="GadgetModAttribute">GadgetMod</see> Attribute.
    /// May have multiple GadgetMods in one uMod Framework mod - They will be able to be individually toggled in the ingame mod manager.
    /// </summary>
    public abstract class GadgetMod
    {
        public bool Enabled { get; internal set; } = false;
        public int ModID { get; internal set; } = -1;
        public virtual Registry[] CreateRegistries()
        {
            return new Registry[0];
        }

        public abstract void Initialize();
        public virtual void ScriptStart() { }
        public virtual void ScriptUpdate() { }
        public virtual void ScriptFixedUpdate() { }
        public virtual string GetModDescription()
        {
            return null;
        }
    }
}
