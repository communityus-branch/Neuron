using UnityEngine;

namespace Static_Interface.Interactables
{
    public class ParkBench : Interactable
    {
        public override GameObject InteractableObject { get { return transform.gameObject; } }
        private bool full = false;

        public override string Name { get { return "Bank";  } }
        public override string GetInteractMessage()
        {
            return "Sit";
        }

        public override bool ShouldShowInteractMessage()
        {
            return true;
        }

        public override void Interact()
        {
            //todo
        }

        public override bool CanInteract()
        {
            return !full;
        }

    }
}