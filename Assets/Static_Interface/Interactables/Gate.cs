namespace Static_Interface.Interactables
{
    public class Gate : AnimatedOpenable
    {
        protected override string OpenAnimation => "GateOpen";
        public override string Name => "Gate";
    }
}