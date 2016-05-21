using System;
using Static_Interface.API.LevelFramework;
using Static_Interface.Internal.Objects;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MonoBehaviour = Static_Interface.API.UnityExtensions.MonoBehaviour;

namespace Static_Interface.Neuron.Menus
{
    public class LoadingMenu : MonoBehaviour
    {
        public CanvasRenderer Text;

        public static AsyncOperation LoadingOperation;
        protected override void Start()
        {
            base.Start();
            InternalObjectUtils.CheckObjects();

            if (!LevelManager.Instance.IsLoading)
            {
                throw new Exception("Not loading??");
            }

            LoadingOperation = SceneManager.LoadSceneAsync(LevelManager.Instance.PendingLevel);
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();
            if (LoadingOperation == null) return;
            double progress = Math.Round(LoadingOperation.progress*100);
            Text.GetComponent<Text>().text = "Loading... (" + progress + "%)";
        }
    }

}
