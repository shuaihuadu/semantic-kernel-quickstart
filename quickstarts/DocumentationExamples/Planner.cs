namespace DocumentationExamples;

public class Planner(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task RunAsync()
    {
        WriteLine("======== Planner ========");

        IKernelBuilder builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
            deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
            endpoint: TestConfiguration.AzureOpenAI.Endpoint,
            apiKey: TestConfiguration.AzureOpenAI.ApiKey);

        builder.Plugins.AddFromType<MathSolver>();
    }
}
