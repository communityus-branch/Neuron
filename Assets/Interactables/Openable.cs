using UnityEngine;

namespace Assets.Interactables
{
    public abstract class Openable :  Interactable
    {
        private bool open;
        public GameObject OpenableObject;
        public GameObject Hinge;
        public override bool ShouldShowInteractMessage()
        {
            return true;
        }

        public virtual bool IsOpen
        {
            get { return open; }
        }

        public override GameObject InteractableObject
        {
            get { return OpenableObject;}
        }

        public override bool CanInteract()
        {
            return true;
        }

        public override string GetInteractMessage()
        {
            return (IsOpen ? "Close" : "Open") + " " + Name.ToLower();
        }

        private const float distance = 5.0f;

        public float Angle = 90f;
        public float Smooth = 2f;

        private Vector3 defaultRot;
        private Vector3 openRot;

        protected virtual void Start()
        {
            defaultRot = Hinge.transform.eulerAngles;
            openRot = GetOpenRotation(defaultRot);
        }

        protected abstract Vector3 GetOpenRotation(Vector3 defaultRotation);

        public override void Interact()
        {
            if (!open)
            {
                Open();
            }
            else
            {
                Close();
            }
        }

        protected virtual void Update()
        {
            UpdateState();
        }

        public virtual void UpdateState()
        {
            Hinge.transform.eulerAngles = Vector3.Slerp(Hinge.transform.eulerAngles, open ?
                openRot : defaultRot, Time.deltaTime * Smooth);
        }

        public virtual void Open()
        {
            open = true;
        }

        public virtual void Close()
        {
            open = false;
        }
    }
}