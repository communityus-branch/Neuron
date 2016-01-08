using System;

namespace Static_Interface.Multiplayer.Server
{
    public class ServerInitializationFailedException : Exception
    {
        public ServerInitializationFailedException(string reason) : base(reason) { }
    }
}