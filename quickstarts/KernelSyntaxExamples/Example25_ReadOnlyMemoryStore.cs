namespace KernelSyntaxExamples;

public class Example25_ReadOnlyMemoryStore(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task RunAsync()
    {
        IMemoryStore store = new ReadOnlyMemoryStore(s_jsonVectorEntries);

        ReadOnlyMemory<float> embedding = new([1000, 20, 0.3F]);

        this.WriteLine("Reading data from custom read-only memory store");

        MemoryRecord? memoryRecord = await store.GetAsync("collection", "key3");

        if (memoryRecord != null)
        {
            this.WriteLine($"ID = {memoryRecord.Metadata.Id}, Embedding = {string.Join(", ", MemoryMarshal.ToEnumerable(memoryRecord.Embedding))}");
        }

        this.WriteLine($"Getting most similar vector to {string.Join(", ", MemoryMarshal.ToEnumerable(embedding))}");

        (MemoryRecord Record, double Score)? result = await store.GetNearestMatchAsync("collection", embedding, 0.0);

        if (result.HasValue)
        {
            this.WriteLine($"Embedding = {string.Join(", ", MemoryMarshal.ToEnumerable(result.Value.Item1.Embedding))}, Similarity = {result.Value.Item2}");
        }
    }

    private sealed class ReadOnlyMemoryStore : IMemoryStore
    {
        private readonly MemoryRecord[]? _memoryRecords = null;
        private readonly int _vectorSize = 3;

        public ReadOnlyMemoryStore(string valueString)
        {
            s_jsonVectorEntries = s_jsonVectorEntries.Replace("\n", string.Empty, StringComparison.Ordinal);
            s_jsonVectorEntries = s_jsonVectorEntries.Replace(" ", string.Empty, StringComparison.Ordinal);

            this._memoryRecords = JsonSerializer.Deserialize<MemoryRecord[]>(valueString);

            if (this._memoryRecords == null)
            {
                throw new Exception("Unable to deserialize memory records");
            }
        }

        public Task CreateCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DoesCollectionExistAsync(string collectionName, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<MemoryRecord?> GetAsync(string collectionName, string key, bool withEmbedding = false, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(this._memoryRecords?.FirstOrDefault(x => x.Key == key));
        }

        public async IAsyncEnumerable<MemoryRecord> GetBatchAsync(string collectionName, IEnumerable<string> keys, bool withEmbeddings = false, CancellationToken cancellationToken = default)
        {
            if (this._memoryRecords is not null)
            {
                foreach (var memoryRecord in this._memoryRecords)
                {
                    if (keys.Contains(memoryRecord.Key))
                    {
                        yield return memoryRecord;
                    }
                }
            }
        }

        public IAsyncEnumerable<string> GetCollectionsAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<(MemoryRecord, double)?> GetNearestMatchAsync(string collectionName, ReadOnlyMemory<float> embedding, double minRelevanceScore = 0, bool withEmbedding = false, CancellationToken cancellationToken = default)
        {
            await foreach (var item in this.GetNearestMatchesAsync(
                collectionName: collectionName,
                embedding: embedding,
                limit: 1,
                minRelevanceScore: minRelevanceScore,
                withEmbeddings: withEmbedding,
                cancellationToken: cancellationToken
                ).ConfigureAwait(false))
            {
                return item;
            };

            return default;
        }

        public async IAsyncEnumerable<(MemoryRecord, double)> GetNearestMatchesAsync(string collectionName, ReadOnlyMemory<float> embedding, int limit, double minRelevanceScore = 0, bool withEmbeddings = false, CancellationToken cancellationToken = default)
        {
            if (this._memoryRecords == null || this._memoryRecords.Length == 0)
            {
                yield break;
            }

            if (embedding.Length != this._vectorSize)
            {
                throw new Exception($"Embedding vector size {embedding.Length} does not match expected size of {this._vectorSize}");
            }

            List<(MemoryRecord Record, double Score)> embeddings = new();

            foreach (var item in this._memoryRecords)
            {
                double similarity = TensorPrimitives.CosineSimilarity(embedding.Span, item.Embedding.Span);

                if (similarity >= minRelevanceScore)
                {
                    embeddings.Add(new(item, similarity));
                }
            }

            foreach (var item in embeddings.OrderByDescending(x => x.Score).Take(limit))
            {
                yield return item;
            }
        }

        public Task RemoveAsync(string collectionName, string key, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task RemoveBatchAsync(string collectionName, IEnumerable<string> keys, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<string> UpsertAsync(string collectionName, MemoryRecord record, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<string> UpsertBatchAsync(string collectionName, IEnumerable<MemoryRecord> records, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    private static string s_jsonVectorEntries = @"[
        {
            ""embedding"": [0, 0, 0],
            ""metadata"": {
                ""is_reference"": false,
                ""external_source_name"": ""externalSourceName"",
                ""id"": ""Id1"",
                ""description"": ""description"",
                ""text"": ""text"",
                ""additional_metadata"" : ""value:""
            },
            ""key"": ""key1"",
            ""timestamp"": null
         },
         {
            ""embedding"": [0, 0, 10],
            ""metadata"": {
                ""is_reference"": false,
                ""external_source_name"": ""externalSourceName"",
                ""id"": ""Id2"",
                ""description"": ""description"",
                ""text"": ""text"",
                ""additional_metadata"" : ""value:""
            },
            ""key"": ""key2"",
            ""timestamp"": null
         },
         {
            ""embedding"": [1, 2, 3],
            ""metadata"": {
                ""is_reference"": false,
                ""external_source_name"": ""externalSourceName"",
                ""id"": ""Id3"",
                ""description"": ""description"",
                ""text"": ""text"",
                ""additional_metadata"" : ""value:""
            },
            ""key"": ""key3"",
            ""timestamp"": null
         },
         {
            ""embedding"": [-1, -2, -3],
            ""metadata"": {
                ""is_reference"": false,
                ""external_source_name"": ""externalSourceName"",
                ""id"": ""Id4"",
                ""description"": ""description"",
                ""text"": ""text"",
                ""additional_metadata"" : ""value:""
            },
            ""key"": ""key4"",
            ""timestamp"": null
         },
         {
            ""embedding"": [12, 8, 4],
            ""metadata"": {
                ""is_reference"": false,
                ""external_source_name"": ""externalSourceName"",
                ""id"": ""Id5"",
                ""description"": ""description"",
                ""text"": ""text"",
                ""additional_metadata"" : ""value:""
            },
            ""key"": ""key5"",
            ""timestamp"": null
        }
    ]";
}
