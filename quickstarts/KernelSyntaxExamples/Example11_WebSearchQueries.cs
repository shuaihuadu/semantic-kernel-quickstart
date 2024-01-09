namespace KernelSyntaxExamples;

public static class Example11_WebSearchQueries
{
    public static async Task RunAsync()
    {
        Console.WriteLine("======== WebSearchQueries ========");

        Kernel kernel = new();

#pragma warning disable SKEXP0054 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        KernelPlugin kernelPlugin = kernel.ImportPluginFromType<SearchUrlPlugin>("search");
#pragma warning restore SKEXP0054 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        string ask = "What's the tallest building in Europe?";
        FunctionResult result = await kernel.InvokeAsync(kernelPlugin["BingSearchUrl"], new() { ["query"] = ask });

        Console.WriteLine(ask + "\n");
        Console.WriteLine(result.GetValue<string>());
    }
}
