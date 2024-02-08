namespace KernelSyntaxExamples;

public abstract class BaseTest
{
    protected ITestOutputHelper Output { get; }

    protected ILoggerFactory LoggerFactory { get; }

    protected BaseTest(ITestOutputHelper output)
    {
        this.Output = output;
        this.LoggerFactory = new XunitLogger(output);

        LoadUserSecrets();
    }

    private static void LoadUserSecrets()
    {
        IConfigurationRoot configurationRoot = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Development.json", true)
            .AddEnvironmentVariables()
            .AddUserSecrets<Env>()
            .Build();

        TestConfiguration.Initialize(configurationRoot);
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
