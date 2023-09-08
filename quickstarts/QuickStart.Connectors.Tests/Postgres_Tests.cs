namespace QuickStart.Connectors.Tests;

[TestClass]
public class Postgres_Tests : TestBase
{
    private PostgresMemoryStore? _memoryStore;
    private IKernel? _kernel;

    const string _postgreCollectionName = "postgres-unit-test";

    [TestInitialize]
    public void InitKernel()
    {
        NpgsqlDataSourceBuilder dataSourceBuilder = new(QuickStartConfiguration.PostgresOptions.ConnectionString);

        dataSourceBuilder.UseVector();

        NpgsqlDataSource dataSource = dataSourceBuilder.Build();

        _memoryStore = new PostgresMemoryStore(dataSource, 1536);

        _kernel = Kernel.Builder
            .WithAzureTextEmbeddingGenerationService(QuickStartConfiguration.AzureOpenAIEmbeddingOptions.EmbeddingDeploymentName, QuickStartConfiguration.AzureOpenAIEmbeddingOptions.Endpoint, QuickStartConfiguration.AzureOpenAIEmbeddingOptions.ApiKey)
            .WithAzureChatCompletionService(QuickStartConfiguration.AzureOpenAIOptions.GPT35ModelDeploymentName, QuickStartConfiguration.AzureOpenAIOptions.Endpoint, QuickStartConfiguration.AzureOpenAIOptions.ApiKey)
            .WithMemoryStorage(_memoryStore)
            .Build();
    }

    [TestMethod]
    public async Task CreateCollection_Test()
    {
        var key = await _kernel!.Memory.SaveInformationAsync(_postgreCollectionName, id: "unittest1", text: "综合外媒报道，美国一位高级政府消息人士8日透露，白宫将在当地时间9日公布禁止美企在华某些先进行业投资敏感技术的细节。");

        Console.WriteLine(key);
    }

    [TestMethod]
    public async Task GetCollection_Test()
    {
        var collections = _memoryStore!.GetCollectionsAsync();

        await foreach (var collection in collections)
        {
            Console.WriteLine(collection);
        }
    }
}