using Ionic.Zip;
using System.Collections;

namespace GadgetCore
{
    /// <summary>
    /// Interface used by Gadget Core Lib.
    /// </summary>
    public interface IGadgetCoreLib
    {
        /// <summary>
        /// Used to give a GadgetLogger instance to GadgetCoreLib. Used by Gadget Core to provide access to its logger.
        /// </summary>
        void ProvideLogger(GadgetLogger logger);

        /// <summary>
        /// Attempts to forward the given port using UPnP. Is a coroutine, so should be treated as such.
        /// </summary>
        IEnumerator ForwardPort(int port);

        /// <summary>
        /// Decrypts the given .umfmod <see cref="ZipFile"/>.
        /// </summary>
        void DecryptUMFModFile(ZipFile zip);

        /*/// <summary>
        /// Initializes the Steam API. Should only be called by Gadget Core.
        /// </summary>
        void InitializeSteamAPI();*/
    }
}
