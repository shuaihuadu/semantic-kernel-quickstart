namespace Functions;

public class FunctionResult_Metadata(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task GetTokenUsageMetadataAsync()
    {
        this.WriteLine("======== Inline Function Definition + Invocation ========");

        Kernel kernel = KernelHelper.AzureOpenAIChatCompletionKernelBuilder().Build();

        const string FunctionDefinition = "Hi, give me 5 book suggestions about: {{$input}}, please response in Chinese.";

        KernelFunction kernelFunction = kernel.CreateFunctionFromPrompt(FunctionDefinition);

        FunctionResult result = await kernel.InvokeAsync(kernelFunction, new() { ["input"] = "秦朝" });

        this.WriteLine(result.GetValue<string>());
        this.WriteLine(result.Metadata?["Usage"]?.AsJson());
        this.WriteLine();
    }

    [Fact]
    public async Task GetFullModelMetadataAsync()
    {
        this.WriteLine("======== Inline Function Definition + Invocation ========");

        Kernel kernel = KernelHelper.CreateKernelWithAzureOpenAIChatCompletion();

        const string FunctionDefinition = "1 + 1 = ?";

        KernelFunction function = kernel.CreateFunctionFromPrompt(FunctionDefinition);

        FunctionResult result = await kernel.InvokeAsync(function);

        Console.WriteLine(result.GetValue<string>());
        Console.WriteLine(result.Metadata?.AsJson());
        Console.WriteLine();
    }

    [Fact]
    public async Task GetMetadataFromStreamAsync()
    {
        Kernel kernel = KernelHelper.CreateKernelWithAzureOpenAIChatCompletion();

        const string FunctionDefinition = "1 + 1 = ?";

        KernelFunction function = kernel.CreateFunctionFromPrompt(FunctionDefinition);

        await foreach (var content in kernel.InvokeStreamingAsync(function))
        {
            Console.WriteLine(content.Metadata?.AsJson());
        }
    }
}
