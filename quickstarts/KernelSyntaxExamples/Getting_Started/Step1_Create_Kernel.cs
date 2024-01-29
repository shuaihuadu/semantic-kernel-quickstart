namespace KernelSyntaxExamples;

public class Step1_Create_Kernel : BaseTest
{
    [Fact]
    public async Task RunAsync()
    {
        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
                endpoint: TestConfiguration.AzureOpenAI.Endpoint,
                apiKey: TestConfiguration.AzureOpenAI.ApiKey)
            .Build();

        WriteLine(await kernel.InvokePromptAsync("What color is the sky?"));
        WriteLine();

        KernelArguments arguments = new()
        {
            { "topic","sea" }
        };

        WriteLine(await kernel.InvokePromptAsync("What color is the {{$topic}}?", arguments));
        WriteLine();

        await foreach (var update in kernel.InvokePromptStreamingAsync("What color is the {{$topic}}? Provide a detailed explanation.", arguments))
        {
            WriteLine(update);
        }

        WriteLine(string.Empty);

        arguments = new KernelArguments(new OpenAIPromptExecutionSettings
        {
            MaxTokens = 500,
            Temperature = 0.5
        })
        {
            { "topic","dogs"}
        };

        WriteLine(await kernel.InvokePromptAsync("Tell me a story about {{$topic}}", arguments));


        arguments = new KernelArguments
        {
            { "topic","chocolate"}
        };
        WriteLine(await kernel.InvokePromptAsync("Create a recipe for a {{$topic}} cake in JSON format", arguments));
    }

    public Step1_Create_Kernel(ITestOutputHelper output) : base(output)
    {
    }
}
