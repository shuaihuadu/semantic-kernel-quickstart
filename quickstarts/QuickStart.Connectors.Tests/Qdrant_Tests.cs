namespace QuickStart.Connectors.Tests;

[TestClass]
public class Qdrant_Tests
{
    private QdrantMemoryStore? _memoryStore;
    private IKernel? _kernel;

    const string _qdrantCollectionName = "qdrant-unit-test";

    [TestInitialize]
    public void InitKernel()
    {
        _memoryStore = new("", 1536);

        _kernel = Kernel.Builder
            .WithAzureOpenAITextEmbeddingGenerationService(QuickStartConfiguration.AzureOpenAIEmbeddings.DeploymentName, QuickStartConfiguration.AzureOpenAIEmbeddings.Endpoint, QuickStartConfiguration.AzureOpenAIEmbeddings.ApiKey)
            .WithAzureOpenAIChatCompletionService(QuickStartConfiguration.AzureOpenAI.ChatDeploymentName, QuickStartConfiguration.AzureOpenAI.Endpoint, QuickStartConfiguration.AzureOpenAI.ApiKey)
            .WithMemoryStorage(_memoryStore)
            .Build();
    }

    [TestMethod]
    public async Task CreateCollection_Test()
    {
        var key1 = await _kernel!.Memory.SaveInformationAsync(_qdrantCollectionName, id: "unittest1", text: "�����塷��������������");
        var key2 = await _kernel.Memory.SaveInformationAsync(_qdrantCollectionName, id: "unittest2", text: "�뵼���Ǻ�����֧����ҵ");
        var key3 = await _kernel.Memory.SaveInformationAsync(_qdrantCollectionName, id: "unittest3", text: "iPhone 15���ڽ���9��22���������У��ù�˾�ƻ���9��12�ջ�9��13�վ�����Ʒ�����ᡣ");
        var key4 = await _kernel.Memory.SaveInformationAsync(_qdrantCollectionName, id: "unittest4", text: "�ο���Ϣ��8��8�ձ������ɶ����˻ἴ����Ļ�������˶�Ա���λ������Ա��������֯�����������ۡ�");
        var key5 = await _kernel.Memory.SaveInformationAsync(_qdrantCollectionName, id: "unittest5", text: "The game player is Sky.");

        Console.WriteLine(key1);
        Console.WriteLine(key2);
        Console.WriteLine(key3);
        Console.WriteLine(key4);
        Console.WriteLine(key5);
    }

    [TestMethod]
    public async Task GetQdrantPoints_Test()
    {
        var key = "0f52785a-0705-4f63-905f-e7db212eb0b8";

        //var memoryRecord = await _memoryStore!.GetWithPointIdAsync(_qdrantCollectionName, key);

        var memoryRecord = await _kernel!.Memory.GetAsync(_qdrantCollectionName, "unittest1");

        Console.WriteLine(memoryRecord!.Metadata.Text);
    }

    [TestMethod]
    public async Task Search_Test()
    {
        var query = "������֧����ҵ��ʲô��";

        var searchResults = _kernel!.Memory.SearchAsync(_qdrantCollectionName, query, limit: 5, minRelevanceScore: 0.8);

        await foreach (var item in searchResults)
        {
            Console.WriteLine(item.Metadata.Text + Environment.NewLine + item.Relevance);
        }
    }

    [TestMethod]
    public async Task DeleteCollection_Test()
    {
        await Task.CompletedTask;
        //await _memoryStore!.DeleteCollectionAsync(_qdrantCollectionName);
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