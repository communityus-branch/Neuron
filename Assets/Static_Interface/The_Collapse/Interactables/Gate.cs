using Static_Interface.API.InteractFramework;

namespace Static_Interface.The_Collapse.Interactables
{
    public class Gate : AnimatedOpenable
    {
        protected override string OpenAnimation => "GateOpen";
        public override string Name => "Gate";
    }
}