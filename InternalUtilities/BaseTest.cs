// Copyright (c) IdeaTech. All rights reserved.

public abstract class BaseTest
{
    protected readonly ILogger Logger;
    protected readonly ILoggerFactory LoggerFactory;

    protected string Model => TestConfiguration.AzureOpenAI.DeploymentName;

    protected Kernel CreateKernelWithChatCompletion()
    {
        var builder = Kernel.CreateBuilder();

        AddChatCompletionToKernel(builder);

        return builder.Build();
    }

    protected void AddChatCompletionToKernel(IKernelBuilder builder)
    {
        builder.AddAzureOpenAIChatCompletion(
            TestConfiguration.AzureOpenAI.DeploymentName,
            TestConfiguration.AzureOpenAI.Endpoint,
            TestConfiguration.AzureOpenAI.ApiKey);
    }

    protected BaseTest()
    {
        IConfigurationRoot configRoot = new ConfigurationBuilder()
            .AddJsonFile(@"D:\appsettings\semantic-kernel-quickstart.json", true)
            .Build();

        TestConfiguration.Initialize(configRoot);

        this.LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });

        Logger = LoggerFactory.CreateLogger<BaseTest>();
    }

    protected void OutputLastMessage(ChatHistory chatHistory)
    {
        ChatMessageContent message = chatHistory[^1];

        Console.WriteLine($"{message.Role}: {message.Content}");
        Console.WriteLine("------------------------");
    }
}
