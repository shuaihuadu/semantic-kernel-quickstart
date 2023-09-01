using System.Runtime.CompilerServices;

namespace Connectors.Memory.Milvus;

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
    public Task<MemoryRecord?> GetAsync(string collectionName, string key, bool withEmbedding = false, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<MemoryRecord> GetBatchAsync(string collectionName, IEnumerable<string> keys, bool withEmbeddings = false,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var result = this._milvusDbClient.GetFiledDataByIdsAsync(collectionName, keys, withEmbeddings, cancellationToken);

        await foreach (var item in result)
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
    public Task<(MemoryRecord, double)?> GetNearestMatchAsync(string collectionName, ReadOnlyMemory<float> embedding, double minRelevanceScore = 0, bool withEmbedding = false, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<(MemoryRecord, double)> GetNearestMatchesAsync(string collectionName, ReadOnlyMemory<float> embedding, int limit, double minRelevanceScore = 0, bool withEmbeddings = false, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public async Task RemoveAsync(string collectionName, string key, CancellationToken cancellationToken = default)
    {

    }

    /// <inheritdoc/>
    public Task RemoveBatchAsync(string collectionName, IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public async Task<string> UpsertAsync(string collectionName, MemoryRecord record, CancellationToken cancellationToken = default)
    {
        return string.Empty;
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<string> UpsertBatchAsync(string collectionName, IEnumerable<MemoryRecord> records, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
