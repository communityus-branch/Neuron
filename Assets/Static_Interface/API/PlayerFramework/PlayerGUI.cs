using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Static_Interface.API.GUIFramework;
using UnityEngine;

namespace Static_Interface.API.PlayerFramework
{
    public class PlayerGUI : PlayerBehaviour
    {
        private Vector2 _res;
        public Canvas MainCanvas { get; private set; }
        private readonly List<View> _registeredViews = new List<View>(); 

        protected override void Awake()
        {
            base.Awake();
            var cObject = (GameObject)Resources.Load("UI/Canvas");
            cObject = Instantiate(cObject);
            MainCanvas = cObject.GetComponent<Canvas>();
            _res = new Vector2(Screen.width, Screen.height);
        }

        protected override void OnPlayerLoaded()
        {
            base.OnPlayerLoaded();
            if (UseGUI()) return;
            Destroy(this);
            Destroy(MainCanvas.gameObject);
        }

        public void AddView(View view)
        {
            view.Parent = MainCanvas.transform;
            _registeredViews.Add(view);
        }

        public void RemoveView(View view)
        {
            view.OnDestroy();
            _registeredViews.Remove(view);
            view.Parent = null;
        }

        protected override void Update()
        {
            base.Update();
            Vector2 currentRes = new Vector2(Screen.width, Screen.height);
            if (_res != currentRes)
            {
                OnResolutionUpdate(_res, currentRes);
                _res = currentRes;
            }
        }

        protected override void OnGUI()
        {
            base.OnGUI();
            foreach (View view in _registeredViews.Where(v => v.Draw))
            {
                view.OnDraw();
            }
        }

        private void OnResolutionUpdate(Vector2 res, Vector2 newRes)
        {
            foreach (View view in _registeredViews)
            {
                view.OnResolutionChanged(res, newRes);
            }
            UpdatePositions();   
        }

        private readonly List<ProgressBar> _statusProgressBars = new List<ProgressBar>();
        public ReadOnlyCollection<ProgressBar> StatusProgressBars => _statusProgressBars.AsReadOnly();

        public void AddStatusProgressBar(ProgressBar progressBar)
        {
            AddView(progressBar);
            _statusProgressBars.Add(progressBar);
            UpdatePositions();
        }

        public void RemoveStatusProgressBar(ProgressBar progressBar, bool destroy = true)
        {
            if (!UseGUI()) return;
            _statusProgressBars.Remove(progressBar);
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
            foreach (ProgressBar progress in _statusProgressBars)
            {
                Vector2 progressPos = new Vector2(basePos.x, basePos.y);
                progressPos.x += progress.Size.x/2;
                progress.Position = progressPos;
                basePos.y += 5 + progress.Size.x;
            }
        }
    }
}