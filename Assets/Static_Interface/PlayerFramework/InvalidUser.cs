using System;
using Static_Interface.Multiplayer.Server;
using UnityEngine;

namespace Static_Interface.PlayerFramework
{
    public class InvalidUser : User
    {
        public override UserIdentity Identity
        {
            get { throw new NotImplementedException("invalid user"); }
        }

        public override Player Player
        {
            get { throw new NotImplementedException("invalid user"); }
        }

        public override Transform Model
        {
            get { throw new NotImplementedException("invalid user"); }
        }
    }
}