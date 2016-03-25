using Static_Interface.API.PlayerFramework;
using UnityEngine;
using MonoBehaviour = Static_Interface.API.UnityExtensions.MonoBehaviour;

namespace Static_Interface.API.InteractionFramework
{
    /// <summary>
    /// Objects which a player can interact with
    /// </summary>
    public abstract class Interactable : MonoBehaviour
    {
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