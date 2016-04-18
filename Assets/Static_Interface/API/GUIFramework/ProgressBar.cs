using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Static_Interface.API.GUIFramework
{
    public class ProgressBar
    {
        protected readonly GameObject ProgressSlider;
        protected readonly GameObject Prefab;
        public readonly string Name;
        protected virtual string PrefabLocation => "UI/ProgressBar";

        public ProgressBar(string name) : this(name, null, 0,0)
        { }

        public ProgressBar(string name, Canvas parent, int x, int y)
        {
            Name = name;
            Prefab= (GameObject)Resources.Load(PrefabLocation);
            Prefab = Object.Instantiate(Prefab);
            ProgressSlider = Prefab.transform.FindChild("ProgressSlider").gameObject;

            if(parent != null)
                Parent = parent.transform;

            Position = new Vector2(x, y);
            Draw = false;
        }

        public void Destroy()
        {
            Object.Destroy(Prefab);
        }

        public Vector2 Size
        {
            get { return ((RectTransform) Prefab.transform).sizeDelta; }
            set { ((RectTransform) Prefab.transform).sizeDelta = value; }
        }

        public Transform Parent
        {
            get { return Prefab.transform.parent; }
            set
            {
                Prefab.transform.SetParent(value);
                Prefab.transform.localRotation = Quaternion.Euler(Vector3.zero);
                Prefab.transform.localScale = new Vector3(1,1,1);
            }
        }

        public Vector2 Position
        {
            get
            {
                RectTransform transform = (RectTransform) Prefab.transform;
                return new Vector2(transform.anchoredPosition.x, transform.anchoredPosition.y);
            }
            set
            {
                RectTransform transform = (RectTransform)Prefab.transform;
                transform.anchoredPosition = value;
            }
        }

        public bool Draw
        {
            get { return ProgressSlider.GetComponent<Slider>().enabled; }
            set { ProgressSlider.GetComponent<Slider>().enabled = value; }
        }

        public string Label; //Todo

        public float Value
        {
            get { return ProgressSlider.GetComponent<Slider>().value; }
            set { ProgressSlider.GetComponent<Slider>().value = value; }
        }

        public float MaxValue
        {
            get { return ProgressSlider.GetComponent<Slider>().maxValue; }
            set { ProgressSlider.GetComponent<Slider>().maxValue = value; }
        }

        public float MinValue
        {
            get { return ProgressSlider.GetComponent<Slider>().minValue; }
            set { ProgressSlider.GetComponent<Slider>().minValue = value; }
        }
    }
}