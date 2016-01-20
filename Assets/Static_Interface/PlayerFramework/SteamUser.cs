using Static_Interface.Multiplayer;
using Static_Interface.Multiplayer.Protocol;
using Static_Interface.Multiplayer.Service.MultiplayerProviderService;
using UnityEngine;

namespace Static_Interface.PlayerFramework
{
    public class SteamUser : User
    {
        public SteamUser(Connection connection, UserIdentity ident, Transform model, int channelId)
        {
            Identity = ident;
            Model = model;
            Channel ch = model.GetComponent<Channel>();
            ch.ID = channelId;
            ch.Owner = this;
            ch.IsOwner = ident.ID == connection.ClientID;
            ch.Setup();
            Player = model.GetComponent<Player>();
            Player.User = this;
        }
    }
}