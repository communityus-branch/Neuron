using Static_Interface.API.PlayerFramework;
using UnityEngine;

namespace Static_Interface.API.InteractionFramework
{
    /// <summary>
    /// Objects which a player can interact with
    /// </summary>
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
        /// <summary>
        /// Called when a player interacts with it
        /// </summary>
        public abstract void Interact(Player player);
        /// <summary>
        /// Human-Readable name of the interactable object
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Message which will be shown to the player if in focus
        /// </summary>
        /// <returns></returns>
        public abstract string GetInteractMessage();
        
        /// <summary>
        /// Can you interact with the object?
        /// </summary>
        /// <returns></returns>
        public abstract bool CanInteract();

        /// <summary>
        /// The GameObject which can be interacted
        /// </summary>
        public abstract GameObject InteractableObject { get; }
    }
}