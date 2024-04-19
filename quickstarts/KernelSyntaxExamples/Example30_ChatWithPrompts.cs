namespace KernelSyntaxExamples;

public class Example30_ChatWithPrompts(ITestOutputHelper output) : BaseTest(output)
{

    [Fact]
    public async Task RunAsync()
    {
        this.WriteLine("======== Chat with prompts ========");

        string systemPromptTemplate = EmbeddedResource.Read("30-system-prompt.txt");
        string selectedText = EmbeddedResource.Read("30-user-context.txt");
        string userPromptTemplate = EmbeddedResource.Read("30-user-prompt.txt");

        Kernel kernel = KernelHelper.AzureOpenAIChatCompletionKernelBuilder().Build();

        kernel.ImportPluginFromType<TimePlugin>("time");

        KernelArguments arguments = new()
        {
            ["selectedText"] = selectedText,
            ["startTime"] = DateTimeOffset.UtcNow.ToString("hh:mm:ss tt zz", CultureInfo.CurrentCulture),
            ["userMessage"] = "extract locations as a bullet point list"
        };

        KernelPromptTemplateFactory kernelPromptTemplateFactory = new();

        string systemMessage = await kernelPromptTemplateFactory.Create(new PromptTemplateConfig(systemPromptTemplate)).RenderAsync(kernel, arguments);
        this.WriteLine($"------------------------------------\n{systemMessage}");

        string userMessage = await kernelPromptTemplateFactory.Create(new PromptTemplateConfig(userPromptTemplate)).RenderAsync(kernel, arguments);
        this.WriteLine($"------------------------------------\n{userMessage}");

        IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        ChatHistory chatHistory = new(systemMessage);

        chatHistory.AddUserMessage(userMessage);

        ChatMessageContent answer = await chatCompletionService.GetChatMessageContentAsync(chatHistory);

        this.WriteLine($"------------------------------------\n{answer}");
    }
}
