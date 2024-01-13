namespace KernelSyntaxExamples;

public static class Example41_HttpClientUsage
{
    public static Task RunAsync()
    {
        UseDefaultHttpClient();

        UseCustomHttpClient();

        UseBasicRegistrationWithHttpClientFactory();

        UseNamedRegistrationWithHttpClientFactory();

        return Task.CompletedTask;
    }

    private static void UseDefaultHttpClient()
    {
        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                TestConfiguration.AzureOpenAI.DeploymentName,
                TestConfiguration.AzureOpenAI.Endpoint,
                TestConfiguration.AzureOpenAI.ApiKey)
            .Build();
    }

    private static void UseCustomHttpClient()
    {
        using HttpClient httpClient = new();

        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                TestConfiguration.AzureOpenAI.DeploymentName,
                TestConfiguration.AzureOpenAI.Endpoint,
                TestConfiguration.AzureOpenAI.ApiKey,
                httpClient: httpClient)
            .Build();
    }

    private static void UseBasicRegistrationWithHttpClientFactory()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddHttpClient();

        IServiceCollection kernelServices = services.AddTransient<Kernel>((sp) =>
        {
            IHttpClientFactory factory = sp.GetRequiredService<IHttpClientFactory>();

            return Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                TestConfiguration.AzureOpenAI.DeploymentName,
                TestConfiguration.AzureOpenAI.Endpoint,
                TestConfiguration.AzureOpenAI.ApiKey,
                httpClient: factory.CreateClient())
            .Build();
        });
    }

    private static void UseNamedRegistrationWithHttpClientFactory()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddHttpClient();

        services.AddHttpClient("test-client", (client) =>
        {
            client.BaseAddress = new Uri("https://api.openai.com/v1/", UriKind.Absolute);
        });

        IServiceCollection kernelServices = services.AddTransient<Kernel>((sp) =>
        {
            IHttpClientFactory factory = sp.GetRequiredService<IHttpClientFactory>();

            return Kernel.CreateBuilder()
                .AddOpenAIChatCompletion(
                    modelId: TestConfiguration.OpenAI.ChatModelId,
                    apiKey: TestConfiguration.OpenAI.ApiKey,
                    httpClient: factory.CreateClient("test-client"))
                .Build();
        });
    }
}
