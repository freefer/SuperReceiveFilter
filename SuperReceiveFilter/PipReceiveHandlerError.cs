using System;

namespace SuperReceiveFilter
{
    public class PipHandlerErrorEventArgs : EventArgs
    {
        public PipHandlerErrorEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; private set; }

    }
}