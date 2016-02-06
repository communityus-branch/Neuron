using System.Linq;
using Static_Interface.Internal.MultiplayerFramework;

namespace Static_Interface.API.PlayerFramework
{
    public abstract class Identity
    {
        public User Owner { get; internal set; }
        public abstract bool IsValid();
        public abstract ulong Serialize();
        public override string ToString()
        {
            return Serialize().ToString();
        }

        public User GetUser()
        {
            return Connection.CurrentConnection.Clients.FirstOrDefault(c => c.Identity.Serialize() == Serialize());
        }
    }
}