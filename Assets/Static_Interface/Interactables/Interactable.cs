using UnityEngine;

namespace Static_Interface.Interactables
{
    public abstract class Interactable : MonoBehaviour
    {
        protected virtual void Start()
        {
            
        }

        protected virtual void Update()
        {
            
        }

        public abstract void Interact();
        public abstract string Name { get; }
        public abstract string GetInteractMessage();
        public abstract bool CanInteract();
        public abstract GameObject InteractableObject { get; }
        public abstract bool ShouldShowInteractMessage();
    }
}