namespace Utils.OnlineMultiplayer
{
    using Object = UnityEngine.Object;
    using System;
    using UnityEngine;

    public enum LogMode
    {
        Critical,
        Warnings,
        Verbose
    }

    public class LogHandler : ILogHandler
    {
        private ILogHandler _defaultLogHandler = Debug.unityLogger.logHandler;
        private static LogHandler _instance;

        public LogMode LogMode = LogMode.Critical;

        public static LogHandler Get()
        {
            if (_instance != null) return _instance;

            _instance = new LogHandler();

            Debug.unityLogger.logHandler = _instance;

            return _instance;
        }

        public void LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            if (logType == LogType.Exception) return;

            if (logType == LogType.Error || logType == LogType.Assert)
            {
                _defaultLogHandler.LogFormat(logType, context, format, args);
                return;
            }

            if (LogMode == LogMode.Critical) return;

            if (logType == LogType.Warning)
            {
                _defaultLogHandler.LogFormat(logType, context, format, args);
                return;
            }

            if (LogMode != LogMode.Verbose) return;

            _defaultLogHandler.LogFormat(logType, context, format, args);
        }

        public void LogException(Exception exception, Object context)
        {
            _defaultLogHandler.LogException(exception, context);
        }
    }
}