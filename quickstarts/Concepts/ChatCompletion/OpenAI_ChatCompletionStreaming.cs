namespace ChatCompletion;

public class OpenAI_ChatCompletionStreaming(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task RunAsync()
    {
        await AzureOpenAIChatStreamSampleAsync();
    }

    private async Task AzureOpenAIChatStreamSampleAsync()
    {
        Console.WriteLine("======== Azure Open AI - ChatGPT Streaming ========");

        AzureOpenAIChatCompletionService chatCompletionService = new(
            deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
            endpoint: TestConfiguration.AzureOpenAI.Endpoint,
            apiKey: TestConfiguration.AzureOpenAI.ApiKey,
            modelId: TestConfiguration.AzureOpenAIConfig.ModelId);

        await StartStreamingChatAsync(chatCompletionService);
    }

    private async Task StartStreamingChatAsync(AzureOpenAIChatCompletionService chatCompletionService)
    {
        Console.WriteLine("Chat content:");
        Console.WriteLine("------------------------");

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
                Console.Write($"{chatUpdate.Role.Value}: {chatUpdate.Content}");

                roleWritten = true;
            }

            if (chatUpdate.Content is { Length: > 0 })
            {
                fullMessage += chatUpdate.Content;

                Console.Write(chatUpdate.Content);
            }
        }

        Console.WriteLine("\n------------------------");

        chatHistory.AddMessage(authorRole, fullMessage);
    }

    private Task MessageOutputAsync(ChatHistory chatHistory)
    {
        ChatMessageContent message = chatHistory.Last();

        Console.WriteLine($"{message.Role}: {message.Content}");
        Console.WriteLine("------------------------");

        return Task.CompletedTask;
    }
}
