using Static_Interface.API.PlayerFramework;
using Static_Interface.Internal.MultiplayerFramework.Server;
using Static_Interface.The_Collapse;
using Steamworks;
using UnityEngine;

namespace Static_Interface.Internal.MultiplayerFramework.Service.MultiplayerProviderService
{
    public class ServerMultiplayerProvider : MultiplayerProvider
    {
        public bool IsHosting;
        public string Description = "A " + GameInfo.NAME + " Server";

        public ServerMultiplayerProvider()
        {
            Callback<P2PSessionRequest_t>.CreateGameServer(OnP2PSessionRequest);
        }

        public void OnP2PSessionRequest(P2PSessionRequest_t callback)
        {
            if (!SteamGameServerNetworking.AcceptP2PSessionWithUser(callback.m_steamIDRemote))
            {
                LogUtils.Debug("Failde to accept P2P Request: " + callback.m_steamIDRemote);
            }
            else
            {
                LogUtils.Debug("Accepted P2P Request: " + callback.m_steamIDRemote);
            }
        }

        public void Close()
        {
            if (!IsHosting) return;
            SteamGameServer.EnableHeartbeats(false);
            SteamGameServer.LogOff();
            GameServer.Shutdown();
            SteamAPI.Shutdown();
            IsHosting = false;
        }

        public void Open(uint ip, ushort port, bool lan)
        {
            if (IsHosting) return;
            EServerMode mode = EServerMode.eServerModeAuthenticationAndSecure;
            //if(lan) mode = EServerMode.eServerModeNoAuthentication;
            if (!GameServer.Init(ip, (ushort)(port+ 2), port, (ushort)(port + 1), mode,
                    GameInfo.VERSION))
            {
                throw new ServerInitializationFailedException("Couldn't start server (Steamworks API initialization failed)");
            }

            SteamGameServer.SetDedicatedServer(!lan);
            SteamGameServer.SetGameDescription(GameInfo.NAME);
            SteamGameServer.SetProduct(GameInfo.NAME);
            SteamGameServer.SetModDir(GameInfo.NAME);
            SteamGameServer.SetServerName(Description);
            SteamGameServer.LogOnAnonymous();
            SteamGameServer.SetPasswordProtected(false); //Todo
            SteamGameServer.EnableHeartbeats(true);

            Application.targetFrameRate = 60;
            IsHosting = true;
        }

        public override bool Read(out CSteamID user, byte[] data, out ulong length, int channel)
        {
            uint num;
            user = CSteamID.Nil;
            length = 0L;
            if (!SteamGameServerNetworking.IsP2PPacketAvailable(out num, channel) || (num > data.Length))
            {
                LogUtils.Debug("No P2P Packet available");
                return false;
            }
            if (!SteamGameServerNetworking.ReadP2PPacket(data, num, out num, out user, channel))
            {
                LogUtils.Debug("P2P Packet reading failed");
                return false;
            }
            length = num;
            return true;
        }

        public override bool Write(CSteamID target, byte[] data, ulong length)
        {
            return SteamGameServerNetworking.SendP2PPacket(target, data, (uint)length, EP2PSend.k_EP2PSendUnreliable);
        }

        public override bool Write(CSteamID target, byte[] data, ulong length, EP2PSend method, int channel)
        {
            return SteamGameServerNetworking.SendP2PPacket(target, data, (uint)length, method, channel);
        }
    }
}