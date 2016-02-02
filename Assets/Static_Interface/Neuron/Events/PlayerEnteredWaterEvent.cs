using Static_Interface.API.Event;
using Static_Interface.API.Player;

namespace Static_Interface.Neuron.Events
{
    public class PlayerEnteredWaterEvent : Event
    {
        public Player Player { get; private set; }
        public PlayerEnteredWaterEvent(Player player) : base(false)
        {
            Player = player;
        }
    }
}