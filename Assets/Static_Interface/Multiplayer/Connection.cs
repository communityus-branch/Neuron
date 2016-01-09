using System.Collections.Generic;
using Static_Interface.Objects;
using Static_Interface.Multiplayer.Server.Impl.Steamworks;
using Steamworks;
using UnityEngine;

namespace Static_Interface.Multiplayer
{
    public abstract class Connection : MonoBehaviour
    {
        public CSteamID ServerID = CSteamID.Nil;

        protected CSteamID ClientIDInternal;
        public CSteamID ClientID
        {
            get { return ClientIDInternal; }
        }
        private int _channels = 1;
        private List<SteamWrappedUser> _clients;
        private static bool isInitialized;

        protected bool IsConnectedInternal;
        public bool IsConnected
        {
            get { return IsConnectedInternal; } 
        }

        public ICollection<SteamWrappedUser> Clients
        {
            get { return _clients.AsReadOnly(); }
        }

        protected Connection(CSteamID id)
        {
            ClientIDInternal = id;
        }

        private void Awake()
        {
            if (isInitialized)
            {
                Destroy(this);
                return;
            }

            isInitialized = true;
            DontDestroyOnLoad(this);
            OnAwake();
            SteamFriends.SetRichPresence("status", "Menu");
        }

        protected abstract void OnAwake();

        private static List<Channel> _receivers;
        public static List<Channel> Receivers
        {
            get
            {
                return _receivers;
            }
        }

        protected void ResetChannels()
        {
            _channels = 1;
            _receivers = new List<Channel>();
            var channelArray = FindObjectsOfType<Channel>();
            foreach (var ch in channelArray)
            {
                OpenChannel(ch);
            }
            _clients = new List<SteamWrappedUser>();
            //pending = new List<SteamPending>();
        }


        public void Update()
        {
            if (!IsConnected || !isInitialized) return;
            Listen();
        }

        protected abstract void Listen();

        public abstract void Send(CSteamID receiver, EPacket type, byte[] data, int length, int id);


        public abstract void Disconnect();

        public void OpenChannel(Channel ch)
        {
            if (Receivers == null)
            {
                ResetChannels();
                return;
            }
            Receivers.Add(ch);
            _channels++;
        }

        public void CloseChannel(Channel ch)
        {
            for (var i = 0; i < Receivers.Count; i++)
            {
                if (Receivers[i].ID != ch.ID) continue;
                Receivers.RemoveAt(i);
                return;
            }
            _channels--;
        }
    }
}