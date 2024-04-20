namespace KernelSyntaxExamples;

public class Example37_CompletionIdentity(ITestOutputHelper output) : BaseTest(output)
{
    private static readonly OpenAIPromptExecutionSettings executionSettings = new OpenAIPromptExecutionSettings
    {
        FrequencyPenalty = 0,
        PresencePenalty = 0,
        Temperature = 1,
        TopP = 0.5
    };

    public async Task CompletionIdentityAsync(bool withName)
    {
        WriteLine("======== Completion Identity ========");

        IChatCompletionService chatCompletionService = KernelHelper.CreateCompletionService();

        ChatHistory chatHistory = CreateHistory(withName);

        StreamingChatMessageContent[] content = await chatHistory.AddStreamingMessageAsync(chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory, executionSettings).Cast<OpenAIStreamingChatMessageContent>()).ToArrayAsync();

        WriteMessage();
    }

    private static ChatHistory CreateHistory(bool withName)
    {
        return
        [
            new ChatMessageContent(AuthorRole.System,"Write one paragraph in response to the user that rhymes") { AuthorName = withName ? "Echo" : null },
            new ChatMessageContent(AuthorRole.User,"Why is AI awesome"){ AuthorName = withName ? "Ralph": null }
        ];
    }
}
