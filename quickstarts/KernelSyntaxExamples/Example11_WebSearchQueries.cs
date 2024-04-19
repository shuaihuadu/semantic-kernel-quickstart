namespace KernelSyntaxExamples;

public class Example11_WebSearchQueries : BaseTest
{
    [Fact]
    public async Task RunAsync()
    {
        this.WriteLine("======== WebSearchQueries ========");

        Kernel kernel = new();

        KernelPlugin kernelPlugin = kernel.ImportPluginFromType<SearchUrlPlugin>("search");

        string ask = "What's the tallest building in Europe?";
        FunctionResult result = await kernel.InvokeAsync(kernelPlugin["BingSearchUrl"], new() { ["query"] = ask });

        this.WriteLine(ask + "\n");
        this.WriteLine(result.GetValue<string>());
    }

    public Example11_WebSearchQueries(ITestOutputHelper output) : base(output)
    {
    }
}