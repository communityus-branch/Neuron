using UnityEngine;

namespace Static_Interface.API.PlayerFramework
{
    public abstract class PlayerModel : PlayerBehaviour
    {
        public abstract PlayerModelController PlayerModelController { get; }
        public abstract GameObject Model { get; }
        protected virtual Quaternion GetCameraLocalRotation()
        {
            return Quaternion.identity;
        }

        protected abstract Vector3 GetFPSCameraLocalPosition();

        protected override void Start()
        {
            base.Start();
            UpdateModel();
        }

        public void UpdateModel()
        {
            Player.Camera.transform.localRotation = GetCameraLocalRotation();
            Player.Camera.transform.localPosition = GetFPSCameraLocalPosition();
            OnModelUpdate();
        }

        protected abstract void OnModelUpdate();
    }
}