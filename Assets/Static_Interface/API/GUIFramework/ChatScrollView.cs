using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

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
        }

        public ChatScrollView(string viewName, ViewParent parent) : base(viewName, parent)
        {
        }

        public ChatScrollView(string viewName, ViewParent parent, int x, int y) : base(viewName, parent, x, y)
        {
        }

        public void AddLine(string s)
        {
            Vector2 basePos = new Vector2(0, 0);
            basePos.y += 10*_lines.Count;

            TextView tv = new TextView(null, ViewParent)
            {
                Parent = Content,
                Position = basePos
            };

            _lines.Add(tv);
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