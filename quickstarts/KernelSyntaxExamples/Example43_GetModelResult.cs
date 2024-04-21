namespace KernelSyntaxExamples;

public class Example43_GetModelResult(ITestOutputHelper output) : BaseTest(output)
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
}
