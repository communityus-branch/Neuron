using System.Collections.Generic;
using UnityEngine;
using MonoBehaviour = Static_Interface.API.UnityExtensions.MonoBehaviour;

namespace Static_Interface.API.Utils
{
    [AddComponentMenu("Camera-Control/Smooth Mouse Look")]
    public class SmoothMouseLook : MonoBehaviour
    {

        public enum RotationAxes { MOUSE_X_AND_Y = 0, MOUSE_X = 1, MOUSE_Y = 2 }
        public RotationAxes Axes = RotationAxes.MOUSE_X_AND_Y;
        public float SensitivityX = 15F;
        public float SensitivityY = 15F;

        public float MinimumX = -360F;
        public float MaximumX = 360F;

        public float MinimumY = -60F;
        public float MaximumY = 60F;

        float _rotationX;
        float _rotationY;

        private readonly List<float> _rotArrayX = new List<float>();
        float _rotAverageX;

        private readonly List<float> _rotArrayY = new List<float>();
        float _rotAverageY;

        public float FrameCounter = 20;

        public Quaternion? OriginalRotation;

        protected override void Update()
        {
            base.Update();
            switch (Axes)
            {
                case RotationAxes.MOUSE_X_AND_Y:
                {
                    _rotAverageY = 0f;
                    _rotAverageX = 0f;

                    _rotationY += Input.GetAxis("Mouse Y") * SensitivityY;
                    _rotationX += Input.GetAxis("Mouse X") * SensitivityX;

                    _rotArrayY.Add(_rotationY);
                    _rotArrayX.Add(_rotationX);

                    if (_rotArrayY.Count >= FrameCounter)
                    {
                        _rotArrayY.RemoveAt(0);
                    }
                    if (_rotArrayX.Count >= FrameCounter)
                    {
                        _rotArrayX.RemoveAt(0);
                    }

                    foreach (float rotY in _rotArrayY)
                    {
                        _rotAverageY += rotY;
                    }
                    foreach (float rotX in _rotArrayX)
                    {
                        _rotAverageX += rotX;
                    }

                    _rotAverageY /= _rotArrayY.Count;
                    _rotAverageX /= _rotArrayX.Count;

                    _rotAverageY = ClampAngle(_rotAverageY, MinimumY, MaximumY);
                    _rotAverageX = ClampAngle(_rotAverageX, MinimumX, MaximumX);

                    Quaternion yQuaternion = Quaternion.AngleAxis(_rotAverageY, Vector3.left);
                    Quaternion xQuaternion = Quaternion.AngleAxis(_rotAverageX, Vector3.up);

                    transform.localRotation = OriginalRotation.GetValueOrDefault(Quaternion.identity) * xQuaternion * yQuaternion;
                }
                    break;
                case RotationAxes.MOUSE_X:
                {
                    _rotAverageX = 0f;

                    _rotationX += Input.GetAxis("Mouse X") * SensitivityX;

                    _rotArrayX.Add(_rotationX);

                    if (_rotArrayX.Count >= FrameCounter)
                    {
                        _rotArrayX.RemoveAt(0);
                    }
                    foreach (float rotX in _rotArrayX)
                    {
                        _rotAverageX += rotX;
                    }
                    _rotAverageX /= _rotArrayX.Count;

                    _rotAverageX = ClampAngle(_rotAverageX, MinimumX, MaximumX);

                    Quaternion xQuaternion = Quaternion.AngleAxis(_rotAverageX, Vector3.up);
                    transform.localRotation = OriginalRotation.GetValueOrDefault(Quaternion.identity) * xQuaternion;
                }
                    break;
                default:
                {
                    _rotAverageY = 0f;

                    _rotationY += Input.GetAxis("Mouse Y") * SensitivityY;

                    _rotArrayY.Add(_rotationY);

                    if (_rotArrayY.Count >= FrameCounter)
                    {
                        _rotArrayY.RemoveAt(0);
                    }
                    foreach (float rotY in _rotArrayY)
                    {
                        _rotAverageY += rotY;
                    }
                    _rotAverageY /= _rotArrayY.Count;

                    _rotAverageY = ClampAngle(_rotAverageY, MinimumY, MaximumY);

                    Quaternion yQuaternion = Quaternion.AngleAxis(_rotAverageY, Vector3.left);
                    transform.localRotation = OriginalRotation.GetValueOrDefault(Quaternion.identity) * yQuaternion;
                }
                    break;
            }
        }

        protected override void Start()
        {
            base.Start();
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb)
                rb.freezeRotation = true;
            if(OriginalRotation == null) OriginalRotation = transform.localRotation;
        }

        public static float ClampAngle(float angle, float min, float max)
        {
            angle = angle % 360;
            if ((angle >= -360F) && (angle <= 360F))
            {
                if (angle < -360F)
                {
                    angle += 360F;
                }
                if (angle > 360F)
                {
                    angle -= 360F;
                }
            }
            return Mathf.Clamp(angle, min, max);
        }
    }
}