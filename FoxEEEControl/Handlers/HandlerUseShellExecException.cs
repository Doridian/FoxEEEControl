using System;

namespace FoxEEEControl.Handlers
{
    class HandlerUseShellExecException : Exception
    {
        public string runwhat;

        public HandlerUseShellExecException() { }
        public HandlerUseShellExecException(string execwhat)
        {
            runwhat = execwhat;
        }
    }
}
