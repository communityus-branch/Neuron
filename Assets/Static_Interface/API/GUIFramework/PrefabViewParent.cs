﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Static_Interface.API.GUIFramework
{
    public abstract class PrefabViewParent : ViewParent
    {
        protected GameObject Prefab { get; set; }
        protected abstract string PrefabLocation { get; }
        private readonly List<View> _childs = new List<View>();

        protected PrefabViewParent(string viewName, ViewParent parent) : base(viewName, parent)
        {
        }

        protected PrefabViewParent(string viewName, ViewParent parent, int x, int y) : base(viewName, parent, x, y)
        {
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

        public override void AddView(View view)
        {
            view.ViewParent = this;
            view.Parent = Transform;
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
    }
}