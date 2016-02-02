using Static_Interface.API.Interact;

namespace Static_Interface.Neuron.Interactables
{
    public class Gate : AnimatedOpenable
    {
        protected override string OpenAnimation => "GateOpen";
        public override string Name => "Gate";
    }
}