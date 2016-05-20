using UnityEngine;

namespace Static_Interface.API.GUIFramework
{
    public class PanelView : PrefabViewParent
    {
        public PanelView(string viewName, ViewParent parent) : base(viewName, parent)
        {
        }

        public PanelView(string viewName, ViewParent parent, int x, int y) : base(viewName, parent, x, y)
        {
        }

        public PanelView(string viewName, Canvas canvas) : base(viewName, canvas)
        {
        }

        protected override string PrefabLocation => "UI/Panel";
    }
}