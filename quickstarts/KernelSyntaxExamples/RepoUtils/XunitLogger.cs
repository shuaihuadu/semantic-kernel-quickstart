namespace KernelSyntaxExamples.RepoUtils;

internal sealed class XunitLogger : ILoggerFactory, ILogger, IDisposable
{
    private readonly ITestOutputHelper _output;

    public XunitLogger(ITestOutputHelper output)
    {
        this._output = output;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        this._output.WriteLine(state?.ToString());
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public ILogger CreateLogger(string categoryName) => this;

    public void AddProvider(ILoggerProvider provider)
    {
        throw new NotImplementedException();
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => this;



    public void Dispose()
    {
    }
}
