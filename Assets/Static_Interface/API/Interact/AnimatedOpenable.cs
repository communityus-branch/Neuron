using UnityEngine;

namespace Static_Interface.API.Interact
{
    public abstract class AnimatedOpenable : Openable
    {
        public GameObject AnimatedObject;
        private Animation _animationComponent;
        private float _animationLength;
        protected abstract string OpenAnimation { get; }

        protected override void Start()
        {
            base.Start();
            _animationComponent = AnimatedObject.GetComponent<Animation>();
            _animationLength = _animationComponent[OpenAnimation].length;
        }

        protected override bool OnOpen()
        {
            _animationComponent[OpenAnimation].speed = 1;
            _animationComponent[OpenAnimation].time = 0;
            _animationComponent.Play(OpenAnimation);
            return true;
        }

        protected override bool OnClose()
        {
            _animationComponent[OpenAnimation].speed = -1;
            _animationComponent[OpenAnimation].time = _animationLength;
            _animationComponent.Play(OpenAnimation);
            return true;
        }
    }
}