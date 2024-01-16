namespace KernelSyntaxExamples;

public static class Example68_GPTVision
{
    public static async Task RunAsync()
    {
        const string ImageUri = "https://img1.baidu.com/it/u=155773520,725746039&fm=253&fmt=auto&app=138&f=JPEG?w=800&h=500";

        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                TestConfiguration.AzureOpenAI.VisionDeploymentName,
                TestConfiguration.AzureOpenAI.Endpoint,
                TestConfiguration.AzureOpenAI.ApiKey)
            .Build();

        IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();


        OpenAIPromptExecutionSettings chatExecutionSettings = new()
        {
            MaxTokens = 1024,
            Temperature = 1,
            TopP = 0.5,
            FrequencyPenalty = 0,
        };

        ChatHistory chatHistory = new("You are a friendly assistant.");

        chatHistory.AddUserMessage(new ChatMessageContentItemCollection
        {
            new TextContent("What's in this image?"),
            new ImageContent(new Uri(ImageUri))
        });
        await MessageOutputAsync(chatHistory);

        ChatMessageContent reply = await chatCompletionService.GetChatMessageContentAsync(chatHistory, chatExecutionSettings);

        chatHistory.AddAssistantMessage(reply.Content!);
        await MessageOutputAsync(chatHistory);

        chatHistory.AddUserMessage("Is it a sheep?");
        await MessageOutputAsync(chatHistory);

        reply = await chatCompletionService.GetChatMessageContentAsync(chatHistory, chatExecutionSettings);
        chatHistory.AddAssistantMessage(reply.Content!);

        await MessageOutputAsync(chatHistory);
    }
    private static Task MessageOutputAsync(ChatHistory chatHistory)
    {
        var message = chatHistory.Last();

        if (message.Items is not null && message.Items!.Count > 1)
        {
            foreach (var item in message.Items)
            {
                if (item is ImageContent imageContent)
                {
                    Console.WriteLine($"{imageContent.Uri!.AbsoluteUri}");
                }
                if (item is TextContent textContent)
                {
                    Console.WriteLine($"{message.Role}: {textContent.Text}");
                }
            }
        }
        else
        {
            Console.WriteLine($"{message.Role}: {message.Content}");
        }

        Console.WriteLine("------------------------");

        return Task.CompletedTask;
    }
}
