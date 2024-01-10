namespace KernelSyntaxExamples;

public static class Example11_WebSearchQueries
{
    public static async Task RunAsync()
    {
        Console.WriteLine("======== WebSearchQueries ========");

        Kernel kernel = new();

        KernelPlugin kernelPlugin = kernel.ImportPluginFromType<SearchUrlPlugin>("search");

        string ask = "What's the tallest building in Europe?";
        FunctionResult result = await kernel.InvokeAsync(kernelPlugin["BingSearchUrl"], new() { ["query"] = ask });

        Console.WriteLine(ask + "\n");
        Console.WriteLine(result.GetValue<string>());
    }
}
