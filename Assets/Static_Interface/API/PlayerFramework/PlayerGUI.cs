using System.Collections.Generic;
using System.Collections.ObjectModel;
using Static_Interface.API.GUIFramework;
using UnityEngine;

namespace Static_Interface.API.PlayerFramework
{
    public class PlayerGUI : PlayerBehaviour
    {
        public Canvas MainCanvas { get; private set; }
        protected override void Awake()
        {
            base.Awake();
            var cObject = (GameObject)Resources.Load("UI/Canvas");
            cObject = Instantiate(cObject);
            MainCanvas = cObject.GetComponent<Canvas>();
        }

        protected override void OnPlayerLoaded()
        {
            base.OnPlayerLoaded();
            if (UseGUI()) return;
            Destroy(this);
            Destroy(MainCanvas.gameObject);
        }



        private readonly List<ProgressBar> _progressBars = new List<ProgressBar>();
        public ReadOnlyCollection<ProgressBar> StatusProgressBars => _progressBars.AsReadOnly();

        public void AddStatusProgressBar(ProgressBar progressBar)
        {
            if (!UseGUI()) return;
            _progressBars.Add(progressBar);
            progressBar.Parent = MainCanvas.transform;
            UpdatePositions();
            progressBar.Draw = true;
        }

        public void RemoveStatusProgressBar(ProgressBar progressBar, bool destroy = true)
        {
            if (!UseGUI()) return;
            _progressBars.Remove(progressBar);
            UpdatePositions();
            if(destroy) progressBar.Destroy();
        }

        public void UpdatePositions()
        {
            if (!UseGUI()) return;
            var scalefactor = MainCanvas.scaleFactor;
            var x = -(Screen.width * scalefactor/2) + 20 * scalefactor;
            var y = -(Screen.height * scalefactor/2) + 20 * scalefactor;
            Vector2 basePos = new Vector2(x, y);
            foreach (ProgressBar progress in _progressBars)
            {
                Vector2 progressPos = new Vector2(basePos.x, basePos.y);
                progressPos.x += progress.Size.x/2;
                progress.Position = progressPos;
                basePos.y += 5 + progress.Size.x;
            }
        }
    }
}