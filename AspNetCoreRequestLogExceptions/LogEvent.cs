using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.Logging;

namespace AspNetCoreRequestLogExceptions
{
    public abstract class LogEvent
    {
        protected LogEvent(
            string      categoryName,
            EventId     eventId,
            Exception?  exception,
            LogLevel    logLevel)
        {
            CategoryName    = categoryName;
            EventId         = eventId;
            Exception       = exception;
            LogLevel        = logLevel;
        }

        public string CategoryName { get; }

        public EventId EventId { get; }
        
        public Exception? Exception { get; }
        
        public LogLevel LogLevel { get; }

        public abstract string Serialize();
    }

    public sealed class LogEvent<TState>
        : LogEvent
    {
        public LogEvent(
                string                              categoryName,
                EventId                             eventId,
                Exception?                          exception,
                Func<TState, Exception?, string>    formatter,
                LogLevel                            logLevel,
                TState                              state)
            : base(
                categoryName,
                eventId,
                exception,
                logLevel)
        {
            State       = state;
            Formatter   = formatter;
        }

        public TState State { get; }
        
        public Func<TState, Exception?, string> Formatter { get; }

        public override string Serialize()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"{LogLevel}:\t{CategoryName}:{EventId.Name}[{EventId.Id}]");

            var lastValidLength = stringBuilder.Length;
            try
            {
                stringBuilder.AppendLine(Formatter.Invoke(State, Exception));
            }
            catch (Exception ex)
            {
                stringBuilder.Length = lastValidLength;
                stringBuilder.AppendLine("\tUNABLE TO SERIALIZE MESSAGE");
                stringBuilder.AppendLine($"\t{ex}");
            }

            lastValidLength = stringBuilder.Length;
            try
            {
                if (State is IReadOnlyList<KeyValuePair<string, object?>> fieldset)
                    foreach (var field in fieldset)
                        if ((field.Value is not IDisposable) && (field.Value is not IAsyncDisposable))
                            stringBuilder.AppendLine($"\t{field.Key}: {field.Value}");
            }
            catch (Exception ex)
            {
                stringBuilder.Length = lastValidLength;
                stringBuilder.AppendLine("\tUNABLE TO SERIALIZE STATE");
                stringBuilder.AppendLine($"\t{ex}");
            }

            return stringBuilder.ToString();
        }
    }
}
