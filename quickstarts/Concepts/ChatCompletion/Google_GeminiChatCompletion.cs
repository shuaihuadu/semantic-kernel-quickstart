namespace ChatCompletion;

public class Google_GeminiChatCompletion(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task GoogleAIAsync()
    {
        Console.WriteLine("============= Google AI - Gemini Chat Completion =============");

        Kernel kernel = Kernel.CreateBuilder()
            .AddGoogleAIGeminiChatCompletion(
                modelId: TestConfiguration.GoogleAI.Gemini.ModelId,
                apiKey: TestConfiguration.GoogleAI.ApiKey
            ).Build();

        await RunSampleAsync(kernel);
    }

    private async Task RunSampleAsync(Kernel kernel)
    {
        await SimpleChatAsync(kernel);
    }

    private async Task SimpleChatAsync(Kernel kernel)
    {
        Console.WriteLine("======== Simple Chat ========");

        ChatHistory chatHistory = [];

        IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        chatHistory.AddUserMessage("Hi, I'm looking for new power tools, any suggestion?");
        await MessageOutputAsync(chatHistory);

        ChatMessageContent reply = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
        chatHistory.Add(reply);
        await MessageOutputAsync(chatHistory);

        chatHistory.AddUserMessage("I'm looking for a drill, a screwdriver and a hammer.");
        await MessageOutputAsync(chatHistory);

        reply = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
        chatHistory.Add(reply);
        await MessageOutputAsync(chatHistory);
    }

    private Task MessageOutputAsync(ChatHistory chatHistory)
    {
        ChatMessageContent message = chatHistory.Last();

        Console.WriteLine($"{message.Role}: {message.Content}");
        Console.WriteLine("------------------------");

        return Task.CompletedTask;
    }
}
