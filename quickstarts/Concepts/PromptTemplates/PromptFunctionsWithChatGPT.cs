namespace PromptTemplates;

public class PromptFunctionsWithChatGPT(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task RunAsync()
    {
        this.WriteLine("======== Using Chat GPT model for text generation ========");

        Kernel kernel = KernelHelper.AzureOpenAIChatCompletionKernelBuilder().Build();

        KernelFunction kernelFunction = kernel.CreateFunctionFromPrompt("List the two planets closest to '{{$input}}', excluding moons, using bullet points.");

        FunctionResult functionResult = await kernel.InvokeAsync(kernelFunction, new() { ["input"] = "Earth" });

        this.WriteLine(functionResult.GetValue<string>());
    }
}
