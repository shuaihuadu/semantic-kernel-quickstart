namespace ChatCompletion;

public class Connectors_WithMultipleLLMs(ITestOutputHelper output) : BaseTest(output)
{
    [RetryFact(typeof(HttpOperationException))]
    public async Task RunAsync()
    {
        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
                endpoint: TestConfiguration.AzureOpenAI.Endpoint,
                apiKey: TestConfiguration.AzureOpenAI.ApiKey,
                serviceId: "aoai_service_1",
                modelId: "aoai_model_1")
            .AddAzureOpenAIChatCompletion(
                deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
                endpoint: TestConfiguration.AzureOpenAI.Endpoint,
                apiKey: TestConfiguration.AzureOpenAI.ApiKey,
                serviceId: "aoai_service_2",
                modelId: "aoai_model_2")
            .Build();

        await RunByServiceIdAsync(kernel, "aoai_service_1");
        await RunByModelIdAsync(kernel, "aoai_model_2");
        await RunByFirstModelIdAsync(kernel, "aoai_model_1", "aoai_model_2");
    }

    private async Task RunByServiceIdAsync(Kernel kernel, string serviceId)
    {
        Console.WriteLine($"======== Service Id: {serviceId} ========");

        string prompt = "Hello AI, what can you do for me?";

        KernelArguments arguments = new()
        {
            ExecutionSettings = new Dictionary<string, PromptExecutionSettings>
            {
                { serviceId,new PromptExecutionSettings() }
            }
        };

        FunctionResult result = await kernel.InvokePromptAsync(prompt, arguments);

        Console.WriteLine(result.GetValue<string>());
    }

    private async Task RunByModelIdAsync(Kernel kernel, string modelId)
    {
        Console.WriteLine($"======== Model Id: {modelId} ========");

        string prompt = "Hello AI, what can you do for me?";

        FunctionResult result = await kernel.InvokePromptAsync(
            prompt,
            new(new PromptExecutionSettings
            {
                ModelId = modelId
            }));

        Console.WriteLine(result.GetValue<string>());
    }

    private async Task RunByFirstModelIdAsync(Kernel kernel, params string[] modelIds)
    {
        Console.WriteLine($"======== Model Ids: {string.Join(", ", modelIds)} ========");

        string prompt = "Hello AI, what can you do for me?";

        Dictionary<string, PromptExecutionSettings> modelSettings = [];

        foreach (string modelId in modelIds)
        {
            modelSettings.Add(modelId, new PromptExecutionSettings()
            {
                ModelId = modelId
            });
        }

        PromptTemplateConfig templateConfig = new(prompt)
        {
            Name = "HelloAI",
            ExecutionSettings = modelSettings
        };

        KernelFunction function = kernel.CreateFunctionFromPrompt(templateConfig);

        FunctionResult result = await kernel.InvokeAsync(function);

        Console.WriteLine(result.GetValue<string>());
    }
}
