using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace AspNetCoreRequestLogExceptions
{
    public sealed class BackgroundLoggerProvider
        : ILoggerProvider,
            ISupportExternalScope,
            IAsyncDisposable
    {
        public BackgroundLoggerProvider()
        {
            _events = Channel.CreateUnbounded<LogEvent>(new()
            {
                AllowSynchronousContinuations   = false,
                SingleReader                    = true,
                SingleWriter                    = false
            });
        }

        public ILogger CreateLogger(string categoryName)
            => _hasDisposeStarted
                ? throw new InvalidOperationException("The provider has been disposed")
                : new BackgroundLogger(
                    categoryName,
                    _externalScopeProvider ?? throw new InvalidOperationException("The provider has not been fully initialized: SetScopeProvider() has not been called."),
                    OnLog);

        public async ValueTask DisposeAsync()
        {
            if (_hasDisposeStarted)
                return;
            _hasDisposeStarted = true;

            if (_backgroundStopTokenSource is not null)
                _backgroundStopTokenSource.Cancel();

            if (_whenBackgroundStopped is not null)
                await _whenBackgroundStopped;
        }

        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
            => _externalScopeProvider = scopeProvider;

        void IDisposable.Dispose()
        {
            var result = DisposeAsync();
            if (!result.IsCompletedSuccessfully)
                result.AsTask().GetAwaiter().GetResult();
        }

        private async Task RunBackgroundAsync(CancellationToken stopToken)
        {
            try
            {
                while (!stopToken.IsCancellationRequested)
                {
                    await _events.Reader.WaitToReadAsync(stopToken);
                    while (_events.Reader.TryRead(out var @event))
                        Console.WriteLine(@event.Serialize());
                }
            }
            catch (OperationCanceledException) { }
        }

        private void OnLog(LogEvent @event)
        {
            _events.Writer.TryWrite(@event);

            if (_hasDisposeStarted)
                return;

            var hasBackgroundStarted = Interlocked.Exchange(ref _hasBackgroundStarted, 1);
            if (hasBackgroundStarted is 0)
            {
                _backgroundStopTokenSource = new CancellationTokenSource();
                _whenBackgroundStopped = Task.Run(() => RunBackgroundAsync(_backgroundStopTokenSource.Token));
            }
        }

        private readonly Channel<LogEvent> _events;

        private CancellationTokenSource?    _backgroundStopTokenSource;
        private IExternalScopeProvider?     _externalScopeProvider;
        private int                         _hasBackgroundStarted;
        private bool                        _hasDisposeStarted;
        private Task?                       _whenBackgroundStopped;
    }
}
