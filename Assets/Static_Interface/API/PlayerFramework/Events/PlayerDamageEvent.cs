using Static_Interface.API.EntityFramework;
using Static_Interface.API.EventFramework;

namespace Static_Interface.API.PlayerFramework.Events
{
    public class PlayerDamageEvent : Event
    {
        public Player Player { get; private set; }
        public IEntity DamageCausingEntity { get; set; }
        public EDamageCause DamageCause { get; set; }
        public int Damage { get; set; }
        public PlayerDamageEvent(Player player) : base(false)
        {
            Player = player;
        }
    }
}