using UnityEngine;

namespace Static_Interface.API.Interact
{
    public abstract class Interactable : MonoBehaviour
    {
        protected virtual void Awake()
        {
            
        }

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