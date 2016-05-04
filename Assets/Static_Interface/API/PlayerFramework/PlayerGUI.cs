using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Static_Interface.API.GUIFramework;
using UnityEngine;

namespace Static_Interface.API.PlayerFramework
{
    public class PlayerGUI : PlayerBehaviour
    {
        private Vector2 _cachedResolution;

        private PlayerGUIViewParent _viewParent;
        public PlayerGUIViewParent ViewParent
        {
            get
            {
                if (_viewParent != null) return _viewParent;
                _viewParent = new PlayerGUIViewParent();
                return _viewParent;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _cachedResolution = new Vector2(Screen.width, Screen.height);
        }

        protected override void OnPlayerLoaded()
        {
            base.OnPlayerLoaded();
            if (UseGUI()) return;
            Destroy(this);
            Destroy(ViewParent.Canvas.gameObject);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            ViewParent.Draw = false;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            ViewParent.Draw = true;
        }

        protected override void Update()
        {
            base.Update();
            Vector2 currentResolution = new Vector2(Screen.width, Screen.height);
            if (_cachedResolution == currentResolution) return;
            ViewParent.OnResolutionUpdate(_cachedResolution, currentResolution);
            _cachedResolution = currentResolution;
        }

        protected override void OnGUI()
        {
            base.OnGUI();
            ViewParent.OnDraw();
        }

        public void AddStatusProgressBar(ProgressBarView progressBarView)
        {
            ViewParent.AddStatusProgressBar(progressBarView);
        }

        public void RemoveStatusProgressBar(ProgressBarView progressBarView, bool destroy = true)
        {
            ViewParent.RemoveStatusProgressBar(progressBarView, destroy);
        }
    }

    public class PlayerGUIViewParent : ViewParent
    {
        private readonly List<ProgressBarView> _statusProgressBars = new List<ProgressBarView>();
        public ReadOnlyCollection<ProgressBarView> StatusProgressBars => _statusProgressBars.AsReadOnly();
        private readonly List<View> _registeredViews = new List<View>();
        public override Canvas Canvas => _canvas;
        private Canvas _canvas;

        public PlayerGUIViewParent() : base("PlayerGUI", null,0,0)
        {

        }

        public override GameObject GetViewObject()
        {
            if (_canvas == null)
            {
                var cObject = (GameObject)Resources.Load("UI/Canvas");
                cObject = Object.Instantiate(cObject);
                _canvas = cObject.GetComponent<Canvas>();
            }
            return base.GetViewObject();
        }

        public override void SetParent(ViewParent parent)
        {
            throw new System.NotSupportedException();
        }

        public override Collection<View> GetChilds()
        {
            return new Collection<View>(_registeredViews);
        }
        
        public override void AddView(View view)
        {
            view.Parent = Canvas.transform;
            _registeredViews.Add(view);
            view.ViewParent = this;
        }

        public override  void RemoveView(View view)
        {
            view.OnDestroy();
            _registeredViews.Remove(view);
            view.Parent = null;
            view.ViewParent = null;
        }

        public override void OnDraw()
        {
            foreach (View view in _registeredViews.Where(v => v.Draw))
            {
                view.OnDraw();
            }
        }

        public void OnResolutionUpdate(Vector2 res, Vector2 newRes)
        {
            foreach (View view in _registeredViews)
            {
                view.OnResolutionChanged(res, newRes);
            }
            UpdatePositions();
        }

        public void UpdatePositions()
        {
            var scalefactor = Canvas.scaleFactor;
            var x = -(Screen.width * scalefactor / 2) + 20 * scalefactor;
            var y = -(Screen.height * scalefactor / 2) + 20 * scalefactor;
            Vector2 basePos = new Vector2(x, y);
            foreach (ProgressBarView progress in _statusProgressBars)
            {
                Vector2 progressPos = new Vector2(basePos.x, basePos.y);
                progressPos.x += progress.Size.x / 2;
                progress.Position = progressPos;
                basePos.y += 5 + progress.Size.x;
            }
        }

        public void AddStatusProgressBar(ProgressBarView progressBarView)
        {
            AddView(progressBarView);
            _statusProgressBars.Add(progressBarView);
            UpdatePositions();
        }

        public void RemoveStatusProgressBar(ProgressBarView progressBarView, bool destroy = true)
        {
            RemoveView(progressBarView);
            UpdatePositions();
            if (destroy) progressBarView.Destroy();
        }
    }
}