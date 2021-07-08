using GadgetCore.API;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UModFramework.API;

namespace GadgetCore
{
    internal class UMFAPI : IUMFAPI
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public string GetConfigsPath()
        {
            return UMFData.ConfigsPath;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string GetModInfosPath()
        {
            return UMFData.ModInfosPath;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string GetLibrariesPath()
        {
            return UMFData.LibrariesPath;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public List<string> GetModLibraries()
        {
            try
            {
                return GetModLibraries_Internal();
            }
            catch (MissingMethodException)
            {
                return new List<string>();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private List<string> GetModLibraries_Internal()
        {
            return UMFData.ModLibraries;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public List<string> GetModNames()
        {
            return UMFData.ModNames;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public List<string> GetModNamesEnabled()
        {
            return UMFData.ModNamesEnabled;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public List<string> GetModNamesMissingDependencies()
        {
            try
            {
                return GetModNamesMissingDependencies_Internal();
            }
            catch (MissingMethodException)
            {
                return new List<string>();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private List<string> GetModNamesMissingDependencies_Internal()
        {
            return UMFData.ModNamesMissingDependencies;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public List<Assembly> GetMods()
        {
            return UMFData.Mods;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string GetModsPath()
        {
            return UMFData.ModsPath;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string GetUMFPath()
        {
            return UMFData.UMFPath;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Version GetVersion()
        {
            return UMFData.Version;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string GetDisabledModsFile()
        {
            return UMFData.DisabledModsFile;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public bool IsLibrary(string modName)
        {
            try
            {
                return IsLibrary_Internal(modName);
            }
            catch (MissingMethodException)
            {
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool IsLibrary_Internal(string modName)
        {
            return UMFData.IsLibrary(modName);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void SendCommand(string command, bool openConsole = false)
        {
            UMFGUI.SendCommand(command, openConsole);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string GetModDescription(string modName, bool caseInsensitive = false)
        {
            return UMFMod.GetModDescription(modName, caseInsensitive);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Version GetModVersion(string modName)
        {
            return UMFMod.GetModVersion(modName);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Assembly GetModAssembly(string modName)
        {
            return UMFMod.GetMod(modName);
        }
    }
}
