using System;

using Microsoft.Extensions.Logging;

namespace AspNetCoreRequestLogExceptions
{
    public class BackgroundLogger
        : ILogger
    {
        public BackgroundLogger(
            string                  categoryName,
            IExternalScopeProvider  externalScopeProvider,
            Action<LogEvent>        onLog)
        {
            _categoryName           = categoryName;
            _externalScopeProvider  = externalScopeProvider;
            _onLog                  = onLog;
        }

        public IDisposable BeginScope<TState>(TState state)
            => _externalScopeProvider.Push(state);

        public bool IsEnabled(LogLevel logLevel)
            => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => _onLog.Invoke(new LogEvent<TState>(
                _categoryName,
                eventId,
                exception,
                formatter,
                logLevel,
                state));

        private readonly string                 _categoryName;
        private readonly IExternalScopeProvider _externalScopeProvider;
        private readonly Action<LogEvent>       _onLog;
    }
}
