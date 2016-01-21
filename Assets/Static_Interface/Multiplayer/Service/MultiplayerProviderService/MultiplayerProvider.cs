﻿using System.Diagnostics;
using System.IO;
using Static_Interface.PlayerFramework;
using Steamworks;
using Debug = UnityEngine.Debug;

namespace Static_Interface.Multiplayer.Service.MultiplayerProviderService
{
    public abstract class MultiplayerProvider : Service
    {
        public const int MIN_PLAYERS = 0;
        public const int MAX_PLAYERS = 16;

        public BinaryReader Deserializer { get; }
        public MemoryStream Stream { get; }
        public BinaryWriter Serializer { get; }
        protected byte[] Buffer = new byte[1024];

        protected MultiplayerProvider()
        {
            Stream = new MemoryStream(Buffer);
            Deserializer = new BinaryReader(Stream);
            Serializer = new BinaryWriter(Stream);
        }

        public abstract bool Read(out CSteamID user, byte[] data, out ulong length, int channel);

        public void OnP2PSessionRequest(P2PSessionRequest_t callback)
        {
            Debug.Log("Accepting P2P Request: " + callback.m_steamIDRemote);
            SteamNetworking.AcceptP2PSessionWithUser(callback.m_steamIDRemote);
        }

        public abstract void Write(User user, byte[] data, ulong length);

        public abstract void Write(User user, byte[] data, ulong length, EP2PSend method, int channel);
    }
}