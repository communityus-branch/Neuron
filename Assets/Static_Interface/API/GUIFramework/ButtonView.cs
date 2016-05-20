using UnityEngine.Events;
using UnityEngine.UI;

namespace Static_Interface.API.GUIFramework
{
    public class ButtonView : PrefabView
    {
        public Text ButtonText => Transform.FindChild("Text").GetComponent<Text>();
        public Button Button => Transform.GetComponent<Button>();
        public UnityEvent OnClick => Button.onClick;

        public ButtonView(string viewName, ViewParent parent) : base(viewName, parent)
        {
        }

        public ButtonView(string viewName, ViewParent parent, int x, int y) : base(viewName, parent, x, y)
        {
        }

        protected override string PrefabLocation => "UI/Button";
    }
}