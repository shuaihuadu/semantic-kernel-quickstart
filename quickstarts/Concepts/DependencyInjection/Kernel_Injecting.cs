namespace DependencyInjection;

public class Kernel_Injecting(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task RunAsync()
    {
        IServiceCollection services = new ServiceCollection();

        services.AddLogging(c => c.AddConsole().SetMinimumLevel(LogLevel.Information));
        services.AddAzureOpenAIChatCompletion(TestConfiguration.AzureOpenAI.DeploymentName, TestConfiguration.AzureOpenAI.Endpoint, TestConfiguration.AzureOpenAI.ApiKey);
        services.AddSingleton<Kernel>();

        services.AddTransient<KernelClient>();

        IServiceProvider serviceProvider = services.BuildServiceProvider();

        KernelClient kernelClient = serviceProvider.GetRequiredService<KernelClient>();

        await kernelClient.SummarizeAsync("What is the tallest building in China?");
    }

    private sealed class KernelClient
    {
        private readonly Kernel _kernel;
        private readonly ILogger _logger;

        public KernelClient(Kernel kernel, ILoggerFactory loggerFactory)
        {
            this._kernel = kernel;
            this._logger = loggerFactory.CreateLogger(nameof(KernelClient));
        }

        public async Task SummarizeAsync(string ask)
        {
            string folder = RepoFiles.SamplePluginsPath();

            KernelPlugin summarizePlugin = this._kernel.ImportPluginFromPromptDirectory(Path.Combine(folder, "SummarizePlugin"));

            FunctionResult result = await this._kernel.InvokeAsync(summarizePlugin["Summarize"], new() { ["input"] = ask });

            this._logger.LogWarning("Result - {value}", result.GetValue<string>());
        }
    }
}