using UnityEngine;
using UnityEngine.UI;

namespace Static_Interface.API.GUIFramework
{
    public class TextView : PrefabView
    {
        public TextView(string viewName, ViewParent parent) : base(viewName, parent)
        {
        }

        public TextView(string viewName, ViewParent parent, int x, int y) : base(viewName, parent, x, y)
        {
        }

        protected override string PrefabLocation => "TextView";

        public string Text
        {
            get { return GetViewObject().GetComponent<Text>().text; }
            set { GetViewObject().GetComponent<Text>().text = value; }
        }
    }
}