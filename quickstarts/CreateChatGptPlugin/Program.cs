Env.LoadUserSecrets();

IKernelBuilder builder = Kernel.CreateBuilder();

builder.Services.AddAzureOpenAIChatCompletion(
    deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
    endpoint: TestConfiguration.AzureOpenAI.Endpoint,
    apiKey: TestConfiguration.AzureOpenAI.ApiKey);

Kernel kernel = builder.Build();

await kernel.ImportPluginFromOpenApiAsync("MathPlugin", new Uri("https://localhost:7161/swagger/v1/swagger.json")).ConfigureAwait(false);

ChatHistory history = [];

IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

while (true)
{
    Console.Write("User > ");

    history.AddUserMessage(Console.ReadLine()!);

    OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
    {
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
    };

    IAsyncEnumerable<StreamingChatMessageContent> result = chatCompletionService.GetStreamingChatMessageContentsAsync(history, executionSettings: openAIPromptExecutionSettings, kernel: kernel);

    string fullMessage = "";

    bool first = true;

    await foreach (StreamingChatMessageContent content in result)
    {
        if (content.Role.HasValue && first)
        {
            Console.Write("Assistant > ");

            first = false;
        }

        Console.Write(content.Content);

        fullMessage += content.Content;
    }

    Console.WriteLine();

    history.AddAssistantMessage(fullMessage);
}