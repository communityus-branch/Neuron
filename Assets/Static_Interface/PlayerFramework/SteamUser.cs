using Static_Interface.Multiplayer;
using Static_Interface.Multiplayer.Protocol;
using UnityEngine;

namespace Static_Interface.PlayerFramework
{
    public class SteamUser : User
    {
        private readonly UserIdentity _identity;
        private readonly Player _player;
        private readonly Transform _model;

        public override Transform Model
        {
            get { return _model; }
        }
        public override UserIdentity Identity
        {
            get { return _identity; }
        }

        public override Player Player
        {
            get { return _player; }
        }

        public SteamUser(Connection connection, UserIdentity ident, Transform model, int channelId)
        {
            _identity = ident;
            _player = new Player();
            _model = model;
            Channel ch = model.GetComponent<Channel>();
            ch.ID = channelId;
            ch.Owner = this;
            ch.IsOwner = ident.ID == connection.ClientID;
            ch.Setup();
            _player = model.GetComponent<Player>();
        }
    }
}