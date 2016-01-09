using System;
using Steamworks;

namespace Static_Interface.Multiplayer.Server.Impl.Steamworks
{
    public class SteamServer : GameServerProvider
    {
        public SteamServer(uint ip, ushort port) : base(ip, port)
        {
        }

        protected override void OnStart()
        {
            if (!GameServer.Init(Ip, (ushort) (Port + 2), Port, (ushort) (Port + 1), EServerMode.eServerModeInvalid,
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
            Callback<P2PSessionRequest_t>.CreateGameServer(OnP2PSessionRequest);
        }

        public void OnP2PSessionRequest(P2PSessionRequest_t request)
        {
            SteamGameServerNetworking.AcceptP2PSessionWithUser(request.m_steamIDRemote);
        }
        protected override void OnStop()
        {
            SteamGameServer.EnableHeartbeats(false);
            SteamGameServer.LogOff();
            GameServer.Shutdown();
        }

        public override bool Read(out WrappedUser user, byte[] data, out ulong length, int channel)
        {
            uint num;
            CSteamID id;
            user = InvalidUser;
            length = 0L;
            if (!SteamGameServerNetworking.IsP2PPacketAvailable(out num, channel) || (num > data.Length))
            {
                return false;
            }
            if (!SteamGameServerNetworking.ReadP2PPacket(data, num, out num, out id, channel))
            {
                return false;
            }
            user = new SteamWrappedUser(id);
            length = num;
            return true;
        }

        public override void Write(WrappedUser user, byte[] data, ulong length)
        {
            SteamGameServerNetworking.SendP2PPacket(((SteamWrappedUser)user).SteamID, data, (uint)length, EP2PSend.k_EP2PSendUnreliable, 0);
        }

        public override void Write(WrappedUser user, byte[] data, ulong length, EP2PSend method, int channel)
        {
            SteamGameServerNetworking.SendP2PPacket(((SteamWrappedUser)user).SteamID, data, (uint)length, method, channel);
        }
    }
}