namespace KernelSyntaxExamples;

public static class Example21_OpenAIPlugins
{
    public static async Task RunAsync()
    {
        await CallKlarnaAsync();
    }

    public static async Task RunOpenAIPluginAsync()
    {
        Kernel kernel = new();

        using HttpClient client = new();

        KernelPlugin plugin = await kernel.ImportPluginFromOpenAIAsync("<plugin name>", new Uri("<OpenAI plugin>"), new OpenAIFunctionExecutionParameters(httpClient: client));

        KernelArguments kernelArguments = new() { ["<pararmeter-name"] = "<parameter-value>" };

        FunctionResult functionResult = await kernel.InvokeAsync(plugin["<function-name>"], kernelArguments);

        RestApiOperationResponse? result = functionResult.GetValue<RestApiOperationResponse>();

        Console.WriteLine("Function execution result: {0}", result?.Content?.ToString());
    }

    public static async Task CallKlarnaAsync()
    {
        Kernel kernel = new();

        KernelPlugin plugin = await kernel.ImportPluginFromOpenAIAsync("Klarna", new Uri("https://www.klarna.com/.well-known/ai-plugin.json"));

        KernelArguments kernelArguments = new()
        {
            ["q"] = "Laptop",
            ["size"] = "3",
            ["budget"] = "200",
            ["countryCode"] = "US"
        };

        FunctionResult functionResult = await kernel.InvokeAsync(plugin["productsUsingGET"], kernelArguments);

        RestApiOperationResponse? result = functionResult.GetValue<RestApiOperationResponse>();

        Console.WriteLine("Function execution result: {0}", result?.Content?.ToString());
    }
}
