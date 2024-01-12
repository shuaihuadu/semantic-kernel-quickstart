namespace KernelSyntaxExamples;

public static class Example32_StreamingCompletion
{
    public static async Task RunAsync()
    {
        //text-davinci-003 已弃用

        //await AzureOpenAITextGenerationStreamAsync();

        await OpenAITextGenerationStreamAsync();
    }

    private static async Task AzureOpenAITextGenerationStreamAsync()
    {
        Console.WriteLine("======== Azure OpenAI - Text Completion - Raw Streaming ========");

        IChatCompletionService chatCompletionService = new AzureOpenAIChatCompletionService(
            deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
            endpoint: TestConfiguration.AzureOpenAI.Endpoint,
            apiKey: TestConfiguration.AzureOpenAI.ApiKey,
            modelId: TestConfiguration.AzureOpenAI.ModelId);

        await TextGenerationStreamAsync(chatCompletionService);
    }

    private static async Task OpenAITextGenerationStreamAsync()
    {
        Console.WriteLine("======== Open AI - Text Completion - Raw Streaming ========");

        IChatCompletionService chatCompletionService = new OpenAIChatCompletionService("gpt-4", TestConfiguration.OpenAI.ApiKey);

        await TextGenerationStreamAsync(chatCompletionService);
    }

    private static async Task TextGenerationStreamAsync(IChatCompletionService chatCompletionService)
    {
        OpenAIPromptExecutionSettings executionSettings = new()
        {
            MaxTokens = 100,
            FrequencyPenalty = 0,
            PresencePenalty = 0,
            Temperature = 1,
            TopP = 0.5
        };

        string prompt = "Write one paragraph why AI is awesome";

        Console.WriteLine($"Prompt: {prompt}");

        await foreach (var content in chatCompletionService.GetStreamingChatMessageContentsAsync(prompt, executionSettings))
        {
            Console.Write(content);
        }

        Console.WriteLine();
    }
}
