using System;
using UnityEngine;
using UnityEngine.UI;

namespace Static_Interface.API.GUIFramework
{
    public class ProgressBar : PrefabView
    {
        protected readonly GameObject ProgressSlider;
        protected override string PrefabLocation => "UI/ProgressBar";

        public ProgressBar(string name) : this(name, null, 0,0)
        { }

        public ProgressBar(string name, Canvas parent, int x, int y) : base(name, parent, x, y) 
        {
            ProgressSlider = Prefab.transform.FindChild("ProgressSlider").gameObject;
        }

        public string Label { get; set; }

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