using System;

namespace Static_Interface.Internal.MultiplayerFramework.Server
{
    public class ServerInitializationFailedException : Exception
    {
        public ServerInitializationFailedException(string reason) : base(reason) { }
    }
}