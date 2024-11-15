using MCS.Library.AI.AzureOpenAI.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;

namespace MCS.Library.AI.AzureOpenAI;

[TestClass]
public class KernelBuilding_Tests
{
    [TestMethod]
    public async Task BuildKernelUsingServiceCollection()
    {
        IConfigurationRoot configRoot = new ConfigurationBuilder()
           .AddJsonFile(@"D:\appsettings\kernel_options.json", true)
           .Build();

        AzureOpenAIChatCompletionOptions azureOpenAICompletionOptions = new();
        configRoot.Bind(nameof(AzureOpenAIChatCompletionOptions), azureOpenAICompletionOptions);
        AzureOpenAIEmbeddingOptions azureOpenAIEmbeddingOptions = new();
        configRoot.Bind(nameof(AzureOpenAIEmbeddingOptions), azureOpenAIEmbeddingOptions);

        AzureOpenAIOptions completionOptionItem = azureOpenAICompletionOptions.GetEnabledService();
        AzureOpenAIOptions embeddingOptionItem = azureOpenAIEmbeddingOptions.GetEnabledService();

        IServiceCollection services = new ServiceCollection();

        services.AddLogging(c => c.AddConsole().SetMinimumLevel(LogLevel.Information))
            .AddHttpClient()
            .AddKernel()
            .AddAzureOpenAITextEmbeddingGeneration(deploymentName: embeddingOptionItem.DeploymentName, endpoint: embeddingOptionItem.Endpoint, apiKey: embeddingOptionItem.ApiKey)
            .AddAzureOpenAIChatCompletion(deploymentName: completionOptionItem.DeploymentName, endpoint: embeddingOptionItem.Endpoint, apiKey: embeddingOptionItem.ApiKey);

        services.AddSingleton<TextCompletionService>();
        services.AddSingleton<TextEmbeddingService>();

        //测试
        IServiceProvider serviceProvider = services.BuildServiceProvider();

        //获取Embeddings
        TextEmbeddingService embeddingService = serviceProvider.GetRequiredService<TextEmbeddingService>();

        ReadOnlyMemory<float> embeddings = await embeddingService.GenerateEmbeddingsAsync("Hello, World!");

        Assert.IsNotNull(embeddings);
        Assert.AreEqual(1536, embeddings.Length);

        //获取Completion
        TextCompletionService completionService = serviceProvider.GetRequiredService<TextCompletionService>();

        ChatHistory chatHistory = [];
        chatHistory.AddUserMessage("你好");

        string reply = await completionService.TalkAsync(chatHistory);

        Console.WriteLine(reply);
        Assert.IsNotNull(reply);
        Assert.IsTrue(reply.Length > 0);

        reply = await completionService.ChatAsync("QnA", "中国男足于当地时间11月14日晚在巴林里法客场与巴林队进行完18强赛第5轮比赛后，将连夜启程回国。全队计划将乘坐于当地时间15日凌晨0点30分起飞的班机离开巴林，随后飞赴18强赛第6轮主场对阵日本队比赛的赛地厦门市。预计全队将于北京时间15日下午1点半左右抵达目的地。如此安排，有利于帮助球队抓紧时间及时休整，并投入到新一轮比赛的备战之中。", "国足几点到的巴林？");

        Console.WriteLine(reply);
        Assert.IsNotNull(reply);
        Assert.IsTrue(reply.Length > 0);
    }
}
