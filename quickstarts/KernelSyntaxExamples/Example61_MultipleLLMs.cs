namespace KernelSyntaxExamples;

public class Example61_MultipleLLMs : BaseTest
{
    [RetryFact(typeof(HttpOperationException))]
    public async Task RunAsync()
    {
        this.WriteLine("======== Example61_MultipleLLMs ========");

        string apiKey = TestConfiguration.AzureOpenAI.ApiKey;
        string deploymentName = TestConfiguration.AzureOpenAI.DeploymentName;
        string endpoint = TestConfiguration.AzureOpenAI.Endpoint;

        if (deploymentName == null
            || endpoint == null
            || apiKey == null)
        {
            this.WriteLine("AzureOpenAI endpoint, apiKey, or deploymentName not found. Skipping example.");
            return;
        }

        Kernel kernel = Kernel.CreateBuilder()
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
                modelId: "aoai_model_2")
            .Build();

        await RunByServiceIdAsync(kernel, "aoai_service_1");
        await RunByModelIdAsync(kernel, "aoai_model_2");
        await RunByFirstModelIdAsync(kernel, "aoai_model_1", "aoai_model_2");
    }

    private async Task RunByServiceIdAsync(Kernel kernel, string serviceId)
    {
        this.WriteLine($"======== Service Id: {serviceId} ========");

        string prompt = "Hello AI, what can you do for me?";

        KernelArguments arguments = new();

        arguments.ExecutionSettings = new Dictionary<string, PromptExecutionSettings>
        {
            { serviceId,new PromptExecutionSettings() }
        };

        FunctionResult result = await kernel.InvokePromptAsync(prompt, arguments);

        this.WriteLine(result.GetValue<string>());
    }

    private async Task RunByModelIdAsync(Kernel kernel, string modelId)
    {
        this.WriteLine($"======== Model Id: {modelId} ========");

        string prompt = "Hello AI, what can you do for me?";

        FunctionResult result = await kernel.InvokePromptAsync(
            prompt,
            new(new PromptExecutionSettings
            {
                ModelId = modelId
            }));

        this.WriteLine(result.GetValue<string>());
    }

    private async Task RunByFirstModelIdAsync(Kernel kernel, params string[] modelIds)
    {
        this.WriteLine($"======== Model Ids: {string.Join(", ", modelIds)} ========");

        string prompt = "Hello AI, what can you do for me?";

        Dictionary<string, PromptExecutionSettings> modelSettings = new();

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

        this.WriteLine(result.GetValue<string>());
    }

    public Example61_MultipleLLMs(ITestOutputHelper output) : base(output)
    {
    }
}
