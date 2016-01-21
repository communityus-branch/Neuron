using Static_Interface.Multiplayer.Server;
using Static_Interface.PlayerFramework;
using Steamworks;
using UnityEngine;

namespace Static_Interface.Multiplayer.Service.MultiplayerProviderService
{
    public class ServerMultiplayerProvider : MultiplayerProvider
    {
        public bool IsHosting;
        public string Description = "A " + Game.NAME + " Server";

        public ServerMultiplayerProvider()
        {
            Callback<P2PSessionRequest_t>.CreateGameServer(OnP2PSessionRequest);
        }

        public void Close()
        {
            if (!IsHosting) return;
            SteamGameServer.EnableHeartbeats(false);
            SteamGameServer.LogOff();
            GameServer.Shutdown();
            IsHosting = false;
        }

        public void Open(uint ip, ushort port, bool lan)
        {
            EServerMode mode = EServerMode.eServerModeAuthenticationAndSecure;
            //if(lan) mode = EServerMode.eServerModeNoAuthentication;
            if (!GameServer.Init(ip, (ushort)(port+ 2), port, (ushort)(port + 1), mode,
                    Game.VERSION))
            {
                throw new ServerInitializationFailedException("Couldn't start server (Steamworks API initialization failed)");
            }

            SteamGameServer.SetDedicatedServer(!lan);
            SteamGameServer.SetGameDescription(Game.NAME);
            SteamGameServer.SetProduct(Game.NAME);
            SteamGameServer.SetModDir(Game.NAME);
            SteamGameServer.SetServerName(Description);
            SteamGameServer.LogOnAnonymous();
            SteamGameServer.EnableHeartbeats(true);

            Application.targetFrameRate = 60;
        }

        public override bool Read(out CSteamID user, byte[] data, out ulong length, int channel)
        {
            uint num;
            user = CSteamID.Nil;
            length = 0L;
            if (!SteamGameServerNetworking.IsP2PPacketAvailable(out num, channel) || (num > data.Length))
            {
                return false;
            }
            if (!SteamGameServerNetworking.ReadP2PPacket(data, num, out num, out user, channel))
            {
                return false;
            }
            length = num;
            return true;
        }

        public override void Write(User user, byte[] data, ulong length)
        {
            SteamGameServerNetworking.SendP2PPacket(user.Identity.ID, data, (uint)length, EP2PSend.k_EP2PSendUnreliable);
        }

        public override void Write(User user, byte[] data, ulong length, EP2PSend method, int channel)
        {
            SteamGameServerNetworking.SendP2PPacket(user.Identity.ID, data, (uint)length, method, channel);
        }
    }
}