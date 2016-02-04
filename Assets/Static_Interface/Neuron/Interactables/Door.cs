using Static_Interface.API.InteractionFramework;

namespace Static_Interface.Neuron.Interactables
{
    public class Door : AnimatedOpenable
    {
        protected override string Animation => "DoorOpen";
        public override string Name => "Door";
    }
}