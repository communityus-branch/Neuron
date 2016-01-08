using System;
using Steamworks;

namespace Static_Interface.Multiplayer.Server.Impl.Steamworks
{
    public class SteamServer : GameServerProvider
    {
        protected override void OnStart(uint ip, ushort port, bool dedicated)
        {
            if (!GameServer.Init(ip, (ushort) (port + 2), port, (ushort) (port + 1), EServerMode.eServerModeInvalid,
                Game.VERSION))
            {
                throw new ServerInitializationFailedException("Couldn't start server (Steamworks API initialization failed)");
            }

            SteamGameServer.SetDedicatedServer(dedicated);
            SteamGameServer.SetGameDescription(Description);
            SteamGameServer.SetProduct(Game.NAME);
            SteamGameServer.SetModDir(Game.NAME);
            SteamGameServer.LogOnAnonymous();
            SteamGameServer.EnableHeartbeats(true);
        }

        protected override void OnStop()
        {
            SteamGameServer.EnableHeartbeats(false);
            SteamGameServer.LogOff();
            GameServer.Shutdown();
        }
    }
}