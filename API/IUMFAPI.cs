using System;
using System.Collections.Generic;
using System.Reflection;

namespace GadgetCore.API
{
    /// <summary>
    /// Interface for accessing UMF API functions while avoiding a hard assembly reference.
    /// </summary>
    public interface IUMFAPI
    {
        /// <summary>
        /// Gets the UMF Version.
        /// </summary>
		Version GetVersion();
        /// <summary>
        /// Gets the UMF Mods list.
        /// </summary>
		List<Assembly> GetMods();
        /// <summary>
        /// Gets the UMF ModNames list.
        /// </summary>
		List<string> GetModNames();
        /// <summary>
        /// Gets the UMF ModNamesEnabled list.
        /// </summary>
		List<string> GetModNamesEnabled();
        /// <summary>
        /// Gets the UMF ModNamesMissingDependencies list.
        /// </summary>
		List<string> GetModNamesMissingDependencies();
        /// <summary>
        /// Gets the UMF ModLibraries list.
        /// </summary>
        List<string> GetModLibraries();
        /// <summary>
        /// Gets the UMF directory path.
        /// </summary>
        string GetUMFPath();
        /// <summary>
        /// Gets the UMF Mods path.
        /// </summary>
        string GetModsPath();
        /// <summary>
        /// Gets the UMF ModInfos path.
        /// </summary>
        string GetModInfosPath();
        /// <summary>
        /// Gets the UMF Libraries path.
        /// </summary>
        string GetLibrariesPath();
        /// <summary>
        /// Gets the UMF Configs path.
        /// </summary>
        string GetConfigsPath();
        /// <summary>
        /// Gets the UMF disabled.txt file path.
        /// </summary>
        /// <returns></returns>
        string GetDisabledModsFile();
        /// <summary>
        /// Checks if the given mod name refers to a mod library.
        /// </summary>
        bool IsLibrary(string modName);
        /// <summary>
        /// Sends a command to the UMF console.
        /// </summary>
        void SendCommand(string command, bool openConsole = false);
        /// <summary>
        /// Gets the description of the mod with the given name.
        /// </summary>
        string GetModDescription(string modName, bool caseInsensitive = false);
        /// <summary>
        /// Gets the version of the mod with the given name.
        /// </summary>
        Version GetModVersion(string modName);
        /// <summary>
        /// Gets the assembly of the mod with the given name.
        /// </summary>
        Assembly GetModAssembly(string modName);
    }
}
