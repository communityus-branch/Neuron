using UnityEngine;

namespace Static_Interface.API.InteractionFramework
{
    public abstract class AnimatedOpenable : Openable
    {
        public GameObject AnimatedObject;
        private Animation _animationComponent;
        private float _animationLength;
        protected abstract string Animation { get; }

        protected override void Start()
        {
            base.Start();
            _animationComponent = AnimatedObject.GetComponent<Animation>();
            _animationLength = _animationComponent[Animation].length;
        }

        protected override bool OnOpen()
        {
            _animationComponent[Animation].speed = 1;
            _animationComponent[Animation].time = 0;
            _animationComponent.Play(Animation);
            return true;
        }

        protected override bool OnClose()
        {
            _animationComponent[Animation].speed = -1;
            _animationComponent[Animation].time = _animationLength;
            _animationComponent.Play(Animation);
            return true;
        }
    }
}