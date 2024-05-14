namespace DependencyInjection;

public class HttpClient_Registration(ITestOutputHelper output) : BaseTest(output)
{
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
