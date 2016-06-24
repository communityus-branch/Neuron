using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Static_Interface.API.PlayerFramework
{
    public class PlayerMouseLook : PlayerBehaviour
    {
        public float SensitivityX = 5f;
        public float SensitivityY = 5f;

        public float DefaultSensX = 5f;
        public float DefaultSensY = 5f;

        public float MinimumY = -60f;
        public float MaximumY = 60f;

        public int FrameCounterX = 15;
        public int FrameCounterY = 15;

        private float _rotationX;
        private float _rotationY;
        
        private Quaternion _xQuaternion;
        private Quaternion _yQuaternion;
        private readonly Quaternion _originalRotation = Quaternion.identity;

        private readonly List<float> _rotArrayX = new List<float>();
        private readonly List<float> _rotArrayY = new List<float>();
       
        protected override void Update()
        {
#if !UNITY_EDITOR
            if (Cursor.visible) return;
#endif
            _rotationX += Input.GetAxis("Mouse X") * SensitivityX;
            _rotArrayX.Add(_rotationX);

            if (_rotArrayX.Count >= FrameCounterX)
            {
                _rotArrayX.RemoveAt(0);
            }

            float rotAverageX = _rotArrayX.Sum();
            rotAverageX /= _rotArrayX.Count;

            _rotationY += Input.GetAxis("Mouse Y") * SensitivityY;
            _rotationY = ClampAngle(_rotationY, MinimumY, MaximumY);

            _rotArrayY.Add(_rotationY);

            if (_rotArrayY.Count >= FrameCounterY)
            {
                _rotArrayY.RemoveAt(0);
            }

            float rotAverageY = _rotArrayY.Sum();
            rotAverageY /= _rotArrayY.Count;

            _xQuaternion = Quaternion.AngleAxis(rotAverageX, Vector3.up);
            _yQuaternion = Quaternion.AngleAxis(rotAverageY, Vector3.left);

            var targetRotation = _originalRotation * _xQuaternion * _yQuaternion;
            var euler = targetRotation.eulerAngles;

            //Apply Y rotation to Model
            var newAngle = Player.Model.transform.localRotation.eulerAngles;
            newAngle.y = euler.y;
            Player.Model.transform.localRotation = Quaternion.Euler(newAngle);

            //Apply X and Z rotation to Camera
            newAngle = Player.Camera.transform.localRotation.eulerAngles;
            newAngle.x = euler.x;
            newAngle.z = euler.z;
            Player.Camera.transform.localRotation = Quaternion.Euler(newAngle);
        }

        public static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360F)
                angle += 360F;
            if (angle > 360F)
                angle -= 360F;
            return Mathf.Clamp(angle, min, max);
        }
    }
}