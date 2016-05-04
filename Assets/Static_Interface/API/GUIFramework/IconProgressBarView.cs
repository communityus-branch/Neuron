using UnityEngine;
using UnityEngine.UI;

namespace Static_Interface.API.GUIFramework
{
    public class IconProgressBarView : ProgressBarView
    {
        protected override string PrefabLocation => "UI/IconProgressBarView";
        private readonly GameObject _iconGameObject;
        public Sprite Icon
        {
            get { return _iconGameObject.GetComponent<Image>().sprite; }
            set { _iconGameObject.GetComponent<Image>().sprite = value; }
        }

        public IconProgressBarView(string viewName, ViewParent parent) : this(viewName, parent, 0, 0)
        {
        }

        public IconProgressBarView(string viewName, ViewParent parent, int x, int y) : base(viewName, parent, x, y)
        {
            _iconGameObject = Prefab.transform.FindChild("Icon").gameObject;
        }
    }
}