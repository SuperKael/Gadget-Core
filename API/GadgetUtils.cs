using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace GadgetCore.API
{
    /// <summary>
    /// Some general-purpose utility methods.
    /// </summary>
    public static class GadgetUtils
    {
        /// <summary>
        /// WaitAndInvoke is a coroutine that will wait for the specified timeout or condition, and then invoke the given method.
        /// </summary>
        /// <param name="method">The MethodBase to invoke.</param>
        /// <param name="timeout">The maximum time, in seconds, to wait.</param>
        /// <param name="condition">The condition on which to stop waiting early.</param>
        /// <param name="invokeInstance">The object to invoke the method on. May be null if the given method is static.</param>
        /// <param name="parameters">The parameters to pass to the invoked method.</param>
        public static IEnumerator WaitAndInvoke(MethodBase method, float timeout, Func<bool> condition = null, object invokeInstance = null, params object[] parameters)
        {
            if (condition == null) condition = () => false;
            float startTime = Time.realtimeSinceStartup;
            yield return new WaitUntil(() => Time.realtimeSinceStartup > (startTime + timeout) || condition());
            method.Invoke(invokeInstance, parameters);
        }

        /// <summary>
        /// Invokes Graphics.CopyTexture on supported systems, otherwise performs the operation using direct pixel manipulation.
        /// </summary>
        public static void SafeCopyTexture(Texture2D src, int srcElement, int srcMip, int srcX, int srcY, int srcWidth, int srcHeight, Texture2D dst, int dstElement, int dstMip, int dstX, int dstY)
        {
            if (SystemInfo.copyTextureSupport != UnityEngine.Rendering.CopyTextureSupport.None)
            {
                Graphics.CopyTexture(src, srcElement, srcMip, srcX, srcY, srcWidth, srcHeight, dst, dstElement, dstMip, dstX, dstY);
            }
            else
            {
                try
                {
                    dst.SetPixels(dstX, dstY, srcWidth, srcHeight, src.GetPixels(srcX, srcY, srcWidth, srcHeight, srcMip), dstMip);
                }
                catch (UnityException e)
                {
                    if (e.Message.StartsWith("Texture '" + src.name + "' is not readable"))
                    {
                        RenderTexture renderTex = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
                        Graphics.Blit(src, renderTex);
                        RenderTexture previous = RenderTexture.active;
                        RenderTexture.active = renderTex;
                        dst.ReadPixels(new Rect(srcX, srcY, srcWidth, srcHeight), dstX, dstY);
                        RenderTexture.active = previous;
                        RenderTexture.ReleaseTemporary(renderTex);
                    }
                    else
                    {
                        throw e;
                    }
                }
                dst.Apply();
            }
        }

        /// <summary>
        /// Uses a recursive algorithm to check if a string matches a given wild
        /// </summary>
        /// <param name="text"></param>
        /// <param name="wild"></param>
        /// <returns></returns>
        public static bool WildcardMatch(string text, string wild)
        {
            if (wild == "*" || text == wild) return true;
            if (text == "") return false;
            if (text[0] == wild[0] || wild[0] == '?') return WildcardMatch(text.Substring(1), wild.Substring(1));
            if (wild[0] == '*') return WildcardMatch(text.Substring(1), wild) || WildcardMatch(text, wild.Substring(1));
            return false;
        }

        /// <summary>
        /// Recursively deletes a given directory and its subdirectories. If deleteFiles is false, silently ignores directories containing files.
        /// </summary>
        public static void RecursivelyDeleteDirectory(string path, bool deleteFiles = false)
        {
            foreach (string directory in Directory.GetDirectories(path))
            {
                RecursivelyDeleteDirectory(directory);
            }

            if (deleteFiles || (Directory.GetFiles(path).Length == 0 && Directory.GetDirectories(path).Length == 0))
            {
                try
                {
                    Directory.Delete(path, deleteFiles);
                }
                catch (IOException)
                {
                    Directory.Delete(path, deleteFiles);
                }
                catch (UnauthorizedAccessException)
                {
                    Directory.Delete(path, deleteFiles);
                }
            }
        }
    }
}
