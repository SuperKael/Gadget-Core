using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GadgetCore.API
{
    /// <summary>
    /// Extend this class to have your mod be identified by Gadget Core. Must also have the <see cref="GadgetModAttribute">GadgetMod</see> Attribute.
    /// May have multiple GadgetMods in one uMod Framework mod - They will be able to be individually toggled in the ingame mod manager.
    /// </summary>
    public abstract class GadgetMod
    {
        /// <summary>
        /// Represents whether this GadgetMod is enabled. Note that this value can be changed on the mod menu, although the game will be forced to restart shortly after.
        /// </summary>
        public bool Enabled { get; internal set; } = false;
        /// <summary>
        /// The Mod ID assigned to this mod. It represents the index of the mod in GadgetMods's list.
        /// </summary>
        public int ModID { get; internal set; } = -1;
        /// <summary>
        /// Override 
        /// </summary>
        /// <returns></returns>
        public virtual Registry[] CreateRegistries()
        {
            return new Registry[0];
        }

        /// <summary>
        /// Called during mod initialization. All data registration should be done from this method.
        /// </summary>
        public abstract void Initialize();
        /// <summary>
        /// Called when the gadget mod hook script starts.
        /// </summary>
        public virtual void ScriptStart() { }
        /// <summary>
        /// Called when the gadget mod hook script has an update.
        /// </summary>
        public virtual void ScriptUpdate() { }
        /// <summary>
        /// Called when the gadget mod hook script has a fixed update.
        /// </summary>
        public virtual void ScriptFixedUpdate() { }

        /// <summary>
        /// Returns the description of this mod. By default, returns null, which will cause the mod's ModInfo to be used as its description.
        /// </summary>
        public virtual string GetModDescription()
        {
            return null;
        }

        /// <summary>
        /// Returns a list of all previous names this mod has had. If you must change the name of your mod, use this to prevent registry data corruption. Null is not a valid return value, return an empty array if your mod has had no previous names.
        /// </summary>
        public virtual string[] GetPreviousModNames()
        {
            return new string[0];
        }

        /// <summary>
        /// Returns the version string of this mod. By default, returns the value of the AssemblyFileVersion attribute.
        /// </summary>
        public virtual string GetModVersionString()
        {
            return (GetType().Assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false)[0] as AssemblyFileVersionAttribute).Version;
        }
    }
}
