using Static_Interface.API.Player;

namespace Static_Interface.Internal.MultiplayerFramework.MultiplayerProvider
{
    public abstract class ClientMultiplayerProvider : MultiplayerProvider
    {
        protected ClientMultiplayerProvider(Connection connection) : base(connection)
        {
        }

        public abstract Identity GetUserID();
        public abstract void AdvertiseGame(Identity serverID, string ip, ushort port);
        public abstract void SetPlayedWith(Identity ident);
        public abstract void AttemptConnect(string ip, ushort port, string password);
        public abstract void SetStatus(string status);
        public abstract string GetClientName();
        public abstract void SetConnectInfo(string ip, ushort port);
        public abstract bool IsFavoritedServer(string ip, ushort port);
        public abstract byte[] OpenTicket();
        public abstract void CloseTicket();
        public abstract void FavoriteServer(string ip, ushort port);
        public abstract void SetIdentity(ulong serializedIdent);
    }
}