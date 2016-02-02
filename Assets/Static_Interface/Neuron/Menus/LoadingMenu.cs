using System;
using Static_Interface.API.Level;
using Static_Interface.Internal;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Static_Interface.Neuron.Menus
{
    public class LoadingMenu : MonoBehaviour
    {
        public CanvasRenderer Text;

        private AsyncOperation _loadingOperation;
        void Start()
        {
            ObjectUtils.CheckObjects();

            if (!LevelManager.Instance.IsLoading)
            {
                throw new Exception("Not loading??");
            }

            _loadingOperation = SceneManager.LoadSceneAsync(LevelManager.Instance.PendingLevel);
        }

        // Update is called once per frame
        void Update()
        {
            if (_loadingOperation == null) return;
            double progress = Math.Round(_loadingOperation.progress*100);
            Text.GetComponent<Text>().text = "Loading... (" + progress + "%)";
        }
    }

}
