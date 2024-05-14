namespace ChatCompletion;

public class OpenAI_UsingLogitBias(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task RunAsync()
    {
        IChatCompletionService chatCompletionService = new AzureOpenAIChatCompletionService(
            TestConfiguration.AzureOpenAI.DeploymentName,
            TestConfiguration.AzureOpenAI.Endpoint,
            TestConfiguration.AzureOpenAI.ApiKey);

        var keys = new[] { 3919, 626, 17201, 1300, 25782, 9800, 32016, 13571, 43582, 20189, 1891, 10424, 9631, 16497, 12984, 20020, 24046, 13159, 805, 15817, 5239, 2070, 13466, 32932, 8095, 1351, 25323 };

        OpenAIPromptExecutionSettings executionSettings = new()
        {
            TokenSelectionBiases = keys.ToDictionary(key => key, key => -100)
        };

        Console.WriteLine("Chat content:");
        Console.WriteLine("------------------------");

        ChatHistory chatHistory = new("You are a librarian expert");

        chatHistory.AddUserMessage("Hi, I'm looking some suggestions");
        await MessageOutputAsync(chatHistory);

        ChatMessageContent replyMessage = await chatCompletionService.GetChatMessageContentAsync(chatHistory, executionSettings);
        chatHistory.AddAssistantMessage(replyMessage.Content!);
        await MessageOutputAsync(chatHistory);

        chatHistory.AddUserMessage("I love history and philosophy, I'd like to learn something new about Greece, any suggestion");
        await MessageOutputAsync(chatHistory);

        replyMessage = await chatCompletionService.GetChatMessageContentAsync(chatHistory, executionSettings);
        chatHistory.AddAssistantMessage(replyMessage.Content!);

        await MessageOutputAsync(chatHistory);
    }

    private Task MessageOutputAsync(ChatHistory chatHistory)
    {
        var message = chatHistory.Last();

        Console.WriteLine($"{message.Role}: {message.Content}");
        Console.WriteLine("------------------------");

        return Task.CompletedTask;
    }
}
