namespace KernelSyntaxExamples;

public static class Example62_CustomAIServiceSelector
{
    public static async Task RunAsync()
    {
        Console.WriteLine("======== Example62_CustomAIServiceSelector ========");

        string apiKey = TestConfiguration.AzureOpenAI.ApiKey;
        string deploymentName = TestConfiguration.AzureOpenAI.ChatDeploymentName;
        string endpoint = TestConfiguration.AzureOpenAI.Endpoint;

        if (deploymentName == null
            || endpoint == null
            || apiKey == null)
        {
            Console.WriteLine("AzureOpenAI endpoint, apiKey, or deploymentName not found. Skipping example.");
            return;
        }

        IKernelBuilder builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: deploymentName,
                endpoint: endpoint,
                apiKey: apiKey,
                serviceId: "aoai_service_1",
                modelId: "aoai_model_1")
            .AddAzureOpenAIChatCompletion(
                deploymentName: deploymentName,
                endpoint: endpoint,
                apiKey: apiKey,
                serviceId: "aoai_service_2",
                modelId: "aoai_model_2");

        builder.Services.AddSingleton<IAIServiceSelector>(new GptAIServiceSelector());

        Kernel kernel = builder.Build();

        string prompt = "Hello AI, what can you do for me?";

        FunctionResult result = await kernel.InvokePromptAsync(prompt);

        Console.WriteLine(result.GetValue<string>());
    }

    private sealed class GptAIServiceSelector : IAIServiceSelector
    {
        bool IAIServiceSelector.TrySelectAIService<T>(
            Kernel kernel,
            KernelFunction function,
            KernelArguments arguments,
            [NotNullWhen(true)] out T? service,
            out PromptExecutionSettings? serviceSettings) where T : class
        {
            foreach (var serviceToCheck in kernel.GetAllServices<T>())
            {
                string? serviceModelId = serviceToCheck.GetModelId();

                string? endpoint = serviceToCheck.GetEndpoint();

                if (!string.IsNullOrEmpty(serviceModelId) && serviceModelId.Equals("aoai_model_1", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Selected model: {serviceModelId} {endpoint}");

                    service = serviceToCheck;
                    serviceSettings = new OpenAIPromptExecutionSettings();

                    return true;
                }
            }

            service = null;
            serviceSettings = null;

            return false;
        }
    }
}
