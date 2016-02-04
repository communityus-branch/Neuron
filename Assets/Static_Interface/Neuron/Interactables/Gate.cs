using Static_Interface.API.InteractionFramework;

namespace Static_Interface.Neuron.Interactables
{
    public class Gate : AnimatedOpenable
    {
        protected override string Animation => "GateOpen";
        public override string Name => "Gate";
    }
}