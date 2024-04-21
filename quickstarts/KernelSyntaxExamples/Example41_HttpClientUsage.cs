namespace KernelSyntaxExamples;

public class Example41_HttpClientUsage(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public void UseDefaultHttpClient()
    {
        Kernel kernel = KernelHelper.AzureOpenAIChatCompletionKernelBuilder().Build();
    }

    [Fact]
    public void UseCustomHttpClient()
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

    [Fact]
    public void UseBasicRegistrationWithHttpClientFactory()
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

    [Fact]
    public void UseNamedRegistrationWithHttpClientFactory()
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
