namespace LocalModels;

public class HuggingFace_ChatCompletionWithTGI(ITestOutputHelper output) : BaseTest(output)
{

    [Fact(Skip = "Requires TGI (text generation inference) deployment")]
    public async Task RunTGI_ChatCompletionAsync()
    {
        WriteLine("\n======== HuggingFace - TGI Chat Completion ========\n");

        Uri endpoint = new Uri("http://localhost:8000");

        const string Model = "teknium/OpenHermes-2.5-Mistral-7B";

        Kernel kernel = Kernel.CreateBuilder()
            .AddHuggingFaceChatCompletion(
                model: Model,
                endpoint: endpoint)
            .Build();

        IChatCompletionService chatCompletion = kernel.GetRequiredService<IChatCompletionService>();

        ChatHistory chatHistory = new("You are a helpful assistant.")
        {
            new ChatMessageContent(AuthorRole.User,"What is deep learning?")
        };

        ChatMessageContent result = await chatCompletion.GetChatMessageContentAsync(chatHistory);

        WriteLine(result.Role);
        WriteLine(result.Content);
    }

    [Fact(Skip = "Requires TGI (text generation inference) deployment")]
    public async Task RunTGI_StreamingChatCompletionAsync()
    {
        WriteLine("\n======== HuggingFace - TGI Chat Completion Streaming ========\n");

        Uri endpoint = new Uri("http://localhost:8000");

        const string Model = "teknium/OpenHermes-2.5-Mistral-7B";

        Kernel kernel = Kernel.CreateBuilder()
            .AddHuggingFaceChatCompletion(
                model: Model,
                endpoint: endpoint)
            .Build();

        IChatCompletionService chatCompletion = kernel.GetRequiredService<IChatCompletionService>();

        ChatHistory chatHistory = new("You are a helpful assistant.")
            {
                new ChatMessageContent(AuthorRole.User,"What is deep learning?")
            };

        AuthorRole? role = null;

        await foreach (StreamingChatMessageContent chatMessageChunk in chatCompletion.GetStreamingChatMessageContentsAsync(chatHistory))
        {
            if (role is null)
            {
                role = chatMessageChunk.Role;

                Write(role);
            }

            Write(chatMessageChunk.Content);
        }
    }
}
