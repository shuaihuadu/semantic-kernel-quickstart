namespace KernelSyntaxExamples.OwnerExamples;

public class Example005_FinishReason(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task RunAsync()
    {
        Kernel kernel = KernelHelper.AzureOpenAIChatCompletionKernelBuilder().Build();

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