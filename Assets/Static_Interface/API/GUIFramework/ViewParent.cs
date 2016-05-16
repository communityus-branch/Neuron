using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Static_Interface.API.GUIFramework
{
    public abstract class ViewParent : View
    {
        private readonly List<View> _childs = new List<View>();
        public abstract Canvas Canvas { get; }

        protected ViewParent(string viewName, ViewParent parent) : base(viewName, parent)
        {
        }

        protected ViewParent(string viewName, ViewParent parent, int x, int y) : base(viewName, parent, x, y)
        {
        }

        public virtual void AddView(View view)
        {
            view.ViewParent = this;
            view.Parent = Transform;
            _childs.Add(view);
        }

        public void RemoveView(View view)
        {
            _childs.Remove(view);
            view.Destroy();
        }

        public ReadOnlyCollection<View> GetChilds()
        {
            return _childs.AsReadOnly();
        }

        public override void OnDestroy()
        {
            ClearChilds();
        }

        public void ClearChilds()
        {
            var tmp = _childs;
            foreach (View v in tmp)
            {
                RemoveView(v);
            }
        }

        public override void OnDraw()
        {
            base.OnDraw();
            foreach (View v in GetChilds())
                v.OnDraw();
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

        internal void NotifyDestroyed(View view)
        {
            if (_childs.Contains(view))
                _childs.Remove(view);
        }
    }
}