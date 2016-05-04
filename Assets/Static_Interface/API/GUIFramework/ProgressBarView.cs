using System;
using UnityEngine;
using UnityEngine.UI;

namespace Static_Interface.API.GUIFramework
{
    public class ProgressBarView : PrefabView
    {
        protected readonly GameObject ProgressSlider;
        protected override string PrefabLocation => "UI/ProgressBarView";

        public ProgressBarView(string viewName, ViewParent parent) : this(viewName, parent, 0,0)
        { }

        public ProgressBarView(string viewName, ViewParent parent, int x, int y) : base(viewName, parent, x, y) 
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