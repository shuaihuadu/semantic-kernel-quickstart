namespace KernelSyntaxExamples;

public class Example36_MultiCompletion(ITestOutputHelper output) : BaseTest(output)
{
    private readonly ITestOutputHelper _output = output;

    [Fact]
    public Task AzureOpenAIMultiChatCompletionAsync()
    {
        WriteLine("======== Azure OpenAI - Multiple Chat Completion ========");

        IChatCompletionService chatCompletionService = new AzureOpenAIChatCompletionService(
            deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
            endpoint: TestConfiguration.AzureOpenAI.Endpoint,
            apiKey: TestConfiguration.AzureOpenAI.ApiKey,
            modelId: TestConfiguration.AzureOpenAI.DeploymentName);

        return ChatCompletionAsync(chatCompletionService);
    }

    private async Task ChatCompletionAsync(IChatCompletionService chatCompletionService)
    {
        OpenAIPromptExecutionSettings executionSettings = new OpenAIPromptExecutionSettings
        {
            MaxTokens = 200,
            FrequencyPenalty = 0,
            PresencePenalty = 0,
            Temperature = 1,
            TopP = 0.5,
            ResultsPerPrompt = 2
        };

        ChatHistory chatHistory = [];

        chatHistory.AddUserMessage("Write one paragraph about why AI is awesome");

        foreach (ChatMessageContent chatMessage in await chatCompletionService.GetChatMessageContentsAsync(chatHistory, executionSettings))
        {
            Write(chatMessage.Content ?? string.Empty);

            WriteLine("\n----------\n");
        }

        WriteLine();
    }
}
