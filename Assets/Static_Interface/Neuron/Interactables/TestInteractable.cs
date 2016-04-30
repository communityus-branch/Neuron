using Static_Interface.API.InteractionFramework;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.PlayerFramework;
using Static_Interface.Internal;
using UnityEngine;

namespace Static_Interface.Neuron.Interactables
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(RigidbodyPositionSyncer))]
    public class TestInteractable : Interactable
    {
        protected override void OnInteract(Player player)
        {
            if (!IsServer())
            {
                return;
            }

            Chat.Instance.SendServerMessage("dont touch me " + player.User.Name);
            GetComponent<Rigidbody>().AddForce(Vector3.up * 20);
        }

        public override string Name => "Test";
        public override string GetInteractMessage()
        {
            return "Test me";
        }

        public override bool CanInteract(Player player)
        {
            return true;
        }

        public override GameObject InteractableObject => gameObject;
    }
}