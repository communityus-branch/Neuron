using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Static_Interface.API.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tizen;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Static_Interface.API.GUIFramework
{
    //Todo: Implement overlapping of windows, focusing etc
    public class WindowView : ViewParent
    {
        protected GameObject Prefab { get; set; }
        protected string PrefabLocation => "UI/Window";
        private readonly List<View> _childs = new List<View>();
        public Button CloseButton { get; protected set; }

        public RectTransform Content { get; private set; }
        public Text Title { get; private set; }

        public UnityEvent ShowEvent { get; } = new UnityEvent();
        public UnityEvent HideEvent { get; } = new UnityEvent();

        public bool HookCursor { get; set; } = true;
        public bool LockInput { get; set; } = true;

        private bool _wasCursorVisible;
        private bool _wasLocked;

        //Todo: implement moving/draging with mouse
        public bool Moveable { get; set; }

        public WindowView(string viewName, ViewParent parent) : base(viewName, parent)
        {
            if(parent == null) throw new ArgumentNullException(nameof(parent));
            OnCreate(parent.Canvas);
        }

        private void OnCreate(Canvas canvas)
        {
            _canvas = canvas;
            Scale = Vector3.one;
            _wasCursorVisible = Cursor.visible;
            Show();
        }

        public WindowView(string viewName, ViewParent parent, int x, int y) : base(viewName, parent, x, y)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            OnCreate(parent.Canvas);
        }

        public WindowView(string viewName, Canvas canvas) : base(viewName, null)
        {
            GetViewObject().transform.SetParent(canvas.transform);
            OnCreate(canvas);
        }

        private Canvas _canvas;
        public override Canvas Canvas => _canvas;

        public override GameObject GetViewObject()
        {
            return Prefab;
        }

        public override void AddView(View view)
        {
            view.ViewParent = this;
            view.Parent = Content;
            _childs.Add(view);
        }
        
        public override void RemoveView(View view)
        {
            _childs.Remove(view);
            view.Destroy();
        }

        public override ReadOnlyCollection<View> GetChilds()
        {
            return _childs.AsReadOnly();
        }

        protected override void InitGameObject()
        {
            base.InitGameObject();
            Prefab = (GameObject)Resources.Load(PrefabLocation);
            Prefab = Object.Instantiate(Prefab);
            Content = (RectTransform) Prefab.transform.FindChild("Content").transform;
            Title = Prefab.transform.FindChild("Title").GetComponent<Text>();
            SetTitle(ViewName);
            CloseButton = Prefab.transform.FindChild("CloseButton").GetComponent<Button>();
            CloseButton.onClick.AddListener(Hide);
            Width = Screen.width/2;
            Height = Screen.height/2;
        }

        public void SetTitle(string title)
        {
            Title.text = title;
        }

        public void Hide()
        {
            if(HookCursor) Cursor.visible = _wasCursorVisible;
            if (LockInput)
            {
                if (_wasLocked) InputUtil.Instance.UnlockInput(this);
                _wasLocked = false;
            }
            Draw = false;
            HideEvent.Invoke();
        }

        public void Show()
        {
            if (HookCursor)
            {
                _wasCursorVisible = Cursor.visible;
                Cursor.visible = true;
            }
            if (LockInput)
            {
                InputUtil.Instance.LockInput(this);
                _wasLocked = true;
            }
            Draw = true;
            ShowEvent.Invoke();
        }
    }
}