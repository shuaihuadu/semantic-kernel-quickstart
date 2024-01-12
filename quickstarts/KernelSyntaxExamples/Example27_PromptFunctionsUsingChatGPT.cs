namespace KernelSyntaxExamples;

public static class Example27_PromptFunctionsUsingChatGPT
{
    public static async Task RunAsync()
    {
        Console.WriteLine("======== Using Chat GPT model for text generation ========");

        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: TestConfiguration.AzureOpenAI.ChatDeploymentName,
                endpoint: TestConfiguration.AzureOpenAI.Endpoint,
                apiKey: TestConfiguration.AzureOpenAI.ApiKey,
                modelId: TestConfiguration.AzureOpenAI.ChatModelId)
            .Build();

        KernelFunction kernelFunction = kernel.CreateFunctionFromPrompt("List the two planets closest to '{{$input}}', excluding moons, using bullet points.");

        FunctionResult functionResult = await kernel.InvokeAsync(kernelFunction, new() { ["input"] = "Earth" });

        Console.WriteLine(functionResult.GetValue<string>());
    }
}
