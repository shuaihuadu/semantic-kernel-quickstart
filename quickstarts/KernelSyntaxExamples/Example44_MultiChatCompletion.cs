namespace KernelSyntaxExamples;

public static class Example44_MultiChatCompletion
{
    public static async Task RunAsync()
    {
        await AzureOpenAIMultipleChatCompletionAsync();
    }

    private static async Task AzureOpenAIMultipleChatCompletionAsync()
    {
        Console.WriteLine("======== Azure OpenAI - Multiple Chat Completion ========");

        AzureOpenAIChatCompletionService chatCompletionService = new AzureOpenAIChatCompletionService(
            deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
            endpoint: TestConfiguration.AzureOpenAI.Endpoint,
            apiKey: TestConfiguration.AzureOpenAI.ApiKey,
            modelId: TestConfiguration.AzureOpenAI.ChatModelId);

        await RunChatAsync(chatCompletionService);
    }

    private static async Task RunChatAsync(IChatCompletionService chatCompletionService)
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

        Console.WriteLine();
    }

    private static Task MessageOutputAsync(ChatHistory chatHistory)
    {
        var message = chatHistory.Last();

        Console.WriteLine($"{message.Role}: {message.Content}");
        Console.WriteLine("------------------------");

        return Task.CompletedTask;
    }
}
