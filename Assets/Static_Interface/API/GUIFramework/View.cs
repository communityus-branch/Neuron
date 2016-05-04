using UnityEngine;
using Object = UnityEngine.Object;

namespace Static_Interface.API.GUIFramework
{
    public abstract class View
    {
        public abstract GameObject GetViewObject();

        public readonly string Name;

        public virtual void OnDraw()
        {
            
        }

        protected View(string name) : this(name, null, 0, 0)
        {
        }
   
        protected View(string name, Canvas parent, int x, int y)
        {
            Name = name;

            if (parent != null)
                Parent = parent.transform;
            InitGameObject();

            Position = new Vector2(x, y);
            Draw = true;
        }

        protected abstract void InitGameObject();

        public void Destroy()
        {
            Object.Destroy(GetViewObject());
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