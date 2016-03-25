using System;
using Static_Interface.API.PlayerFramework;
using UnityEngine;

namespace Static_Interface.API.InteractionFramework
{
    public abstract class PickableItem : Interactable
    {      
        public override void Interact(Player player)
        {
            throw new NotImplementedException();
        }

        public abstract override string Name { get; }

        public override string GetInteractMessage()
        {
            return "Pickup " + Name;
        }

        public override bool CanInteract()
        {
            //Check if Inventory has space
            throw new NotImplementedException();
        }

        public override GameObject InteractableObject => gameObject;
    }
}