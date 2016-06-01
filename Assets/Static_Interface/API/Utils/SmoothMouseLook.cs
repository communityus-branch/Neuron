using UnityEngine;
using MonoBehaviour = Static_Interface.API.UnityExtensions.MonoBehaviour;

namespace Static_Interface.API.Utils
{
    [AddComponentMenu("Camera/Simple Smooth Mouse Look ")]
    public class SmoothMouseLook : MonoBehaviour
    {

        [Range(0.1f, 10f)] public float Sensitivity = 5f;

        [Range(0.01f, 1f)] public float Smoothing = 0.05f;

        public bool InvertY = false;

        public Vector2 RotationVelocity;

        private Vector2 _mouse;
        private Vector2 _smooth;


        protected override void Update()
        {
            _mouse.y += (Input.GetAxis("Mouse Y")*Sensitivity)*(InvertY ? 1 : -1);
            _mouse.x += Input.GetAxis("Mouse X") * Sensitivity;

            _smooth.x = Mathf.SmoothDamp(_smooth.x, _mouse.x, ref RotationVelocity.x, Smoothing);
            _smooth.y = Mathf.SmoothDamp(_smooth.y, _mouse.y, ref RotationVelocity.y, Smoothing);
            
            transform.localRotation = Quaternion.identity;
            transform.Rotate(_smooth.y, _smooth.x, 0);
        }
    }
}