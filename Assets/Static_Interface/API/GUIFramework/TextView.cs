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

        protected override string PrefabLocation => "UI/TextView";

        public string Text
        {
            get { return GetViewObject().GetComponent<Text>().text; }
            set { GetViewObject().GetComponent<Text>().text = value; }
        }

        public int FontSize
        {
            get { return GetViewObject().GetComponent<Text>().fontSize; }
            set { GetViewObject().GetComponent<Text>().fontSize= value; }
        }
    }
}