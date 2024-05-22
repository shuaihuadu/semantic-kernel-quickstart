namespace TextToImage;

public class OpenAI_TextToImageDalle3(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task RunAsync()
    {
        await AzureOpenAIDallEAsync();
    }

    [Fact(Skip = "Generating the Image can take too long and often break the test")]
    public async Task AzureOpenAIDallEAsync()
    {
        this.WriteLine("========Azure OpenAI Dall-E 3 Text To Image ========");

        IKernelBuilder kernelBuilder = KernelHelper.AzureOpenAIChatCompletionKernelBuilder();

        kernelBuilder.AddAzureOpenAITextToImage(
            deploymentName: TestConfiguration.AzureOpenAIDalle3.DeploymentName,
            endpoint: TestConfiguration.AzureOpenAIDalle3.Endpoint,
            apiKey: TestConfiguration.AzureOpenAIDalle3.ApiKey,
            modelId: TestConfiguration.AzureOpenAIDalle3Config.ModelId,
            apiVersion: "2024-02-15-preview");

        //kernelBuilder.Services.ConfigureHttpClientDefaults(configure1 =>
        //{
        //    configure1.AddStandardResilienceHandler().Configure(configure2 =>
        //    {
        //        configure2.Retry.MaxRetryAttempts = 5;
        //    });
        //});

        Kernel kernel = kernelBuilder.Build();

        ITextToImageService dallE = kernel.GetRequiredService<ITextToImageService>();

        string imageDescription = "A cute baby sea otter";

        string image = await dallE.GenerateImageAsync(imageDescription, 1024, 1024);

        this.WriteLine(imageDescription);
        this.WriteLine("Image URL: " + image);

        this.WriteLine("======== Chat with images ========");

        IChatCompletionService completionService = kernel.GetRequiredService<IChatCompletionService>();

        ChatHistory chatHistory = new(
            "You're chatting with a user. Instead of replying directly to the user" +
            " provide the description of an image that expresses what you want to say." +
            " The user won't see your message, they will see only the image. The system " +
            " generates an image using your description, so it's important you describe the image with details.");

        string message = "Hi, I'm from Tokyo, where are you from?";
        chatHistory.AddUserMessage(message);
        this.WriteLine("User: " + message);

        ChatMessageContent reply = await completionService.GetChatMessageContentAsync(chatHistory);
        chatHistory.Add(reply);

        image = await dallE.GenerateImageAsync(reply.Content!, 1024, 1024);
        this.WriteLine("Bot: " + image);
        this.WriteLine("Img description: " + reply);

        message = "Oh, wow. Not sure where that is, could you provide more details?";
        chatHistory.AddUserMessage(message);
        this.WriteLine("User: " + message);

        reply = await completionService.GetChatMessageContentAsync(chatHistory);
        chatHistory.Add(reply);

        image = await dallE.GenerateImageAsync(reply.Content!, 1024, 1024);
        this.WriteLine("Bot: " + image);
        this.WriteLine("Img description: " + reply);
    }
}
