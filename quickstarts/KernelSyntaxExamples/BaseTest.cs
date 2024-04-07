namespace KernelSyntaxExamples;

public abstract class BaseTest
{
    protected ITestOutputHelper Output { get; }

    protected ILoggerFactory LoggerFactory { get; }

    protected BaseTest(ITestOutputHelper output)
    {
        this.Output = output;
        this.LoggerFactory = new XunitLogger(output);

        TestConfiguration.Initialize();
    }

    protected void WriteLine(object? target = null)
    {
        this.Output.WriteLine(target != null ? target.ToString() : string.Empty);
    }

    protected void Write(object? target = null)
    {
        this.Output.WriteLine(target != null ? target.ToString() : string.Empty);
    }
}
