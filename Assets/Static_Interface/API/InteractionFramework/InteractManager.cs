﻿using System;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;
using Static_Interface.Internal;
using UnityEngine;

namespace Static_Interface.API.InteractionFramework
{
    public class InteractManager : NetworkedSingletonBehaviour<InteractManager>
    {
        public const float INTERACT_RANGE = 5.0f;
        public KeyCode InteractKey = KeyCode.F;
        private RaycastHit _hit;
        public Interactable CurrentInteractable;

        private void ResetInteract()
        {
            if (CurrentInteractable == null) return;
            Highlighter.Highlight(CurrentInteractable.gameObject);
            CurrentInteractable = null;
        }

        protected override void Update()
        {
            base.Update();
            if (InputUtil.Instance.IsInputLocked() || Camera.main == null ||
                (Player.MainPlayer == null || Player.MainPlayer.Health.IsDead))
            {
                CurrentInteractable = null;
                return;
            }

            Vector3 p = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width/2, Screen.height/2, Camera.main.nearClipPlane));
            if (!Physics.Raycast(p, Camera.main.transform.forward, out _hit, INTERACT_RANGE))
            {
                CurrentInteractable = null;
                return;
            }

            Interactable previousInteractable = CurrentInteractable;
            CurrentInteractable = _hit.collider.gameObject.GetComponent<Interactable>();

            if (CurrentInteractable == null)
            {
                Transform currentGameObject = _hit.collider.gameObject.transform;
                while (currentGameObject != null)
                {
                    CurrentInteractable = currentGameObject.GetComponent<Interactable>();
                    if (CurrentInteractable != null)
                    {
                        break;
                    }
                    currentGameObject = currentGameObject.transform.parent;
                }

                if (currentGameObject == null)
                {
                    Reset();
                    return;
                }
            }
            if (previousInteractable != null && CurrentInteractable != previousInteractable)
            {
                Highlighter.Unhighlight(previousInteractable.gameObject);
            }

            if (!CurrentInteractable.CanInteract(Player.MainPlayer))
            {
                CurrentInteractable = null;
                return;
            }

            Highlighter.Highlight(CurrentInteractable.gameObject);

            if (!Input.GetKeyDown(InteractKey))
            {
                return;
            }
            //Todo: onInteract Event

            LogUtils.Debug("Interacting...");
            CurrentInteractable.Interact(Player.MainPlayer);
        }

        protected override void OnGUI()
        {
            base.OnGUI();
            if (Player.MainPlayer != null && !Player.MainPlayer.Health.IsDead && CurrentInteractable != null && CurrentInteractable.CanInteract(Player.MainPlayer))
            {
                GUI.Label(new Rect(Screen.width / 2 - 75, Screen.height - 100, Screen.width / 2, 30), "[" + InteractKey + "] " + CurrentInteractable.GetInteractMessage());
            }
        }
    }
}