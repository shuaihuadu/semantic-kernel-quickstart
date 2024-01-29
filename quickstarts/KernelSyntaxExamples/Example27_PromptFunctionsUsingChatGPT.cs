namespace KernelSyntaxExamples;

public class Example27_PromptFunctionsUsingChatGPT : BaseTest
{
    [Fact]
    public async Task RunAsync()
    {
        this.WriteLine("======== Using Chat GPT model for text generation ========");

        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: TestConfiguration.AzureOpenAI.ChatDeploymentName,
                endpoint: TestConfiguration.AzureOpenAI.Endpoint,
                apiKey: TestConfiguration.AzureOpenAI.ApiKey,
                modelId: TestConfiguration.AzureOpenAI.ChatModelId)
            .Build();

        KernelFunction kernelFunction = kernel.CreateFunctionFromPrompt("List the two planets closest to '{{$input}}', excluding moons, using bullet points.");

        FunctionResult functionResult = await kernel.InvokeAsync(kernelFunction, new() { ["input"] = "Earth" });

        this.WriteLine(functionResult.GetValue<string>());
    }

    public Example27_PromptFunctionsUsingChatGPT(ITestOutputHelper output) : base(output)
    {
    }
}
