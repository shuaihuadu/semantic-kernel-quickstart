
namespace KernelSyntaxExamples;

public class Example67_KernelStreaming : BaseTest
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

        this.WriteLine("\n===  Prompt Function - Streaming ===\n");

        await foreach (var update in kernel.InvokeStreamingAsync<OpenAIStreamingChatMessageContent>(funnyParagraphFunction))
        {
            if (roleDisplayed && update.Role.HasValue)
            {
                this.WriteLine($"Role: {update.Role}");
                roleDisplayed = true;
            }

            if (update.Content is { Length: > 0 })
            {
                this.Write(update.Content);
            }
        }
    }

    public Example67_KernelStreaming(ITestOutputHelper output) : base(output)
    {
    }
}
