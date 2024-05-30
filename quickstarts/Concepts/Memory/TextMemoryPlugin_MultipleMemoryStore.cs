namespace Memory;

public class TextMemoryPlugin_MultipleMemoryStore(ITestOutputHelper output) : BaseTest(output)
{
    private const string MemoryCollectionName = "aboutMe";

    [Theory]
    [InlineData("Volatile")]
    [InlineData("AzureAISearch")]
    public async Task RunAsync(string provider)
    {
        //The bellow memorystore can be run on my PC:

        IMemoryStore store = provider switch
        {
            "AzureAISearch" => CreateSampleAzureAISearchMemoryStore(),
            _ => new VolatileMemoryStore(),
        };

        // Sqlite Memory Store - a file-based store that persists data in a Sqlite database
        // store = await CreateSampleSqliteMemoryStoreAsync();

        // Milvus Memory Store
        // store = CreateSampleMilvusMemoryStore();

        // Qdrant Memory Store - a store that persists data in a local or remote Qdrant database
        // store = CreateSampleQdrantMemoryStore();

        // Postgres Memory Store
        // store = CreateSamplePostgresMemoryStore();

        // DuckDB Memory Store - a file-based store that persists data in a DuckDB database
        //store = await CreateSampleDuckDbMemoryStoreAsync();

        // Chroma Memory Store
        //store = CreateSampleChromaMemoryStore();

        await RunWithStoreAsync(store);
    }

    private async Task RunWithStoreAsync(IMemoryStore memoryStore)
    {
        Kernel kernel =
            KernelHelper.AzureOpenAIChatCompletionKernelBuilder()
            .AddAzureOpenAITextEmbeddingGeneration(
                TestConfiguration.AzureOpenAIEmbeddings.DeploymentName,
                TestConfiguration.AzureOpenAIEmbeddings.Endpoint,
                TestConfiguration.AzureOpenAIEmbeddings.ApiKey)
            .Build();

        AzureOpenAITextEmbeddingGenerationService embeddingGenerationService = new(
            TestConfiguration.AzureOpenAIEmbeddings.DeploymentName,
            TestConfiguration.AzureOpenAIEmbeddings.Endpoint,
            TestConfiguration.AzureOpenAIEmbeddings.ApiKey);

        SemanticTextMemory semanticTextMemory = new(memoryStore, embeddingGenerationService);

        this.WriteLine("== PART 1a: Saving Memories through the ISemanticTextMemory object ==");

        this.WriteLine("Saving memory with key 'info1': \"My name is Andrea\"");

        await semanticTextMemory.SaveInformationAsync(MemoryCollectionName, id: "info1", text: "My name is SK");

        this.WriteLine("Saving memory with key 'info2': \"I work as a tourist operator\"");
        await semanticTextMemory.SaveInformationAsync(MemoryCollectionName, id: "info2", text: "I work as a tourist operator");

        this.WriteLine("Saving memory with key 'info3': \"I've been living in Seattle since 2005\"");
        await semanticTextMemory.SaveInformationAsync(MemoryCollectionName, id: "info3", text: "I've been living in Seattle since 2005");

        this.WriteLine("Saving memory with key 'info4': \"I visited France and Italy five times since 2015\"");
        await semanticTextMemory.SaveInformationAsync(MemoryCollectionName, id: "info4", text: "I visited France and Italy five times since 2015");

        this.WriteLine("== PART 1b: Retrieving Memories through the ISemanticTextMemory object ==");

        MemoryQueryResult? memoryQueryResult = await semanticTextMemory.GetAsync(MemoryCollectionName, "info1");

        this.WriteLine("Memory with key 'info1':" + memoryQueryResult?.Metadata.Text ?? "ERROR: memory not found");

        this.WriteLine("== PART 2: Search Memories through the ISemanticTextMemory object ==");

        string query = "France";

        this.WriteLine($"Query:{query}");

        IAsyncEnumerable<MemoryQueryResult> searchResults = semanticTextMemory.SearchAsync(MemoryCollectionName, query, 2);

        await foreach (MemoryQueryResult result in searchResults)
        {
            this.WriteLine($"Result: {result.Metadata.Text}, Score: {result.Relevance}");
        }
    }

    private static async Task<IMemoryStore> CreateSampleSqliteMemoryStoreAsync()
    {
        IMemoryStore store = await SqliteMemoryStore.ConnectAsync("memories.sqlite");

        return store;
    }

    private static async Task<IMemoryStore> CreateSampleDuckDbMemoryStoreAsync()
    {
        IMemoryStore store = await DuckDBMemoryStore.ConnectAsync("memories.duckdb");

        return store;
    }

    private static IMemoryStore CreateSampleMongoDBMemoryStore()
    {
        IMemoryStore store = new MongoDBMemoryStore(TestConfiguration.Mongo.ConnectionString, "memoryPluginExample");

        return store;
    }

    private static IMemoryStore CreateSampleAzureAISearchMemoryStore()
    {
        IMemoryStore store = new AzureAISearchMemoryStore(TestConfiguration.AzureAISearch.Endpoint, TestConfiguration.AzureAISearch.ApiKey);

        return store;
    }

    private static IMemoryStore CreateSampleChromaMemoryStore()
    {
        IMemoryStore store = new ChromaMemoryStore(TestConfiguration.Chroma.Endpoint, ConsoleLogger.LoggerFactory);

        return store;
    }

    private static IMemoryStore CreateSampleQdrantMemoryStore()
    {
        IMemoryStore store = new QdrantMemoryStore(TestConfiguration.Qdrant.Endpoint, 1536, ConsoleLogger.LoggerFactory);

        return store;
    }

    private static IMemoryStore CreateSamplePineconeMemoryStore()
    {
        IMemoryStore store = new PineconeMemoryStore(TestConfiguration.Pinecone.Environment, TestConfiguration.Pinecone.ApiKey, ConsoleLogger.LoggerFactory);

        return store;
    }

    private static IMemoryStore CreateSampleWeaviateMemoryStore()
    {
        IMemoryStore store = new WeaviateMemoryStore(TestConfiguration.Weaviate.Endpoint, TestConfiguration.Weaviate.ApiKey);

        return store;
    }

    private static async Task<IMemoryStore> CreateSampleRedisMemoryStoreAsync()
    {
        string configuration = TestConfiguration.Redis.Configuration;

        ConnectionMultiplexer connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(configuration);

        IDatabase database = connectionMultiplexer.GetDatabase();

        IMemoryStore store = new RedisMemoryStore(database, vectorSize: 1536);

        return store;
    }

    private static IMemoryStore CreateSamplePostgresMemoryStore()
    {
        NpgsqlDataSourceBuilder npgsqlDataSourceBuilder = new(TestConfiguration.Postgres.ConnectionString);

        npgsqlDataSourceBuilder.UseVector();

        NpgsqlDataSource dataSource = npgsqlDataSourceBuilder.Build();

        IMemoryStore store = new PostgresMemoryStore(dataSource, vectorSize: 1536, schema: "public");

        return store;
    }

    private static IMemoryStore CreateSampleKustoMemoryStore()
    {
        KustoConnectionStringBuilder? kustoConnectionStringBuilder = new KustoConnectionStringBuilder(TestConfiguration.Kusto.ConnectionString).WithAadUserPromptAuthentication();

        IMemoryStore store = new KustoMemoryStore(kustoConnectionStringBuilder, "MyDatabase");

        return store;
    }

    private static IMemoryStore CreateSampleMilvusMemoryStore()
    {
        IMemoryStore store = new MilvusMemoryStore(TestConfiguration.Milvus.Host, TestConfiguration.Milvus.Port);

        return store;
    }
}
