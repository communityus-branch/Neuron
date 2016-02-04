using System;

namespace Static_Interface.API.Utils
{
    /// <summary>
    /// Provides thread-safe logging methods
    /// </summary>
    public static class LogUtils
    {
        public static void Log(this Exception exception)
        {
            Action action = delegate
            {
                UnityEngine.Debug.LogException(exception);
            };
            if (ThreadPool.IsMainThread)
            {
                action.Invoke();
                return;
            }
            ThreadPool.QueueMainFixed(action);
        }

        public static void Log(this Exception exception, string msg)
        {
            Action action = delegate
            {
                UnityEngine.Debug.LogError(msg + ": ");
                UnityEngine.Debug.LogException(exception);
            };
            if (ThreadPool.IsMainThread)
            {
                action.Invoke();
                return;
            }
            ThreadPool.QueueMainFixed(action);
        }

        public static void Log(string msg, bool appendInfo = true)
        {
            Action action = delegate
            {
                if (UnityEngine.Debug.isDebugBuild)
                {
                    UnityEngine.Debug.Log(msg);
                    return;
                }

                if (Console.Instance != null)
                {
                    Console.Instance.Print(appendInfo ? "[Info] " : "" + msg);
                }
            };
            if (ThreadPool.IsMainThread)
            {
                action.Invoke();
                return;
            }
            ThreadPool.QueueMainFixed(action);
        }


        public static void LogError(string msg)
        {
            Action action = delegate
            {
                if (UnityEngine.Debug.isDebugBuild)
                {
                    UnityEngine.Debug.LogError(msg);
                    return;
                }

                if (Console.Instance != null)
                {
                    Console.Instance.Print("[Error] " + msg);
                }
            };
            if (ThreadPool.IsMainThread)
            {
                action.Invoke();
                return;
            }
            ThreadPool.QueueMainFixed(action);
        }

        public static void Debug(object obj)
        {
            Action action = delegate
            {
                if (!UnityEngine.Debug.isDebugBuild) return;
                UnityEngine.Debug.Log(obj);
            };
            if (ThreadPool.IsMainThread)
            {
                action.Invoke();
                return;
            }
            ThreadPool.QueueMainFixed(action);
        }
    }
}