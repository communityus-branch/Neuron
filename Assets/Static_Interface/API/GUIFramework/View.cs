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

            if (parent != null)
                Parent = parent.Canvas.transform;
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

        public virtual Vector2 Size
        {
            get { return ((RectTransform)GetViewObject().transform).sizeDelta; }
            set { ((RectTransform)GetViewObject().transform).sizeDelta = value; }
        }

        public virtual Transform Parent
        {
            get { return GetViewObject().transform.parent; }
            set
            {
                GetViewObject().transform.SetParent(value);
                GetViewObject().transform.localRotation = Quaternion.Euler(Vector3.zero);
                GetViewObject().transform.localScale = new Vector3(1, 1, 1);
            }
        }

        public virtual Vector2 Position
        {
            get
            {
                RectTransform transform = (RectTransform) GetViewObject().transform;
                return new Vector2(transform.anchoredPosition.x, transform.anchoredPosition.y);
            }
            set
            {
                RectTransform transform = (RectTransform)GetViewObject().transform;
                transform.anchoredPosition = value;
            }
        }

        public virtual bool Draw
        {
            get { return GetViewObject().activeInHierarchy; }
            set { GetViewObject().SetActive(value); }
        }

        public virtual void OnDestroy()
        {
            
        }

        public virtual void OnResolutionChanged(Vector2 oldRes, Vector2 newRes)
        {

        }
    }

}