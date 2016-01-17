namespace Static_Interface.Interactables
{
    public class Door : AnimatedOpenable
    {
        protected override string OpenAnimation => "DoorOpen";
        public override string Name => "Door";
    }
}