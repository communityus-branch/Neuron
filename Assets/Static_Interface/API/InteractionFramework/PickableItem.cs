using System;
using Static_Interface.API.PlayerFramework;
using UnityEngine;

namespace Static_Interface.API.InteractionFramework
{
    public abstract class PickableItem : Interactable
    {      
        protected override void OnInteract(Player player)
        {
            throw new NotImplementedException();
        }

        public abstract override string Name { get; }

        public override string GetInteractMessage()
        {
            return "Pickup " + Name;
        }

        public override bool CanInteract(Player player)
        {
            //Check if Inventory has space
            throw new NotImplementedException();
        }

        public override GameObject InteractableObject => gameObject;
    }
}