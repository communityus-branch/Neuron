
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
            GetViewObject().name = viewName;
            Position = new Vector2(x, y);
            Draw = true;
        }

        protected virtual void InitGameObject()
        {

        }

        public void Destroy()
        {
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

        public virtual Vector2 Position
        {
            get { return new Vector2(Transform.position.x, Transform.position.y); }
            set { Transform.position = value; }
        }

        public virtual Vector2 LocalPosition
        {
            get { return new Vector2(Transform.localPosition.x, Transform.localPosition.y); }
            set { Transform.localPosition = value; }
        }

        public virtual Vector2 Scale
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

        public float Width
        {
            get { return SizeDelta.x; }

            set
            {
                SizeDelta = new Vector2(value, SizeDelta.y);
            }
        }

        public float Height
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