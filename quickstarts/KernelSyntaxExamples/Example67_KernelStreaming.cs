namespace KernelSyntaxExamples;

public static class Example67_KernelStreaming
{
    public static async Task RunAsync()
    {
        string apiKey = TestConfiguration.AzureOpenAI.ApiKey;
        string deploymentName = TestConfiguration.AzureOpenAI.ChatDeploymentName;
        string endpoint = TestConfiguration.AzureOpenAI.Endpoint;

        if (deploymentName == null
            || endpoint == null
            || apiKey == null)
        {
            Console.WriteLine("AzureOpenAI endpoint, apiKey, or deploymentName not found. Skipping example.");
            return;
        }

        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: deploymentName,
                endpoint: endpoint,
                apiKey: apiKey)
            .Build();

        KernelFunction funnyParagraphFunction = kernel.CreateFunctionFromPrompt("Wtite a funny paragraph about streaming",
            new OpenAIPromptExecutionSettings
            {
                MaxTokens = 500,
                Temperature = 0.4,
                TopP = 1
            });

        bool roleDisplayed = false;

        Console.WriteLine("\n===  Prompt Function - Streaming ===\n");

        await foreach (var update in kernel.InvokeStreamingAsync<OpenAIStreamingChatMessageContent>(funnyParagraphFunction))
        {
            if (roleDisplayed && update.Role.HasValue)
            {
                Console.WriteLine($"Role: {update.Role}");
                roleDisplayed = true;
            }

            if (update.Content is { Length: > 0 })
            {
                Console.Write(update.Content);
            }
        }
    }
}
