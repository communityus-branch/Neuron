using Assets.API.EventFramework;

namespace Assets.Events
{
    public class PlayerLeftWaterEvent : Event
    {
        public PlayerLeftWaterEvent() : base(false)
        {

        }
    }
}