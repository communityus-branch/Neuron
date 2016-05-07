using System.Collections.ObjectModel;
using UnityEngine;

namespace Static_Interface.API.GUIFramework
{
    public abstract class ViewParent : View
    {
        public abstract Canvas Canvas { get; }
        public abstract void AddView(View view);
        public abstract void RemoveView(View view);
        public abstract Collection<View> GetChilds();
        protected ViewParent(string viewName, ViewParent parent) : base(viewName, parent)
        {
        }

        protected ViewParent(string viewName, ViewParent parent, int x, int y) : base(viewName, parent, x, y)
        {
        }

        public override void OnDestroy()
        {
            foreach (View v in GetChilds())
            {
                v.Destroy();
            }
        }

        public override GameObject GetViewObject()
        {
            return Canvas.gameObject;
        }

        public override void OnResolutionChanged(Vector2 newRes)
        {
            base.OnResolutionChanged(newRes);
            foreach (View v in GetChilds())
            {
                v.OnResolutionChanged(newRes);
            }
        }
    }
}