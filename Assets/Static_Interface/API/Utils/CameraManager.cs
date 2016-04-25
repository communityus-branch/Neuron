using System;
using Static_Interface.API.UnityExtensions;
using UnityEngine;

namespace Static_Interface.API.Utils
{
    public class CameraManager : PersistentScript<CameraManager>
    {
        private bool parentSet;
        private Camera _currentCamera;

        public Camera CurrentCamera
        {
            get { return _currentCamera; }
            set
            {
                try
                {
                    if (_currentCamera != null) _currentCamera.enabled = false;
                }
                catch (Exception)
                {
                    // Who cares?
                }


                _currentCamera = value;

                if (value == null) return;

                if (UnistormCamera != null)
                {
                    SetUnistormParent(_currentCamera.transform);
                    parentSet = true;
                }
                else
                {
                    parentSet = false;
                }

                _currentCamera.enabled = true;
            }
        }

        private void SetUnistormParent(Transform parentTransform)
        {
            UnistormCamera.transform.SetParent(parentTransform);
            UnistormCamera.transform.localPosition = Vector3.zero;
            UnistormCamera.transform.localRotation = Quaternion.identity;
        }

        protected override void Update()
        {
            base.Update();
            if (!parentSet && UnistormCamera != null && CurrentCamera != null)
            {
                SetUnistormParent(CurrentCamera.transform);
                parentSet = true;
            }
        }

        private GameObject _unistormCamera ;
        public GameObject UnistormCamera
        {
            get
            {
                if (_unistormCamera != null) return _unistormCamera;
                _unistormCamera = (GameObject)Instantiate(Resources.Load("UnistormCamera"));
                return _unistormCamera;
            }
        }
    }
}