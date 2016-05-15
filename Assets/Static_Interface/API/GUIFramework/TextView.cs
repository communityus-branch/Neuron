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

        public Text Text => GetViewObject().GetComponent<Text>();

        public void SetText(string s)
        {
            Text.text = s;
        }
    }
}