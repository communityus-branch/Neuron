using System;

namespace Static_Interface.Internal
{
    public static class LogUtils
    {
        public static void Log(this Exception exception)
        {
            UnityEngine.Debug.LogException(exception);
        }

        public static void Log(this Exception exception, string msg)
        {
            UnityEngine.Debug.LogError(msg + ": ");
            UnityEngine.Debug.LogException(exception);
        }

        public static void Log(string msg, bool appendInfo = true)
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
        }


        public static void Error(string msg)
        {
#if UNITY_STANDALONE
            if (UnityEngine.Debug.isDebugBuild)
            {
                UnityEngine.Debug.LogError(msg);
                return;
            }

            if (Console.Instance != null)
            {
                Console.Instance.Print("[Error] " + msg);
            }
#else
            Console.WriteLine(msg);
#endif
        }

        public static void Debug(string msg)
        {
            if (!UnityEngine.Debug.isDebugBuild) return;
            UnityEngine.Debug.Log(msg);
        }
    }
}