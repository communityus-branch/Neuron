using System.Reflection;
using UnityEngine;

namespace Static_Interface.API.Network
{
    public class ChannelMethod
    {
        public ChannelMethod(Component newComponent, MethodInfo newMethod, System.Type[] newTypes)
        {
            Component = newComponent;
            Method = newMethod;
            Types = newTypes;
        }

        public Component Component { get; }

        public MethodInfo Method { get; }

        public System.Type[] Types { get; }
    }
}
