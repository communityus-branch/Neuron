using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;

namespace Static_Interface.API.GUIFramework
{
    public class ChatScrollView : ScrollView
    {
        private readonly List<TextView> _lines  = new List<TextView>();
        public ReadOnlyCollection<TextView> Lines => _lines.AsReadOnly();

        protected override void InitGameObject()
        {
            base.InitGameObject();
            ScrollHorizontal = false;
            Transform.localPosition = Vector2.zero;
            UpdatePos(new Vector2(Screen.width, Screen.height));
            ScrollToTop();
        }

        public override void OnResolutionChanged(Vector2 newRes)
        {
            base.OnResolutionChanged(newRes);
            UpdatePos(newRes);
        }

        private void UpdatePos(Vector2 newRes, Vector2? sizeDelta = null)
        {
            if (sizeDelta == null) sizeDelta = SizeDelta;
            var x = -Screen.width / 2 + sizeDelta.Value.x / 2;
            var y = Screen.height / 2 - sizeDelta.Value.y / 2;
            LocalPosition = new Vector2(x, y);
            SizeDelta = new Vector2(newRes.x / 3, newRes.y / 2);
        }

        public ChatScrollView(string viewName, ViewParent parent) : base(viewName, parent)
        {
        }

        public ChatScrollView(string viewName, ViewParent parent, int x, int y) : base(viewName, parent, x, y)
        {
        }

        public override Vector2 SizeDelta
        {
            get
            {
                UpdatePos(new Vector2(Screen.width, Screen.height), base.SizeDelta);
                return base.SizeDelta;
            }
            set
            {
                base.SizeDelta = value;
            }
        }

        public void AddLine(string s)
        {
            TextView tv = new TextView(null, this);
            tv.SetText(s);

            _lines.Add(tv);

            tv.SizeDelta = new Vector2(SizeDelta.x, 16f);
            //var basePos = new Vector2(4, 0);
            //basePos.x += tv.SizeDelta.x/2;
            //basePos.y -= Lines.Count*tv.FontSize*2;
            //tv.LocalPosition = basePos;
            var content = (RectTransform) Transform.FindChild("Viewport").FindChild("Content").transform;
            float height = 0;
            
            List<TextView> toRemove = new List<TextView>();
            foreach (TextView t in Lines)
            {
                try
                {
                    if (t == null)
                    {
                        toRemove.Add(t);
                        continue;
                    }
                    height += t.Rect.height;
                }
                catch (Exception e)
                {
                    toRemove.Add(t);
                }

            }
            foreach (TextView t in toRemove)
            {
                _lines.Remove(t);
            }
            content.sizeDelta = new Vector2(content.sizeDelta.x, height);
            ScrollToBottom();
        }


        private void ScrollToTop()
        {
            Transform.GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;
        }

        public void ScrollToBottom()
        {
            Transform.GetComponent<ScrollRect>().verticalNormalizedPosition = 0f;
        }

        public void Clear()
        {
            foreach (TextView tv in _lines)
            {
                ViewParent.RemoveView(tv);
                tv.Destroy();
            }
        }
    }
}