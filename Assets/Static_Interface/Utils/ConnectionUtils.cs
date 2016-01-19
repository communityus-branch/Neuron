using Static_Interface.Multiplayer;
using Static_Interface.Multiplayer.Service.MultiplayerProviderService;

namespace Static_Interface.Utils
{
    public class ConnectionUtils
    {
        public const float UPDATE_TIME = 0.15f;

        public static bool IsServer()
        {
            return Connection<MultiplayerProvider>.CurrentConnection.Provider is ServerMultiplayerProvider;
        }

        public static bool IsClient()
        {
            return Connection<MultiplayerProvider>.CurrentConnection.Provider is ClientMultiplayerProvider;
        }
    }
}