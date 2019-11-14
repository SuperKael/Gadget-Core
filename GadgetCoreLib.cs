using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UModFramework.API;

namespace GadgetCore
{
    /// <summary>
    /// Interface used by Gadget Core Lib.
    /// </summary>
    public interface IGadgetCoreLib
    {
        /// <summary>
        /// Used to give a UMFLog instance to GadgetCoreLib. Used by Gadget Core to provide access to its logger.
        /// </summary>
        void ProvideLogger(UMFLog logger);

        /// <summary>
        /// Attempts to forward the given port using UPnP. Is a coroutine, so should be treated as such.
        /// </summary>
        IEnumerator ForwardPort(int port);
    }
}
