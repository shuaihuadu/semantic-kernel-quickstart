


namespace KernelSyntaxExamples;

public static class Example45_MultiStreamingChatCompletion
{
    public static async Task RunAsync()
    {
        await AzureOpenAIMultiStreamingChatCompletionAsync();
    }

    private static async Task AzureOpenAIMultiStreamingChatCompletionAsync()
    {
        Console.WriteLine("======== Azure OpenAI - Multiple Chat Completions - Raw Streaming ========");

        AzureOpenAIChatCompletionService chatCompletionService = new(
            deploymentName: TestConfiguration.AzureOpenAI.ChatDeploymentName,
            endpoint: TestConfiguration.AzureOpenAI.Endpoint,
            apiKey: TestConfiguration.AzureOpenAI.ApiKey,
            modelId: TestConfiguration.AzureOpenAI.ChatModelId);

        await StreamingChatCompetionAsync(chatCompletionService, 3);
    }

    private static async Task StreamingChatCompetionAsync(IChatCompletionService chatCompletionService, int numResultsPerPrompt)
    {
        OpenAIPromptExecutionSettings executionSettings = new OpenAIPromptExecutionSettings()
        {
            MaxTokens = 200,
            FrequencyPenalty = 0,
            PresencePenalty = 0,
            Temperature = 1,
            TopP = 0.5,
            ResultsPerPrompt = numResultsPerPrompt
        };

        var consoleLinesPerResult = 10;

        ClearDisplayByAddingEmptyLines();

        string prompt = "Hi, I'm looking for 5 random title names for sci-fi books";

        await ProcessStreamAsyncEnumerableAsync(chatCompletionService, prompt, executionSettings, consoleLinesPerResult);

        Console.WriteLine();

        Console.SetCursorPosition(0, executionSettings.ResultsPerPrompt * consoleLinesPerResult);

        Console.WriteLine();
    }

    private static async Task ProcessStreamAsyncEnumerableAsync(IChatCompletionService chatCompletionService, string prompt, OpenAIPromptExecutionSettings executionSettings, int consoleLinesPerResult)
    {
        Dictionary<int, string> messagesPerChoice = new();

        ChatHistory chatHistory = new(prompt);

        await foreach (StreamingChatMessageContent chatUpdate in chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory, executionSettings))
        {
            Console.SetCursorPosition(0, chatUpdate.ChoiceIndex * consoleLinesPerResult + 1);

            if (!messagesPerChoice.ContainsKey(chatUpdate.ChoiceIndex))
            {
                messagesPerChoice[chatUpdate.ChoiceIndex] = $"Role: {chatUpdate.Role ?? new AuthorRole()}\n";
            }

            if (chatUpdate.Content is { Length: > 0 })
            {
                messagesPerChoice[chatUpdate.ChoiceIndex] += chatUpdate.Content;
            }

            Console.WriteLine(messagesPerChoice[chatUpdate.ChoiceIndex]);
        }
    }

    private static void ClearDisplayByAddingEmptyLines()
    {
        for (int i = 0; i < Console.WindowHeight - 2; i++)
        {
            Console.WriteLine();
        }
    }
}
