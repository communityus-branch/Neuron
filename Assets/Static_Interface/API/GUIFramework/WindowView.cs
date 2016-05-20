using Static_Interface.API.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Static_Interface.API.GUIFramework
{
    //Todo: Implement overlapping of windows, focusing etc
    public class WindowView : PrefabViewParent
    {
        protected override string PrefabLocation => "UI/Window";

        public virtual Button CloseButton { get; protected set; }

        public virtual RectTransform Content { get; private set; }
        public virtual Text Title { get; protected set; }

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
            OnCreate(parent.Canvas);
        }

        public WindowView(string viewName, ViewParent parent, int x, int y) : base(viewName, parent, x, y)
        {
            OnCreate(parent.Canvas);
        }

        public WindowView(string viewName, Canvas canvas) : base(viewName, canvas)
        {
            OnCreate(canvas);
        }
        
        private void OnCreate(Canvas canvas)
        {
            Scale = Vector3.one;
            _wasCursorVisible = Cursor.visible;
            Show();
        }

        public override void AddView(View view)
        {
            base.AddView(view);
            view.Parent = Content;
        }
        
        protected override void InitGameObject()
        {
            base.InitGameObject();
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