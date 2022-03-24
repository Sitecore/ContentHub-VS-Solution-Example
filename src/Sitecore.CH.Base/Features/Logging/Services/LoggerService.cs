using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sitecore.CH.Base.Features.Logging.Services
{
    public interface ILoggerService<T> : ILogger<T>
    {
    }

    public class LoggerService<T> : ILogger<T>, ILoggerService<T>
    {
        private readonly ILogger<T> _logger;
        private readonly ILoggingContextService _loggingContextService;

        public LoggerService(ILogger<T> logger, ILoggingContextService loggingContextService)
        {
            this._logger = logger;
            this._loggingContextService = loggingContextService;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return _logger.BeginScope(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _logger.IsEnabled(logLevel);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Func<TState, Exception, string> function = (TState t, Exception x) =>
            {
                var result = formatter(t, x);

                if (_loggingContextService.Context != null)
                {
                    result = $"{ _loggingContextService.Context}|{result}";
                }
                return result;
            };

            _logger.Log(logLevel, eventId, state, exception, function);
        }
    }
}
