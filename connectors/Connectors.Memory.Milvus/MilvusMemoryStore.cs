namespace Microsoft.SemanticKernel.Connectors.Memory.Milvus;

public class MilvusMemoryStore : IMemoryStore
{
    private readonly IMilvusDbClient _milvusDbClient;
    private readonly ILogger _logger;


    //Default field values
    public const string ID_FIELD = "id";
    public const string EMBEDDING_FIELD = "embedding";

    public MilvusMemoryStore(IMilvusDbClient milvusDbClient, ILoggerFactory? loggerFactory = null)
    {
        this._milvusDbClient = milvusDbClient;
        this._logger = loggerFactory is not null ? loggerFactory.CreateLogger(nameof(MilvusMemoryStore)) : NullLogger.Instance;
    }

    /// <inheritdoc/>
    public async Task CreateCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        if (!await this._milvusDbClient.DoesCollectionExistAsync(collectionName, cancellationToken).ConfigureAwait(false))
        {
            await this._milvusDbClient.CreateCollectionAsync(collectionName, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public async Task DeleteCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        if (await this._milvusDbClient.DoesCollectionExistAsync(collectionName).ConfigureAwait(false))
        {
            await this._milvusDbClient.DeleteCollectionAsync(collectionName, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DoesCollectionExistAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        return await this._milvusDbClient.DoesCollectionExistAsync(collectionName, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<MemoryRecord?> GetAsync(string collectionName, string key, bool withEmbedding = false, CancellationToken cancellationToken = default)
    {
        var result = await this._milvusDbClient.GetFieldDataByIdsAsync(collectionName, new[] { key }, withEmbedding, cancellationToken);

        return result.FirstOrDefault();
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<MemoryRecord> GetBatchAsync(string collectionName, IEnumerable<string> keys, bool withEmbeddings = false, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var result = await this._milvusDbClient.GetFieldDataByIdsAsync(collectionName, keys, withEmbeddings, cancellationToken);

        foreach (var item in result)
        {
            yield return item;
        }
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<string> GetCollectionsAsync(CancellationToken cancellationToken = default)
    {
        return this._milvusDbClient.ListCollectionsAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<(MemoryRecord, double)?> GetNearestMatchAsync(string collectionName, ReadOnlyMemory<float> embedding, double minRelevanceScore = 0, bool withEmbedding = false, CancellationToken cancellationToken = default)
    {
        return await GetNearestMatchesAsync(collectionName, embedding, 1, minRelevanceScore, withEmbedding, cancellationToken).FirstOrDefaultAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<(MemoryRecord, double)> GetNearestMatchesAsync(
        string collectionName,
        ReadOnlyMemory<float> embedding,
        int limit, double minRelevanceScore = 0,
        bool withEmbeddings = false,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var records = await this._milvusDbClient.FindNearestInCollectionAsync(collectionName, embedding, minRelevanceScore, limit, withEmbeddings, cancellationToken);

        foreach (var record in records)
        {
            yield return (record.Item1, record.Item2);
        }
    }

    /// <inheritdoc/>
    public async Task RemoveAsync(string collectionName, string key, CancellationToken cancellationToken = default)
    {
        await this._milvusDbClient.DeleteEntitiesByIdsAsync(collectionName, new string[] { key }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task RemoveBatchAsync(string collectionName, IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        await this._milvusDbClient.DeleteEntitiesByIdsAsync(collectionName, keys, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<string> UpsertAsync(string collectionName, MemoryRecord record, CancellationToken cancellationToken = default)
    {
        var ids = await this._milvusDbClient.UpsertEntitiesAsync(collectionName, new[] { record }, cancellationToken);

        return ids.FirstOrDefault() ?? string.Empty;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<string> UpsertBatchAsync(string collectionName, IEnumerable<MemoryRecord> records, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var ids = await this._milvusDbClient.UpsertEntitiesAsync(collectionName, records, cancellationToken);

        foreach (var id in ids)
        {
            yield return id;
        }
    }
}