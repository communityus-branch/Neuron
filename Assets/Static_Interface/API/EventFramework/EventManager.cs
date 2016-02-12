using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Static_Interface.API.ExtensionFramework;
using Static_Interface.API.Utils;

namespace Static_Interface.API.EventFramework
{
    public class EventManager
    {
        private static EventManager _instance;

        private readonly List<object> _listeners = new List<object>();
        private readonly Dictionary<Type, List<MethodInfo>> _eventListeners = new Dictionary<Type, List<MethodInfo>>();
        private readonly Dictionary<MethodInfo, IListener> _listenerInstances = new Dictionary<MethodInfo, IListener>();

        public static EventManager Instance => _instance ?? (_instance = new EventManager());

        /// <summary>
        /// Register a listener for events
        /// </summary>
        /// <param name="listener">The listener class which implements the <see cref="EventHandler"/> listener methods</param>
        /// <param name="extension">The extension which wants to register a new listener</param>
        public void RegisterEvents(IListener listener, Extension extension)
        {
            //Todo
            RegisterEvents(listener);
        }


        internal void RegisterEvents(IListener listener)
        {
            if (!_listeners.Contains(listener)) _listeners.Add(listener);

            Type type = listener.GetType();
            foreach (MethodInfo method in type.GetMethods())
            {
                bool isEventMethod = method.GetCustomAttributes(false).OfType<EventHandler>().Any();

                if (!isEventMethod)
                {
                    continue;
                }

                ParameterInfo[] methodArgs = method.GetParameters();
                if (methodArgs.Length != 1)
                {
                    //Listener methods should have only one argument
                    continue;
                }

                Type t = methodArgs[0].ParameterType;
                if (!t.IsSubclassOf(typeof(Event)))
                {
                    //The arg type should be instanceof Event
                    continue;
                }

                List<MethodInfo> methods;
                try
                {
                    methods = _eventListeners[t];
                }
                catch (KeyNotFoundException)
                {

                    methods = new List<MethodInfo>();
                }
                if (!methods.Contains(method)) methods.Add(method);

                if (_eventListeners.ContainsKey(t))
                {
                    _eventListeners[t] = methods;
                }
                else
                {
                    _eventListeners.Add(t, methods);
                }

                if (!_listenerInstances.ContainsKey(method)) _listenerInstances.Add(method, listener);
            }
        }

        /// <summary>
        /// Call an event which will be send to all <see cref="IListener"/>s which are listening for it
        /// </summary>
        /// <param name="evnt">The event to fire</param>
        public void CallEvent(Event evnt)
        {
            LogUtils.Debug("Firing event:" + evnt.Name);
            Type t = evnt.GetType();
            List<MethodInfo> methods;

            try
            {
                methods = _eventListeners[t];
            }
            catch (KeyNotFoundException)
            {
                return;
            }

            methods.Sort(EventComprarer.Compare);

            foreach (MethodInfo info in from info in methods let handler = info.GetCustomAttributes(false).OfType<EventHandler>().FirstOrDefault() where handler != null where !(evnt is ICancellable) || !((ICancellable)evnt).IsCancelled || handler.IgnoreCancelled select info)
            {
                object instance;
                try
                {
                    instance = _listenerInstances[info];
                }
                catch (KeyNotFoundException e)
                {
                    e.Log("This shouldn't happen");
                    return;
                }


                Action action = delegate
                {
                    info.Invoke(instance, BindingFlags.InvokeMethod, null, new object[] {evnt},
                        CultureInfo.CurrentCulture);
                };

                if (evnt.IsAsync)
                {
                    ThreadPool.QueueAsync(action);
                }
                else
                {
                    ThreadPool.QueueMainFixed(action);
                }
            }
        }

        public void ClearListeners(Extension extension)
        {
            throw new NotImplementedException();
        }

        internal void Shutdown()
        {
            _instance= null;
        }
    }

    public class EventComprarer
    {
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public static int Compare(MethodInfo a, MethodInfo b)
        {
            EventPriority priorityA = a.GetCustomAttributes(false).OfType<EventHandler>().FirstOrDefault().Priority;
            EventPriority priorityB = b.GetCustomAttributes(false).OfType<EventHandler>().FirstOrDefault().Priority;

            if (priorityA > priorityB)
            {
                return 1;
            }

            if (priorityB > priorityA)
            {
                return -1;
            }

            return 0;
        }
    }
}