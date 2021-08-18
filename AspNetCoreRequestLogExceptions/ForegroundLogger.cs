using System;
using System.Text;

using Microsoft.Extensions.Logging;

namespace AspNetCoreRequestLogExceptions
{
    public class ForegroundLogger
        : ILogger
    {
        public ForegroundLogger(
            string                  categoryName,
            IExternalScopeProvider  externalScopeProvider)
        {
            _categoryName           = categoryName;
            _externalScopeProvider  = externalScopeProvider;
        }

        public IDisposable BeginScope<TState>(TState state)
            => _externalScopeProvider.Push(state);

        public bool IsEnabled(LogLevel logLevel)
            => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var @event = new LogEvent<TState>(
                _categoryName,
                eventId,
                exception,
                formatter,
                logLevel,
                state);

            Console.WriteLine(@event.Serialize());
        }

        private readonly string                 _categoryName;
        private readonly IExternalScopeProvider _externalScopeProvider;
    }
}
