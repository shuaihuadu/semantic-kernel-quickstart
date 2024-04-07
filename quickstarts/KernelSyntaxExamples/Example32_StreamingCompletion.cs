namespace KernelSyntaxExamples;

public class Example32_StreamingCompletion(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task RunAsync()
    {
        //text-davinci-003 已弃用

        await AzureOpenAITextGenerationStreamAsync();

        //await OpenAITextGenerationStreamAsync();
    }

    private async Task AzureOpenAITextGenerationStreamAsync()
    {
        this.WriteLine("======== Azure OpenAI - Text Completion - Raw Streaming ========");

        IChatCompletionService chatCompletionService = new AzureOpenAIChatCompletionService(
            deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
            endpoint: TestConfiguration.AzureOpenAI.Endpoint,
            apiKey: TestConfiguration.AzureOpenAI.ApiKey,
            modelId: TestConfiguration.AzureOpenAIConfig.ModelId);

        await TextGenerationStreamAsync(chatCompletionService);
    }

    private async Task OpenAITextGenerationStreamAsync()
    {
        this.WriteLine("======== Open AI - Text Completion - Raw Streaming ========");

        IChatCompletionService chatCompletionService = new OpenAIChatCompletionService("gpt-4", TestConfiguration.OpenAI.ApiKey);

        await TextGenerationStreamAsync(chatCompletionService);
    }

    private async Task TextGenerationStreamAsync(IChatCompletionService chatCompletionService)
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

        this.WriteLine($"Prompt: {prompt}");

        await foreach (var content in chatCompletionService.GetStreamingChatMessageContentsAsync(prompt, executionSettings))
        {
            this.Write(content);
        }

        this.WriteLine();
    }
}
