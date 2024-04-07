namespace KernelSyntaxExamples;

public class Example17_ChatGPT(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task RunAsync()
    {
        await AzureOpenAIChatSampleAsync();
    }

    private async Task AzureOpenAIChatSampleAsync()
    {
        this.WriteLine("======== Azure Open AI - ChatGPT ========");

        AzureOpenAIChatCompletionService chatCompletionService = new(
            TestConfiguration.AzureOpenAI.DeploymentName, TestConfiguration.AzureOpenAI.Endpoint,
            TestConfiguration.AzureOpenAI.ApiKey);

        await StartChatAsync(chatCompletionService);
    }

    private async Task StartChatAsync(IChatCompletionService chatCompletionService)
    {
        this.WriteLine("Chat content:");
        this.WriteLine("------------------------");

        var chatHistory = new ChatHistory("You are a librarian, expert about books");

        chatHistory.AddUserMessage("Hi, I'm looking for book suggestions");
        await MessageOutputAsync(chatHistory);

        ChatMessageContent reply = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
        chatHistory.Add(reply);
        await MessageOutputAsync(chatHistory);

        chatHistory.AddUserMessage("I love history and philosophy, I'd like to learn something new about Greece, any suggestion");
        await MessageOutputAsync(chatHistory);

        reply = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
        chatHistory.Add(reply);
        await MessageOutputAsync(chatHistory);
    }

    private Task MessageOutputAsync(ChatHistory chatHistory)
    {
        ChatMessageContent message = chatHistory.Last();

        this.WriteLine($"{message.Role}: {message.Content}");
        this.WriteLine("------------------------");

        return Task.CompletedTask;
    }
}
