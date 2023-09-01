using System.Runtime.CompilerServices;

namespace Connectors.Memory.Milvus;

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

    public MilvusDbClient(string host, int port = 19530, string? userName = null, string? password = null, string? database = null, ILoggerFactory? loggerFactory = null)
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
            Name = collectionName
        };

        collectionSchema.Fields.Add(FieldSchema.CreateVarchar(ID_FIELD, VARCHAR_MAX_LENGTH, true, false));
        collectionSchema.Fields.Add(FieldSchema.CreateFloatVector(EMBEDDING_FIELD, VECTOR_SIZE));

        await this._milvusClient.CreateCollectionAsync(collectionName, collectionSchema, cancellationToken: cancellationToken).ConfigureAwait(false);
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
    public async IAsyncEnumerable<MemoryRecord> GetFiledDataByIdsAsync(string collectionName, IEnumerable<string> keys, bool withEmbeddings, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var collection = this._milvusClient.GetCollection(collectionName);

        var expression = GetIdQueryExpression(keys);

        QueryParameters queryParameters = new QueryParameters();

        queryParameters.OutputFields.Add("*");

        if (withEmbeddings)
        {
            queryParameters.OutputFields.Add(EMBEDDING_FIELD);
        }

        var queryResult = await collection.QueryAsync(expression, queryParameters, cancellationToken: cancellationToken).ConfigureAwait(false);

        var memoryRecords = this.GetMemoryRecordFromFieldData(queryResult);

        foreach (var memoryRecord in memoryRecords)
        {
            yield return memoryRecord;
        }
    }

    public Task UpsertVectorsAsync(string collectionName, IEnumerable<MemoryRecord> records, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
        //MilvusCollection collection = _milvusClient.GetCollection(collectionName);

        //// Convert the records to dicts  
        //var insertList = records.Select(record => MemoryRecordToMilvusDict(record)).ToList();

        //// The ids to remove  
        //var deleteIds = insertList.Select(insert => insert[ID_FIELD]).ToList();

        //try
        //{
        //    // First delete then insert to have upsert
        //    collection.DeleteAsync(GetIdQueryExpression(deleteIds));

        //    collection.InsertAsync()

        //    await _client.Insert(collectionName, insertList, batchSize);

        //    _milvusClient.Ins

        //}
        //catch (Exception e)
        //{
        //    _logger.Debug($"Upsert failed due to: {e}");
        //    throw;
        //}
    }

    private string GetIdQueryExpression(IEnumerable<string> ids)
    {
        ids = ids.Select(entry => $"\"{entry}\"").ToList();

        return $"{ID_FIELD} in [{string.Join(",", ids)}]";
    }


    private IEnumerable<MemoryRecord> GetMemoryRecordFromFieldData(IEnumerable<FieldData> fieldDatas)
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

    private FieldData GetFieldDataFromMemoryRecoed(MemoryRecord memoryRecord, string fieldName)
    {
        return FieldData.Create(fieldName, new List<string> { memoryRecord.Key });
    }
}

