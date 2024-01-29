namespace KernelSyntaxExamples.GettingStart;

public class Step4_Dependency_Injection : BaseTest
{

    [Fact]
    public async Task RunAsync()
    {
        ServiceProvider serviceProvider = BuildServiceProvider();

        Kernel kernel = serviceProvider.GetRequiredService<Kernel>();

        KernelArguments arguments = new()
            {
                { "topic","earth when viewed from space"}
            };

        await foreach (StreamingKernelContent update in kernel.InvokePromptStreamingAsync("What color is the {{$topic}}? Provider a detailed explanation.", arguments))
        {
            Write(update);
        }
    }

    private static ServiceProvider BuildServiceProvider()
    {
        ServiceCollection services = new ServiceCollection();

        services.AddSingleton(ConsoleLogger.LoggerFactory);

        IKernelBuilder KernelBuilder = services.AddKernel();

        KernelBuilder.AddAzureOpenAIChatCompletion(
            deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
            endpoint: TestConfiguration.AzureOpenAI.Endpoint,
            apiKey: TestConfiguration.AzureOpenAI.ApiKey);

        KernelBuilder.Plugins.AddFromType<TimeInformation>();

        return services.BuildServiceProvider();
    }

    public class TimeInformation
    {
        private readonly ILogger _logger;

        public TimeInformation(ILoggerFactory loggerFactory)
        {
            this._logger = loggerFactory.CreateLogger<TimeInformation>();
        }
        [KernelFunction]
        [Description("Retrieves the current time in UTC.")]
        public string GetCurrentUtcTime()
        {
            string utcNow = DateTime.UtcNow.ToString("R");

            this._logger.LogInformation("Returning current time {time}", utcNow);

            return utcNow;
        }
    }

    public Step4_Dependency_Injection(ITestOutputHelper output) : base(output)
    {
    }
}
