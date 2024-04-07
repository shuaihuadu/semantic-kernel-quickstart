namespace KernelSyntaxExamples;

public class Example68_GPTVision : BaseTest
{
    [Fact]
    public async Task RunAsync()
    {
        const string ImageUri1 = "https://nimg.ws.126.net/?url=http%3A%2F%2Fdingyue.ws.126.net%2F2024%2F0208%2F26a8f4b0j00s8iho2000fd000g0009gp.jpg&thumbnail=660x2147483647&quality=80&type=jpg";
        const string ImageUri2 = "https://pics2.baidu.com/feed/42166d224f4a20a4b343e2fa7656ec2f720ed045.jpeg@f_auto?token=830b80889236bf1e4922dd1cf1b6bfd0";

        Kernel kernel = KernelHelper.AzureOpenAIChatCompletionKernelBuilder().Build();

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
                    this.WriteLine($"{imageContent.Uri!.AbsoluteUri}");
                }
                if (item is TextContent textContent)
                {
                    this.WriteLine($"{message.Role}: {textContent.Text}");
                }
            }
        }
        else
        {
            this.WriteLine($"{message.Role}: {message.Content}");
        }

        this.WriteLine("------------------------");

        return Task.CompletedTask;
    }

    public Example68_GPTVision(ITestOutputHelper output) : base(output)
    {
    }
}
