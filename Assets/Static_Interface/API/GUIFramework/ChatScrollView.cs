using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;

namespace Static_Interface.API.GUIFramework
{
    public class ChatScrollView : ScrollView
    {
        protected override string PrefabLocation => "UI/ScrollView";

        private readonly List<TextView> _lines = new List<TextView>();
        public ReadOnlyCollection<TextView> Lines => _lines.AsReadOnly();

        protected override void InitGameObject()
        {
            base.InitGameObject();
            ScrollHorizontal = false;
            Transform.localPosition = Vector2.zero;
            UpdatePos(new Vector2(Screen.width, Screen.height));
            ScrollToTop();
            OnClearChilds.AddListener(delegate
            {
                _lines.Clear();
                _yPosition = 0;
            });
        }

        protected override void OnPostInit()
        {
            base.OnPostInit();
            GetViewObject().GetComponent<ScrollRect>().verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
        }

        public override void OnResolutionChanged(Vector2 newRes)
        {
            base.OnResolutionChanged(newRes);
            UpdatePos(newRes);
        }

        private void UpdatePos(Vector2 newRes, Vector2? sizeDelta = null)
        {
            if (sizeDelta == null) sizeDelta = SizeDelta;
            var x = -Screen.width/2 + sizeDelta.Value.x/2;
            var y = Screen.height/2 - sizeDelta.Value.y/2;
            LocalPosition = new Vector2(x, y);
            SizeDelta = new Vector2(newRes.x/3, newRes.y/2);
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
            set { base.SizeDelta = value; }
        }

        private float _yPosition;
        public void AddLine(string s)
        {
            TextView tv = new TextView(null, this);
            tv.SetText(s);      
            _lines.Add(tv);

            int marginLeft = 2;
            int marginTop = 2;

            tv.SizeDelta = new Vector2(SizeDelta.x, tv.SizeDelta.y);
            if (_yPosition == 0) _yPosition = -tv.Height / 2 + -marginTop;
            tv.LocalPosition = new Vector3(tv.Width/2 + marginLeft, _yPosition);
            _yPosition -= tv.Height - marginTop;
            ScrollToBottom();
        }

        public void Clear()
        {
            ClearChilds();
        }
    }
}