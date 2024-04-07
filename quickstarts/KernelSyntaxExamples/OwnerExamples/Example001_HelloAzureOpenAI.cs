namespace KernelSyntaxExamples.OwnerExamples;

public class Example001_HelloAzureOpenAI(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task RunAsync()
    {
        string prompt = "请帮我回答一下什么是OpenAI";

        Kernel kernel1 = KernelHelper.AzureOpenAIChatCompletionKernelBuilder().Build();

        IChatCompletionService chatCompletionService1 = kernel1.GetRequiredService<IChatCompletionService>();

        ChatMessageContent content1 = await chatCompletionService1.GetChatMessageContentAsync(prompt);

        WriteLine(TestConfiguration.AzureOpenAI.DeploymentName);
        WriteLine(content1.Metadata!["Usage"]!.AsJson());


        Kernel kernel2 = KernelHelper.AzureOpenAIChatCompletionKernelBuilder().Build();

        IChatCompletionService chatCompletionService2 = kernel2.GetRequiredService<IChatCompletionService>();

        ChatMessageContent content2 = await chatCompletionService2.GetChatMessageContentAsync(prompt);

        WriteLine(TestConfiguration.AzureOpenAI.DeploymentName);
        WriteLine(content2.Metadata!["Usage"]!.AsJson());


        Kernel kernel3 = KernelHelper.AzureOpenAIChatCompletionKernelBuilder().Build();

        IChatCompletionService chatCompletionService3 = kernel3.GetRequiredService<IChatCompletionService>();

        ChatMessageContent content3 = await chatCompletionService3.GetChatMessageContentAsync(prompt);

        WriteLine("gpt-4-8k");
        WriteLine(content3.Metadata!["Usage"]!.AsJson());


        Kernel kernel4 = KernelHelper.AzureOpenAIChatCompletionKernelBuilder().Build();

        IChatCompletionService chatCompletionService4 = kernel4.GetRequiredService<IChatCompletionService>();

        ChatMessageContent content4 = await chatCompletionService4.GetChatMessageContentAsync(prompt);

        WriteLine("gpt-4-turbo");
        WriteLine(content4.Metadata!["Usage"]!.AsJson());

    }
}
