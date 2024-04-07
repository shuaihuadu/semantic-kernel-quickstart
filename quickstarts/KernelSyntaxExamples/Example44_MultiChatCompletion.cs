
namespace KernelSyntaxExamples;

public class Example44_MultiChatCompletion : BaseTest
{
    [Fact]
    public async Task AzureOpenAIMultipleChatCompletionAsync()
    {
        this.WriteLine("======== Azure OpenAI - Multiple Chat Completion ========");

        AzureOpenAIChatCompletionService chatCompletionService = new AzureOpenAIChatCompletionService(
            deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
            endpoint: TestConfiguration.AzureOpenAI.Endpoint,
            apiKey: TestConfiguration.AzureOpenAI.ApiKey,
            modelId: TestConfiguration.AzureOpenAIConfig.ModelId);

        await RunChatAsync(chatCompletionService);
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

    public Example44_MultiChatCompletion(ITestOutputHelper output) : base(output)
    {
    }
}
