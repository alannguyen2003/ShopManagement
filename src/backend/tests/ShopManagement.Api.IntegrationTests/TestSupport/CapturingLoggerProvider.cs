using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace ShopManagement.Api.IntegrationTests.TestSupport;

public sealed record LogEntry(LogLevel Level, string Category, string Message);

public sealed class CapturingLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentQueue<LogEntry> _entries = new();

    public IReadOnlyList<LogEntry> Entries => _entries.ToArray();

    public ILogger CreateLogger(string categoryName) => new CapturingLogger(categoryName, _entries);

    public void Dispose()
    {
    }

    private sealed class CapturingLogger(string category, ConcurrentQueue<LogEntry> entries) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter) =>
            entries.Enqueue(new LogEntry(logLevel, category, formatter(state, exception)));
    }
}
