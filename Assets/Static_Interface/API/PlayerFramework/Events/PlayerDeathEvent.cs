using Static_Interface.API.EntityFramework;
using Static_Interface.API.EventFramework;

namespace Static_Interface.API.PlayerFramework.Events
{
    public class PlayerDeathEvent : Event
    {
        public Player Player { get; private set; }
        public IEntity Killer { get; set; }
        public string DeathMessage { get; set; }
        public EDamageCause DeathCause { get; set; }

        public PlayerDeathEvent(Player player) : base(false)
        {
            Player = player;
        }
    }
}