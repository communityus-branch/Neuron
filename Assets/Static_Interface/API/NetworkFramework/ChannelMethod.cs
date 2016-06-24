using System;
using System.Reflection;
using UnityEngine;

namespace Static_Interface.API.NetworkFramework
{
    public class ChannelMethod
    {
        public ChannelMethod(object newComponent, MethodInfo newMethod, System.Type[] newTypes)
        {
            Component = newComponent;
            Method = newMethod;
            Types = newTypes;
        }

        public object Component { get; }

        public MethodInfo Method { get; }

        public System.Type[] Types { get; }
    }
}
