namespace KernelSyntaxExamples;

public class Example63_ChatCompletionPrompts : BaseTest
{
    [Fact]
    public async Task RunAsync()
    {
        const string ChatPrompt = @"
            <message role=""user"">What is Seattle?</message>
            <message role=""system"">Respond with JSON.</message>
        ";

        Kernel kernel = KernelHelper.AzureOpenAIChatCompletionKernelBuilder().Build();

        KernelFunction chatSemanticFunction = kernel.CreateFunctionFromPrompt(ChatPrompt);

        FunctionResult chatPromptResult = await kernel.InvokeAsync(chatSemanticFunction);

        this.WriteLine("Chat Prompt:");
        this.WriteLine(ChatPrompt);
        this.WriteLine("Chat Prompt Result:");
        this.WriteLine(chatPromptResult.GetValue<string>());

        this.Write("Chat Prompt Streaming Result:");

        string completeMessage = string.Empty;

        await foreach (string message in kernel.InvokeStreamingAsync<string>(chatSemanticFunction))
        {
            completeMessage += message;

            this.Write(message);
        }

        this.WriteLine("---------- Streamed Content ----------");
        this.WriteLine(completeMessage);
    }

    public Example63_ChatCompletionPrompts(ITestOutputHelper output) : base(output)
    {
    }
}
