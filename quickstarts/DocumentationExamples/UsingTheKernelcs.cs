namespace DocumentationExamples;

public class UsingTheKernelcs(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task RunAsync()
    {
        WriteLine("======== Kernel ========");

        IKernelBuilder builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
            deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
            endpoint: TestConfiguration.AzureOpenAI.Endpoint,
            apiKey: TestConfiguration.AzureOpenAI.ApiKey);

        builder.Services.AddLogging(c => c.AddDebug().SetMinimumLevel(LogLevel.Trace));

        builder.Plugins.AddFromType<TimePlugin>();
        builder.Plugins.AddFromPromptDirectory(Path.Join(AppContext.BaseDirectory, "Plugins", "WriterPlugin"), "WriterPlugin");

        Kernel kernel = builder.Build();

        FunctionResult currentTime = await kernel.InvokeAsync("TimePlugin", "UtcNow");

        WriteLine(currentTime.ToString());


        FunctionResult poemResult = await kernel.InvokeAsync("WriterPlugin", "ShortPoem", new()
        {
            ["input"] = currentTime
        });

        WriteLine(poemResult.ToString());
    }
}
