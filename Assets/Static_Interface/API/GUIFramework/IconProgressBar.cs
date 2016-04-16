using UnityEngine;
using UnityEngine.UI;

namespace Static_Interface.API.GUIFramework
{
    public class IconProgressBar : ProgressBar
    {
        protected override string PrefabLocation => "UI/IconProgressBar";
        private readonly GameObject _iconGameObject;
        public Sprite Icon
        {
            get { return _iconGameObject.GetComponent<Image>().sprite; }
            set { _iconGameObject.GetComponent<Image>().sprite = value; }
        }

        public IconProgressBar(string name) : this(name, null, 0, 0)
        {
        }

        public IconProgressBar(string name, Canvas parent, int x, int y) : base(name, parent, x, y)
        {
            _iconGameObject = Prefab.transform.FindChild("Icon").gameObject;
        }
    }
}