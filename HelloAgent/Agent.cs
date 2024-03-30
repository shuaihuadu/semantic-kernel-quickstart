namespace HelloAgent;

public class Agent
{
    public async Task RunAsync()
    {
        IKernelBuilder builder = Kernel.CreateBuilder();

        builder.Services.AddLogging(c => c.SetMinimumLevel(LogLevel.Trace).AddDebug());
        builder.Services.AddAzureOpenAIChatCompletion(
            deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
            endpoint: TestConfiguration.AzureOpenAI.Endpoint,
            apiKey: TestConfiguration.AzureOpenAI.ApiKey);

        builder.Plugins.AddFromType<AuthorEmailPlanner>();
        builder.Plugins.AddFromType<EmailPlugin>();

        Kernel kernel = builder.Build();

        IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        ChatHistory chatMessages = new("""
            You are a friendly assistant who likes to follow the rules. You will complete required steps
            and request approval before taking any consequential actions. If the user doesn't provide
            enough information for you to complete a task, you will keep asking questions until you have
            enough information to complete the task.
            """);

        while (true)
        {
            Console.Write("User > ");

            string userMessage = Console.ReadLine()!;

            chatMessages.AddUserMessage(userMessage);

            OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            IAsyncEnumerable<StreamingChatMessageContent> result = chatCompletionService.GetStreamingChatMessageContentsAsync(chatMessages, openAIPromptExecutionSettings, kernel);

            string fullMessage = "";

            await foreach (StreamingChatMessageContent content in result)
            {
                if (content.Role.HasValue)
                {
                    Console.Write($"Assistant > ");
                }

                Console.Write(content.ToString());

                fullMessage += content.ToString();
            }

            Console.WriteLine();

            chatMessages.AddAssistantMessage(fullMessage);
        }
    }
}