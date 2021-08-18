using System;

using Microsoft.Extensions.Logging;

namespace AspNetCoreRequestLogExceptions
{
    public sealed class ForegroundLoggerProvider
        : ILoggerProvider,
            ISupportExternalScope
    {
        public ILogger CreateLogger(string categoryName)
            => new ForegroundLogger(
                categoryName,
                _externalScopeProvider ?? throw new InvalidOperationException("The provider has not been fully initialized: SetScopeProvider() has not been called."));

        public void Dispose() { }

        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
            => _externalScopeProvider = scopeProvider;

        private IExternalScopeProvider? _externalScopeProvider;
    }
}
