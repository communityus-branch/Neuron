using UnityEngine;

namespace Static_Interface.API.PlayerFramework.ModelImpl
{
    public class UMAModel : PlayerModel
    {
        public override string Name { get; protected set; } = "Random UMA Model";

        protected override Quaternion GetCameraLocalRotation()
        {
            throw new System.NotImplementedException();
        }

        protected override Vector3 GetCameraLocalPosition()
        {
            throw new System.NotImplementedException();
        }

        protected override GameObject LoadModel()
        {
            throw new System.NotImplementedException();
        }
    }
}