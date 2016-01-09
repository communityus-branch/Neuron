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

        private AsyncOperation loadingOperation;
        void Start()
        {
            if (!LevelManager.Instance.IsLoading)
            {
                throw new Exception("Not loading??");
            }

            loadingOperation = SceneManager.LoadSceneAsync(LevelManager.Instance.PendingLevel);
        }

        // Update is called once per frame
        void Update()
        {
            if (loadingOperation == null) return;
            double progress = Math.Round(loadingOperation.progress*100);
            Text.GetComponent<Text>().text = "Loading... (" + progress + "%)";
        }
    }

}
