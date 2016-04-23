using Static_Interface.API.UnityExtensions;
using UnityEngine;
using MonoBehaviour = Static_Interface.API.UnityExtensions.MonoBehaviour;

namespace Static_Interface.API.Utils
{
    public class CameraManager : SingletonComponent<CameraManager>
    {
        private Camera _currentCamera;

        public Camera CurrentCamera
        {
            get { return _currentCamera; }
            set
            {
                if (_currentCamera != null) _currentCamera.enabled = false;
                UnistormCamera.transform.SetParent(value.gameObject.transform);
                UnistormCamera.transform.localPosition = Vector3.zero;
                UnistormCamera.transform.localRotation = Quaternion.identity;
                value.enabled = true;
                _currentCamera = value;
            }
        }

        public GameObject UnistormCamera { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            UnistormCamera = (GameObject)Instantiate(Resources.Load("UnistormCamera"));
        }
    }
}