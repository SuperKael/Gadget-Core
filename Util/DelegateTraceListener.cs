using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace GadgetCore.Util
{
    /// <summary>
    /// A trace listener that routes messages to a given Action&lt;string&gt;.
    /// </summary>
    public class DelegateTraceListener : TraceListener
    {
        private Action<string> action;

        /// <summary>
        /// Constructs a new DelegateTraceListener.
        /// </summary>
        public DelegateTraceListener(Action<string> action)
        {
            this.action = action;
        }

        /// <summary>
        /// Sends a string to this DelegateTraceListener's action delegate.
        /// </summary>
        public override void Write(string message)
        {
            action(message);
        }

        /// <summary>
        /// Sends a line to this DelegateTraceListener's action delegate.
        /// </summary>
        public override void WriteLine(string message)
        {
            action(message);
        }
    }
}
