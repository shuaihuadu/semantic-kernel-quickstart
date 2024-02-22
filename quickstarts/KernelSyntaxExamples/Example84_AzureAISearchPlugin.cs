namespace KernelSyntaxExamples;

public class Example84_AzureAISearchPlugin : BaseTest
{

    [Fact(Skip = "Azure AI Search Configuration")]
    public async Task AzureAISearchPluginAsync()
    {
        Uri endpoint = new(TestConfiguration.AzureAISearch.Endpoint);
        AzureKeyCredential keyCredential = new(TestConfiguration.AzureAISearch.ApiKey);

        IKernelBuilder kernelBuilder = Kernel.CreateBuilder();

        kernelBuilder.Services.AddSingleton((_) => new SearchIndexClient(endpoint, keyCredential));

        kernelBuilder.Services.AddSingleton<IAzureAISearchService, AzureAISearchService>();

        kernelBuilder.AddAzureOpenAITextEmbeddingGeneration(
            deploymentName: TestConfiguration.AzureOpenAIEmbeddings.DeploymentName,
            endpoint: TestConfiguration.AzureOpenAIEmbeddings.Endpoint,
            apiKey: TestConfiguration.AzureOpenAIEmbeddings.ApiKey);

        kernelBuilder.AddAzureOpenAIChatCompletion(
            deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
            endpoint: TestConfiguration.AzureOpenAI.Endpoint,
            apiKey: TestConfiguration.AzureOpenAI.ApiKey);

        kernelBuilder.Plugins.AddFromType<AzureAISearchPlugin>();

        Kernel kernel = kernelBuilder.Build();

        FunctionResult result1 = await kernel.InvokePromptAsync("{{search 'David' collection='index-1'}} Who is David?");

        WriteLine(result1);

        KernelArguments arguments = new()
        {
            ["searchFields"] = JsonSerializer.Serialize(new List<string> { "vector" })
        };

        FunctionResult result2 = await kernel.InvokePromptAsync("{{search 'David' collection='index-2' searchFields=$searchFields}} Who is Elara?", arguments);

        WriteLine(result2);
    }
    public Example84_AzureAISearchPlugin(ITestOutputHelper output) : base(output)
    {
    }

    private sealed class IndexSchema
    {
        [JsonPropertyName("chunk_id")]
        public string ChunkId { get; set; } = string.Empty;

        [JsonPropertyName("parent_id")]
        public string ParentId { get; set; } = string.Empty;

        [JsonPropertyName("chunk")]
        public string Chunk { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("vector")]
        public ReadOnlyMemory<float> Vector { get; set; }
    }

    private sealed class AzureAISearchPlugin
    {
        private readonly ITextEmbeddingGenerationService _textEmbeddingGenerationService;
        private readonly IAzureAISearchService _azureAISearchService;

        public AzureAISearchPlugin(ITextEmbeddingGenerationService textEmbeddingGenerationService, IAzureAISearchService searchService)
        {
            this._textEmbeddingGenerationService = textEmbeddingGenerationService;
            this._azureAISearchService = searchService;
        }

        [KernelFunction("Search")]
        public async Task<string> SearchAsync(string query, string collection, List<string>? searchFields, CancellationToken cancellationToken = default)
        {
            ReadOnlyMemory<float> embedding = await this._textEmbeddingGenerationService.GenerateEmbeddingAsync(query, cancellationToken: cancellationToken);

            return await this._azureAISearchService.SearchAsync(collection, embedding, searchFields, cancellationToken) ?? string.Empty;
        }
    }

    private interface IAzureAISearchService
    {
        Task<string?> SearchAsync(string collectionName, ReadOnlyMemory<float> vector, List<string>? searchFields = null, CancellationToken cancellationToken = default);
    }

    private sealed class AzureAISearchService : IAzureAISearchService
    {
        private readonly List<string> _defaultVectorFields = ["vector"];

        private readonly SearchIndexClient _searchIndexClient;

        public AzureAISearchService(SearchIndexClient searchIndexClient)
        {
            this._searchIndexClient = searchIndexClient;
        }

        public async Task<string?> SearchAsync(string collectionName, ReadOnlyMemory<float> vector, List<string>? searchFields = null, CancellationToken cancellationToken = default)
        {
            SearchClient searchClient = this._searchIndexClient.GetSearchClient(collectionName);

            List<string> fields = searchFields is { Count: > 0 } ? searchFields : this._defaultVectorFields;

            VectorizedQuery vectorizedQuery = new(vector);
            fields.ForEach(field => vectorizedQuery.Fields.Add(field));

            SearchOptions searchOptions = new() { VectorSearch = new() { Queries = { vectorizedQuery } } };

            Response<SearchResults<IndexSchema>> response = await searchClient.SearchAsync<IndexSchema>(searchOptions, cancellationToken);

            List<IndexSchema> results = new();

            await foreach (SearchResult<IndexSchema> result in response.Value.GetResultsAsync())
            {
                results.Add(result.Document);
            }

            return results.FirstOrDefault()?.Chunk;
        }
    }
}