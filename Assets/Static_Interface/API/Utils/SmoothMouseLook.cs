using Static_Interface.API.PlayerFramework;
using Static_Interface.Internal;
using UnityEngine;
using MonoBehaviour = Static_Interface.API.UnityExtensions.MonoBehaviour;

namespace Static_Interface.API.Utils
{
    [AddComponentMenu("Camera/Simple Smooth Mouse Look ")]
    public class SmoothMouseLook : PlayerBehaviour

    {
        Vector2 _mouseAbsolute;
        Vector2 _smoothMouse;

        public Vector2 ClampInDegrees = new Vector2(360, 180);
        public Vector2 Sensitivity = new Vector2(2, 2);
        public Vector2 Smoothing = new Vector2(3, 3);
        public Vector2 TargetDirection;

        protected override void Start()
        {
            // Set target direction to the camera's initial orientation.
            TargetDirection = Player.Model.rotation.eulerAngles;
        }

        protected override void Update()
        {
            if (PauseHook.IsPaused) return;
            // Allow the script to clamp based on a desired target value.
            Quaternion targetOrientation = Quaternion.Euler(TargetDirection);

            // Get raw mouse input for a cleaner reading on more sensitive mice.
            var mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

            // Scale input against the Sensitivity setting and multiply that against the Smoothing value.
            mouseDelta = Vector2.Scale(mouseDelta, new Vector2(Sensitivity.x * Smoothing.x, Sensitivity.y * Smoothing.y));

            // Interpolate mouse movement over time to apply Smoothing delta.
            _smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1f / Smoothing.x);
            _smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1f / Smoothing.y);

            // Find the absolute mouse movement value from point zero.
            _mouseAbsolute += _smoothMouse;

            // Clamp and apply the local x value first, so as not to be affected by world transforms.
            if (ClampInDegrees.x < 360)
                _mouseAbsolute.x = Mathf.Clamp(_mouseAbsolute.x, -ClampInDegrees.x * 0.5f, ClampInDegrees.x * 0.5f);

            var xRotation = Quaternion.AngleAxis(-_mouseAbsolute.y, targetOrientation * Vector3.right);
            Player.Model.localRotation = xRotation;

            // Then clamp and apply the global y value.
            if (ClampInDegrees.y < 360)
                _mouseAbsolute.y = Mathf.Clamp(_mouseAbsolute.y, -ClampInDegrees.y * 0.5f, ClampInDegrees.y * 0.5f);

            var yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, Player.Model.InverseTransformDirection(Vector3.up));
            Player.Model.localRotation *= yRotation;
            Player.Model.rotation *= targetOrientation;
        }

    }
}