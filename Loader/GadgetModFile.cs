using GadgetCore.API;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GadgetCore.Loader
{
    /// <summary>
    /// A temporary reference to a file contained with a Gadget Mod. Since it is a temporary reference, you should call <see cref="Dispose"/> once you are done with it. Be aware that you should never write to this file.
    /// </summary>
    public class GadgetModFile : IDisposable
    {
        private static List<GadgetModFile> openReferences = new List<GadgetModFile>();

        /// <summary>
        /// The path to the file.
        /// </summary>
        public string FilePath
        {
            private set
            {
                m_FilePath = value;
            }
            get
            {
                if (Disposed) throw new ObjectDisposedException("FilePath is no longer valid on a disposed GadgetModFile!");
                return m_FilePath;
            }
        }

        private string m_FilePath;
        /// <summary>
        /// Indicates whether this file is contained within a .zip archive.
        /// </summary>
        public readonly bool IsArchivedFile;
        /// <summary>
        /// The <see cref="GadgetMod"/> that contains this file.
        /// </summary>
        public readonly GadgetMod Mod;
        /// <summary>
        /// Indicates whether this GadgetModFile has been disposed. If so, know that <see cref="FilePath"/> is no longer valid.
        /// </summary>
        public bool Disposed { get; private set; }

        internal GadgetModFile(string FilePath, bool IsArchivedFile, GadgetMod Mod)
        {
            this.FilePath = FilePath;
            this.IsArchivedFile = IsArchivedFile;
            this.Mod = Mod;
        }

        /// <summary>
        /// Reads all lines of the file.
        /// </summary>
        public string[] ReadAllLines()
        {
            return File.ReadAllLines(FilePath);
        }

        /// <summary>
        /// Reads all text of the file.
        /// </summary>
        public string ReadAllText()
        {
            return File.ReadAllText(FilePath);
        }

        /// <summary>
        /// Reads all bytes of the file.
        /// </summary>
        public byte[] ReadAllBytes()
        {
            return File.ReadAllBytes(FilePath);
        }

        /// <summary>
        /// Disposes of this temporary file reference.
        /// </summary>
        public void Dispose()
        {
            Disposed = true;
            if (IsArchivedFile)
            {
                File.Delete(m_FilePath);
                DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(m_FilePath));
                while (dir.FullName != GadgetPaths.TempPath && dir.GetFiles().Length == 0 && dir.GetDirectories().Length == 0)
                {
                    DirectoryInfo parent = dir.Parent;
                    dir.Delete();
                    dir = parent;
                }
            }
        }

        /// <summary>
        /// Disposes of this temporary file reference after a certain condition is fulfilled.
        /// </summary>
        public void DisposeOnCondition(Func<bool> condition)
        {
            CoroutineHooker.StartCoroutine(DisposeEnumerator(condition));
        }

        private IEnumerator DisposeEnumerator(Func<bool> condition)
        {
            yield return new WaitUntil(condition);
            Dispose();
        }

        internal static void DisposeAll()
        {
            foreach (GadgetModFile reference in openReferences)
            {
                GadgetMod Mod = reference.Mod;
                GadgetCore.CoreLogger.Log("WARNING: Forcibly disposing of GadgetModFile that " + Mod.Name + " failed to dispose of!");
                reference.Dispose();
            }
        }

        internal static void DisposeAll(GadgetMod mod)
        {
            foreach (GadgetModFile reference in openReferences)
            {
                if (reference.Mod == mod)
                {
                    GadgetCore.CoreLogger.Log("WARNING: Forcibly disposing of GadgetModFile that " + mod.Name + " failed to dispose of: " + reference.FilePath);
                    reference.Dispose();
                }
            }
        }
    }
}
