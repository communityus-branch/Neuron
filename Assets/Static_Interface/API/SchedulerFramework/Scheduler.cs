using System;
using System.Collections.Generic;
using System.Linq;
using Static_Interface.API.ExtensionFramework;
using Static_Interface.API.Utils;
using MonoBehaviour = Static_Interface.API.UnityExtensions.MonoBehaviour;

namespace Static_Interface.API.SchedulerFramework
{
    class Scheduler : MonoBehaviour
    {
        private readonly Dictionary<int, Task> _tasks = new Dictionary<int, Task>();
        private int _lastId;
        protected override void Start()
        {
            base.Start();  
            Instance = this;
        }

        protected override void OnDestroySafe()
        {
            base.OnDestroySafe();
            Instance = null;
        }

        public static Scheduler Instance { get; private set; }

        public Task RunTask(Extension ext, Action action, uint delay = 1)
        {
            if (delay == 0) delay = 1;
            return GetTask(ext, action, false, delay);
        }

        public Task RunAsyncTask(Extension ext, Action action, uint delay = 1)
        {
            if (delay == 0) delay = 1;
            return GetTask(ext, action, true, delay);
        }

        private Task GetTask(Extension ext, Action action, bool isAsync, uint delay = 0, uint period = 0)
        {
            Task task = new Task
            {
                Action = action,
                Id = _lastId,
                Extension = ext,
                IsAsync = isAsync,
                Delay = delay,
                Period = period,
                ScheduledTime = TimeUtil.GetCurrentTime()
            };

            _lastId++;
            _tasks.Add(task.Id, task);
            return task;
        }

        public Task GetTask(int id)
        {
            if (!_tasks.ContainsKey(id))
            {
                throw new Exception("Task not found");
            }
            return _tasks[id];
        }

        public Task RunTaskTimer(Extension ext, Action action, uint delay, uint period = 1)
        {
            return GetTask(ext, action, false, delay, period);
        }

        public Task RunAsyncTaskTimer(Extension ext, Action action, uint delay, uint period = 1)
        {
            return GetTask(ext, action, true, delay, period);
        }

        protected override void Update()
        {
            base.Update();
            List<int> toRemove = new List<int>();
            foreach (var t in _tasks.Values)
            {
                if (t.IsCancelled)
                {
                    toRemove.Add(t.Id);
                    continue;
                }

                if (t.Pause)
                {
                    continue;
                }

                bool queued = false;

                if (t.Delay > 0 && t.LastRunTime == 0 && TimeUtil.GetCurrentTime() - t.ScheduledTime >= t.Delay)
                {
                    QueueTask(t);
                    queued = true;
                }

                if (t.Period == 0)
                {
                    if(!queued) QueueTask(t);
                    t.Remove();
                    continue;
                }


                if (!queued && t.LastRunTime > 0 && TimeUtil.GetCurrentTime() - t.LastRunTime >= t.Period)
                {
                    QueueTask(t);
                }
            }

            foreach (int i in toRemove)
            {
                _tasks.Remove(i);
            }
        }

        private void QueueTask(Task t)
        {
            t.LastRunTime = TimeUtil.GetCurrentTime();
            if (t.IsAsync)
            {
                ThreadPool.Instance.QueueAsync(t.Action);
            }
            else
            {
                ThreadPool.Instance.QueueMain(t.Action);
            }
        }

        public void RemoveAllTasks(Extension ext)
        {
            foreach (Task t in _tasks.Values.Where(t => t.Extension == ext))
            {
                t.Remove();
            }
        }

        public void Shutdown()
        {
            Instance = null;
            Destroy(this);
        }
    }
}
