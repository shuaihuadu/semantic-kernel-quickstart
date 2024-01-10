namespace KernelSyntaxExamples;

public static class Example15_TextMemoryPlugin
{
    private const string MemoryCollectionName = "aboutMe";

    public static async Task RunAsync(CancellationToken cancellationToken = default)
    {
        IMemoryStore store;

        store = new VolatileMemoryStore();

        await RunWithStoreAsync(store, cancellationToken);
    }

    private static async Task RunWithStoreAsync(IMemoryStore memoryStore, CancellationToken cancellationToken)
    {
        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                TestConfiguration.AzureOpenAI.DeploymentName,
                TestConfiguration.AzureOpenAI.Endpoint,
                TestConfiguration.AzureOpenAI.ApiKey)
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

        Console.WriteLine("== PART 1a: Saving Memories through the ISemanticTextMemory object ==");

        Console.WriteLine("Saving memory with key 'info1': \"My name is Andrea\"");

        await semanticTextMemory.SaveInformationAsync(MemoryCollectionName, id: "info1", text: "My name is SK", cancellationToken: cancellationToken);

        Console.WriteLine("Saving memory with key 'info2': \"I work as a tourist operator\"");
        await semanticTextMemory.SaveInformationAsync(MemoryCollectionName, id: "info2", text: "I work as a tourist operator", cancellationToken: cancellationToken);

        Console.WriteLine("Saving memory with key 'info3': \"I've been living in Seattle since 2005\"");
        await semanticTextMemory.SaveInformationAsync(MemoryCollectionName, id: "info3", text: "I've been living in Seattle since 2005", cancellationToken: cancellationToken);

        Console.WriteLine("Saving memory with key 'info4': \"I visited France and Italy five times since 2015\"");
        await semanticTextMemory.SaveInformationAsync(MemoryCollectionName, id: "info4", text: "I visited France and Italy five times since 2015", cancellationToken: cancellationToken);

        Console.WriteLine("== PART 1b: Retrieving Memories through the ISemanticTextMemory object ==");

        MemoryQueryResult? memoryQueryResult = await semanticTextMemory.GetAsync(MemoryCollectionName, "info1", cancellationToken: cancellationToken);

        Console.WriteLine("Memory with key 'info1':" + memoryQueryResult?.Metadata.Text ?? "ERROR: memory not found");

        Console.WriteLine("== PART 2: Search Memories through the ISemanticTextMemory object ==");

        string query = "France";

        Console.WriteLine("Query:{0}", query);

        IAsyncEnumerable<MemoryQueryResult> searchResults = semanticTextMemory.SearchAsync(MemoryCollectionName, query, 2);

        await foreach (MemoryQueryResult result in searchResults)
        {
            Console.WriteLine($"Result: {result.Metadata.Text}, Score: {result.Relevance}");
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
