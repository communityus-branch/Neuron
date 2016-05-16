using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Static_Interface.API.GUIFramework
{
    public abstract class PrefabViewParent : ViewParent
    {
        protected GameObject Prefab { get; set; }
        protected abstract string PrefabLocation { get; }
        public override Canvas Canvas { get; }

        protected PrefabViewParent(string viewName, ViewParent parent) : base(viewName, parent)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            Canvas = parent.Canvas;
        }

        protected PrefabViewParent(string viewName, ViewParent parent, int x, int y) : base(viewName, parent, x, y)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            Canvas = parent.Canvas;
        }

        protected PrefabViewParent(string viewName, Canvas canvas) : base(viewName, null)
        {
            Canvas = canvas;
            Parent = canvas.transform;
        }

        public override GameObject GetViewObject()
        {
            return Prefab;
        }

        protected override void InitGameObject()
        {
            base.InitGameObject();
            Prefab = (GameObject)Resources.Load(PrefabLocation);
            Prefab = Object.Instantiate(Prefab);
        }
    }
}