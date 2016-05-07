using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Static_Interface.API.GUIFramework;
using Static_Interface.API.NetworkFramework;
using Static_Interface.Internal.Objects;
using UnityEngine;

namespace Static_Interface.API.PlayerFramework
{
    public class PlayerGUI : PlayerBehaviour
    {
        private Vector2 _cachedResolution;

        private PlayerGUIViewParent _rootView;
        public PlayerGUIViewParent RootView
        {
            get
            {
                if (_rootView != null) return _rootView;
                _rootView = new PlayerGUIViewParent();
                return _rootView;
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
            Destroy(RootView.Canvas.gameObject);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            RootView.Draw = false;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            RootView.Draw = true;
        }

        protected override void Update()
        {
            base.Update();
            Vector2 currentResolution = new Vector2(Screen.width, Screen.height);
            if (_cachedResolution == currentResolution) return;
            ObjectUtils.BroadcastAll("OnResolutionChanged", currentResolution);
            _cachedResolution = currentResolution;
        }

        protected override void OnResolutionChanged(Vector2 newRes)
        {
            RootView.OnResolutionChanged(newRes);
        }

        protected override void OnGUI()
        {
            base.OnGUI();
            RootView.OnDraw();
        }

        public void AddStatusProgressBar(ProgressBarView progressBarView)
        {
            RootView.AddStatusProgressBar(progressBarView);
        }

        public void RemoveStatusProgressBar(ProgressBarView progressBarView, bool destroy = true)
        {
            RootView.RemoveStatusProgressBar(progressBarView, destroy);
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

        public override void OnResolutionChanged(Vector2 newRes)
        {
            base.OnResolutionChanged(newRes);
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
                progressPos.x += progress.SizeDelta.x / 2;
                progress.AnchoredPosition = progressPos;
                basePos.y += 5 + progress.SizeDelta.x;
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