namespace KernelSyntaxExamples;

public class BuildingKernel(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public void BuildKernelWithAzureChatCompletion()
    {
        // KernelBuilder provides a simple way to configure a Kernel. This constructs a kernel
        // with logging and an Azure OpenAI chat completion service configured.
        Kernel kernel1 = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
                endpoint: TestConfiguration.AzureOpenAI.Endpoint,
                apiKey: TestConfiguration.AzureOpenAI.ApiKey,
                modelId: TestConfiguration.AzureOpenAI.DeploymentName)
            .Build();
    }

    [Fact]
    public void BuildKernelWithPlugins()
    {
        // Plugins may also be configured via the corresponding Plugins property.
        var builder = Kernel.CreateBuilder();
        builder.Plugins.AddFromType<HttpPlugin>();
        Kernel kernel3 = builder.Build();
    }
}