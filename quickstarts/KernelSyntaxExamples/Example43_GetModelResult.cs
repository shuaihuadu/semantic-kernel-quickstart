namespace KernelSyntaxExamples;

public static class Example43_GetModelResult
{
    public static async Task RunAsync()
    {
        Console.WriteLine("======== Inline Function Definition + Invocation ========");

        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
             deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
             endpoint: TestConfiguration.AzureOpenAI.Endpoint,
             apiKey: TestConfiguration.AzureOpenAI.ApiKey)
            .Build();

        const string FunctionDefinition = "Hi, give me 5 book suggestions about: {{$input}}, please response in Chinese.";

        KernelFunction kernelFunction = kernel.CreateFunctionFromPrompt(FunctionDefinition);

        FunctionResult result = await kernel.InvokeAsync(kernelFunction, new() { ["input"] = "秦朝" });

        Console.WriteLine(result.GetValue<string>());
        Console.WriteLine(result.Metadata?["Usage"]?.AsJson());
        Console.WriteLine();
    }
}
