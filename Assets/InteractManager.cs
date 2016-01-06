using System;
using Assets.Interactables;
using UnityEngine;
using Assets.Utils;

namespace Assets
{
    public class InteractManager : MonoBehaviour
    {
        public const float InteractRange = 5.0f;
        public KeyCode InteractKey = KeyCode.F;
        private RaycastHit hit;
        public Interactable CurrentInteractable;
        public static InteractManager Instance;

        void Start()
        {
            if(Instance != null) throw new Exception("Only one instance allowed");
            Instance = this;
        }

        void Update()
        {
            Vector3 p = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width/2, Screen.height/2, Camera.main.nearClipPlane));
            if (!Physics.Raycast(p, Camera.main.transform.forward, out hit, InteractRange) || hit.collider.gameObject.GetComponent<Interactable>() == null)
            {
                if (CurrentInteractable == null) return;
                Highlighter.Highlight(CurrentInteractable.InteractableObject);
                CurrentInteractable = null;
                return;
            }

            Interactable previousInteractable = CurrentInteractable;
            CurrentInteractable = hit.collider.gameObject.GetComponent<Interactable>();

            if (previousInteractable != null && CurrentInteractable != previousInteractable)
            {
                Highlighter.Unhighlight(previousInteractable.InteractableObject);
            }

            if (!CurrentInteractable.CanInteract())
            {
                CurrentInteractable = null;
                return;
            }

            Highlighter.Highlight(CurrentInteractable.InteractableObject);

            if (!Input.GetKeyDown(InteractKey))
            {
                return;
            }

            Debug.Log("Interacting...");
            CurrentInteractable.Interact();
        }

        public void OnGUI()
        {
            if (CurrentInteractable != null && CurrentInteractable.ShouldShowInteractMessage())
            {
                GUI.Label(new Rect(Screen.width / 2 - 75, Screen.height - 100, Screen.width / 2, 30), "[" + InteractKey + "] " + CurrentInteractable.GetInteractMessage());
            }
        }
    }
}