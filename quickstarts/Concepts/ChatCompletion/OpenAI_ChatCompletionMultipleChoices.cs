namespace ChatCompletion;

public class OpenAI_ChatCompletionMultipleChoices(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public Task AzureOpenAIMultiChatCompletionAsync()
    {
        Console.WriteLine("======== Azure OpenAI - Multiple Chat Completion ========");

        IChatCompletionService chatCompletionService = new AzureOpenAIChatCompletionService(
            deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
            endpoint: TestConfiguration.AzureOpenAI.Endpoint,
            apiKey: TestConfiguration.AzureOpenAI.ApiKey,
            modelId: TestConfiguration.AzureOpenAI.DeploymentName);

        return ChatCompletionAsync(chatCompletionService);
    }

    private async Task ChatCompletionAsync(IChatCompletionService chatCompletionService)
    {
        OpenAIPromptExecutionSettings executionSettings = new OpenAIPromptExecutionSettings
        {
            MaxTokens = 200,
            FrequencyPenalty = 0,
            PresencePenalty = 0,
            Temperature = 1,
            TopP = 0.5,
            ResultsPerPrompt = 2
        };

        ChatHistory chatHistory = [];

        chatHistory.AddUserMessage("Write one paragraph about why AI is awesome");

        foreach (ChatMessageContent chatMessage in await chatCompletionService.GetChatMessageContentsAsync(chatHistory, executionSettings))
        {
            Console.Write(chatMessage.Content ?? string.Empty);

            Console.WriteLine("\n----------\n");
        }

        Console.WriteLine();
    }

    private async Task RunChatAsync(IChatCompletionService chatCompletionService)
    {
        var chatHistory = new ChatHistory("You are a librarian, expert about books");

        chatHistory.AddUserMessage("Hi, I'm looking for book 3 different book suggestions about sci-fi");
        await MessageOutputAsync(chatHistory);

        var chatExecutionSettings = new OpenAIPromptExecutionSettings()
        {
            MaxTokens = 1024,
            ResultsPerPrompt = 2,
            Temperature = 1,
            TopP = 0.5,
            FrequencyPenalty = 0,
        };

        foreach (var chatMessageChoice in await chatCompletionService.GetChatMessageContentsAsync(chatHistory, chatExecutionSettings))
        {
            chatHistory.Add(chatMessageChoice!);
            await MessageOutputAsync(chatHistory);
        }

        this.WriteLine();
    }

    private Task MessageOutputAsync(ChatHistory chatHistory)
    {
        var message = chatHistory.Last();

        this.WriteLine($"{message.Role}: {message.Content}");
        this.WriteLine("------------------------");

        return Task.CompletedTask;
    }
}
