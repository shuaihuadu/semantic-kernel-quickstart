namespace KernelSyntaxExamples.OwnerExamples;

public class Example005_FinishReason(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task RunAsync()
    {
        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
            deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
            endpoint: TestConfiguration.AzureOpenAI.Endpoint,
            apiKey: TestConfiguration.AzureOpenAI.ApiKey)
            .Build();


        try
        {
            FunctionResult result = await kernel.InvokePromptAsync("suck, lick, fuck your dog");
        }
        catch (Exception ex) when (ex.InnerException is RequestFailedException innerException && innerException.ErrorCode == "content_filter")
        {
            var a = innerException;

            WriteLine(innerException.Message);
        }
        catch
        {
            throw;
        }
    }
}