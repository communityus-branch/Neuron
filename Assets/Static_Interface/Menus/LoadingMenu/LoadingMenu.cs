using System;
using UnityEngine;
using UnityEngine.UI;
using Static_Interface.Level;
using UnityEngine.SceneManagement;

namespace Static_Interface.Menus.LoadingMenu
{
    public class LoadingMenu : MonoBehaviour
    {
        public CanvasRenderer Text;

        private AsyncOperation _loadingOperation;
        void Start()
        {
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
