using System;
using System.Collections.Generic;
using System.Threading;
using Static_Interface.API.UnityExtensions;

namespace Static_Interface.API.Utils
{
    public class ThreadPool : PersistentScript<ThreadPool>
    {
        private static readonly List<Action> QueuedMainActions = new List<Action>();
        private static readonly List<Action> QueuedMainFixedActions = new List<Action>();
        private static readonly List<Action> QueuedAsyncActions = new List<Action>();
        private static int _mainThreadId;
        private Thread _thread;

        protected override void Start()
        {
            base.Start();
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
            _thread = new Thread(AsyncUpdate);
            _thread.Start();
        }

        public bool IsMainThread => Thread.CurrentThread.ManagedThreadId == _mainThreadId;

        /// <summary>
        /// Calls the action on the main thread on the next Update()
        /// </summary>
        /// <param name="action">The action to queue for the next Update() call</param>
        public void QueueMain(Action action)
        {
            lock (QueuedMainActions)
            {
                QueuedMainActions.Add(action);
            }
        }
        /// <summary>
        /// Runs the given action on the main thread
        /// </summary>
        /// <param name="action">The action to run on the main thread</param>
        public void RunOnMainThread(Action action)
        {
            if(IsMainThread) action.Invoke();
            else QueueMain(action);
        }

        /// <summary>
        /// Calls the action on the main thread on the next FixedUpdate()
        /// </summary>
        /// <param name="action">The action to queue for thenext FixedUpdate() call</param>
        public void QueueMainFixed(Action action)
        {
            lock (QueuedMainFixedActions)
            {
                QueuedMainFixedActions.Add(action);
            }
        }

        /// <summary>
        /// Calls the action on the next async thread Update() call
        /// </summary>
        /// <param name="action">The action to call async</param>
        public void QueueAsync(Action action)
        {
            lock (QueuedAsyncActions)
            {
                QueuedAsyncActions.Add(action);
            }
        }

        protected override void Update()
        {
            base.Update();
            lock (QueuedMainActions)
            {
                if (QueuedMainActions.Count == 0)
                {
                    return;
                }

                foreach (Action action in QueuedMainActions)
                {
                    action.Invoke();
                }
                QueuedMainActions.Clear();
            }
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            lock (QueuedMainFixedActions)
            {
                if (QueuedMainFixedActions.Count == 0)
                {
                    return;
                }
                foreach (Action action in QueuedMainFixedActions)
                {
                    action.Invoke();
                }
                QueuedMainFixedActions.Clear();
            }
        }

        private void AsyncUpdate()
        {
            lock (QueuedAsyncActions)
            {
                if (QueuedAsyncActions.Count == 0)
                {
                    return;
                }

                foreach (Action action in QueuedAsyncActions)
                {
                    action.Invoke();
                }
                QueuedAsyncActions.Clear();
            }
            Thread.Sleep(10);
        }

        public void Shutdown()
        {
            lock (QueuedAsyncActions)
            {
                QueuedAsyncActions?.Clear();
            }

            lock (QueuedMainActions)
            {
                QueuedAsyncActions?.Clear();
            }

            lock (QueuedAsyncActions)
            {
                QueuedAsyncActions?.Clear();
            }

            _thread?.Interrupt();
            _thread = new Thread(AsyncUpdate);
            _thread.Start();
        }
    }
}