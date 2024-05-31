namespace LocalModels;

public class HuggingFace_ChatCompletionWithTGI(ITestOutputHelper output) : BaseTest(output)
{
    [Fact(Skip = "Requires TGI (text generation inference) deployment")]
    public async Task RunTGI_ChatCompletionAsync()
    {
        WriteLine("\n======== HuggingFace - TGI Chat Completion ========\n");

        Uri endpoint = new Uri("http://localhost:8080");

        const string Model = "tgi";

        Kernel kernel = Kernel.CreateBuilder()
            .AddHuggingFaceChatCompletion(
                model: Model,
                apiKey: TestConfiguration.HuggingFace.ApiKey,
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

        Uri endpoint = new Uri("http://localhost:8080");

        const string Model = "tgi";


        Kernel kernel = Kernel.CreateBuilder()
            .AddHuggingFaceChatCompletion(
                model: Model,
                apiKey: TestConfiguration.HuggingFace.ApiKey,
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
