using UnityEngine;

namespace Assets.Interactables
{
    public class Gate : Openable
    {
        public override string Name
        {
            get { return "Gate"; }
        }

        protected override Vector3 GetOpenRotation(Vector3 defaultRotation)
        {
            return new Vector3(defaultRotation.x, defaultRotation.y, defaultRotation.z + Angle);
        }
    }
}