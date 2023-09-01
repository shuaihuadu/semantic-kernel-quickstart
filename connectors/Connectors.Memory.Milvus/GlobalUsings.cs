using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Memory;
using Milvus.Client;

public class MilvusMemoryStore : IMemoryStore
{
    private readonly MilvusClient _milvusClient;
    private readonly ILogger _logger;


    //Default field values
    public const string ID_FIELD = "id";
    public const string EMBEDDING_FIELD = "embedding";

    public MilvusMemoryStore(string host, int port = 19350, string? userName = null, string? password = null, string? database = null, ILoggerFactory? loggerFactory = null)
    {
        this._milvusClient = new MilvusClient(host, port, userName, password, database, loggerFactory: loggerFactory);
        this._logger = loggerFactory is not null ? loggerFactory.CreateLogger(nameof(MilvusMemoryStore)) : NullLogger.Instance;

    }

    /// <inheritdoc/>
    public async Task CreateCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        bool doesCollectionExists = await this.DoesCollectionExistAsync(collectionName, cancellationToken).ConfigureAwait(false);

        if (!doesCollectionExists)
        {
            //var fieldSchemas = new List<FieldSchema>();

            //fieldSchemas.Add(FieldSchema.CreateVarchar(ID_FIELD, 65535, true, false));
            //fieldSchemas.Add(FieldSchema.CreateFloatVector(EMBEDDING_FIELD, 1536));

            var collectionSchema = new CollectionSchema();

            collectionSchema.Name = collectionName;
            collectionSchema.Fields.Add(FieldSchema.CreateVarchar(ID_FIELD, 65535, true, false));
            collectionSchema.Fields.Add(FieldSchema.CreateFloatVector(EMBEDDING_FIELD, 1536));

            await this._milvusClient.CreateCollectionAsync(collectionName, collectionSchema).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public async Task DeleteCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        var collection = _milvusClient.GetCollection(collectionName);

        await collection.DropAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> DoesCollectionExistAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        return await this._milvusClient.HasCollectionAsync(collectionName, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public Task<MemoryRecord?> GetAsync(string collectionName, string key, bool withEmbedding = false, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<MemoryRecord> GetBatchAsync(string collectionName, IEnumerable<string> keys, bool withEmbeddings = false, CancellationToken cancellationToken = default)
    {
        var collection = this._milvusClient.GetCollection(collectionName);

        var expression = GetIdQueryExpression(keys);

        var outputFields = new string[] { "*" };

        if (withEmbeddings)
        {
            outputFields = outputFields.Concat(new string[] { EMBEDDING_FIELD }).ToArray();
        }

        collection.QueryAsync(expression);
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<string> GetCollectionsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
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
        var collection = _milvusClient.GetCollection(collectionName);

        await collection.DropAsync();
    }

    /// <inheritdoc/>
    public Task RemoveBatchAsync(string collectionName, IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<string> UpsertAsync(string collectionName, MemoryRecord record, CancellationToken cancellationToken = default)
    {
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<string> UpsertBatchAsync(string collectionName, IEnumerable<MemoryRecord> records, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    private string GetIdQueryExpression(IEnumerable<string> ids)
    {
        //var ids = ids.Select(entry => $"'{entry}'").ToList();
        ids = ids.Select(entry => $"\"{entry}\"").ToList();

        return $"{ID_FIELD} in [{string.Join(",", ids)}]";
    }
}