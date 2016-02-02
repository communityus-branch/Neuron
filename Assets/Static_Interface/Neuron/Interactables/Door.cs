using Static_Interface.API.Interact;

namespace Static_Interface.Neuron.Interactables
{
    public class Door : AnimatedOpenable
    {
        protected override string OpenAnimation => "DoorOpen";
        public override string Name => "Door";
    }
}