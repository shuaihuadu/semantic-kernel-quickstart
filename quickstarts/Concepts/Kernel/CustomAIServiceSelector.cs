using Microsoft.SemanticKernel.Services;
using System.Diagnostics.CodeAnalysis;

namespace KernelSyntaxExamples;

public class CustomAIServiceSelector(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task RunAsync()
    {
        this.WriteLine("======== Example62_CustomAIServiceSelector ========");

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

        builder.Services.AddSingleton<IAIServiceSelector>(new GptAIServiceSelector(this.Output));

        Kernel kernel = builder.Build();

        string prompt = "Hello AI, what can you do for me?";

        FunctionResult result = await kernel.InvokePromptAsync(prompt);

        this.WriteLine(result.GetValue<string>());
    }

    private sealed class GptAIServiceSelector(ITestOutputHelper output) : IAIServiceSelector
    {
        private readonly ITestOutputHelper _output = output;

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
                    this._output.WriteLine($"Selected model: {serviceModelId} {endpoint}");

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