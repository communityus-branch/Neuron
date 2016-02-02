using Static_Interface.API.Player;
using Static_Interface.Neuron;

namespace Static_Interface.Internal.MultiplayerFramework.MultiplayerProvider
{
    public abstract class ServerMultiplayerProvider : MultiplayerProvider
    {
        public bool IsHosting { get; protected set; }
        public string Description { get; protected set; } = "A " + GameInfo.NAME + " Server";

        public abstract void EndAuthSession(Identity user);

        protected ServerMultiplayerProvider(Connection connection) : base(connection)
        {
        }

        public abstract void Open(uint ip, ushort port, bool lan);
        public abstract void Close();
        public abstract void UpdateScore(Identity ident, uint score);
        public abstract bool VerifyTicket(Identity ident, byte[] data);
        public abstract Identity GetServerIdentity();
        public abstract uint GetPublicIP();
        public abstract void SetMaxPlayerCount(int maxPlayers);
        public abstract void SetServerName(string name);
        public abstract void SetPasswordProtected(bool b);
        public abstract void SetMapName(string map);
    }
}