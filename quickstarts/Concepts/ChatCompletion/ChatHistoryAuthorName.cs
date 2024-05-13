namespace ChatCompletion;

public class ChatHistoryAuthorName(ITestOutputHelper output) : BaseTest(output)
{
    private static readonly OpenAIPromptExecutionSettings executionSettings = new OpenAIPromptExecutionSettings
    {
        FrequencyPenalty = 0,
        PresencePenalty = 0,
        Temperature = 1,
        TopP = 0.5
    };

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CompletionIdentityAsync(bool withName)
    {
        WriteLine("======== Completion Identity ========");

        IChatCompletionService chatCompletionService = KernelHelper.CreateCompletionService();

        ChatHistory chatHistory = CreateHistory(withName);

        WriteMessages(chatHistory);

        WriteMessages(await chatCompletionService.GetChatMessageContentsAsync(chatHistory, executionSettings), chatHistory);

        ValidateMessages(chatHistory, withName);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task StreamingIdentityAsync(bool withName)
    {
        WriteLine("======== Completion Identity ========");

        IChatCompletionService chatCompletionService = KernelHelper.CreateCompletionService();

        ChatHistory chatHistory = CreateHistory(withName);

        StreamingChatMessageContent[] content = await chatHistory.AddStreamingMessageAsync(chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory, executionSettings).Cast<OpenAIStreamingChatMessageContent>()).ToArrayAsync();

        WriteMessages(chatHistory);

        ValidateMessages(chatHistory, withName);
    }

    private static ChatHistory CreateHistory(bool withName)
    {
        return
        [
            new ChatMessageContent(AuthorRole.System,"Write one paragraph in response to the user that rhymes") { AuthorName = withName ? "Echo" : null },
            new ChatMessageContent(AuthorRole.User,"Why is AI awesome"){ AuthorName = withName ? "Ralph": null }
        ];
    }

    private void ValidateMessages(ChatHistory chatHistory, bool expectName)
    {
        foreach (var message in chatHistory)
        {
            if (expectName && message.Role != AuthorRole.Assistant)
            {
                Assert.NotNull(message.AuthorName);
            }
            else
            {
                Assert.Null(message.AuthorName);
            }
        }
    }

    private void WriteMessages(IReadOnlyList<ChatMessageContent> messages, ChatHistory? history = null)
    {
        foreach (var message in messages)
        {
            WriteLine($"# {message.Role}:{message.AuthorName ?? "?"} - {message.Content ?? "-"}");
        }

        history?.AddRange(messages);
    }
}
