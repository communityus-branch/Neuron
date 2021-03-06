﻿//#define LOG_NETWORK // enable network debug output
using System;
using Console = Static_Interface.API.ConsoleFramework.Console;
namespace Static_Interface.API.Utils
{
    /// <summary>
    /// Provides thread-safe logging methods
    /// </summary>
    public static class LogUtils
    {
        public static void Log(this Exception exception)
        {
            ThreadPool.Instance.RunOnMainThread(delegate
            {
                UnityEngine.Debug.LogException(exception);
            });
        }

        public static void Log(this Exception exception, string msg)
        {
            ThreadPool.Instance.RunOnMainThread(delegate
            {
                UnityEngine.Debug.LogError(msg + ": ");
                UnityEngine.Debug.LogException(exception);
            });
        }

        public static void Log(string msg, bool appendInfo = true)
        {
            ThreadPool.Instance.RunOnMainThread(delegate
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
            });
        }

        internal static void LogNetwork(string msg)
        {
#if LOG_NETWORK
            Log(msg);
#endif
        }


        public static void LogError(string msg)
        {
            ThreadPool.Instance.RunOnMainThread(delegate
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
            });
        }

        public static void LogWarning(string msg)
        {
            ThreadPool.Instance.RunOnMainThread(delegate
            {
                if (UnityEngine.Debug.isDebugBuild)
                {
                    UnityEngine.Debug.LogWarning(msg);
                    return;
                }

                if (Console.Instance != null)
                {
                    Console.Instance.Print("[Warning] " + msg);
                }
            });
        }

        public static void Debug(object obj)
        {
            ThreadPool.Instance.RunOnMainThread(delegate
            {
                if (!UnityEngine.Debug.isDebugBuild) return;
                UnityEngine.Debug.Log(obj);
            });
        }
    }
}