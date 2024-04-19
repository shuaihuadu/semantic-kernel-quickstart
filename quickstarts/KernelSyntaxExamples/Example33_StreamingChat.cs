namespace KernelSyntaxExamples;

public class Example33_StreamingChat(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task RunAsync()
    {
        await AzureOpenAIChatStreamSampleAsync();
    }

    private async Task AzureOpenAIChatStreamSampleAsync()
    {
        this.WriteLine("======== Azure Open AI - ChatGPT Streaming ========");

        AzureOpenAIChatCompletionService chatCompletionService = new(
            deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
            endpoint: TestConfiguration.AzureOpenAI.Endpoint,
            apiKey: TestConfiguration.AzureOpenAI.ApiKey,
            modelId: TestConfiguration.AzureOpenAIConfig.ModelId);

        await StartStreamingChatAsync(chatCompletionService);
    }

    private async Task StartStreamingChatAsync(AzureOpenAIChatCompletionService chatCompletionService)
    {
        this.WriteLine("Chat content:");
        this.WriteLine("------------------------");

        ChatHistory chatHistory = new("You are a librarian, expert about books");
        await MessageOutputAsync(chatHistory);

        chatHistory.AddUserMessage("Hi, I'm looking for book suggestions");
        await MessageOutputAsync(chatHistory);

        await StreamMessageOutputAsync(chatCompletionService, chatHistory, AuthorRole.Assistant);

        chatHistory.AddUserMessage("I love history and philosophy, I'd like to learn something new about Greece, any suggestion?");
        await MessageOutputAsync(chatHistory);

        await StreamMessageOutputAsync(chatCompletionService, chatHistory, AuthorRole.Assistant);
    }

    private async Task StreamMessageOutputAsync(IChatCompletionService chatCompletionService, ChatHistory chatHistory, AuthorRole authorRole)
    {
        bool roleWritten = false;

        string fullMessage = string.Empty;

        await foreach (var chatUpdate in chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory))
        {
            if (!roleWritten && chatUpdate.Role.HasValue)
            {
                this.Write($"{chatUpdate.Role.Value}: {chatUpdate.Content}");

                roleWritten = true;
            }

            if (chatUpdate.Content is { Length: > 0 })
            {
                fullMessage += chatUpdate.Content;

                this.Write(chatUpdate.Content);
            }
        }

        this.WriteLine("\n------------------------");

        chatHistory.AddMessage(authorRole, fullMessage);
    }

    private Task MessageOutputAsync(ChatHistory chatHistory)
    {
        ChatMessageContent message = chatHistory.Last();

        this.WriteLine($"{message.Role}: {message.Content}");
        this.WriteLine("------------------------");

        return Task.CompletedTask;
    }
}
