﻿namespace ChatCompletion;

public class OpenAI_ChatCompletionStreamingMultipleChoices(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task AzureOpenAIMultiStreamingChatCompletionAsync()
    {
        Console.WriteLine("======== Azure OpenAI - Multiple Chat Completions - Raw Streaming ========");

        AzureOpenAIChatCompletionService chatCompletionService = new(
            deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
            endpoint: TestConfiguration.AzureOpenAI.Endpoint,
            apiKey: TestConfiguration.AzureOpenAI.ApiKey,
            modelId: TestConfiguration.AzureOpenAIConfig.ModelId);

        await StreamingChatCompetionAsync(chatCompletionService, 3);
    }

    private async Task StreamingChatCompetionAsync(IChatCompletionService chatCompletionService, int numResultsPerPrompt)
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

        string prompt = "Hi, I'm looking for 5 random title names for sci-fi books";

        await ProcessStreamAsyncEnumerableAsync(chatCompletionService, prompt, executionSettings, consoleLinesPerResult);

        Console.WriteLine();
    }

    private async Task ProcessStreamAsyncEnumerableAsync(IChatCompletionService chatCompletionService, string prompt, OpenAIPromptExecutionSettings executionSettings, int consoleLinesPerResult)
    {
        Dictionary<int, string> messagesPerChoice = new();

        ChatHistory chatHistory = new(prompt);

        await foreach (StreamingChatMessageContent chatUpdate in chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory, executionSettings))
        {
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
}
