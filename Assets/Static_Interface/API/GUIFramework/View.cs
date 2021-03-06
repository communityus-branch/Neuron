﻿
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Static_Interface.API.GUIFramework
{
    public abstract class View
    {
        public abstract GameObject GetViewObject();

        public readonly string ViewName;
        public ViewParent ViewParent { get; internal set; }

        public virtual void SetParent(ViewParent parent)
        {
            ViewParent = parent;
            parent.AddView(this);
        }

        public virtual void OnDraw()
        {

        }

        protected View(string viewName, ViewParent parent) : this(viewName, parent, 0, 0)
        {

        }

        protected View(string viewName, ViewParent parent, int x, int y)
        {
            ViewName = viewName;
            ViewParent = parent;
            InitGameObject();
            parent?.AddView(this);
            if (viewName != null) GetViewObject().name = viewName;
            else ViewName = GetViewObject().name;
            Position = new Vector3(x, y);
            Draw = true;
            Scale = Vector3.one;
            OnPostInit();
        }

        protected virtual void OnPostInit()
        {

        }

        protected virtual void InitGameObject()
        {

        }

        public void FillParent()
        {
            if(!(Parent?.transform is RectTransform)) throw new Exception("View has no parent or parent is not RectTransform");
            var rect = ((RectTransform) Parent.transform).rect;
            Width = rect.width;
            Height = rect.height;
            LocalPosition = Vector3.zero;
        }

        public void Destroy()
        {
            ViewParent?.NotifyDestroyed(this);
            Object.Destroy(GetViewObject());
            OnDestroy();
        }

        public virtual Vector2 SizeDelta
        {
            get { return Transform.sizeDelta; }
            set { Transform.sizeDelta = value; }
        }

        // Read-only
        public virtual Rect Rect => Transform.rect;

        public virtual Transform Parent
        {
            get { return GetViewObject().transform.parent; }
            set
            {
                Transform.SetParent(value);
                Transform.localRotation = Quaternion.Euler(Vector3.zero);
                Transform.localScale = new Vector3(1, 1, 1);
            }
        }

        public virtual Vector2 AnchoredPosition
        {
            get { return new Vector2(Transform.anchoredPosition.x, Transform.anchoredPosition.y); }
            set { Transform.anchoredPosition = value; }
        }

        public virtual Vector3 Position
        {
            get { return new Vector3(Transform.position.x, Transform.position.y, Transform.position.z); }
            set { Transform.position = value; }
        }

        public virtual Vector3 LocalPosition
        {
            get { return new Vector3(Transform.localPosition.x, Transform.localPosition.y, Transform.localPosition.z); }
            set { Transform.localPosition = value; }
        }

        public virtual Vector3 Scale
        {
            get { return Transform.localScale; }
            set { Transform.localScale = value; }
        }


        public virtual bool Draw
        {
            get { return GetViewObject().activeInHierarchy; }
            set { GetViewObject().SetActive(value); }
        }

        public virtual void OnDestroy()
        {

        }

        public virtual void OnResolutionChanged(Vector2 newRes)
        {

        }

        public virtual float Width
        {
            get { return SizeDelta.x; }

            set
            {
                SizeDelta = new Vector2(value, SizeDelta.y);
            }
        }

        public virtual float Height
        {
            get { return SizeDelta.y; }

            set
            {
                SizeDelta = new Vector2(SizeDelta.x, value);
            }
        }

        public RectTransform Transform => (RectTransform)GetViewObject().transform;
    }

}