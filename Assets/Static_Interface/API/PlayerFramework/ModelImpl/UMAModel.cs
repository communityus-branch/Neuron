using System;
using UMA;
using UnityEngine;

namespace Static_Interface.API.PlayerFramework.ModelImpl
{
    public class UMAModel : PlayerModel
    {
        //Todo: network sync DNA and slots
        public UMADynamicAvatar UMADynamicAvatar => Model.GetComponent<UMADynamicAvatar>();
        internal GameObject InternalModel;
        internal PlayerModelController Controller;
        public override PlayerModelController PlayerModelController => Controller;
        public override GameObject Model => InternalModel;

        protected override Vector3 GetFPSCameraLocalPosition()
        {
            return new Vector3(0f, UMADynamicAvatar.umaData.characterHeight - 0.35f, -0.18f);
        }

        protected override void OnModelUpdate()
        {
            //do nothing
        }
    }
}