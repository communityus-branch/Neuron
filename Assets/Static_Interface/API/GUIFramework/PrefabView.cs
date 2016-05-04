using UnityEngine;

namespace Static_Interface.API.GUIFramework
{
    public abstract class PrefabView : View
    {
        protected GameObject Prefab { get; set; }
        public override GameObject GetViewObject()
        {
            return Prefab;
        }

        protected abstract string PrefabLocation { get; }
        protected PrefabView(string name) : base(name)
        {
        }

        protected PrefabView(string name, Canvas parent, int x, int y) : base(name, parent, x, y)
        {

        }

        protected override void InitGameObject()
        {
            Prefab = (GameObject)Resources.Load(PrefabLocation);
            Prefab = Object.Instantiate(Prefab);
        }
    }
}