using System.Linq;
using Static_Interface.API.Network;
using Static_Interface.API.Player;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework.MultiplayerProvider;
using Static_Interface.Internal.MultiplayerFramework.Server;
using Static_Interface.Neuron;
using Steamworks;
using UnityEngine;

namespace Static_Interface.Internal.MultiplayerFramework.Impl.Steamworks
{
    public class SteamworksServerProvider : ServerMultiplayerProvider
    {

        public SteamworksServerProvider(Connection conn) : base(conn)
        {
            Callback<P2PSessionRequest_t>.CreateGameServer(OnP2PSessionRequest);
            Callback<GSPolicyResponse_t>.CreateGameServer(OnGsPolicyResponse);
            Callback<P2PSessionConnectFail_t>.CreateGameServer(OnP2PSessionConnectFail);
            Callback<ValidateAuthTicketResponse_t>.CreateGameServer(OnValidateAuthTicketResponse);
            SteamUtils.SetWarningMessageHook(OnAPIWarningMessage);
        }

        private void OnValidateAuthTicketResponse(ValidateAuthTicketResponse_t callback)
        {
            Identity ident = (SteamIdentity)callback.m_SteamID;
            if (callback.m_eAuthSessionResponse != EAuthSessionResponse.k_EAuthSessionResponseOK)
            {
                if (callback.m_eAuthSessionResponse == EAuthSessionResponse.k_EAuthSessionResponseUserNotConnectedToSteam)
                {
                    ((ServerConnection)Connection).Reject(ident, ERejectionReason.AUTH_NO_STEAM);
                }
                else if (callback.m_eAuthSessionResponse == EAuthSessionResponse.k_EAuthSessionResponseNoLicenseOrExpired)
                {
                    ((ServerConnection)Connection).Reject(ident, ERejectionReason.AUTH_LICENSE_EXPIRED);
                }
                else if (callback.m_eAuthSessionResponse == EAuthSessionResponse.k_EAuthSessionResponseVACBanned)
                {
                    ((ServerConnection)Connection).Reject(ident, ERejectionReason.AUTH_VAC_BAN);
                }
                else if (callback.m_eAuthSessionResponse == EAuthSessionResponse.k_EAuthSessionResponseLoggedInElseWhere)
                {
                    ((ServerConnection)Connection).Reject(ident, ERejectionReason.AUTH_ELSEWHERE);
                }
                else if (callback.m_eAuthSessionResponse == EAuthSessionResponse.k_EAuthSessionResponseVACCheckTimedOut)
                {
                    ((ServerConnection)Connection).Reject(ident, ERejectionReason.AUTH_TIMED_OUT);
                }
                else if (callback.m_eAuthSessionResponse == EAuthSessionResponse.k_EAuthSessionResponseAuthTicketCanceled)
                {
                    ((ServerConnection)Connection).DisconnectClient(ident);
                }
                else if (callback.m_eAuthSessionResponse == EAuthSessionResponse.k_EAuthSessionResponseAuthTicketInvalidAlreadyUsed)
                {
                    ((ServerConnection)Connection).Reject(ident, ERejectionReason.AUTH_USED);
                }
                else if (callback.m_eAuthSessionResponse == EAuthSessionResponse.k_EAuthSessionResponseAuthTicketInvalid)
                {
                    ((ServerConnection)Connection).Reject(ident, ERejectionReason.AUTH_NO_USER);
                }
                else if (callback.m_eAuthSessionResponse == EAuthSessionResponse.k_EAuthSessionResponsePublisherIssuedBan)
                {
                    ((ServerConnection)Connection).Reject(ident, ERejectionReason.AUTH_PUB_BAN);
                }
                return;
            }

            PendingUser pending = ((ServerConnection)Connection).PendingPlayers.FirstOrDefault(pendingPlayer => pendingPlayer.Identity == ident);
            if (pending == null)
            {
                ((ServerConnection)Connection).Reject(ident, ERejectionReason.NOT_PENDING);
                return;
            }

            pending.HasAuthentication = true;
            ((ServerConnection)Connection).Accept(pending);
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

        public override void Close()
        {
            if (!IsHosting) return;
            SteamGameServer.EnableHeartbeats(false);
            SteamGameServer.LogOff();
            GameServer.Shutdown();
            SteamAPI.Shutdown();
            IsHosting = false;
        }

        public override void Open(uint ip, ushort port, bool lan)
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

        private void OnGsPolicyResponse(GSPolicyResponse_t callback)
        {
            if (callback.m_bSecure != 0)
            {
                ((ServerConnection)Connection).IsSecure = true;
            }
            else if (((ServerConnection)Connection).IsSecure)
            {
                ((ServerConnection)Connection).IsSecure = false;
            }
            LogUtils.Debug("OnGsPolicyResponse: IsSecure: " + callback.m_bSecure);
        }

        private void OnP2PSessionConnectFail(P2PSessionConnectFail_t callback)
        {
            Identity ident = (SteamIdentity) callback.m_steamIDRemote;
            LogUtils.Error("P2P connection failed for: " + callback.m_steamIDRemote + ", error: " + callback.m_eP2PSessionError);
            ((ServerConnection)Connection).DisconnectClient(ident);
        }

        public override bool Read(out Identity user, byte[] data, out ulong length, int channel)
        {
            uint num;
            user = null;
            CSteamID id;
            length = 0L;
            if (!SteamGameServerNetworking.IsP2PPacketAvailable(out num, channel) || (num > data.Length))
            {
                LogUtils.Debug("No P2P Packet available on channel " + channel);
                return false;
            }
            if (!SteamGameServerNetworking.ReadP2PPacket(data, num, out num, out id, channel))
            {
                LogUtils.Debug("P2P Packet reading failed on channel " + channel);
                return false;
            }
            user = (SteamIdentity) id;
            length = num;
            return true;
        }

        public override bool Write(Identity target, byte[] data, ulong length)
        {
            return SteamGameServerNetworking.SendP2PPacket((CSteamID)(SteamIdentity)target, data, (uint)length, EP2PSend.k_EP2PSendUnreliable);
        }

        public override bool Write(Identity target, byte[] data, ulong length, SendMethod method, int channel)
        {
            return SteamGameServerNetworking.SendP2PPacket((CSteamID)(SteamIdentity)target, data, (uint)length, (EP2PSend)method, channel);
        }

        public override void CloseConnection(Identity user)
        {
            SteamGameServerNetworking.CloseP2PSessionWithUser((CSteamID) (SteamIdentity) user);
        }

        public override void UpdateScore(Identity ident, uint score)
        {
            if (ident.Owner == null) return;
            var steamIdent = (SteamIdentity) ident;
            SteamGameServer.BUpdateUserData(steamIdent.SteamID, steamIdent.Owner.Name, score);
        }

        public override bool VerifyTicket(Identity ident, byte[] data)
        {
            return (SteamGameServer.BeginAuthSession(data, data.Length, (CSteamID)(SteamIdentity)ident) == EBeginAuthSessionResult.k_EBeginAuthSessionResultOK);
        }

        private SteamIdentity _ident;
        public override Identity GetServerIdentity()
        {
            return _ident ?? (_ident = (SteamIdentity) SteamGameServer.GetSteamID());
        }

        public override uint GetPublicIP()
        {
            return SteamGameServer.GetPublicIP();
        }

        public override void SetMaxPlayerCount(int maxPlayers)
        {
            SteamGameServer.SetMaxPlayerCount(maxPlayers);
        }

        public override void SetServerName(string name)
        {
            SteamGameServer.SetServerName(name);
        }

        public override void SetPasswordProtected(bool passwordProtected)
        {
            SteamGameServer.SetPasswordProtected(passwordProtected);
        }

        public override void SetMapName(string map)
        {
            SteamGameServer.SetMapName(map);
        }

        public override uint GetServerRealTime()
        {
            return SteamUtils.GetServerRealTime();
        }

        public override void EndAuthSession(Identity user)
        {
            SteamGameServer.EndAuthSession((CSteamID)(SteamIdentity)user);
        }
    }
}