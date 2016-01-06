using UnityEngine;

namespace Assets.Interactables
{
    public class Door : Openable
    {
        public override string Name
        {
            get { return "Door"; }
        }

        protected override Vector3 GetOpenRotation(Vector3 defaultRotation)
        {
            return new Vector3(defaultRotation.x, defaultRotation.y + Angle, defaultRotation.z);
        }
    }
}