namespace Microsoft.SemanticKernel.Connectors.Memory.Milvus;

public class MilvusDbClient : IMilvusDbClient
{
    private readonly MilvusClient _milvusClient;
    private readonly ILogger _logger;

    //Default field values
    public const string ID_FIELD = "id";
    public const string EMBEDDING_FIELD = "embedding";
    public const string META_FIELD = "$meta";

    public const int VECTOR_SIZE = 1536;
    public const int VARCHAR_MAX_LENGTH = 65535;

    public const int MilvusPort = 19530;

    public MilvusDbClient(string host, int port = MilvusPort, string? userName = null, string? password = null, string? database = null, ILoggerFactory? loggerFactory = null)
    {
        this._milvusClient = new MilvusClient(host, port, userName, password, database, loggerFactory: loggerFactory);
        this._logger = loggerFactory is not null ? loggerFactory.CreateLogger(nameof(MilvusDbClient)) : NullLogger.Instance;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<string> ListCollectionsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        this._logger.LogDebug("Listing collections");

        var collections = await _milvusClient.ListCollectionsAsync();

        foreach (var collection in collections)
        {
            yield return collection.Name;
        }
    }

    /// <inheritdoc />
    public async Task CreateCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        this._logger.LogDebug($"Creating collection {collectionName}");

        CollectionSchema collectionSchema = new CollectionSchema
        {
            Name = collectionName,
            EnableDynamicFields = true
        };

        collectionSchema.Fields.Add(FieldSchema.CreateVarchar(ID_FIELD, VARCHAR_MAX_LENGTH, true, false));
        collectionSchema.Fields.Add(FieldSchema.CreateFloatVector(EMBEDDING_FIELD, VECTOR_SIZE));

        MilvusCollection collection = await this._milvusClient.CreateCollectionAsync(collectionName, collectionSchema, cancellationToken: cancellationToken).ConfigureAwait(false);

        await collection.CreateIndexAsync(EMBEDDING_FIELD, milvusIndexType: IndexType.AutoIndex, milvusMetricType: SimilarityMetricType.Ip, cancellationToken: cancellationToken).ConfigureAwait(false);

        await collection.LoadAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> DoesCollectionExistAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        this._logger.LogDebug($"Checking collection {collectionName} exists");

        return await this._milvusClient.HasCollectionAsync(collectionName, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        this._logger.LogDebug($"Deleting collection {collectionName}");

        var collection = _milvusClient.GetCollection(collectionName);

        await collection.DropAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MemoryRecord>> GetFieldDataByIdsAsync(string collectionName, IEnumerable<string> ids, bool withEmbeddings, CancellationToken cancellationToken)
    {
        var collection = this._milvusClient.GetCollection(collectionName);

        var expression = GetIdQueryExpression(ids);

        QueryParameters queryParameters = new();

        queryParameters.OutputFields.Add("*");

        if (withEmbeddings)
        {
            queryParameters.OutputFields.Add(EMBEDDING_FIELD);
        }

        var queryResult = await collection.QueryAsync(expression, queryParameters, cancellationToken: cancellationToken).ConfigureAwait(false);

        return this.GetMemoryRecordFromFieldData(queryResult);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> UpsertEntitiesAsync(string collectionName, IEnumerable<MemoryRecord> records, CancellationToken cancellationToken = default)
    {
        MilvusCollection collection = _milvusClient.GetCollection(collectionName);

        var ids = records.Select(r => r.Key).ToList();

        var deleteExpression = GetIdQueryExpression(ids);

        MutationResult deleteResult = await collection.DeleteAsync(deleteExpression, cancellationToken: cancellationToken);

        var fieldDatas = GetFieldDataFromMemoryRecord(records.ToArray());

        MutationResult insertResult = await collection.InsertAsync(fieldDatas, cancellationToken: cancellationToken);

        return insertResult.Ids.StringIds ?? Enumerable.Empty<string>().ToList().AsReadOnly();
    }

    /// <inheritdoc />
    public async Task DeleteEntitiesByIdsAsync(string collectionName, IEnumerable<string> ids, CancellationToken cancellationToken = default)
    {
        MilvusCollection collection = _milvusClient.GetCollection(collectionName);

        var deleteExpression = GetIdQueryExpression(ids);

        await collection.DeleteAsync(deleteExpression, cancellationToken: cancellationToken);
    }

    private string GetIdQueryExpression(IEnumerable<string> ids)
    {
        ids = ids.Select(entry => $"\"{entry}\"").ToList();

        return $"{ID_FIELD} in [{string.Join(",", ids)}]";
    }


    private IReadOnlyList<MemoryRecord> GetMemoryRecordFromFieldData(IEnumerable<FieldData> fieldDatas)
    {
        var idFiled = fieldDatas.FirstOrDefault(field => field.FieldName == ID_FIELD);
        var metaField = fieldDatas.FirstOrDefault(field => field.FieldName == META_FIELD);
        var embeddingField = fieldDatas.FirstOrDefault(field => field.FieldName == EMBEDDING_FIELD);

        var result = new List<MemoryRecord>();

        if (idFiled != null && metaField != null)
        {
            var idStringField = idFiled as FieldData<string>;
            var metaStringField = metaField as FieldData<string>;
            FloatVectorFieldData? vectorField = null;

            if (embeddingField != null)
            {
                vectorField = embeddingField as FloatVectorFieldData;
            }

            for (var i = 0; i < idFiled!.RowCount; i++)
            {
                result.Add(MemoryRecord.FromJsonMetadata(metaStringField!.Data[i], vectorField == null ? null : vectorField.Data[i], idStringField!.Data[i]));
            }
        }

        return result;
    }

    private IReadOnlyList<FieldData> GetFieldDataFromMemoryRecord(MemoryRecord memoryRecord)
    {
        return this.GetFieldDataFromMemoryRecord(new MemoryRecord[] { memoryRecord });
    }

    private IReadOnlyList<FieldData> GetFieldDataFromMemoryRecord(IReadOnlyList<MemoryRecord> memoryRecords)
    {
        if (memoryRecords == null || memoryRecords.Count == 0)
        {
            return Array.Empty<FieldData>();
        }

        var ids = memoryRecords.Select(record => record.Key).ToList().AsReadOnly();
        var embeddings = memoryRecords.Select(record => record.Embedding).ToList().AsReadOnly();
        var metas = memoryRecords.Select(record => record.GetSerializedMetadata()).ToList().AsReadOnly();

        return new List<FieldData> {
            FieldData.CreateVarChar(ID_FIELD, ids),
            FieldData.CreateFloatVector(EMBEDDING_FIELD, embeddings),
            FieldData.CreateJson(META_FIELD, metas,true)
        }.AsReadOnly();
    }
}