using System.Diagnostics;
using UnityEngine;

namespace JH.Portfolio
{
    public class Debug
    {
        [Conditional("UNITY_EDITOR")]
        public static void Log(object message)
        {
            UnityEngine.Debug.Log(message);
        }
        [Conditional("UNITY_EDITOR")]
        public static void Log(object message, UnityEngine.Object context)
        {
            UnityEngine.Debug.Log(message, context);
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void LogWarning(object message)
        {
            UnityEngine.Debug.LogWarning(message);
        }
        [Conditional("UNITY_EDITOR")]
        public static void LogWarning(object message, UnityEngine.Object context)
        {
            UnityEngine.Debug.LogWarning(message, context);
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void LogError(object message)
        {
            UnityEngine.Debug.LogError(message);
        }
        [Conditional("UNITY_EDITOR")]
        public static void LogError(object message, UnityEngine.Object context)
        {
            UnityEngine.Debug.LogError(message, context);
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void LogException(System.Exception exception)
        {
            UnityEngine.Debug.LogException(exception);
        }
        [Conditional("UNITY_EDITOR")]
        public static void LogException(System.Exception exception, UnityEngine.Object context)
        {
            UnityEngine.Debug.LogException(exception, context);
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void LogAssertion(object message)
        {
            UnityEngine.Debug.LogAssertion(message);
        }
        [Conditional("UNITY_EDITOR")]
        public static void LogAssertion(object message, UnityEngine.Object context)
        {
            UnityEngine.Debug.LogAssertion(message, context);
        }
        [Conditional("UNITY_EDITOR")]
        public static void LogAssertionFormat(string format, params object[] args)
        {
            UnityEngine.Debug.LogAssertionFormat(format, args);
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void LogFormat(string format, params object[] args)
        {
            UnityEngine.Debug.LogFormat(format, args);
        }
        [Conditional("UNITY_EDITOR")]
        public static void LogFormat(UnityEngine.LogType logType, UnityEngine.LogOption logOptions, UnityEngine.Object context, string format, params object[] args)
        {
            UnityEngine.Debug.LogFormat(logType, logOptions, context, format, args);
        }
        [Conditional("UNITY_EDITOR")]
        public static void LogFormat(UnityEngine.LogType logType, UnityEngine.Object context, string format, params object[] args)
        {
            UnityEngine.Debug.LogFormat(logType, LogOption.None,context, format, args);
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void LogWarningFormat(string format, params object[] args)
        {
            UnityEngine.Debug.LogWarningFormat(format, args);
        }
        [Conditional("UNITY_EDITOR")]
        public static void LogWarningFormat(UnityEngine.Object context, string format, params object[] args)
        {
            UnityEngine.Debug.LogWarningFormat(context, format, args);
        }

        [Conditional("UNITY_EDITOR")]
        public static void LogErrorFormat(string format, params object[] args)
        {
            UnityEngine.Debug.LogErrorFormat(format, args);
        }
        [Conditional("UNITY_EDITOR")]
        public static void LogErrorFormat(UnityEngine.Object context, string format, params object[] args)
        {
            UnityEngine.Debug.LogErrorFormat(context, format, args);
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void DrawLine(Vector3 start, Vector3 end, Color color = default(Color), float duration = 0f, bool depthTest = true)
        {
            UnityEngine.Debug.DrawLine(start, end, color, duration, depthTest);
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void DrawRay(Vector3 start, Vector3 dir, Color color = default(Color), float duration = 0f, bool depthTest = true)
        {
            UnityEngine.Debug.DrawRay(start, dir, color, duration, depthTest);
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void Break()
        {
            UnityEngine.Debug.Break();
        }
        [Conditional("UNITY_EDITOR")]
        public static void DebugBreak()
        {
            UnityEngine.Debug.DebugBreak();
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void ClearDeveloperConsole()
        {
            UnityEngine.Debug.ClearDeveloperConsole();
        }
    }
}