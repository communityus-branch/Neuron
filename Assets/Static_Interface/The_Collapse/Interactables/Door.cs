using Static_Interface.API.InteractFramework;

namespace Static_Interface.The_Collapse.Interactables
{
    public class Door : AnimatedOpenable
    {
        protected override string OpenAnimation => "DoorOpen";
        public override string Name => "Door";
    }
}