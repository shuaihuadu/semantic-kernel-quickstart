namespace ChatCompletion;

public class OpenAI_ChatCompletionWithVision(ITestOutputHelper output) : BaseTest(output)
{
    //[Fact(Skip = "Microsoft.SemanticKernel.HttpOperationException : The SSL connection could not be established, see inner exception.")]
    [Fact]
    public async Task RunAsync()
    {
        const string ImageUri1 = "https://ailabpocdemo.blob.core.windows.net/ai-lab-images-poc-demo/1238715435310907443/1239583826439569480/60763b20-174f-4d1e-8eef-b9c9c15629d5/1715610214/2346d5a6-f496-4d8a-a719-a4d7461ca772.png";
        const string ImageUri2 = "https://ailabpocdemo.blob.core.windows.net/ai-lab-images-poc-demo/1238715435310907443/1239583826439569480/60763b20-174f-4d1e-8eef-b9c9c15629d5/1715610214/2346d5a6-f496-4d8a-a719-a4d7461ca772.png";

        Kernel kernel = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(
            deploymentName: TestConfiguration.AzureOpenAI.VisionDeploymentName,
            endpoint: TestConfiguration.AzureOpenAI.Endpoint,
            apiKey: TestConfiguration.AzureOpenAI.ApiKey).Build();

        IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();


        OpenAIPromptExecutionSettings chatExecutionSettings = new()
        {
            MaxTokens = 1024,
            Temperature = 1,
            TopP = 0.5,
            FrequencyPenalty = 0,
        };

        ChatHistory chatHistory = new("You are trained to interpret images about people and make responsible assumptions about them.");

        chatHistory.AddUserMessage(new ChatMessageContentItemCollection
        {
            new ImageContent(new Uri(ImageUri1)),
            new ImageContent(new Uri(ImageUri2))
        });
        await MessageOutputAsync(chatHistory);

        ChatMessageContent reply = await chatCompletionService.GetChatMessageContentAsync(chatHistory, chatExecutionSettings);

        chatHistory.AddAssistantMessage(reply.Content!);
        await MessageOutputAsync(chatHistory);

        chatHistory.AddUserMessage("两张图片有啥区别？");
        await MessageOutputAsync(chatHistory);

        reply = await chatCompletionService.GetChatMessageContentAsync(chatHistory, chatExecutionSettings);
        chatHistory.AddAssistantMessage(reply.Content!);

        await MessageOutputAsync(chatHistory);
    }

    private Task MessageOutputAsync(ChatHistory chatHistory)
    {
        var message = chatHistory.Last();

        if (message.Items is not null && message.Items!.Count > 1)
        {
            foreach (var item in message.Items)
            {
                if (item is ImageContent imageContent && imageContent.Uri is not null)
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
