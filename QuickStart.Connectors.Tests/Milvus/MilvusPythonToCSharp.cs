namespace QuickStart.Connectors.Tests.Milvus
{
    internal class MilvusPythonToCSharp
    {
        const string ID_FIELD = "id";
        const string ID_TYPE = "str";
        const string EMBEDDING_FIELD = "embedding";

        MilvusClient _client = new MilvusClient("192.168.186.129", port: 19530, null, null, null);

        public Dictionary<string, object> MemoryRecordToMilvusDict(MemoryRecord mem)
        {
            // Create an empty dictionary  
            var retDict = new Dictionary<string, object>();

            // Get all the properties of the class  
            var properties = mem.GetType().GetProperties();

            // Iterate over each property  
            foreach (var property in properties)
            {
                // Get the value of the property  
                var val = property.GetValue(mem);

                // If the value is not null  
                if (val != null)
                {
                    // Remove underscore and add to dictionary  
                    retDict.Add(property.Name.TrimStart('_'), val);
                }
            }

            // Return the dictionary  
            return retDict;
        }
        public MemoryRecord MilvusDictToMemoryRecord(Dictionary<string, object> milvusDict)
        {
            // Embedding needs conversion to array  
            var embedding = milvusDict.ContainsKey("embedding") ? milvusDict["embedding"] : null;
            if (embedding != null)
            {
                embedding = ((List<double>)embedding).ToArray(); // assumes the embedding was a list of doubles  
            }

            // Create a new MemoryRecord  
            var memoryRecord = new MemoryRecord
            {
                IsReference = milvusDict.ContainsKey("is_reference") ? (bool?)milvusDict["is_reference"] : null,
                ExternalSourceName = milvusDict.ContainsKey("external_source_name") ? (string)milvusDict["external_source_name"] : null,
                Id = milvusDict.ContainsKey("id") ? (string)milvusDict["id"] : null,
                Description = milvusDict.ContainsKey("description") ? (string)milvusDict["description"] : null,
                Text = milvusDict.ContainsKey("text") ? (string)milvusDict["text"] : null,
                AdditionalMetadata = milvusDict.ContainsKey("additional_metadata") ? (string)milvusDict["additional_metadata"] : null,
                Embedding = (double[])embedding,
                Key = milvusDict.ContainsKey("key") ? (string)milvusDict["key"] : null,
                Timestamp = milvusDict.ContainsKey("timestamp") ? (DateTime?)milvusDict["timestamp"] : null,
            };

            return memoryRecord;
        }
        public async Task<List<string>> UpsertBatchAsync(string collectionName, List<MemoryRecord> records, int batchSize = 100)
        {
            var collections = await _client.ListCollectionsAsync();

            // Check if the collection exists.  
            if (!collections.Any(x => x.Name == collectionName))
            {
                Console.WriteLine($"Collection {collectionName} does not exist, cannot insert.");
                throw new Exception($"Collection {collectionName} does not exist, cannot insert.");
            }

            // Convert the records to dicts  
            var insertList = records.Select(MemoryRecordToMilvusDict).ToList();

            // The ids to remove  
            var deleteIds = insertList.Select(insert => insert[ID_FIELD]).ToList();

            try
            {
                // First delete then insert to have upsert  
                await _client.GetCollection(collectionName).DeleteAsync(deleteIds);
                return await _client.GetCollection(collectionName).InsertAsync(insertList);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Upsert failed due to: {e}");
                throw;
            }
        }
        public async Task<List<MemoryRecord>> GetBatchAsync(string collectionName, List<string> keys, bool withEmbeddings)
        {
            var collections = await _client.ListCollectionsAsync();

            // Check if the collection exists.  
            if (!collections.Any(x => x.Name == collectionName))
            {
                Console.WriteLine($"Collection {collectionName} does not exist, cannot get.");
                throw new Exception($"Collection {collectionName} does not exist, cannot get.");
            }
            try
            {
                var outputFields = withEmbeddings ? new List<string> { "*", EMBEDDING_FIELD } : new List<string> { "*" };
                var gets = await _client.GetAsync(collectionName, keys, outputFields);

                return gets.Select(get => MilvusDictToMemoryRecord(get)).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Get failed due to: {e}");
                throw;
            }
        }

    }

    public List<List<float>> Get(string collectionName, dynamic ids, List<string> outputFields = null, float? timeout = null, params object[] kwargs)
    {
        if (!(ids is List<dynamic>))
        {
            ids = new List<dynamic> { ids };
        }

        if (ids.Count == 0)
        {
            return new List<List<float>>();
        }

        var conn = _getConnection();
        Dictionary<string, object> schemaDict;
        try
        {
            schemaDict = conn.DescribeCollection(collectionName, timeout, kwargs);
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to describe collection: {collectionName}");
            throw;
        }

        if (outputFields == null)
        {
            outputFields = new List<string> { "*" };
            var vecFieldName = _getVectorFieldName(schemaDict);
            if (!string.IsNullOrEmpty(vecFieldName))
            {
                outputFields.Add(vecFieldName);
            }
        }

        var expr = _packPksExpr(schemaDict, ids);
        List<List<float>> res;
        try
        {
            res = conn.Query(collectionName, expr, outputFields, timeout, kwargs);
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to get collection: {collectionName}");
            throw;
        }

        return res;
    }
    public string GetVectorFieldName(Dictionary<string, object> schemaDict)
    {
        List<object> fields = schemaDict.ContainsKey("fields") ? (List<object>)schemaDict["fields"] : new List<object>();
        if (!fields.Any())
        {
            return "";
        }

        foreach (Dictionary<string, object> fieldDict in fields)
        {
            if (fieldDict.ContainsKey("type") && fieldDict["type"].Equals(DataType.FLOAT_VECTOR))
            {
                return fieldDict.ContainsKey("name") ? fieldDict["name"].ToString() : "";
            }
        }
        return "";
    }

    public string PackPksExpr(Dictionary<string, object> schemaDict, List<object> pks)
    {
        Dictionary<string, object> primaryField = ExtractPrimaryField(schemaDict);
        string pkFieldName = primaryField["name"].ToString();
        DataType dataType = (DataType)primaryField["type"];

        string expr = "";
        if (dataType == DataType.VARCHAR)
        {
            List<string> ids = pks.Select(entry => $"'{entry.ToString()}'").ToList();
            expr = $"{pkFieldName} in [{string.Join(",", ids)}]";
        }
        else
        {
            List<string> ids = pks.Select(entry => entry.ToString()).ToList();
            expr = $"{pkFieldName} in [{string.Join(",", ids)}]";
        }
        return expr;
    }

    public async Task CreateCollectionAsync(
    string collectionName,
    int dimensionNum = 1536,
    string distanceType = "IP",
    bool overwrite = false,
    string consistency = "Session"
)
    {
        // Assuming _client is an instance of your client class which has the methods ListCollections, DropCollection and CreateCollection.  

        if (_client.ListCollections().Contains(collectionName))
        {
            if (overwrite)
            {
                await _client.DropCollectionAsync(collectionName);
                await _client.CreateCollectionAsync(
                    collectionName,
                    dimensionNum,
                    ID_FIELD,
                    ID_TYPE,
                    false,
                    EMBEDDING_FIELD,
                    distanceType,
                    65535,
                    consistency
                );
            }
        }
        else
        {
            await _client.CreateCollectionAsync(
                collectionName,
                dimensionNum,
                ID_FIELD,
                ID_TYPE,
                false,
                EMBEDDING_FIELD,
                distanceType,
                65535,
                consistency
            );
        }
    }

}
