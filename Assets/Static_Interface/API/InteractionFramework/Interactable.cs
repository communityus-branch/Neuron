using Static_Interface.API.NetworkFramework;
using Static_Interface.API.PlayerFramework;
using Static_Interface.Internal;
using UnityEngine;
using MonoBehaviour = Static_Interface.API.UnityExtensions.MonoBehaviour;

namespace Static_Interface.API.InteractionFramework
{
    /// <summary>
    /// Objects which a player can interact with
    /// </summary>
    public abstract class Interactable : NetworkedBehaviour
    {
        protected override Channel SetupChannel()
        {
            var ch = World.Instance.GetComponent<Channel>();
            ch.Build(this);
            return ch;
        }

        /// <summary>
        /// Called when a player interacts with it
        /// </summary>
        public virtual void Interact(Player player)
        {
            if (player.Health.IsDead) return;
            if (!CanInteract(player)) return;
            if (IsClient())
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
            var player = ident.Owner.Player;
            if (player == null || player.Health.IsDead) return;
            if (!CanInteract(player)) return;
            OnInteract(player);
            //Todo: add radius
            Channel.Send(nameof(Network_Interact), ECall.Clients, ident);
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
        public abstract GameObject InteractableObject { get; }
    }
}