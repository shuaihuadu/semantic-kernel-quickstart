
namespace KernelSyntaxExamples;

public class Example42_KernelBuilder : BaseTest
{
    [Fact]
    public void BuildKernelWithAzureChatCompletion()
    {
        Kernel kernel1 = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: TestConfiguration.AzureOpenAI.ChatDeploymentName,
                endpoint: TestConfiguration.AzureOpenAI.Endpoint,
                apiKey: TestConfiguration.AzureOpenAI.ApiKey,
                modelId: TestConfiguration.AzureOpenAI.ChatModelId)
            .Build();
    }

    [Fact]
    public void BuildKernelUsingServiceCollection()
    {
        IKernelBuilder builder = Kernel.CreateBuilder();

        builder.Services.AddLogging(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Information))
            .AddHttpClient()
            .AddAzureOpenAIChatCompletion(
                deploymentName: TestConfiguration.AzureOpenAI.ChatDeploymentName,
                endpoint: TestConfiguration.AzureOpenAI.Endpoint,
                apiKey: TestConfiguration.AzureOpenAI.ApiKey,
                modelId: TestConfiguration.AzureOpenAI.ChatModelId);

        Kernel kernel2 = builder.Build();
    }

    [Fact]
    public void BuildKernelWithPlugins()
    {

        IKernelBuilder builder = Kernel.CreateBuilder();
        builder.Plugins.AddFromType<HttpPlugin>();

        Kernel kernel3 = builder.Build();
    }

    [Fact]
    public void BuildKernelUsingServiceProvider()
    {
        IKernelBuilder builder = Kernel.CreateBuilder();

        Debug.Assert(!ReferenceEquals(builder.Build(), builder.Build()));

        IServiceCollection services = new ServiceCollection();
        services.AddLogging(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Information));
        services.AddHttpClient();
        services.AddAzureOpenAIChatCompletion(
            deploymentName: TestConfiguration.AzureOpenAI.ChatDeploymentName,
            endpoint: TestConfiguration.AzureOpenAI.Endpoint,
            apiKey: TestConfiguration.AzureOpenAI.ApiKey,
            modelId: TestConfiguration.AzureOpenAI.ChatModelId);

        Kernel kernel4 = new(services.BuildServiceProvider());
        services.AddTransient<Kernel>();

        Kernel kernel5 = services.BuildServiceProvider().GetRequiredService<Kernel>();
    }

    [Fact]
    public void BuildKernelUsingServiceCollectionExtensions()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddLogging(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Information));
        services.AddHttpClient();
        services.AddKernel().AddAzureOpenAIChatCompletion(
                deploymentName: TestConfiguration.AzureOpenAI.ChatDeploymentName,
                endpoint: TestConfiguration.AzureOpenAI.Endpoint,
                apiKey: TestConfiguration.AzureOpenAI.ApiKey,
                modelId: TestConfiguration.AzureOpenAI.ChatModelId);

        services.AddSingleton(sp => KernelPluginFactory.CreateFromType<TimePlugin>(serviceProvider: sp));
        services.AddSingleton(sp => KernelPluginFactory.CreateFromType<HttpPlugin>(serviceProvider: sp));

        Kernel kernel6 = services.BuildServiceProvider().GetRequiredService<Kernel>();
    }


    public Example42_KernelBuilder(ITestOutputHelper output) : base(output)
    {
    }
}
