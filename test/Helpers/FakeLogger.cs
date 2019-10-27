using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Eleia.Test.Helpers
{
    public sealed class FakeLoggerFactory : ILoggerFactory, IDisposable
    {
        private static FakeLogger instance;

        public void AddProvider(ILoggerProvider provider)
        {
            throw new NotImplementedException();
        }

        public ILogger CreateLogger(string categoryName)
        {
            if (instance == null)
            {
                instance = new FakeLogger();
            }

            return instance;
        }

        public void Dispose()
        {
        }
    }

    public class FakeLogger : ILogger
    {
        public List<string> Logs { get; }

        public FakeLogger()
        {
            Logs = new List<string>();
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Logs.Add(string.Format("[{0}] {1}", logLevel, formatter.Invoke(state, exception)));
        }
    }
}