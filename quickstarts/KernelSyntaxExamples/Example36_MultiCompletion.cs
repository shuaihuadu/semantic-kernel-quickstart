namespace KernelSyntaxExamples;

public static class Example36_MultiCompletion
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

        await ChatCompletionAsync(chatCompletionService);
    }

    private static async Task ChatCompletionAsync(IChatCompletionService chatCompletionService)
    {
        OpenAIPromptExecutionSettings executionSettings = new OpenAIPromptExecutionSettings
        {
            MaxTokens = 200,
            FrequencyPenalty = 0,
            PresencePenalty = 0,
            Temperature = 1,
            TopP = 0.5,
            ResultsPerPrompt = 5
        };

        ChatHistory chatHistory = new();
        chatHistory.AddUserMessage("Write one paragraph about why AI is awesome");

        foreach (ChatMessageContent chatMessageChoice in await chatCompletionService.GetChatMessageContentsAsync(chatHistory, executionSettings))
        {
            Console.WriteLine(chatMessageChoice.Content);
            Console.WriteLine("\n----------------\n");
        }

        Console.WriteLine();
    }
}
