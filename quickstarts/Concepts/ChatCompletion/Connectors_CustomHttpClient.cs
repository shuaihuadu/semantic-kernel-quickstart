namespace ChatCompletion;

public class Connectors_CustomHttpClient(ITestOutputHelper output) : BaseTest(output)
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

}
