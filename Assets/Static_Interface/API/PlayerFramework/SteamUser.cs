using Static_Interface.Internal;
using Static_Interface.Internal.MultiplayerFramework;
using UnityEngine;

namespace Static_Interface.API.PlayerFramework
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