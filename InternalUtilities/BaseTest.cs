public abstract class BaseTest
{
    protected readonly ILogger Logger;

    protected Kernel CreateKernelWithChatCompletion()
    {
        var builder = Kernel.CreateBuilder();

        AddChatCompletionToKernel(builder);

        return builder.Build();
    }

    protected void AddChatCompletionToKernel(IKernelBuilder builder)
    {
        builder.AddAzureOpenAIChatCompletion(
            TestConfiguration.AzureOpenAI.ChatDeploymentName,
            TestConfiguration.AzureOpenAI.Endpoint,
            TestConfiguration.AzureOpenAI.ApiKey);
    }

    protected BaseTest()
    {
        IConfigurationRoot configRoot = new ConfigurationBuilder()
            .AddJsonFile(@"D:\appsettings\semantic-kernel-quickstart.json", true)
            .Build();

        TestConfiguration.Initialize(configRoot);

        ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });

        Logger = loggerFactory.CreateLogger<BaseTest>();
    }

    protected void OutputLastMessage(ChatHistory chatHistory)
    {
        ChatMessageContent message = chatHistory[^1];

        Console.WriteLine($"{message.Role}: {message.Content}");
        Console.WriteLine("------------------------");
    }
}
