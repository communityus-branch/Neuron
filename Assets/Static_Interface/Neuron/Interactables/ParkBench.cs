using Static_Interface.API.InteractionFramework;
using UnityEngine;

namespace Static_Interface.Neuron.Interactables
{ 
    public class ParkBench : Interactable
    {
        public override GameObject InteractableObject => transform.gameObject;
        private bool full = false;

        public override string Name => "Bank";

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