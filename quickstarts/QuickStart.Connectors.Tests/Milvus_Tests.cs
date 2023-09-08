namespace QuickStart.Connectors.Tests;

[TestClass]
public class Milvus_Tests : TestBase
{
    private MilvusMemoryStore? _memoryStore;
    private IMilvusDbClient? _milvusDbClient;
    private IKernel? _kernel;

    const string _milvusCollectionName = "milvus_unit_test";

    [TestInitialize]
    public void InitKernel()
    {
        _milvusDbClient = new MilvusDbClient("192.168.186.129");

        _memoryStore = new MilvusMemoryStore(_milvusDbClient);

        _kernel = Kernel.Builder
            .WithAzureTextEmbeddingGenerationService(QuickStartConfiguration.AzureOpenAIEmbeddingOptions.EmbeddingDeploymentName, QuickStartConfiguration.AzureOpenAIEmbeddingOptions.Endpoint, QuickStartConfiguration.AzureOpenAIEmbeddingOptions.ApiKey)
            .WithAzureChatCompletionService(QuickStartConfiguration.AzureOpenAIOptions.GPT35ModelDeploymentName, QuickStartConfiguration.AzureOpenAIOptions.Endpoint, QuickStartConfiguration.AzureOpenAIOptions.ApiKey)
            .WithMemoryStorage(_memoryStore)
            .Build();
    }

    [TestMethod]
    public async Task ItCanSaveInformationAsync()
    {
        var key1 = await _kernel!.Memory.SaveInformationAsync(_milvusCollectionName, id: "id1", text: "综合外媒报道，美国一位高级政府消息人士8日透露，白宫将在当地时间9日公布禁止美企在华某些先进行业投资敏感技术的细节。");
        var key2 = await _kernel!.Memory.SaveInformationAsync(_milvusCollectionName, id: "id2", text: "业内消息来源确认，高通良心的120Hz高刷新率屏幕这次将在所有iPhone 15型号上普及，而不再限于Pro系列。");
        var key3 = await _kernel!.Memory.SaveInformationAsync(_milvusCollectionName, id: "id3", text: "从9月开始，微信将全面实施一系列的新功能和新规则，这些改变将影响到你我的使用体验。");
        var key4 = await _kernel!.Memory.SaveInformationAsync(_milvusCollectionName, id: "id4", text: "自9月起，微信将不再将其手机号、微信号等个人资料提交给第三方平台或软件。");
        var key5 = await _kernel!.Memory.SaveInformationAsync(_milvusCollectionName, id: "id5", text: "搜狐体育消息,北京时间8月16日,前中国男足国脚徐亮近日在与网友互动中吐露了心声,直言他对中国足球以及青少年培养环境的失望。");

        Console.WriteLine(key1);
        Console.WriteLine(key2);
        Console.WriteLine(key3);
        Console.WriteLine(key4);
        Console.WriteLine(key5);
    }

    [TestMethod]
    public async Task ItCanSearchAsync()
    {
        var searchResult = await _kernel!.Memory.SearchAsync(_milvusCollectionName, "9月", 3).ToListAsync();

        foreach (var item in searchResult)
        {
            Console.WriteLine(item.Metadata.Text);
        }
    }
}
