using Static_Interface.Multiplayer.Server;
using Steamworks;

namespace Static_Interface.Multiplayer.Service.ConnectionProviderService
{
    public class ServerMultiplayerProvider : MultiplayerProvider
    {
        public bool IsHosting;
        public string Description = "A " + Game.NAME + " Server";

        public void Close()
        {
            if (!IsHosting) return;
            SteamGameServer.EnableHeartbeats(false);
            SteamGameServer.LogOff();
            GameServer.Shutdown();
            IsHosting = false;
        }

        public void Open(uint ip, ushort port)
        {
            if (!GameServer.Init(ip, (ushort)(port+ 2), port, (ushort)(port + 1), EServerMode.eServerModeInvalid,
                    Game.VERSION))
            {
                throw new ServerInitializationFailedException("Couldn't start server (Steamworks API initialization failed)");
            }

            SteamGameServer.SetDedicatedServer(true);
            SteamGameServer.SetGameDescription(Description);
            SteamGameServer.SetProduct(Game.NAME);
            SteamGameServer.SetModDir(Game.NAME);
            SteamGameServer.LogOnAnonymous();
            SteamGameServer.EnableHeartbeats(true);
        }
    }
}