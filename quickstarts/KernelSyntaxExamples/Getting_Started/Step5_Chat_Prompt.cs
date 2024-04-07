namespace KernelSyntaxExamples.GettingStart;

public class Step5_Chat_Prompt : BaseTest
{
    [Fact]
    public async Task RunAsync()
    {
        Kernel kernel = KernelHelper.AzureOpenAIChatCompletionKernelBuilder().Build();

        string chatPrompt = @"
<message role=""user"">What is Seattle?</message>
<message role=""system"">Respond with JSON.</message>
";

        FunctionResult result = await kernel.InvokePromptAsync(chatPrompt);

        WriteLine(result.ToString());
    }

    public Step5_Chat_Prompt(ITestOutputHelper output) : base(output)
    {
    }
}
