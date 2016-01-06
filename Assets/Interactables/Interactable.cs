using UnityEngine;

namespace Assets.Interactables
{
    public abstract class Interactable : MonoBehaviour
    {
        public abstract void Interact();
        public abstract string Name { get; }
        public abstract string GetInteractMessage();
        public abstract bool CanInteract();
        public abstract GameObject InteractableObject { get; }
        public abstract bool ShouldShowInteractMessage();
    }
}