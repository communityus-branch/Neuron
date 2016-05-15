using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Static_Interface.API.GUIFramework
{
    public class WindowView : ViewParent
    {
        protected GameObject Prefab { get; set; }
        protected string PrefabLocation => "UI/Window";
        private readonly List<View> _childs = new List<View>();
        private Button _closeButton;
        public RectTransform Content { get; private set; }
        public Text Title { get; private set; }

        //Todo: implement moving/draging with mouse
        public bool Moveable { get; set; }

        public WindowView(string viewName, ViewParent parent) : base(viewName, parent)
        {
            if(parent == null) throw new ArgumentNullException(nameof(parent));
            Canvas = parent.Canvas;
        }

        public WindowView(string viewName, ViewParent parent, int x, int y) : base(viewName, parent, x, y)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            Canvas = parent.Canvas;
        }

        public WindowView(string viewName, Canvas canvas) : base(viewName, null)
        {
            Canvas = canvas;
        }

        public override Canvas Canvas { get; }

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
            _closeButton = Prefab.transform.FindChild("CloseButton").GetComponent<Button>();
            _closeButton.onClick.AddListener(Close);
            Width = Screen.width/2;
            Height = Screen.height/2;
        }

        public void SetTitle(string title)
        {
            Title.text = title;
        }

        public void Close()
        {
            Draw = false;
            OnClose();
        }

        protected virtual void OnClose()
        {

        }

        public void Show()
        {
            Draw = true;
            OnShow();
        }

        protected virtual void OnShow()
        {

        }
    }
}