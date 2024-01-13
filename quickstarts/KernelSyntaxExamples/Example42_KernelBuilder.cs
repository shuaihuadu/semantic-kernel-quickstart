namespace KernelSyntaxExamples;

public static class Example42_KernelBuilder
{
    public static Task RunAsync()
    {
        string azureOpenAIKey = TestConfiguration.AzureOpenAI.ApiKey;
        string azureOpenAIEndpoint = TestConfiguration.AzureOpenAI.Endpoint;
        string azureOpenAIChatDeploymentName = TestConfiguration.AzureOpenAI.ChatDeploymentName;
        string azureOpenAIChatModelId = TestConfiguration.AzureOpenAI.ChatModelId;


        Kernel kernel1 = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: azureOpenAIChatDeploymentName,
                endpoint: azureOpenAIEndpoint,
                apiKey: azureOpenAIKey,
                modelId: azureOpenAIChatModelId)
            .Build();

        IKernelBuilder builder = Kernel.CreateBuilder();

        builder.Services.AddLogging(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Information))
            .AddHttpClient()
            .AddAzureOpenAIChatCompletion(
                deploymentName: azureOpenAIChatDeploymentName,
                endpoint: azureOpenAIEndpoint,
                apiKey: azureOpenAIKey,
                modelId: azureOpenAIChatModelId);

        Kernel kernel2 = builder.Build();

        builder = Kernel.CreateBuilder();
        builder.Plugins.AddFromType<HttpPlugin>();

        Kernel kernel3 = builder.Build();

        builder = Kernel.CreateBuilder();

        Debug.Assert(!ReferenceEquals(builder.Build(), builder.Build()));

        IServiceCollection services = new ServiceCollection();
        services.AddLogging(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Information));
        services.AddHttpClient();
        services.AddAzureOpenAIChatCompletion(
            deploymentName: azureOpenAIChatDeploymentName,
            endpoint: azureOpenAIEndpoint,
            apiKey: azureOpenAIKey,
            modelId: azureOpenAIChatModelId);

        Kernel kernel4 = new(services.BuildServiceProvider());
        services.AddTransient<Kernel>();

        Kernel kernel5 = services.BuildServiceProvider().GetRequiredService<Kernel>();

        services = new ServiceCollection();
        services.AddLogging(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Information));
        services.AddHttpClient();
        services.AddKernel().AddAzureOpenAIChatCompletion(
                deploymentName: azureOpenAIChatDeploymentName,
                endpoint: azureOpenAIEndpoint,
                apiKey: azureOpenAIKey,
                modelId: azureOpenAIChatModelId);

        services.AddSingleton(sp => KernelPluginFactory.CreateFromType<TimePlugin>(serviceProvider: sp));
        services.AddSingleton(sp => KernelPluginFactory.CreateFromType<HttpPlugin>(serviceProvider: sp));

        Kernel kernel6 = services.BuildServiceProvider().GetRequiredService<Kernel>();

        return Task.CompletedTask;
    }
}
