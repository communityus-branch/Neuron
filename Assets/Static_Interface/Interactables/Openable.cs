using UnityEngine;

namespace Static_Interface.Interactables
{
    public abstract class Openable :  Interactable
    {
        public GameObject OpenableObject;
        public override bool ShouldShowInteractMessage()
        {
            return true;
        }

        public bool IsOpen { get; private set; }

        public override GameObject InteractableObject => OpenableObject;

        public override bool CanInteract()
        {
            return true;
        }

        public override string GetInteractMessage()
        {
            return (IsOpen ? "Close" : "Open") + " " + Name.ToLower();
        }

        public override void Interact()
        {
            if (!IsOpen)
            {
                Open();
            }
            else
            {
                Close();
            }
        }

        public void Open()
        {
            if(OnOpen()) IsOpen = true;
        }

        protected abstract bool OnOpen();
        protected abstract bool OnClose();

        public virtual void Close()
        {
            if(OnClose()) IsOpen = false;
        }
    }
}