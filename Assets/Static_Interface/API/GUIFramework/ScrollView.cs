using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Static_Interface.API.GUIFramework
{
    public class ScrollView : PrefabViewParent
    {
        public ScrollView(string viewName, ViewParent parent) : base(viewName, parent)
        {
        }

        public ScrollView(string viewName, ViewParent parent, int x, int y) : base(viewName, parent, x, y)
        {
        }

        public override void AddView(View view)
        {
            base.AddView(view);
            view.Parent = Content;

            float height = 0;

            List<View> toRemove = new List<View>();
            foreach (View v in GetChilds())
            {
                try
                {
                    if (v == null)
                    {
                        toRemove.Add(v);
                        continue;
                    }
                    height += v.Rect.height;
                }
                catch (Exception e)
                {
                    toRemove.Add(v);
                }

            }
            foreach (View v in toRemove)
            {
                v.Destroy();
            }
            Content.sizeDelta = new Vector2(Content.sizeDelta.x, height);
        }

        protected override string PrefabLocation => "UI/ScrollView";

        public RectTransform Content
        {
            get { return GetViewObject().GetComponent<ScrollRect>().content; }
            set { GetViewObject().GetComponent<ScrollRect>().content = value; }
        }

        public bool ScrollVertical
        {
            get { return GetViewObject().GetComponent<ScrollRect>().vertical; }
            set { GetViewObject().GetComponent<ScrollRect>().vertical = value; }
        }

        public bool ScrollHorizontal
        {
            get { return GetViewObject().GetComponent<ScrollRect>().horizontal; }
            set { GetViewObject().GetComponent<ScrollRect>().horizontal = value; }
        }

        public float ScrollSensitivty
        {
            get { return GetViewObject().GetComponent<ScrollRect>().scrollSensitivity; }
            set { GetViewObject().GetComponent<ScrollRect>().scrollSensitivity= value; }
        }

        public ScrollRect.MovementType MovementType
        {
            get { return GetViewObject().GetComponent<ScrollRect>().movementType; }
            set { GetViewObject().GetComponent<ScrollRect>().movementType = value; }
        }

        public bool Inertia
        {
            get { return GetViewObject().GetComponent<ScrollRect>().inertia; }
            set { GetViewObject().GetComponent<ScrollRect>().inertia = value; }
        }

        public float InertiaDecelerationRate
        {
            get { return GetViewObject().GetComponent<ScrollRect>().decelerationRate; }
            set { GetViewObject().GetComponent<ScrollRect>().decelerationRate = value; }
        }

        public void ScrollToTop()
        {
            Transform.GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;
        }

        public void ScrollToBottom()
        {
            Transform.GetComponent<ScrollRect>().verticalNormalizedPosition = 0f;
        }
    }
}