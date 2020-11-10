using GadgetCore.API;
using System;
using System.Collections.Generic;
using System.Reflection;
using UModFramework.API;

namespace GadgetCore
{
    internal class UMFAPI : IUMFAPI
    {
        public string GetConfigsPath()
        {
            return UMFData.ConfigsPath;
        }

        public string GetModInfosPath()
        {
            return UMFData.ModInfosPath;
        }

        public string GetLibrariesPath()
        {
            return UMFData.LibrariesPath;
        }

        public List<string> GetModLibraries()
        {
            return UMFData.ModLibraries;
        }

        public List<string> GetModNames()
        {
            return UMFData.ModNames;
        }

        public List<string> GetModNamesEnabled()
        {
            return UMFData.ModNamesEnabled;
        }

        public List<string> GetModNamesMissingDependencies()
        {
            return UMFData.ModNamesMissingDependencies;
        }

        public List<Assembly> GetMods()
        {
            return UMFData.Mods;
        }

        public string GetModsPath()
        {
            return UMFData.ModsPath;
        }

        public string GetUMFPath()
        {
            return UMFData.UMFPath;
        }

        public Version GetVersion()
        {
            return UMFData.Version;
        }

        public string GetDisabledModsFile()
        {
            return UMFData.DisabledModsFile;
        }

        public bool IsLibrary(string modName)
        {
            return UMFData.IsLibrary(modName);
        }

        public void SendCommand(string command, bool openConsole = false)
        {
            UMFGUI.SendCommand(command, openConsole);
        }

        public string GetModDescription(string modName, bool caseInsensitive = false)
        {
            return UMFMod.GetModDescription(modName, caseInsensitive);
        }

        public Version GetModVersion(string modName)
        {
            return UMFMod.GetModVersion(modName);
        }

        public Assembly GetModAssembly(string modName)
        {
            return UMFMod.GetMod(modName);
        }
    }
}
