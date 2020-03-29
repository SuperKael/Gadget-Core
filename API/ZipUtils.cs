using Ionic.Zip;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace GadgetCore.API
{
    /// <summary>
    /// Utilities for handling packaged UMF mod zip files. Should only be used if UMF is installed.
    /// </summary>
    public static class UMFZipUtils
    {
        internal static Process coreLibInstallerProcess;

        /// <summary>
        /// Unpacks the .zip mod with the specified name.
        /// </summary>
        public static bool UnpackMod(string name)
        {
            string[] mods = Directory.GetFiles(GadgetPaths.ModsPath, name + "*.zip");
            if (mods.Length >= 1)
            {
                return UnpackModFile(mods[0]);
            }
            else
            {
                mods = Directory.GetFiles(Path.Combine(GadgetPaths.ModsPath, "PackedMods"), name + "*.zip");
                if (mods.Length >= 1)
                {
                    return UnpackModFile(mods[0]);
                }
            }
            return false;
        }

        /// <summary>
        /// Unpacks the mod with the specified file path.
        /// </summary>
        public static bool UnpackModFile(string filePath)
        {
            try
            {
                using (ZipFile modZip = ZipFile.Read(filePath))
                {
                    if (File.Exists(Path.Combine(GadgetPaths.ModsPath, "ModInfo.txt")))
                    {
                        File.Move(Path.Combine(GadgetPaths.ModsPath, "ModInfo.txt"), Path.Combine(GadgetPaths.TempPath, "ModInfoBackup.txt"));
                    }
                    modZip.ExtractAll(GadgetPaths.ModsPath, ExtractExistingFileAction.OverwriteSilently);
                    string dllFile = null;
                    foreach (string file in Directory.GetFiles(GadgetPaths.ModsPath, "*.dll"))
                    {
                        if (Path.GetFileNameWithoutExtension(filePath).StartsWith(Path.GetFileNameWithoutExtension(file)))
                        {
                            dllFile = file;
                            break;
                        }
                    }
                    if (dllFile == null) GadgetCore.Log("WARNING! Mod .zip " + Path.GetFileName(filePath) + " is being unpacked, but it does not contain a .dll whos name matches the .zip!");
                    if (Directory.Exists(Path.Combine(GadgetPaths.ModsPath, "Tools")))
                    {
                        foreach (string file in Directory.GetFiles(Path.Combine(GadgetPaths.ModsPath, "Tools")))
                        {
                            string fileName = Path.Combine(Path.Combine(GadgetCore.UMFAPI.GetUMFPath(), "Tools"), Path.GetFileName(file));
                            if (File.Exists(fileName))
                            {
                                File.Delete(fileName);
                            }
                            File.Move(file, fileName);
                        }
                        Directory.Delete(Path.Combine(GadgetPaths.ModsPath, "Tools"), true);
                    }
                    if (Directory.Exists(Path.Combine(GadgetPaths.ModsPath, "CoreLibs")) && coreLibInstallerProcess == null)
                    {
                        if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                        {
                            ProcessStartInfo coreLibInstallerPI = new ProcessStartInfo
                            {
                                FileName = "mono",
                                Arguments = Path.Combine(Path.Combine(GadgetCore.UMFAPI.GetUMFPath(), "Tools"), "GadgetCore.CoreLibInstaller.exe") + " " + Process.GetCurrentProcess().Id,
                                WindowStyle = ProcessWindowStyle.Hidden,
                                ErrorDialog = true,
                                UseShellExecute = false,
                            };
                            coreLibInstallerPI.WorkingDirectory = Path.GetDirectoryName(coreLibInstallerPI.FileName);
                            coreLibInstallerProcess = Process.Start(coreLibInstallerPI);
                        }
                        else
                        {
                            ProcessStartInfo coreLibInstallerPI = new ProcessStartInfo
                            {
                                FileName = Path.Combine(Path.Combine(GadgetCore.UMFAPI.GetUMFPath(), "Tools"), "GadgetCore.CoreLibInstaller.exe"),
                                Arguments = Process.GetCurrentProcess().Id.ToString(),
                                WindowStyle = ProcessWindowStyle.Hidden,
                                ErrorDialog = true,
                                UseShellExecute = false,
                            };
                            coreLibInstallerPI.WorkingDirectory = Path.GetDirectoryName(coreLibInstallerPI.FileName);
                            coreLibInstallerProcess = Process.Start(coreLibInstallerPI);
                        }
                    }
                    if (File.Exists(Path.Combine(GadgetPaths.ModsPath, "ModInfo.txt")))
                    {
                        string modInfoName;
                        if (dllFile != null)
                        {
                            string trimmedVersionNumber = FileVersionInfo.GetVersionInfo(dllFile).ToString().Split('.').TakeWhile(x => !x.Equals("0")).Aggregate(new StringBuilder(), (a, b) => { if (a.Length > 0) a.Append("."); a.Append(b); return a; }).ToString();
                            modInfoName = Path.GetFileNameWithoutExtension(dllFile) + "_v" + FileVersionInfo.GetVersionInfo(dllFile);
                        }
                        else
                        {
                            modInfoName = Path.GetFileNameWithoutExtension(filePath);
                        }
                        if (File.Exists(Path.Combine(GadgetCore.UMFAPI.GetModInfosPath(), modInfoName + "_ModInfo.txt"))) File.Delete(Path.Combine(GadgetCore.UMFAPI.GetModInfosPath(), modInfoName + "_ModInfo.txt"));
                        File.Move(Path.Combine(GadgetPaths.ModsPath, "ModInfo.txt"), Path.Combine(GadgetCore.UMFAPI.GetModInfosPath(), modInfoName + "_ModInfo.txt"));
                    }
                    if (File.Exists(Path.Combine(GadgetPaths.TempPath, "ModInfoBackup.txt")))
                    {
                        File.Move(Path.Combine(GadgetPaths.TempPath, "ModInfoBackup.txt"), Path.Combine(GadgetPaths.ModsPath, "ModInfo.txt"));
                    }
                }
                File.SetAttributes(filePath, FileAttributes.Normal);
                File.Delete(filePath);
                return true;
            }
            catch (Exception e)
            {
                GadgetCore.Log("Error unpacking mod '" + filePath.Split('\\', '/').Last() + "': " + e.ToString());
                return false;
            }
        }
    }
}
