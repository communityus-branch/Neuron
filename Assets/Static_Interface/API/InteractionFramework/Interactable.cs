using Static_Interface.API.NetworkFramework;
using Static_Interface.API.PlayerFramework;
using UnityEngine;

namespace Static_Interface.API.InteractionFramework
{
    /// <summary>
    /// Objects which a player can interact with
    /// </summary>
    public abstract class Interactable : NetworkedBehaviour
    {
        public bool IsInteractable { get; set; } = true;
        /// <summary>
        /// Called when a player interacts with it
        /// </summary>
        public virtual void Interact(Player player)
        {
            if (player.Health.IsDead) return;
            if (IsInteractable && !CanInteract(player)) return;
            if (IsClient() && !IsServer())
            {
                Channel.Send(nameof(Network_InteractRequest), ECall.Server);
            }
            else
            {
                Network_InteractRequest(player.User.Identity);   
            }
        }

        protected abstract void OnInteract(Player player);

        [NetworkCall(ConnectionEnd = ConnectionEnd.SERVER)]
        public void Network_InteractRequest(Identity ident)
        {
            //todo: check player range to object
            var player = ident.Owner?.Player;
            if (player == null || player.Health.IsDead) return;
            if (!IsInteractable || !CanInteract(player)) return;
            OnInteract(player);
            //Todo: add radius
            Channel.Send(nameof(Network_Interact), ECall.Others, ident);
        }

        [NetworkCall(ConnectionEnd = ConnectionEnd.CLIENT, ValidateServer = true)]
        public void Network_Interact(Identity sender, Identity player)
        {
            OnInteract(player.Owner.Player);
        }

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
        public abstract bool CanInteract(Player player);

        /// <summary>
        /// The GameObject which can be interacted
        /// </summary>
        public abstract Mesh InteractableObject { get; }
    }
}