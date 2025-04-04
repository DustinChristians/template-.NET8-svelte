﻿using System;
using Microsoft.Extensions.Logging;

namespace CompanyName.ProjectName.TestUtilities
{
    public class EFCoreLogger : ILogger
    {
        private readonly Action<string> efCoreLogAction;
        private readonly LogLevel loggerLogLevel;

        public EFCoreLogger(Action<string> efCoreLogAction, LogLevel loggerLogLevel)
        {
            this.efCoreLogAction = efCoreLogAction;
            this.loggerLogLevel = loggerLogLevel;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= loggerLogLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            var message = formatter(state, exception);

            if (!string.IsNullOrEmpty(message))
            {
                efCoreLogAction($"Log Level: {logLevel}, {message}");
            }
        }
    }
}
