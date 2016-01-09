using System.Reflection;
using UnityEngine;

namespace Static_Interface.Objects
{
    public class ChannelMethod
    {
        private Component _component;
        private MethodInfo _method;
        private System.Type[] _types;

        public ChannelMethod(Component newComponent, MethodInfo newMethod, System.Type[] newTypes)
        {
            _component = newComponent;
            _method = newMethod;
            _types = newTypes;
        }

        public Component Component
        {
            get
            {
                return _component;
            }
        }

        public MethodInfo Method
        {
            get
            {
                return _method;
            }
        }

        public System.Type[] Types
        {
            get
            {
                return _types;
            }
        }
    }
}
