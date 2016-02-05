﻿using Static_Interface.API.PlayerFramework;
using UnityEngine;

namespace Static_Interface.API.InteractionFramework
{
    public abstract class Openable :  Interactable
    {
        public GameObject OpenableObject;

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

        public override void Interact(Player player)
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