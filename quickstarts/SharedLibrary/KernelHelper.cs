using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace SharedLibrary;

public class KernelHelper
{
    public static IKernelBuilder AzureOpenAIChatCompletionKernelBuilder()
    {
        TestConfiguration.Initialize();

        IKernelBuilder builder = Kernel.CreateBuilder();

        builder.AddAzureOpenAIChatCompletion(
            deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
             endpoint: TestConfiguration.AzureOpenAI.Endpoint,
             apiKey: TestConfiguration.AzureOpenAI.ApiKey);

        return builder;
    }

    public static IChatCompletionService CreateCompletionService()
    {
        return new AzureOpenAIChatCompletionService(
            deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
            endpoint: TestConfiguration.AzureOpenAI.Endpoint,
            apiKey: TestConfiguration.AzureOpenAI.ApiKey);
    }
}
