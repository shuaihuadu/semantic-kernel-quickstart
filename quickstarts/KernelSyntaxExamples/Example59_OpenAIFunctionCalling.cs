namespace KernelSyntaxExamples;

public class Example59_OpenAIFunctionCalling : BaseTest
{
    [Fact(Skip = "Azure OpenAI function call")]
    public Task RunAsync()
    {
        IKernelBuilder builder = Kernel.CreateBuilder();

        builder.AddAzureOpenAIChatCompletion(
            TestConfiguration.AzureOpenAI.ChatDeploymentName,
            TestConfiguration.AzureOpenAI.Endpoint,
            TestConfiguration.AzureOpenAI.ApiKey);

        builder.Services.AddLogging(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Trace));

        Kernel kernel = builder.Build();

        //TODO kernel.ImportPluginFromFunctions;

        Console.WriteLine("======== Example 1: Use automated function calling with a non-streaming prompt ========");
        Console.WriteLine("======== Example 2: Use automated function calling with a streaming prompt ========");
        Console.WriteLine("======== Example 3: Use manual function calling with a non-streaming prompt ========");
        Console.WriteLine("======== Example 4: Use automated function calling with a streaming chat ========");

        return Task.CompletedTask;
    }

    public Example59_OpenAIFunctionCalling(ITestOutputHelper output) : base(output)
    {
    }
}
