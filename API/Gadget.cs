using GadgetCore.API.ConfigMenu;
using GadgetCore.Loader;
using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace GadgetCore.API
{
    /// <summary>
    /// At least one class in your mod must extend this for your mod to be identified by Gadget Core. Must also have the <see cref="GadgetAttribute">Gadget</see> Attribute.
    /// May have multiple Gadgets in one mod - They will be able to be individually enabled or disabled in the ingame mod manager.
    /// </summary>
    public abstract class Gadget
    {
        /// <summary>
        /// Represents whether this Gadget is enabled. Note that mods can be enabled and disabled in the mod menu.
        /// </summary>
        public bool Enabled { get; internal set; } = false;
        /// <summary>
        /// The Mod ID assigned to this mod. It represents the index of the mod in <see cref="Gadgets"/>' list.
        /// </summary>
        public int ModID { get; internal set; } = -1;
        /// <summary>
        /// The Logger used by this Gadget.
        /// </summary>
        public GadgetLogger Logger { get; internal set; }
        /// <summary>
        /// The config used by this Gadget. Can use to write to your own config.
        /// </summary>
        public GadgetConfig Config { get; internal set; }
        /// <summary>
        /// The harmony instances used by this Gadget.
        /// </summary>
        public Harmony HarmonyInstance { get; internal set; }
        /// <summary>
        /// The reference to this Gadget's <see cref="GadgetInfo"/>.
        /// </summary>
        public GadgetInfo Info { get; internal set; }

        /// <summary>
        /// Override to add custom <see cref="Registry"/>s to the game.
        /// </summary>
        public virtual Registry[] CreateRegistries()
        {
            return new Registry[0];
        }

        /// <summary>
        /// Call to trigger a config reload. If this mod has <see cref="GadgetAttribute.AllowConfigReloading"/> set to false, then this will throw an exception.
        /// </summary>
        public void ReloadConfig()
        {
            if (!Info.Attribute.AllowConfigReloading) throw new InvalidOperationException("This mod (" + Info.Attribute.Name + ") does not support config reloading!");
            LoadConfig();
        }

        /// <summary>
        /// Called to load this gadget's config from the gadget's <see cref="Config"/> instance.
        /// </summary>
        protected internal virtual void LoadConfig() { }
        /// <summary>
        /// Called during gadget initialization, before Harmony patches are applied.
        /// </summary>
        protected internal virtual void PrePatch() { }
        /// <summary>
        /// Called during gadget initialization. All data registration should be done from this method.
        /// </summary>
        protected internal abstract void Initialize();
        /// <summary>
        /// Called when this gadget is unloaded or reloaded. 
        /// </summary>
        protected internal virtual void Unload() { }
        /// <summary>
        /// Called when the gadget hook script starts.
        /// </summary>
        protected internal virtual void ScriptStart() { }
        /// <summary>
        /// Called when the gadget hook script has an update.
        /// </summary>
        protected internal virtual void ScriptUpdate() { }
        /// <summary>
        /// Called when the gadget hook script has a fixed update.
        /// </summary>
        protected internal virtual void ScriptFixedUpdate() { }

        /// <summary>
        /// Returns the description of this Gadget. By default, returns null, which will cause the Gadget's ModInfo to be used as its description.
        /// </summary>
        public virtual string GetModDescription()
        {
            return null;
        }

        /// <summary>
        /// Returns a list of all previous names this Gadget has had. If you must change the name of your Gadget, use this to prevent registry data corruption. Null is not a valid return value, return an empty array if your Gadget has had no previous names.
        /// </summary>
        public virtual string[] GetPreviousModNames()
        {
            return new string[0];
        }

        /// <summary>
        /// Returns the version string of this Gadget. By default, returns the value of the AssemblyFileVersion attribute.
        /// </summary>
        public virtual string GetModVersionString()
        {
            return (GetType().Assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false)[0] as AssemblyFileVersionAttribute).Version;
        }

        /// <summary>
        /// Returns an <see cref="IGadgetConfigMenu"/> for this Gadget. By default, returns a <see cref="INIGadgetConfigMenu"/> for this Gadget's UMF config file. May return null if the Gadget should not have a config menu.
        /// </summary>
        public virtual IGadgetConfigMenu GetConfigMenu()
        {
            try
            {
                return new INIGadgetConfigMenu(Regex.Replace(Info.Attribute.Name, @"\s+", ""), false, Path.Combine(GadgetPaths.ConfigsPath, Assembly.GetAssembly(GetType()).GetName().Name) + ".ini", Info.Attribute.AllowConfigReloading ? ModMenuController.modEntries[GadgetMods.IndexOfMod(Info.Mod)] : null);
            }
            catch (InvalidOperationException e)
            {
                if (e.Message == INIGadgetConfigMenu.NO_CONFIGURABLE_DATA)
                {
                    return null;
                }
                else throw;
            }
        }

        internal virtual void CreateSingleton(Gadget singleton) { }
    }

    /// <summary>
    /// At least one class in your mod must extend this for your mod to be identified by Gadget Core. Must also have the <see cref="GadgetAttribute">Gadget</see> Attribute.
    /// May have multiple Gadgets in one mod - They will be able to be individually enabled or disabled in the ingame mod manager.
    /// 
    /// This self-referencing generic-form Gadget includes a "GetSingleton" method.
    /// </summary>
    public abstract class Gadget<T> : Gadget where T : Gadget<T>
    {
        internal static Gadget<T> singleton;

        /// <summary>
        /// Returns this <see cref="Gadget{T}"/>'s singleton.
        /// </summary>
        public static Gadget<T> GetSingleton()
        {
            return singleton;
        }

        /// <summary>
        /// Returns this <see cref="Gadget{T}"/>'s logger.
        /// </summary>
        public static GadgetLogger GetLogger()
        {
            return singleton.Logger;
        }

        internal sealed override void CreateSingleton(Gadget singleton)
        {
            Gadget<T>.singleton = singleton as Gadget<T>;
        }
    }
}
