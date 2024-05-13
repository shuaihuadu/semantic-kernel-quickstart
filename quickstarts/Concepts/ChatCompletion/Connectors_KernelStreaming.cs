namespace ChatCompletion;

public class Connectors_KernelStreaming(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task RunAsync()
    {
        Kernel kernel = KernelHelper.AzureOpenAIChatCompletionKernelBuilder().Build();

        KernelFunction funnyParagraphFunction = kernel.CreateFunctionFromPrompt("Wtite a funny paragraph about streaming",
            new OpenAIPromptExecutionSettings
            {
                MaxTokens = 500,
                Temperature = 0.4,
                TopP = 1
            });

        bool roleDisplayed = false;

        Console.WriteLine("\n===  Prompt Function - Streaming ===\n");

        string fullContent = string.Empty;

        await foreach (var update in kernel.InvokeStreamingAsync<OpenAIStreamingChatMessageContent>(funnyParagraphFunction))
        {
            if (roleDisplayed && update.Role.HasValue)
            {
                Console.WriteLine($"Role: {update.Role}");
                fullContent += $"Role: {update.Role}\n";
                roleDisplayed = true;
            }

            if (update.Content is { Length: > 0 })
            {
                fullContent += update.Content;
                Console.Write(update.Content);
            }
        }

        Console.WriteLine("\n------  Streamed Content ------\n");
        Console.WriteLine(fullContent);
    }
}
